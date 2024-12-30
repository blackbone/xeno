using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class Filter : IEquatable<Filter> {
    public static readonly IEqualityComparer<Filter> Comparer = new EqualityComparer();
    public class EqualityComparer : IEqualityComparer<Filter> {
        public bool Equals(Filter x, Filter y) {
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

        public int GetHashCode(Filter obj) => obj.FieldName != null ? obj.FieldName.GetHashCode() : 0;
    }

    public readonly ImmutableArray<Component> With;
    public readonly ImmutableArray<Component> Without;
    public readonly string FieldName;
    public readonly string Initializer;
    public readonly string ReadOnlyInitializer;

    public Filter(Generation generation, ImmutableArray<ITypeSymbol> with, ImmutableArray<ITypeSymbol> without) {
        var withHash = 0;
        var withIndices = Array.Empty<ushort>();
        var withData = new[] { 0UL };
        if (with is { Length: > 0 }) {
            With = GetSortedArguments(generation, with);
            withHash = With.Select(c => c.TypeFullName.GetHashCode()).Aggregate(HashCode.Combine);
            withIndices = With.Select(c => c.Index).ToArray();
            withData = GetData(withIndices);
        } else {
            With = ImmutableArray<Component>.Empty;
        }

        var withoutHash = 0;
        var withoutIndices = Array.Empty<ushort>();
        var withoutData = new[] { 0UL };
        if (without is { Length: > 0 }) {
            Without = GetSortedArguments(generation, without);
            withoutHash = Without.Select(c => c.TypeFullName.GetHashCode()).Aggregate(HashCode.Combine);
            withoutIndices = Without.Select(c => c.Index).ToArray();
            withoutData = GetData(withoutIndices);
        }
        else {
            Without = ImmutableArray<Component>.Empty;
        }

        FieldName = $"filter_{withHash:X}{withoutHash:X}";

        Initializer = $"new Filter(stackalloc ulong[] {{ {string.Join(", ", withData.Select(s => s.ToString()))} }}, {string.Join(" | ", withIndices)}, stackalloc ulong[] {{ {string.Join(", ", withoutData.Select(s => s.ToString()))} }}, {string.Join(" | ", withoutIndices)})";
        ReadOnlyInitializer = $"new FilterReadOnly({Initializer})";
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
    private static ImmutableArray<Component> GetSortedArguments(Generation generation, ImmutableArray<ITypeSymbol> with) {
        if (generation.AssemblyInfo == null)
            throw new NullReferenceException("assembly");
        if (generation.AssemblyInfo.Components == null)
            throw new NullReferenceException("components");
        if (generation.AssemblyInfo.Components[0] == null)
            throw new NullReferenceException("component");

        var list = new List<Component>();
        foreach (var type in with) {
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

    public bool Equals(Filter other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return (With.IsEmpty && other.With.IsEmpty || With.SequenceEqual(other.With)) && (Without.IsEmpty && other.Without.IsEmpty || Without.SequenceEqual(other.Without));
    }

    public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Filter other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(With, Without);
}
