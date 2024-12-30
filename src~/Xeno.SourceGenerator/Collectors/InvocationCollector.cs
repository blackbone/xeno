using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator.Collectors;

internal static class InvocationCollector {
    public static bool Filter(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        => syntaxNode is InvocationExpressionSyntax;

    public static InvocationCandidate Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.ArgumentList.Arguments.Count == 0)
            return null;

        var semanticModel = context.SemanticModel;

        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        var objectType = memberAccess != null
            ? ModelExtensions.GetTypeInfo(semanticModel, memberAccess.Expression).Type as INamedTypeSymbol
            : null;

        if (objectType == null)
            return null;

        var argumentTypes = invocation.ArgumentList.Arguments
            .Select(arg => new InvocationCandidateArg(semanticModel, arg))
            .ToImmutableArray();

        return new InvocationCandidate(objectType, memberAccess.Name.Identifier.Text, argumentTypes);
    }
}
