using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xeno.SourceGenerator.SyntaxReceivers;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SystemAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor NonClassSystemDeclaration = new(
        "XEN000",
        "System is not a class",
        "System must be a class!",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Don't use [System] attribute on structs or records, it can cause unexpected behaviour!");

    private static readonly DiagnosticDescriptor NonPartialClassSystemDeclaration = new(
        "XEN001",
        "System is not partial",
        "System must be partial!",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Add partial modifier to system type under [System] attribute.");

    private static readonly DiagnosticDescriptor NoMethodsDeclared = new(
        "XEN002",
        "No Methods Declared",
        "At least one method with [SystemMethod] must be declared!",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Add methods with with [SystemMethod] attributes.");
    
    private static readonly DiagnosticDescriptor SystemMethodNotMachSignature = new(
        "XEN003",
        "Invalid method signature",
        "System method must match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...)",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Change method signature to match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...).");
        
    private static readonly DiagnosticDescriptor SystemMethodNotMachSignature_ReturnType = new(
        "XEN003.1",
        "Invalid method signature",
        "System method must return void",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Change method return type to void.");
    
    private static readonly DiagnosticDescriptor SystemMethodNotMachSignature_NoParameters = new(
        "XEN003.2",
        "Invalid method signature",
        "System method must return have parameters",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Change method signature to match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...).");
    
    private static readonly DiagnosticDescriptor StartupShutdownMethodInvalidSignature = new(
        "XEN004",
        "Invalid method signature",
        "Startup and shut down methods must be void with no parameters",
        "Xeno",
        DiagnosticSeverity.Error,
        true,
        "Change method signature to void().");


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            NonClassSystemDeclaration,
            NonPartialClassSystemDeclaration,
            NoMethodsDeclared,
            SystemMethodNotMachSignature,
            SystemMethodNotMachSignature_ReturnType,
            SystemMethodNotMachSignature_NoParameters,
            StartupShutdownMethodInvalidSignature);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        if (!Ensure.Type(context.Compilation, "Xeno.SystemAttribute", out var systemAttributeType)) return;
        if (!Ensure.Type(context.Compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType)) return;
        if (!Ensure.Type(context.Compilation, "Xeno.IComponent", out var componentInterfaceType)) return;
        if (!Ensure.Type(context.Compilation, "Xeno.Entity", out var entityType)) return;
        
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        var attributes = typeSymbol.GetAttributes();
        if (!attributes.Any(a => a.AttributeClass?.Equals(systemAttributeType, SymbolEqualityComparer.Default) ?? false))
            return;

        // now we know that it has attribute
        // check it's class
        var classDeclaration = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault(s => s.GetSyntax() is ClassDeclarationSyntax)?.GetSyntax() as ClassDeclarationSyntax;
        if (classDeclaration == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(NonClassSystemDeclaration, typeSymbol.Locations[0], typeSymbol.Name));
            return;
        }

        if (classDeclaration.Modifiers.All(m => !m.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(NonPartialClassSystemDeclaration, typeSymbol.Locations[0], typeSymbol.Name));
            return;
        }
        
        foreach(var method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            var attribute = method.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false);
            if (attribute == null) continue;
            if (attribute.ConstructorArguments[0].Value == null) continue;

            var type = (SystemMethodType)(int)attribute.ConstructorArguments[0].Value;
            if (type is SystemMethodType.Startup or SystemMethodType.Shutdown)
            {
                if (!method.ReturnsVoid || method.Parameters.Length > 0)
                    context.ReportDiagnostic(Diagnostic.Create(StartupShutdownMethodInvalidSignature, method.Locations[0], method.Name));
            }
            else
            {
                // void (ref C1, ref C2, ref C3...)
                if (method.Parameters.All(p => p.Type.AllInterfaces.Contains(componentInterfaceType) && p.RefKind == RefKind.Ref))
                    continue;
                
                // void (in Entity, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.Equals(entityType, SymbolEqualityComparer.Default) && method.Parameters[0].RefKind == RefKind.In
                    && method.Parameters.Skip(1).All(p => p.Type.AllInterfaces.Contains(componentInterfaceType) && p.RefKind == RefKind.Ref))
                    continue;
                
                // void (in Uniform, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.IsValueType && method.Parameters[0].RefKind == RefKind.In
                    && method.Parameters.Skip(1).All(p => p.Type.AllInterfaces.Contains(componentInterfaceType) && p.RefKind == RefKind.Ref))
                    continue;
                
                // void (in Entity, in Uniform, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.Equals(entityType, SymbolEqualityComparer.Default) && method.Parameters[0].RefKind == RefKind.In
                    && method.Parameters[1].Type.IsValueType && method.Parameters[1].RefKind == RefKind.In
                    && method.Parameters.Skip(2).All(p => p.Type.AllInterfaces.Contains(componentInterfaceType) && p.RefKind == RefKind.Ref))
                    continue;
                
                // TODO restore later
                // context.ReportDiagnostic(Diagnostic.Create(SystemMethodNotMachSignature, method.Locations[0], method.Name));
                if (!method.ReturnsVoid) context.ReportDiagnostic(Diagnostic.Create(SystemMethodNotMachSignature_ReturnType, method.Locations[0], method.Name));
                if (method.Parameters.Length == 0) context.ReportDiagnostic(Diagnostic.Create(SystemMethodNotMachSignature_NoParameters, method.Locations[0], method.Name));
            }
        }
    }
}