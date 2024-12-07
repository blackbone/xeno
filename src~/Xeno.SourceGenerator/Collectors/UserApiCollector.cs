using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        return new UserApiCall(invocation);
    }
}
