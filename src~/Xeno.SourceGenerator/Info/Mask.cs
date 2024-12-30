using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class Mask : IEquatable<Mask> {
    public static readonly IEqualityComparer<Mask> Comparer = new EqualityComparer();
    public class EqualityComparer : IEqualityComparer<Mask> {
        public bool Equals(Mask x, Mask y) {
            if (ReferenceEquals(x, y)) {
                return true;
            }
            if (x is null) {
                return false;
            }
            if (y is null) {
                return false;
            }
            if (x.GetType() != y.GetType()) {
                return false;
            }
            return x.FieldName == y.FieldName;
        }

        public int GetHashCode(Mask obj) => obj.FieldName != null ? obj.FieldName.GetHashCode() : 0;
    }

    public readonly ImmutableArray<Component> ComponentArgs;
    public readonly string FieldName;
    public readonly string Initializer;
    public readonly string ReadOnlyInitializer;

    public Mask(Generation generation, ImmutableArray<INamedTypeSymbol> args) {
        var withHash = 0;
        var withIndices = Array.Empty<ushort>();
        var withData = new[] { 0UL };
        if (args is { Length: > 0 }) {
            ComponentArgs = GetSortedArguments(generation, args);
            withHash = ComponentArgs.Select(c => c.TypeFullName.GetHashCode()).Aggregate(HashCode.Combine);
            withIndices = ComponentArgs.Select(c => c.Index).ToArray();
            withData = GetData(withIndices);
        } else {
            ComponentArgs = ImmutableArray<Component>.Empty;
        }

        FieldName = $"set_{withHash:X}";
        Initializer = $"new Set(stackalloc ulong[] {{ {string.Join(", ", withData.Select(s => s.ToString()))} }}, {string.Join(" | ", withIndices)})";
        ReadOnlyInitializer = $"new SetReadOnly({Initializer})";
        return;

        static ulong[] GetData(ushort[] indices) {
            var mask = new List<ulong>();
            foreach (var index in indices) {
                var dataIndex = index / 64;
                var inDataIndex = index % 64;

                while (mask.Count <= inDataIndex)
                    mask.Add(0);

                mask[dataIndex] |= 1ul << inDataIndex;
            }
            return mask.ToArray();
        }
    }
    private static ImmutableArray<Component> GetSortedArguments(Generation generation, ImmutableArray<INamedTypeSymbol> args) {
        if (generation.AssemblyInfo == null)
            throw new NullReferenceException("assembly");
        if (generation.AssemblyInfo.Components == null)
            throw new NullReferenceException("components");
        if (generation.AssemblyInfo.Components[0] == null)
            throw new NullReferenceException("component");

        var list = new List<Component>();
        foreach (var type in args) {
            var component = generation.AssemblyInfo.Components.FirstOrDefault(c => c?.Type?.Equals(type, SymbolEqualityComparer.Default) ?? false);
            if (component == null)
                throw new NullReferenceException("1");

            if (list.Contains(component))
                throw new InvalidOperationException("2");

            list.Add(component);
        }
        list.Sort((a, b) => a.Index.CompareTo(b.Index));
        return list.ToImmutableArray();
    }

    public string GetArchetypeFieldName() {
        return ComponentArgs.IsDefaultOrEmpty
            ? "zeroArchetype" :
            $"archetype_{string.Join("_", ComponentArgs.Select(c => c.Index))}";
    }

    public bool Equals(Mask other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ComponentArgs.IsEmpty && other.ComponentArgs.IsEmpty || ComponentArgs.SequenceEqual(other.ComponentArgs);
    }

    public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Mask other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(ComponentArgs);
}
