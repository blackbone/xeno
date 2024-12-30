using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator;

using static SyntaxHelpers;

internal sealed class Component {
    public static readonly IEqualityComparer<Component> Comparer = new EqualityComparer();

    public readonly INamedTypeSymbol Type;
    public readonly int Order;
    public readonly int? FixedCapacity;

    public readonly string TypeFullName;

    public bool IsReferenceType => !Type.IsValueType;

    // AFTER INIT
    public ushort Index { get; private set; }
    public Filter Filter { get; private set; }

    public string ArgName => $"c_{Index}";
    public string StoreTypeName => $"Store_{Index}";
    public string StoreFieldName => $"s_{Index}";


    public Component(INamedTypeSymbol type, int order, int? fixedCapacity) {
        Type = type;
        Order = order;
        FixedCapacity = fixedCapacity;
        TypeFullName = type.ToDisplayString();
    }

    public void Init(Generation generation, ushort index) {
        Index = index;
        Filter = new Filter(generation, new ITypeSymbol[] { Type }.ToImmutableArray(), ImmutableArray<ITypeSymbol>.Empty);
    }

    public IEnumerable<StatementSyntax> GetAddStatements(string entityIdentifier) {
        yield return Statement($"if ({StoreFieldName}.count == {StoreFieldName}.data.Length)");
        yield return Block(
            $"__size = {StoreFieldName}.count << 1;",
            $"Utils.Resize(ref {StoreFieldName}.dense, __size);",
            $"Utils.Resize(ref {StoreFieldName}.data, __size);"
        );
        yield return Statement($"{StoreFieldName}.sparse[{entityIdentifier}] = {StoreFieldName}.count;");
        yield return Statement($"{StoreFieldName}.dense[{StoreFieldName}.count] = {entityIdentifier};");
        yield return Statement($"{StoreFieldName}.data[{StoreFieldName}.count] = {ArgName};");
        yield return Statement($"{StoreFieldName}.count++;").Line();
    }

    private class EqualityComparer : IEqualityComparer<Component> {
        public bool Equals(Component x, Component y) {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.GetType() == y.GetType() && x.Type.Equals(y.Type, SymbolEqualityComparer.Default);
        }
        public int GetHashCode(Component obj)
            => obj.Type != null ? SymbolEqualityComparer.Default.GetHashCode(obj.Type) : 0;
    }
}
