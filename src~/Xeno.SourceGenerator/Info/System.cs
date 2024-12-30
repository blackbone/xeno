using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class System {
    public readonly SystemGroup Group;
    public readonly SystemType Type;
    public readonly int Order;
    public readonly IMethodSymbol Method;
    public readonly bool IsStatic;
    public readonly ImmutableArray<IParameterSymbol> Parameters;

    public Filter Filter;

    public System(SystemGroup group, SystemType type, int order, IMethodSymbol method) {
        Group = group;
        Type = type;
        Order = order;
        Method = method;
        IsStatic = method.IsStatic;
        Parameters = method.Parameters;
    }

    public void Init(Generation generation, ImmutableArray<ITypeSymbol> with, ImmutableArray<ITypeSymbol> without) {
        var usedComponents = Parameters
            .Where(p => p.IsValidComponentParameter(generation.Compilation, generation.AssemblyInfo.Components))
            .Select(p => p.Type);

        with = with.AddRange(usedComponents);
        if (!with.IsEmpty || !without.IsEmpty)
            Filter = new Filter(generation, with, without);
    }

    public string Invocation() => IsStatic
        ? $"{Group.TypeFullName}.{Method.Name}"
        : $"{Group.FieldName}.{Method.Name}";
}
