using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator.Collectors;

internal class InvocationCandidateArg {
    public readonly RefKind RefKind;
    public readonly INamedTypeSymbol Type;
    public readonly Location Location;

    public InvocationCandidateArg(SemanticModel semanticModel, ArgumentSyntax arg) {
        RefKind = ToRefKind(arg.RefOrOutKeyword);
        Type = ModelExtensions.GetTypeInfo(semanticModel, arg.Expression).Type as INamedTypeSymbol;
        Location = arg.GetLocation();
    }


    private static RefKind ToRefKind(SyntaxToken token) => token.Kind() switch {
        SyntaxKind.RefKeyword => RefKind.Ref,
        SyntaxKind.OutKeyword => RefKind.Out,
        _ => RefKind.None
    };
}

internal class InvocationCandidate {
    public readonly INamedTypeSymbol Target;
    public readonly string MethodName;
    public readonly ImmutableArray<InvocationCandidateArg> Args;

    public InvocationCandidate(INamedTypeSymbol target, string methodName, ImmutableArray<InvocationCandidateArg> args) {
        Target = target;
        MethodName = methodName;
        Args = args;
    }

    public bool IsValid(Generation generation) {
        switch (MethodName) {
            case "Create":
                return Target.Equals(generation.AssemblyInfo.WorldType, SymbolEqualityComparer.Default)
                    && Args.Length > 0
                    && Args.All(a => Extensions.IsValidComponentType(a.Type, generation.Compilation));
            case "Add":
                if (Target.Equals(generation.AssemblyInfo.WorldType, SymbolEqualityComparer.Default)) {
                    return Args.Length > 1
                        && Args.ElementAt(0).Type.Equals(generation.AssemblyInfo.EntityType, SymbolEqualityComparer.Default)
                        && Args.Skip(1).All(a => a.Type.IsValidComponentType(generation.Compilation));
                }
                if (Target.Equals(generation.AssemblyInfo.EntityType, SymbolEqualityComparer.Default)) {
                    return Args.Length > 0
                        && Args.All(a => a.Type.IsValidComponentType(generation.Compilation));
                }
                return false;
            case "Remove":
                if (Target.Equals(generation.AssemblyInfo.WorldType, SymbolEqualityComparer.Default)) {
                    return Args.Length > 1
                        && Args.ElementAt(0).Type.Equals(generation.AssemblyInfo.EntityType, SymbolEqualityComparer.Default)
                        && Args.Skip(1).All(a => a.Type.IsValidComponentType(generation.Compilation));
                }
                if (Target.Equals(generation.AssemblyInfo.EntityType, SymbolEqualityComparer.Default)) {
                    return Args.Length > 0
                        && Args.All(a => a.Type.IsValidComponentType(generation.Compilation) && a.RefKind == RefKind.Out);
                }
                return false;
            case "Has":
            case "HasAll":
            case "HasAny":
                return true;
            default:
                return false;
        }
    }
}
