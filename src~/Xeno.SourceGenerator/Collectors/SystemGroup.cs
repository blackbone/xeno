using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class SystemGroup {
    public readonly INamedTypeSymbol SystemGroupGroupType;
    public readonly IEnumerable<System> Systems;
    public readonly bool RequiresExternalInstance;
    public bool RequiresInstance => !SystemGroupGroupType.IsStatic && Systems.Any(s => !s.IsStatic);

    public string TypeFullName => $"{SystemGroupGroupType.ContainingNamespace}.{SystemGroupGroupType.Name}";
    public string FieldName => $"sys_{TypeFullName.GetHashCode():X}";

    public SystemGroup(Compilation compilation, INamedTypeSymbol systemGroupType, bool requiresExternalInstance) {
        SystemGroupGroupType = systemGroupType;
        Systems = ExtractSystems(compilation);
        RequiresExternalInstance = requiresExternalInstance && RequiresInstance;
    }

    private ImmutableArray<System> ExtractSystems(Compilation compilation) {
        foreach (var systemMethod in SystemGroupGroupType.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsValidSystemMethod(compilation))) {
            systemMethod.GetSystemAttributeValues(compilation, out var type, out var order);
        }

        return SystemGroupGroupType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsValidSystemMethod(compilation))
            .Select(m => {
                m.GetSystemAttributeValues(compilation, out var type, out var order);
                return new System(this, type, order, m);
            })
            .ToImmutableArray();
    }
}