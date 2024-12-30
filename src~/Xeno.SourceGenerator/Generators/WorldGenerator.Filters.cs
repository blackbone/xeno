using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxHelpers;
using static SyntaxFactory;
internal static partial class WorldGenerator {
    private static void GenerateFilters(Generation generation) {
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
                .Inner(
                    SetStruct,
                    SetReadOnlyStruct,
                    FilterStruct,
                    FilterReadOnlyStruct
                    );
        var root = CompilationUnit()
            .Usings(
                "System",
                "System.Runtime.CompilerServices",
                "System.Runtime.InteropServices"
            )
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "World.Filters");
    }

    private static readonly StructDeclarationSyntax SetStruct =
        Struct("Set", SyntaxKind.PrivateKeyword, SyntaxKind.RefKeyword)
            .Attributes(Serializable, StructLayoutSequential)
            .Fields(
                Field("ushort", "indexJoin", null, SyntaxKind.InternalKeyword),
                Field("ulong", "hash", null, SyntaxKind.InternalKeyword),
                Field("Span<ulong>", "data", null, SyntaxKind.InternalKeyword)
                    .Line()
            )
            .Constructors(
                Constructor("Set", "in Span<ulong> data, in ushort indexJoin", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "this.data = data;",
                        "this.indexJoin = indexJoin;",
                        "this.hash = 0;"
                    )
            )
            .Methods(
                Method("void", "FinalizeHash", null, SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        Statement("switch (data.Length)"),
                        Block(
                            Statement("case 0: hash = 0; return;"),
                            Statement("case 1: hash = data[0]; return;"),
                            Statement("case 2: hash = data[0] ^ data[1]; return;"),
                            Statement("case 3: hash = data[0] ^ data[1] ^ data[2]; return;"),
                            Statement("case 4: hash = data[0] ^ data[1] ^ data[2] ^ data[3]; return;"),
                            Statement("case 5: hash = data[0] ^ data[1] ^ data[2] ^ data[3] ^ data[4]; return;"),
                            Statement("case 6: hash = data[0] ^ data[1] ^ data[2] ^ data[3] ^ data[4] ^ data[5]; return;"),
                            Statement("case 7: hash = data[0] ^ data[1] ^ data[2] ^ data[3] ^ data[4] ^ data[5] ^ data[6]; return;"),
                            Statement("case 8: hash = data[0] ^ data[1] ^ data[2] ^ data[3] ^ data[4] ^ data[5] ^ data[6] ^ data[7]; return;")
                        ),
                        Statement("hash = 0;"),
                        Statement("for (var i = 0; i < data.Length; i++)"),
                        Block(
                            Statement("hash ^= data[i];")
                        )
                    ),
                Method("ushort[]", "GetIndices", null, SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "Span<ushort> values = stackalloc ushort[8];",
                        "var n = 0;",
                        "var l = data.Length;",
                        "ushort u_s = 0;",
                        "for (var i = 0; i < l; i++, u_s++) {",
                        "var v = data[i];",
                        "ushort k = 0;",
                        "while (v != 0) {",
                        "if ((v & 1ul) == 1ul) values[n++] = (ushort)(u_s * 64 + k);",
                        "v >>= 1;",
                        "k++;",
                        "}",
                        "}",
                        "return values[..n].ToArray();"
                        )
            );

    private static readonly StructDeclarationSyntax SetReadOnlyStruct =
        Struct("SetReadOnly", SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            .Attributes(Serializable, StructLayoutSequential)
            .Fields(
                Field("ulong[]", "emptyUlong", "{ 0 }", SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("ushort[]", "emptyUShort", "{ }", SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("SetReadOnly", "Zero", "new SetReadOnly(0)", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword)
                    .Line(),
                Field("ushort", "indexJoin", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("ulong", "hash", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("ulong[]", "data", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("ushort[]", "indices", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
                    .Line()
                )
            .Constructors(
                Constructor("SetReadOnly", "int _", SyntaxKind.PrivateKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "hash = 0;",
                        "indexJoin = 0;",
                        "data = emptyUlong;",
                        "indices = emptyUShort;"
                    ),
                Constructor("SetReadOnly", "in Set set", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "set.FinalizeHash();",
                        "hash = set.hash;",
                        "indexJoin = set.indexJoin;",
                        "data = set.data.ToArray();",
                        "indices = set.GetIndices();"
                    )
                )
        ;

    private static readonly StructDeclarationSyntax FilterStruct =
        Struct("Filter", SyntaxKind.PrivateKeyword, SyntaxKind.RefKeyword)
            .Attributes(Serializable, StructLayoutSequential)
            .Fields(
                Field("Set", "with", null, SyntaxKind.PublicKeyword),
                Field("Set", "without", null, SyntaxKind.PublicKeyword)
                    .Line()
                )
            .Constructors(
                Constructor("Filter", "in Span<ulong> withData, in ushort withJoin, in Span<ulong> withoutData, in ushort withoutJoin", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "with = new Set(withData, withJoin);",
                        "without = new Set(withoutData, withoutJoin);"
                        )
                );

    private static readonly StructDeclarationSyntax FilterReadOnlyStruct =
        Struct("FilterReadOnly", SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            .Attributes(Serializable, StructLayoutSequential)
            .Fields(
                Field("SetReadOnly", "with", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("SetReadOnly", "without", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)
                    .Line()
                )
            .Constructors(
                Constructor("FilterReadOnly", "in Filter filter", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "with = new SetReadOnly(filter.with);",
                        "without = new SetReadOnly(filter.without);"
                    )
            );
}
