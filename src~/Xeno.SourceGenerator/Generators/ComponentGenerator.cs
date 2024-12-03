using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

public static class ComponentGenerator {
    public static void Generate(GeneratorInfo info) {
        GenerateExtensions(info);
        GenerateInternalExtensions(info);
    }

    private static void GenerateExtensions(GeneratorInfo info) {
        var root = Helpers.ExtensionsClass(GetMembers(info), info);
        info.Context.Add("Xeno/Component.Extensions.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers(GeneratorInfo info) {
            // defaults for ref extensions
            foreach (var c in info.RegisteredComponents) {
                yield return Helpers.PrivateStaticField(c.TypeFullName, $"default_{c.Index}", "default")
                    .WithTrailingTrivia(Comment(c == info.RegisteredComponents.Last() ? "\n" : ""));
            }

            foreach (var c in info.RegisteredComponents) {
                yield return Helpers.ExtensionMethod($"ref {c.TypeFullName}", $"{c.TypeFullName} _", "MakeRef")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(ParseStatement($"return ref default_{c.Index};")));
            }
        }
    }

    private static void GenerateInternalExtensions(GeneratorInfo info) {
        var root = Helpers.InternalExtensionsClass(GetMembers(info), info);
        info.Context.Add("Xeno/Component.InternalExtensions.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers(GeneratorInfo info) {
            foreach (var c in info.RegisteredComponents) {
                yield return Helpers.ExtensionMethod("uint", $"{(c.Type.IsValueType ? "ref " : "")}{c.TypeFullName} _", "Id")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(ParseStatement($"return {c.Index};")));
            }
        }
    }
}
