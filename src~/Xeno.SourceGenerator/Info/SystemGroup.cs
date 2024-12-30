using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class SystemGroup {
    private class EqualityComparer : IEqualityComparer<SystemGroup> {
        public bool Equals(SystemGroup x, SystemGroup y) {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.GetType() == y.GetType() && x.Type.Equals(y.Type, SymbolEqualityComparer.Default);
        }
        public int GetHashCode(SystemGroup obj)
            => obj.Type != null ? SymbolEqualityComparer.Default.GetHashCode(obj.Type) : 0;
    }
    public static readonly IEqualityComparer<SystemGroup> Comparer = new EqualityComparer();


    private static int N;

    public readonly INamedTypeSymbol Type;
    public readonly IEnumerable<System> Systems;
    public readonly bool RequiresInstance;
    public readonly bool RequiresExternalInstance;
    public readonly string TypeFullName;
    public readonly string FieldName;


    public SystemGroup(Generation generation, INamedTypeSymbol systemGroupType, bool requiresExternalInstance) {
        Type = systemGroupType;
        Systems = ExtractSystems(generation);
        RequiresInstance = !Type.IsStatic && Systems.Any(s => !s.IsStatic);
        RequiresExternalInstance = requiresExternalInstance && RequiresInstance;
        TypeFullName = $"{Type.ContainingNamespace}.{Type.Name}";
        FieldName = $"sys_{TypeFullName.GetHashCode() + N++:X}";
    }

    private ImmutableArray<System> ExtractSystems(Generation generation) {
        return Type.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => m.IsSystemMethod(generation.Compilation, this, out var system) ? system : null)
            .Where(s => s != null)
            .ToImmutableArray();
    }
}
