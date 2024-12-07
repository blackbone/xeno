using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xeno.SourceGenerator.SyntaxReceivers;

namespace Xeno.SourceGenerator;

internal static class Extensions
{
    public static void Add(this SourceProductionContext context, string hint, CompilationUnitSyntax root) {
        context.AddSource(hint, SourceText.From(root.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
    }

    public static bool IsValidSystemMethod(this IMethodSymbol method, Compilation compilation) {
        Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType);
        return method.GetAttributes().Any(a => a.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false)
            && method.Parameters.All(p => p.IsValidEntityParameter() || p.IsValidUniformParameter(compilation) || p.IsValidComponentParameter());
    }

    public static bool GetSystemAttributeValues(this IMethodSymbol method, Compilation compilation, out SystemMethodType type, out int order) {
        type = default;
        order = default;

        Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType);
        var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null) return false;
        type = attribute.ConstructorArguments.ElementAtOrDefault(0).Value is int intv1  ? (SystemMethodType)intv1 : default;
        order = attribute.ConstructorArguments.ElementAtOrDefault(1).Value is int intv2 ? intv2 : 0;
        return true;
    }

    public static bool IsValidEntityParameter(this IParameterSymbol parameter) {
        return parameter.Type.Name.Equals("Entity")
            && parameter.RefKind == RefKind.In;
    }

    public static bool IsValidUniformParameter(this IParameterSymbol parameter, in Compilation compilation) {
        Ensure.Type(compilation, "Xeno.UniformAttribute", out var uniformAttributeType);
        return parameter.GetAttributes().Any(a => a.AttributeClass?.Equals(uniformAttributeType, SymbolEqualityComparer.Default) ?? false)
            && parameter.RefKind != RefKind.Out;
    }

    public static bool IsValidComponentParameter(this IParameterSymbol parameter) {
        // can't really check anything in this step
        return parameter.RefKind is RefKind.In or RefKind.Ref;
    }

    public static uint GetPersistentHashCode(this INamedTypeSymbol type, Compilation compilation) {
        Ensure.Type(compilation, "System.Runtime.InteropServices.GuidAttribute", out var guidAttributeType);

        var attribute = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(guidAttributeType, SymbolEqualityComparer.Default) ?? false);
        var name = attribute?.ConstructorArguments.ElementAtOrDefault(0).Value as string;
        name ??= $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";

        return MurMur.Hash32(Encoding.UTF8.GetBytes(name), 37);
    }

    private static readonly Dictionary<string, string[]> entityCallNames = new() {
        { "Add", [null] },              // void Add(in Component c, ...)
        { "Remove", ["ref"] },          // bool Remove(out Component c, ...)
        { "Has", [null] },              // void Has(default(Component), ...)
        { "Get", ["ref"] }              // void Get(ref Component1 c, ...)
    };

    private static readonly Dictionary<string, string[]> worldCallNames = new() {
        { "Create", [null] },           // void Create(in Component c, ...)
        { "Add", [null] },              // void Add(in Component c, ...)
        { "Remove", ["ref"] },          // bool Remove(ref Component c, ...)
        { "Has", [null] },              // void Has(default(Component), ...)
        { "Get", ["ref"] }              // void Get(ref Component1 c, ...)
    };

    public static bool IsValidEntityCallCandidate(this InvocationExpressionSyntax invocation) {
        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && entityCallNames.TryGetValue(memberAccess.Name.ToString(), out var refKind)
            && invocation.ArgumentList.Arguments.Count > 0 // at least one component arg
            && invocation.ArgumentList.Arguments.All(a => refKind.Contains(a.RefOrOutKeyword.Value?.ToString()));
    }

    public static bool IsValidWorldCallCandidate(this InvocationExpressionSyntax invocation) {
        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && worldCallNames.TryGetValue(memberAccess.Name.ToString(), out var refKind)
            && invocation.ArgumentList.Arguments.Count > 1 // entity + at least one component arg
            && invocation.ArgumentList.Arguments.All(a => refKind.Contains(a.RefOrOutKeyword.Value?.ToString()));
    }
}
