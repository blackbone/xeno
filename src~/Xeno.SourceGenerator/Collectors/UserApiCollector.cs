using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Xeno.SourceGenerator;

public sealed class UserApiCall {
    public readonly InvocationExpressionSyntax MethodSyntax;

    public UserApiCall(InvocationExpressionSyntax methodSyntax) {
        MethodSyntax = methodSyntax;
    }
}

public class UserApiCollector {

    public static bool Check(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not InvocationExpressionSyntax invocation) return false;

        return invocation.IsValidEntityCallCandidate() || invocation.IsValidWorldCallCandidate();
    }

    public static UserApiCall Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        if (context.Node is not InvocationExpressionSyntax invocation) return null;

        Ensure.Type(context.SemanticModel.Compilation, "EntityExtensions", out var entityExtensionsType);
        Ensure.Type(context.SemanticModel.Compilation, "WorldExtensions", out var worldExtensionsType);
        var entityMethods = entityExtensionsType.GetMembers().OfType<IMethodSymbol>().ToArray();
        var worldMethods = worldExtensionsType.GetMembers().OfType<IMethodSymbol>().ToArray();

        var methodSymbol = entityMethods.FirstOrDefault(m => Match(context, invocation, m));
        methodSymbol ??= worldMethods.FirstOrDefault(m => Match(context, invocation, m));

        if (methodSymbol == null)
            return null;

        return new UserApiCall(invocation);
    }

    private static bool Match(GeneratorSyntaxContext context, InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) {
        return false;
    }
}
