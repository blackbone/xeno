using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Xeno.SourceGenerator;

public sealed class System(IMethodSymbol method) {
    public readonly IMethodSymbol Method = method;

    public IEnumerable<IParameterSymbol> Parameters => Method.Parameters;
}

public sealed class SystemGroup {
    public readonly INamedTypeSymbol SystemGroupGroupType;
    public readonly bool RequiresExternalInstance;
    public readonly IEnumerable<System> Systems;

    public SystemGroup(Compilation compilation, INamedTypeSymbol systemGroupType) {
        SystemGroupGroupType = systemGroupType;
        Systems = ExtractSystems(compilation);
    }

    private ImmutableArray<System> ExtractSystems(Compilation compilation) {
        return SystemGroupGroupType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsValidSystemMethod(compilation))
            .Select(m => new System(m))
            .ToImmutableArray();
    }
}

public static class SystemCollector {
    public static bool Check(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not AttributeSyntax attributeSyntax) return false;
        if (!attributeSyntax.Name.ToString().Contains("RegisterSystem")) return false;

        return true;
    }

    public static SystemGroup Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        if (!Ensure.IsEcsAssembly(context.SemanticModel.Compilation)) return null;
        if (!Ensure.Type(context.SemanticModel.Compilation, "Xeno.RegisterSystemAttribute", out var attributeType)) return null;

        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;
        if (context.Node is not AttributeSyntax attribute) return null;

        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute.Name);
        if (attributeSymbol.Symbol == null) return null;
        if (!attributeSymbol.Symbol.Equals(attributeType, SymbolEqualityComparer.Default)) return null;

        var type = (attribute.ArgumentList?.Arguments[0].Expression as TypeOfExpressionSyntax)?.Type;
        if (type == null) return null;

        Ensure.Type(compilation, type.ToString(), out var typeSymbol);

        return new SystemGroup(compilation, typeSymbol);
    }
}
