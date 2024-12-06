using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal static class FiltersGenerator {
    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation)) return;

        GenerateSet(info);
        GenerateSetReadOnly(info);

        GenerateFilter(info);
        GenerateFilterReadOnly(info);

        GenerateSetExtensions(info);
    }

    private static void GenerateSet(GeneratorInfo info) {
        var root = RefStructWithMembers("Set", info, GetMembers());
        info.Context.Add("Xeno/Set.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.InternalField("uint", "indexJoin");
            yield return Helpers.InternalField("ulong", "hash");
            yield return Helpers.InternalField("Span<ulong>", "data")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicConstructor("Set", "in Span<ulong> data, in uint join")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("this.data = data;"),
                    ParseStatement("indexJoin = join;"),
                    ParseStatement("this.hash = 0;"),
                    ParseStatement("this.FinalizeHash();")
                ));
        }
    }

    private static void GenerateSetReadOnly(GeneratorInfo info) {
        var root = ReadOnlyStructWithMembers("SetReadOnly", info, GetMembers());
        info.Context.Add("Xeno/SetReadOnly.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PrivateStaticReadOnlyField("ulong[]", "emptyUlong", "{ 0 }");
            yield return Helpers.PrivateStaticReadOnlyField("uint[]", "emptyUInt", "{}")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicStaticField("SetReadOnly", "Zero", "new SetReadOnly(0)")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.InternalReadOnlyField("uint", "indexJoin");
            yield return Helpers.InternalReadOnlyField("ulong", "hash");
            yield return Helpers.InternalReadOnlyField("ulong[]", "data");
            yield return Helpers.InternalReadOnlyField("uint[]", "indices")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PrivateConstructor("SetReadOnly", "int _")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                        ParseStatement("hash = 0;"),
                        ParseStatement("indexJoin = 0;"),
                        ParseStatement("data = emptyUlong;"),
                        ParseStatement("indices = emptyUInt;")
                    ));

            yield return Helpers.PublicConstructor("SetReadOnly", "ref Set set")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(
                    Block(
                        ParseStatement("indexJoin = set.indexJoin;"),
                        ParseStatement("hash = set.hash;"),
                        ParseStatement("data = set.data.ToArray();"),
                        ParseStatement("indices = set.GetIndices();")
                        ));
        }
    }

    private static void GenerateFilter(GeneratorInfo info) {
        var root = RefStructWithMembers("Filter", info, GetMembers());
        info.Context.Add("Xeno/Filter.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PublicField("Set", "with");
            yield return Helpers.PublicField("Set", "without")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicConstructor("Filter", "in Span<ulong> withData, in uint withJoin, in Span<ulong> withoutData, in uint withoutJoin")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("with = new Set(withData, withJoin);"),
                    ParseStatement("without = new Set(withoutData, withoutJoin);")
                ));

            yield return Helpers.PublicMethod("FilterReadOnly", "AsReadOnly")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(ParseStatement("return new FilterReadOnly(ref this);")));
        }
    }

    private static void GenerateFilterReadOnly(GeneratorInfo info) {
        var root = ReadOnlyStructWithMembers("FilterReadOnly", info, GetMembers());
        info.Context.Add("Xeno/FilterReadOnly.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PublicReadOnlyField("SetReadOnly", "with");
            yield return Helpers.PublicReadOnlyField("SetReadOnly", "without")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicConstructor("FilterReadOnly", "ref Filter filter")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("with = new SetReadOnly(ref filter.with);"),
                    ParseStatement("without = new SetReadOnly(ref filter.without);")
                ));
        }
    }

    private static void GenerateSetExtensions(GeneratorInfo info) {
        var root = StaticClass("SetExtensions", info, GetMembers());
        info.Context.Add("Xeno/SetExtensions.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.ExtensionMethod("void", "ref Set set", "FinalizeHash")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(@"
switch (set.data.Length)
{
    case 0: set.hash = 0; return;
    case 1: set.hash = set.data[0]; return;
    case 2: set.hash = set.data[0] ^ set.data[1]; return;
    case 3: set.hash = set.data[0] ^ set.data[1] ^ set.data[2]; return;
    case 4: set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]; return;
    case 5: set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4]; return;
    case 6: set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5]; return;
    case 7: set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5] ^ set.data[6]; return;
    case 8: set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5] ^ set.data[6] ^ set.data[7]; return;
}
set.hash = 0;
for (var i = 0; i < set.data.Length; i++)
    set.hash ^= set.data[i];".Split("\n").Select(s => ParseStatement(s))));

            yield return Helpers.ExtensionMethod("uint[]", "ref Set set", "GetIndices")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(@"
Span<uint> values = stackalloc uint[8];
var n = 0;
var l = set.data.Length;
uint u_i = 0;
for (var i = 0; i < l; i++, u_i++) {
    var v = set.data[i];

    var k = 0u;
    while (v != 0) {
        if ((v & 1ul) == 1ul) values[n++] = u_i * 64 + k;
        v >>= 1;
        k++;
    }
}
return values[..n].ToArray();".Split("\n").Select(s => ParseStatement(s))));
        }
    }

    private static CompilationUnitSyntax RefStructWithMembers(string name, GeneratorInfo info, IEnumerable<MemberDeclarationSyntax> members) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(StructDeclaration(name)
                    .AddAttributeLists(Helpers.StructLayoutKind_Sequential)
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.RefKeyword)))
                    .WithMembers(List(members))
                ));
    }

    private static CompilationUnitSyntax ReadOnlyStructWithMembers(string name, GeneratorInfo info, IEnumerable<MemberDeclarationSyntax> members) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(StructDeclaration(name)
                    .AddAttributeLists(Helpers.StructLayoutKind_Sequential)
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
                    .WithMembers(List(members))
                ));
    }

    private static CompilationUnitSyntax StaticClass(string name, GeneratorInfo info, IEnumerable<MemberDeclarationSyntax> members) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration(name)
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithMembers(List(members))
                ));
    }
}
