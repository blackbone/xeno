using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal sealed class Filter : IEquatable<Filter> {
    public readonly ImmutableArray<Component> With;
    public readonly ImmutableArray<Component> Without;
    public readonly string FieldName;
    public readonly string Initializer;

    public Filter(GeneratorInfo info, ImmutableArray<ITypeSymbol> with, ImmutableArray<ITypeSymbol> without) {
        With = with.Select(t => info.RegisteredComponents.FirstOrDefault(c => c.Type.Equals(t, SymbolEqualityComparer.Default))).ToImmutableArray().Sort((c1, c2) => c1.Index.CompareTo(c2.Index));
        Without = without.Select(t => info.RegisteredComponents.FirstOrDefault(c => c.Type.Equals(t, SymbolEqualityComparer.Default))).ToImmutableArray().Sort((c1, c2) => c1.Index.CompareTo(c2.Index));

        var withHash = With.IsEmpty ? 0 : With.Select(c => c.TypeFullName.GetHashCode()).Aggregate(HashCode.Combine);
        var withoutHash = Without.IsEmpty ? 0 : Without.Select(c => c.TypeFullName.GetHashCode()).Aggregate(HashCode.Combine);
        FieldName = $"filter_{withHash:X}{withoutHash:X}";
        Initializer = "default";
    }

    public bool Equals(Filter other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return (With.IsEmpty && other.With.IsEmpty || With.SequenceEqual(other.With)) && (Without.IsEmpty && other.Without.IsEmpty || Without.SequenceEqual(other.Without));
    }

    public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Filter other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(With, Without);
    public int CompareTo(object obj) => obj is Filter other ? CompareTo(other) : 1;
}
