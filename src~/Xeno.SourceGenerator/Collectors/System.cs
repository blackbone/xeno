using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Xeno.SourceGenerator.SyntaxReceivers;

namespace Xeno.SourceGenerator;

internal sealed class System(SystemGroup group, SystemMethodType type, int order, IMethodSymbol method) {
    public readonly SystemGroup Group = group;
    public readonly SystemMethodType Type = type;
    public readonly int Order = order;
    public readonly IMethodSymbol Method = method;
    public readonly bool IsStatic = method.IsStatic;
    public readonly ImmutableArray<IParameterSymbol> Parameters = method.Parameters;

    public int Index;

    public string FilterName => $"filter_{Index}";

    public string Invocation() => IsStatic
        ? $"{Group.TypeFullName}.{Method.Name}"
        : $"{Group.FieldName}.{Method.Name}";

    private string GetParametersString() {
        var result = "";
        foreach (var parameter in Method.Parameters) {
            if (parameter.RefKind == RefKind.Ref)
                result += "ref ";
            else if (parameter.RefKind == RefKind.Out)
                result += "out ";
        }
        return result;
    }
}
