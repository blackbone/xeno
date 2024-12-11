using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class SystemGroup {
    private static int N;
    
    public readonly INamedTypeSymbol SystemGroupGroupType;
    public readonly IEnumerable<System> Systems;
    public readonly bool RequiresInstance;
    public readonly bool RequiresExternalInstance;
    public readonly string TypeFullName;
    public readonly string FieldName;


    public SystemGroup(Compilation compilation, INamedTypeSymbol systemGroupType, bool requiresExternalInstance) {
        SystemGroupGroupType = systemGroupType;
        Systems = ExtractSystems(compilation);
        RequiresInstance = !SystemGroupGroupType.IsStatic && Systems.Any(s => !s.IsStatic);
        RequiresExternalInstance = requiresExternalInstance && RequiresInstance;
        TypeFullName = $"{SystemGroupGroupType.ContainingNamespace}.{SystemGroupGroupType.Name}";
        FieldName = $"sys_{TypeFullName.GetHashCode() + N++:X}";
    }

    private ImmutableArray<System> ExtractSystems(Compilation compilation) {
        var result = new List<System>();
        foreach (var systemMethod in SystemGroupGroupType.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsValidSystemMethod(compilation))) {
            systemMethod.GetSystemAttributeValues(compilation, out var type, out var order);
            result.Add(new System(this, type, order, systemMethod));
        }
        return result.ToImmutableArray();
    }
}
