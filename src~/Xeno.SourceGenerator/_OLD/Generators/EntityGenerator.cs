using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator.Old;

using static SyntaxFactory;

internal static class EntityGenerator {
    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation)) return;

        GenerateEntity(info);
        GenerateEntityExtensions(info);
        GenerateInternalExtensions(info);
    }

    private static void GenerateEntity(GeneratorInfo info) {
        var root = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System.Runtime.InteropServices")))
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(GetEntityDeclarationWithName("Entity")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
                ));

        info.Context.Add("Xeno/Entity.g.cs", root);
    }

    private static void GenerateEntityExtensions(GeneratorInfo info) {
        var root = Helpers.ExtensionsClass(GetMembers(), info);
        info.Context.Add("Xeno/Entity.Extensions.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.ExtensionMethod("bool", "in Entity e", "IsValid")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(ParseStatement("return e.World != null && !e.World.IsDisposed() && e.World.IsValid(e);")));

            yield return Helpers.ExtensionMethod("bool", "in Entity e", "Destroy")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(ParseStatement("return e.World != null && !e.World.IsDisposed() && e.World.Destroy(e);")));

            // registered components api
            foreach (var c in info.RegisteredComponents) {
                continue;

                yield return Helpers.ExtensionVoidMethod("in Entity e", "Add", $"in {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("Xeno.WorldDisposedException.ThrowIf(e.World?.IsDisposed() ?? true, e.World.Describe());"),
                        ParseStatement("Xeno.EntityNotValidException.ThrowIf(!e.World.IsValid(e), e.Describe());"),
                        ParseStatement("e.World.Add(e, value);")
                    ));

                yield return Helpers.ExtensionVoidMethod( "in Entity e", "Get", $"ref {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("Xeno.WorldDisposedException.ThrowIf(e.World?.IsDisposed() ?? true, e.World.Describe());"),
                        ParseStatement("Xeno.EntityNotValidException.ThrowIf(!e.World.IsValid(e), e.Describe());"),
                        ParseStatement("e.World.Get(e, ref value);")
                        ));

                yield return Helpers.ExtensionVoidMethod( "in Entity e", "Remove", $"{c.TypeFullName} _")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("Xeno.WorldDisposedException.ThrowIf(e.World?.IsDisposed() ?? true, e.World.Describe());"),
                        ParseStatement("Xeno.EntityNotValidException.ThrowIf(!e.World.IsValid(e), e.Describe());"),
                        ParseStatement("e.World.Remove(e, _);")
                        ));
            }
        }
    }

    private static void GenerateInternalExtensions(GeneratorInfo info) {
        var root = Helpers.InternalExtensionsClass(GetMembers(), info);
        info.Context.Add("Xeno/Entity.InternalExtensions.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.ExtensionMethod("(uint Id, uint Version, (string Name, ushort Id) World)", "in Entity entity", "Describe")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("return (entity.Id, entity.Version, entity.World.Describe());")
                ));
        }
    }

    public static StructDeclarationSyntax GetEntityDeclarationWithName(string name, bool readOnly = true) {
        return StructDeclaration(name)
            .AddAttributeLists(Helpers.StructLayoutKind_Sequential)
            .AddMembers(
                Helpers.PublicField("uint", "Id").AddModifiers(readOnly ? new [] {Token(SyntaxKind.ReadOnlyKeyword)} : Array.Empty<SyntaxToken>()),
                Helpers.PublicField("uint","Version").AddModifiers(readOnly ? new [] {Token(SyntaxKind.ReadOnlyKeyword)} : Array.Empty<SyntaxToken>()),
                Helpers.PublicField("World","World").AddModifiers(readOnly ? new [] {Token(SyntaxKind.ReadOnlyKeyword)} : Array.Empty<SyntaxToken>())
            );
    }
}
