using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

public sealed class Component(INamedTypeSymbol type, int? priority) {
    public readonly INamedTypeSymbol Type = type;
    public readonly int? Priority = priority;
    public int Index;
    public uint PersistentId;

    public string StoreName => $"s_{Index}";
    public string StoreTypeName => $"Store_{Type.Name}";
    public string TypeFullName => Type.ToDisplayString();
}

public static class ComponentCollector {
    public static bool Check(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not AttributeSyntax attributeSyntax) return false;
        if (!attributeSyntax.Name.ToString().StartsWith("RegisterComponent")) return false;

        return true;
    }

    public static Component Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        if (!Ensure.IsEcsAssembly(context.SemanticModel.Compilation)) return null;

        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;
        var attribute = context.Node as AttributeSyntax;

        var type = (attribute?.ArgumentList?.Arguments[0].Expression as TypeOfExpressionSyntax)?.Type;
        if (type == null) return null;

        if (!Ensure.Type(compilation, type, out var typeSymbol)) return null;

        int? priority = null;
        if (attribute.ArgumentList?.Arguments.Count >= 2)
            priority = (attribute.ArgumentList?.Arguments[1].Expression as LiteralExpressionSyntax)?.Token.Value as int?;

        return new Component(typeSymbol, priority);
    }
}
