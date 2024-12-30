using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxFactory;
using static SyntaxHelpers;

internal static partial class WorldGenerator {
    private static void GenerateSystems(Generation generation) {
        // class
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword);

        // system fields
        classSyntax = classSyntax.Fields(generation.AssemblyInfo.RegisteredSystemGroups.Where(s => s.RequiresInstance).Select(SystemField).ToArray());

        // unnamed uniforms
        var uniques = new HashSet<(ITypeSymbol, string)>();
        foreach (var system in generation.AssemblyInfo.SystemCalls) {
            foreach (var uniform in system.Parameters.Where(p =>
                         p.IsValidUniformParameter(generation.Compilation, out var kind, out var name)
                         && kind == UniformKind.Named
                         && !system.Group.Type.HasMatchingField(name, p, out _))) {
                uniform.IsValidUniformParameter(generation.Compilation, out _, out var name);
                uniques.Add((uniform.Type, name));
            }
        }

        // uniform fields
        classSyntax = classSyntax.Fields(
            uniques.Select(tn => {
                var (type, name) = tn;
                return Field(type.ToDisplayString(), $"{name}_{type.ToDisplayString().Replace(".", "_")}", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword);
            }).ToArray());

        // iteration locals
        classSyntax = classSyntax.Fields(
                Field("uint[]", "iterationBuffer", null, SyntaxKind.PrivateKeyword),
                Field("Archetype", "iterationCurrent", null, SyntaxKind.PrivateKeyword),
                Field("uint", "iterationCount", null, SyntaxKind.PrivateKeyword),
                Field("int", "iterationI", null, SyntaxKind.PrivateKeyword),
                Field("uint", "iterationEid", null, SyntaxKind.PrivateKeyword)
                    .Line()
            );

        var root = CompilationUnit()
            .Usings(
                "System",
                "System.Runtime.CompilerServices",
                "System.Runtime.InteropServices"
            )
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "World.Systems");
    }

    private static FieldDeclarationSyntax SystemField(SystemGroup group)
        => Field(group.TypeFullName, group.FieldName, null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword);
}
