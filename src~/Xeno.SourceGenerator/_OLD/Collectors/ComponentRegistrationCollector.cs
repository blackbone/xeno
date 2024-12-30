using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

public static class ComponentRegistrationCollector {
    internal static IncrementalValuesProvider<Component> Register(Func<Func<SyntaxNode, CancellationToken, bool>, Func<GeneratorSyntaxContext, CancellationToken, Component>, IncrementalValuesProvider<Component>> register) => register(Check, Transform);

    private static bool Check(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not AttributeSyntax attributeSyntax) return false;
        if (!attributeSyntax.Name.ToString().StartsWith("RegisterComponent")) return false;

        return true;
    }

    private static Component Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        return null;
        // if (!Ensure.IsEcsAssembly(context.SemanticModel.Compilation)) return null;
        //
        // var semanticModel = context.SemanticModel;
        // var compilation = semanticModel.Compilation;
        // if (context.Node is not AttributeSyntax attribute) return null;
        //
        // var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute, cancellationToken);
        //
        // var type = (attribute?.ArgumentList?.Arguments[0].Expression as TypeOfExpressionSyntax)?.Type;
        // if (type == null) return null;
        //
        // if (!Ensure.Type(compilation, type, out var typeSymbol)) return null;
        //
        //
        // int? priority = null;
        // if (attribute.ArgumentList?.Arguments.Count >= 2)
        //     priority = (attribute.ArgumentList?.Arguments[1].Expression as LiteralExpressionSyntax)?.Token.Value as int?;
        //
        // return new Component(typeSymbol, priority);
    }
}
