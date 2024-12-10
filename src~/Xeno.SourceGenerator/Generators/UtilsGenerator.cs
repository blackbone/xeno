using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal static class UtilsGenerator {
    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation))
            return;

        GenerateUtils(info);
    }

    private static void GenerateUtils(GeneratorInfo info) {
        var root = Helpers.InternalStaticClass("Utils", GetMembers(), info);
        info.Context.Add("Xeno/Utils.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.GenericInternalStaticVoidMethod("Resize", "T", "ref T[] array, in uint size")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("var tmp = new T[size];"),
                    ParseStatement("Array.Copy(array, 0, tmp, 0, Math.Min(size, array.Length));"),
                    ParseStatement("array = tmp;")
                    ));
        }
    }
}
