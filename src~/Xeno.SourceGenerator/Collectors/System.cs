using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xeno.SourceGenerator.SyntaxReceivers;

namespace Xeno.SourceGenerator;

internal sealed class System {
    public readonly SystemGroup Group;
    public readonly SystemMethodType Type;
    public readonly int Order;
    public readonly IMethodSymbol Method;
    public readonly bool IsStatic;
    public readonly ImmutableArray<IParameterSymbol> Parameters;
    public Filter Filter;

    public int Index;
    public System(SystemGroup group, SystemMethodType type, int order, IMethodSymbol method) {
        Group = group;
        Type = type;
        Order = order;
        Method = method;
        IsStatic = method.IsStatic;
        Parameters = method.Parameters;
    }

    public void InitFilter(GeneratorInfo info, ImmutableArray<ITypeSymbol> with, ImmutableArray<ITypeSymbol> without) {
        var usedComponents = Parameters
            .Where(p => p.IsValidComponentParameter(info))
            .Select(p => p.Type);
        Filter = new Filter(info, with.AddRange(usedComponents), without);
    }

    public string Invocation() => IsStatic
        ? $"{Group.TypeFullName}.{Method.Name}"
        : $"{Group.FieldName}.{Method.Name}";
}
