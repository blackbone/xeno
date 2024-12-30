using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xeno.SourceGenerator;

internal static class Extensions
{
    public static void Add(this SourceProductionContext context, string hint, CompilationUnitSyntax root) {
        context.AddSource(hint, SourceText.From(root.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
    }

    public static bool IsEcsAssembly(this IAssemblySymbol assembly, Generation generation) {
        Ensure.EcsAssemblyAttribute(generation.Compilation, out var attributeType);
        return assembly.GetAttributes().Any(a => a.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);
    }

    public static bool IsRegisterComponent(this AttributeData attribute, Generation generation, out Component component) {
        Ensure.RegisterComponentAttribute(generation.Compilation, out var attributeType);
        if (attribute.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false) {
            var args = attribute.ConstructorArguments;
            var type = args.ElementAtOrDefault(0).Value as INamedTypeSymbol;
            var order = args.ElementAtOrDefault(1).Value as int? ?? int.MaxValue;
            var fixedCapacity = args.ElementAtOrDefault(2).Value as int?;
            component = generation.GetCached<Component>(type) ?? new Component(type, order, fixedCapacity);
            generation.SetCached(type, component);
            return true;
        }
        component = null;
        return false;
    }

    public static bool IsRegisteredSystemGroup(this AttributeData attribute, Generation generation, ImmutableArray<Component> components, out SystemGroup systemGroup) {
        Ensure.RegisterSystemGroupAttribute(generation.Compilation, out var attributeType);
        if (attribute.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false) {
            var args = attribute.ConstructorArguments;
            var type = args.ElementAtOrDefault(0).Value as INamedTypeSymbol;
            var requiresExternalInstance = args.ElementAtOrDefault(1).Value as bool? ?? false;
            systemGroup = generation.GetCached<SystemGroup>(type) ?? new SystemGroup(generation, type, requiresExternalInstance);
            generation.SetCached(type, systemGroup);
            return true;
        }
        systemGroup = null;
        return false;
    }

    public static bool IsSystemMethod(this IMethodSymbol method, in Compilation compilation, SystemGroup group, out System system) {
        Ensure.RegisterSystemGroupAttribute(compilation, out var attributeType);
        var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null) {
            system = null;
            return false;
        }

        var args = attribute.ConstructorArguments;
        var type = (SystemType)(args.ElementAtOrDefault(0).Value as int? ?? 0);
        var order = args.ElementAtOrDefault(1).Value as int? ?? 0;
        system = new System(group, type, order, method);
        return true;
    }

    public static bool IsValidEntityParameter(this IParameterSymbol parameter) {
        return parameter.Type.Name.Equals("Entity")
            && parameter.RefKind == RefKind.In;
    }

    public static bool IsValidUniformParameter(this IParameterSymbol parameter, in Compilation compilation, out UniformKind kind, out string name) {
        Ensure.Type(compilation, "Xeno.UniformAttribute", out var uniformAttributeType);

        kind = default;
        name = null;

        if (parameter.RefKind != RefKind.In && parameter.RefKind != RefKind.Ref)
            return false;

        var attribute = parameter.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(uniformAttributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null)
            return false;

        switch (attribute.ConstructorArguments[0].Value) {
            case bool b:
                kind = b ? UniformKind.Delta : UniformKind.None;
                return true;
            case string s:
                name = s;
                kind = UniformKind.Named;
                return true;
        }

        return false;
    }

    public static bool IsValidComponentParameter(this IParameterSymbol parameter, in Compilation compilation, ICollection<Component> components = null) {
        if (parameter.IsValidEntityParameter()) return false;
        if (parameter.IsValidUniformParameter(compilation, out _, out _)) return false;
        if (components != null && components.All(c => !c.Type.Equals(parameter.Type, SymbolEqualityComparer.Default))) return false;
        return parameter.RefKind is RefKind.In or RefKind.Ref;
    }

    public static bool IsValidEntityType(this INamedTypeSymbol type) => type.Name.Equals("Entity");

    public static bool IsValidComponentType(this INamedTypeSymbol type, in Compilation compilation) {
        if (type.IsValidEntityType()) return false;
        return true;
    }


    public static string FullName(this INamedTypeSymbol type)
        => $"{type.ContainingNamespace}.{type.Name}";

    public static bool HasMatchingField(this INamedTypeSymbol namedTypeSymbol, string name, IParameterSymbol parameterSymbol, out bool isStatic) {
        isStatic = false;
        var members = namedTypeSymbol.GetMembers(name);
        if (members.Length == 0)
            return false;

        var field = members.OfType<IFieldSymbol>().FirstOrDefault();
        if (field != null) {
            if (field.DeclaredAccessibility != Accessibility.Public) return false;
            if (!field.Type.Equals(parameterSymbol.Type, SymbolEqualityComparer.Default)) return false;
            isStatic = field.IsStatic;
            if (parameterSymbol.RefKind == RefKind.In) return true;
            return !field.IsReadOnly;
        }

        var property = members.OfType<IPropertySymbol>().FirstOrDefault();
        if (property != null) {
            if (property.DeclaredAccessibility != Accessibility.Public) return false;
            if (!property.Type.Equals(parameterSymbol.Type, SymbolEqualityComparer.Default)) return false;
            if (property.GetMethod == null) return false;
            isStatic = property.IsStatic;
            if (parameterSymbol.RefKind == RefKind.In) return true;
            return property.RefKind == RefKind.Ref;
        }

        return false;
    }

    internal static string ToParameterPrefix(this RefKind kind) {
        switch (kind) {
            case RefKind.Out: return "out ";
            case RefKind.Ref: return "ref ";
            case RefKind.In: return "in ";
            case RefKind.None: return string.Empty;

            default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    public static void GetWithAttributeValues(this IMethodSymbol method, Compilation compilation, out ImmutableArray<ITypeSymbol> withTypes) {
        withTypes = ImmutableArray<ITypeSymbol>.Empty;
        Ensure.Type(compilation, "Xeno.WithAttribute", out var attributeType);
        var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null) return;

        var args = attribute.ConstructorArguments;
        withTypes = args[0].Values.Select(v => v.Value).Cast<ITypeSymbol>().ToImmutableArray();
    }

    public static void GetWithoutAttributeValues(this IMethodSymbol method, Compilation compilation, out ImmutableArray<ITypeSymbol> withoutTypes) {
        withoutTypes = ImmutableArray<ITypeSymbol>.Empty;
        Ensure.Type(compilation, "Xeno.WithoutAttribute", out var attributeType);
        var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null) return;

        var args = attribute.ConstructorArguments;
        withoutTypes = args[0].Values.Select(v => v.Value).Cast<ITypeSymbol>().ToImmutableArray();
    }
/*










    public static bool IsValidSystemMethod(this IMethodSymbol method, Compilation compilation) {
        Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType);
        return method.GetAttributes().Any(a => a.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false)
            && method.Parameters.All(p =>
                p.IsValidEntityParameter()
                || p.IsValidUniformParameter(compilation, out _, out _)
                || p.IsValidComponentParameter());
    }

    public static void GetSystemAttributeValues(this IMethodSymbol method, Compilation compilation, out SystemMethodType type, out int order) {
        type = default;
        order = default;

        Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType);
        var attribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false);
        if (attribute == null) return;
        type = attribute.ConstructorArguments.ElementAtOrDefault(0).Value is int intv1  ? (SystemMethodType)intv1 : default;
        order = attribute.ConstructorArguments.ElementAtOrDefault(1).Value is int intv2 ? intv2 : 0;
    }









    public static uint GetPersistentHashCode(this INamedTypeSymbol type, Compilation compilation) {
        Ensure.Type(compilation, "System.Runtime.InteropServices.GuidAttribute", out var guidAttributeType);

        var attribute = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(guidAttributeType, SymbolEqualityComparer.Default) ?? false);
        var name = attribute?.ConstructorArguments.ElementAtOrDefault(0).Value as string;
        name ??= $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";

        return MurMur.Hash32(Encoding.UTF8.GetBytes(name), 37);
    }

    private static readonly Dictionary<string, string[]> entityCallNames = new() {
        { "Add", [null] },              // void Add(in Component c, ...)
        { "Remove", ["ref"] },          // bool Remove(out Component c, ...)
        { "Has", [null] },              // void Has(default(Component), ...)
        { "Get", ["ref"] }              // void Get(ref Component1 c, ...)
    };

    private static readonly Dictionary<string, string[]> worldCallNames = new() {
        { "Create", [null] },           // void Create(in Component c, ...)
        { "Add", [null] },              // void Add(in Component c, ...)
        { "Remove", ["ref"] },          // bool Remove(ref Component c, ...)
        { "Has", [null] },              // void Has(default(Component), ...)
        { "Get", ["ref"] }              // void Get(ref Component1 c, ...)
    };

    public static bool IsValidEntityCallCandidate(this InvocationExpressionSyntax invocation) {
        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && entityCallNames.TryGetValue(memberAccess.Name.ToString(), out var refKind)
            && invocation.ArgumentList.Arguments.Count > 0 // at least one component arg
            && invocation.ArgumentList.Arguments.All(a => refKind.Contains(a.RefOrOutKeyword.Value?.ToString()));
    }

    public static bool IsValidWorldCallCandidate(this InvocationExpressionSyntax invocation) {
        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && worldCallNames.TryGetValue(memberAccess.Name.ToString(), out var refKind)
            && invocation.ArgumentList.Arguments.Count > 1 // entity + at least one component arg
            && invocation.ArgumentList.Arguments.All(a => refKind.Contains(a.RefOrOutKeyword.Value?.ToString()));
    }
    */
}
