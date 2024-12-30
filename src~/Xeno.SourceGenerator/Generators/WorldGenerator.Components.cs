using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxHelpers;
using static SyntaxFactory;

internal static partial class WorldGenerator {
    public static void GenerateComponents(Generation generation) {
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
                .Attributes(Serializable)
                .Fields(generation.AssemblyInfo.Components.Select(GetStoreField).ToArray())
                .Methods(
                    InitializeStoresMethod(generation),
                    GrowStoresCapacityMethod(generation),
                    DisposeStoresMethod(generation)
                    )
                .Inner(generation.AssemblyInfo.Components.Select(GetStoreDefinition).ToArray())
            ;
        var root = CompilationUnit()
            .Usings(
                "System",
                "System.Runtime.CompilerServices",
                "System.Runtime.InteropServices"
                )
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "World.Components");
    }

    private static MethodDeclarationSyntax InitializeStoresMethod(Generation generation) {
        return Method("void", "InitializeStores", "in uint sparseCapacity, in uint denseCapacity", SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .Body(generation.AssemblyInfo.Components.Select(GetStoreFieldInitializer).ToArray());
    }

    private static MethodDeclarationSyntax GrowStoresCapacityMethod(Generation generation) {
        return Method("void", "GrowStoresCapacity", "in uint sparseCapacity", SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .Body(generation.AssemblyInfo.Components.Select(GetStoreFieldResizer).ToArray());
    }

    private static MethodDeclarationSyntax DisposeStoresMethod(Generation generation) {
        return Method("void", "DisposeStores", "in uint sparseCapacity, in uint denseCapacity", SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .Body(generation.AssemblyInfo.Components.Select(GetStoreFieldDisposer).ToArray());
    }

    private static TypeDeclarationSyntax GetStoreDefinition(Component component) {
        return Struct(component.StoreTypeName, SyntaxKind.PrivateKeyword)
            .Attributes(
                Serializable,
                StructLayoutSequential
                )
            .Fields(
                Field("uint", "count", null, SyntaxKind.PublicKeyword),
                Field("uint[]", "sparse", null, SyntaxKind.PublicKeyword),
                Field("uint[]", "dense", null, SyntaxKind.PublicKeyword),
                Field($"{component.TypeFullName}[]","data", null, SyntaxKind.PublicKeyword)
                )
            .Constructors(
                Constructor(component.StoreTypeName, "in uint sparseCapacity, in uint denseCapacity", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "count = 0;",
                        "sparse = new uint[sparseCapacity];",
                        "dense = new uint[denseCapacity];",
                        $"data = new {component.TypeFullName}[denseCapacity];"
                        )
                );
    }

    private static FieldDeclarationSyntax GetStoreField(Component component) {
        return Field(component.StoreTypeName, component.StoreFieldName, null, SyntaxKind.PrivateKeyword);
    }

    private static string GetStoreFieldInitializer(Component component) {
        return $"{component.StoreFieldName} = new(sparseCapacity, denseCapacity);";
    }

    private static string GetStoreFieldResizer(Component component) {
        return $"Utils.Resize(ref {component.StoreFieldName}.sparse, sparseCapacity);";
    }

    private static string GetStoreFieldDisposer(Component component) {
        return $"{component.StoreFieldName} = default;";
    }
}
