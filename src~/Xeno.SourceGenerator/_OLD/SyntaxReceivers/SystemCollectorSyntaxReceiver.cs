using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Xeno.SourceGenerator.SyntaxReceivers;

internal enum SystemMethodType
{
    Startup,
    PreUpdate,
    Update,
    PostUpdate,
    Shutdown
}

internal readonly struct SystemInfo
{
    public readonly ClassDeclarationSyntax Declaration;
    public readonly INamedTypeSymbol Symbol;
    public readonly ImmutableArray<SystemMethodInfo> Methods;

    public SystemInfo(ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, ImmutableArray<SystemMethodInfo> methods)
    {
        Declaration = declaration;
        Symbol = symbol;
        Methods = methods;
    }
}

internal readonly struct SystemMethodInfo
{
    public readonly MethodDeclarationSyntax Declaration;
    public readonly IMethodSymbol Symbol;
    public readonly SystemMethodType InvocationGroup;
    public readonly int Order;
    public readonly ImmutableArray<IParameterSymbol> ComponentParameters;

    public SystemMethodInfo(IMethodSymbol symbol, AttributeData attribute, INamedTypeSymbol componentInterfaceType)
    {
        Symbol = symbol;
        Declaration = symbol.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
        InvocationGroup = (SystemMethodType)(int)attribute.ConstructorArguments[0].Value!;
        Order = (int)attribute.ConstructorArguments[1].Value!;
        ComponentParameters = symbol.Parameters.RemoveAll(
            p => !p.Type.IsValueType || !p.Type.AllInterfaces.Contains(componentInterfaceType));
    }
}

internal class SystemCollectorSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<ClassDeclarationSyntax> candidates = new();

    public ImmutableArray<SystemInfo> Value { get; private set; }

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration) return;
        candidates.Add(classDeclaration);
    }

    public void VerifySystems(Compilation compilation)
    {
        if (!Ensure.Type(compilation, "Xeno.IComponent", out var componentInterfaceType))
            throw new InvalidOperationException();
        
        if (!Ensure.Type(compilation, "Xeno.SystemAttribute", out var systemAttributeType))
            throw new InvalidOperationException();
        
        if (!Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType))
            throw new InvalidOperationException();

        List<SystemInfo> systems = [];
        for (var i = candidates.Count - 1; i >= 0; i--)
        {
            var semanticModel = compilation.GetSemanticModel(candidates[i].SyntaxTree, true);
            var symbol = semanticModel.GetDeclaredSymbol(candidates[i]);
            if (symbol == null)
            {
                candidates.RemoveAt(i);
                continue;
            }
            
            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                candidates.RemoveAt(i);
                continue;
            }

            var attributes = namedTypeSymbol.GetAttributes();
            if (attributes.Length == 0)
            {
                candidates.RemoveAt(i);
                continue;
            }

            if (attributes.All(ad => !ad.AttributeClass?.Equals(systemAttributeType, SymbolEqualityComparer.Default) ?? false))
            {
                candidates.RemoveAt(i);
                continue;
            }

            List<SystemMethodInfo> systemMethodInfos = [];
            foreach (var methodSymbol in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var attribute = methodSymbol
                    .GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false);
                if (attribute == null) continue;
                
                systemMethodInfos.Add(new SystemMethodInfo(methodSymbol, attribute, componentInterfaceType));
            }

            systems.Add(new SystemInfo(candidates[i], namedTypeSymbol, systemMethodInfos.ToImmutableArray()));
        }

        Value = systems.ToImmutableArray();
    }
}
