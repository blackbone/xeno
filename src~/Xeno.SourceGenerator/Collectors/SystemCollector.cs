using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

public static class SystemCollector {
    public static bool Check(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not AttributeSyntax attributeSyntax) return false;
        if (!attributeSyntax.Name.ToString().Contains("RegisterSystem")) return false;

        return true;
    }

    internal static SystemGroup Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        if (!Ensure.IsEcsAssembly(context.SemanticModel.Compilation)) return null;
        if (!Ensure.Type(context.SemanticModel.Compilation, "Xeno.RegisterSystemAttribute", out var attributeType)) return null;

        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;
        if (context.Node is not AttributeSyntax attribute) return null;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute.Name);
        if (symbolInfo.Symbol == null) return null;
        if (symbolInfo.Symbol is not IMethodSymbol attributeCtorSymbol) return null;
        if (!attributeCtorSymbol.ContainingType.Equals(attributeType, SymbolEqualityComparer.Default)) return null;

        var type = (attribute.ArgumentList?.Arguments[0].Expression as TypeOfExpressionSyntax)?.Type;
        if (type == null) return null;
        if (!Ensure.Type(compilation, type.ToString(), out var typeSymbol)) return null;

        var requiresExternalInstance = (attribute.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression as LiteralExpressionSyntax)?.Token.Value as bool?;

        return new SystemGroup(compilation, typeSymbol, requiresExternalInstance ?? false);
    }
}
