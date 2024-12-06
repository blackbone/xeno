using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NotImplementedException = System.NotImplementedException;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal static class StoresGenerator {
    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation)) return;

        foreach (var component in info.RegisteredComponents) {
            GenerateStore(info, component);
        }
    }
    private static void GenerateStore(GeneratorInfo info, Component component) {
        var root = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System.Runtime.InteropServices")))
            .AddUsings(UsingDirective(ParseName("System.Numerics")))
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(StructDeclaration(component.StoreTypeName)
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                    .WithAttributeLists(List([Helpers.StructLayoutKind_Sequential]))
                    .AddMembers(
                        Helpers.InternalField("uint", "count"),
                        Helpers.InternalField("uint[]", "sparse"),
                        Helpers.InternalField("uint[]", "dense"),
                        Helpers.InternalField($"{component.TypeFullName}[]", "data")
                            .WithTrailingTrivia(EndOfLine("")),
                        Helpers.PublicConstructor(component.StoreTypeName, "in uint sparseCapacity, in uint denseCapacity")
                            .WithBody(Block(
                                ParseStatement("count = 0;"),
                                ParseStatement("sparse = new uint[sparseCapacity];"),
                                ParseStatement("dense = new uint[denseCapacity];"),
                                ParseStatement($"data = new {component.TypeFullName}[denseCapacity];")
                                ))
                        )));

        info.Context.Add($"Xeno/{component.StoreTypeName}.g.cs", root);
    }
}
