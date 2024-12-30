using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxFactory;
using static SyntaxHelpers;

internal static class ApiGenerator {
    private static readonly MethodDeclarationSyntax[] commonConstantMethods = {
        Method("Entity", "Create", "this World world", SyntaxKind.StaticKeyword)
            .Body("return world.Create();"),

        Method("bool", "IsValid", "this World world, in Entity entity", SyntaxKind.StaticKeyword)
            .Body("return world != null && !world.IsDisposed() && world.IsValid(in entity);"),

        Method("bool", "IsValid", "this in Entity entity", SyntaxKind.StaticKeyword)
            .Body("return entity.World != null && !entity.World.IsDisposed() && entity.World.IsValid(in entity);"),

        Method("bool", "Delete", "this World world, ref Entity entity", SyntaxKind.StaticKeyword)
            .Body("return world != null && !world.IsDisposed() && world.Delete(ref entity);"),

        Method("bool", "Delete", "this ref Entity entity", SyntaxKind.StaticKeyword)
        .Body("return entity.World != null && !entity.World.IsDisposed() && entity.World.Delete(ref entity);")
    };

    private static IEnumerable<FieldDeclarationSyntax> GetFields(Generation generation, bool isPluginApi) {
        var methods = new List<MethodDeclarationSyntax>(commonConstantMethods);

        for (var i = 0; i < methods.Count; i++) {
            var method = methods[i];
            var type = GetDelegatePointerType(method);

            var field = Field(
                type,
                $"__{method.Identifier.Text}_{type.GetHashCode():X}",
                isPluginApi ? null : $"&{method.Identifier.Text}", SyntaxKind.StaticKeyword);

            if (!isPluginApi) field = field.AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword));
            else field = field.AddModifiers(Token(SyntaxKind.PublicKeyword));

            if (i == 0 && isPluginApi) field = field.WithLeadingTrivia(Comment("\n#pragma warning disable 0649"));
            if (i == methods.Count - 1 && isPluginApi) field = field.WithTrailingTrivia(Comment("\n#pragma warning restore 0649"));

            yield return field;
        }

        foreach (var invocation in generation.Invocations) {
            var type = GetDelegatePointerType(invocation);

            var field = Field(
                type,
                $"__{invocation.Name}_{type.GetHashCode():X}",
                isPluginApi ? null : $"&{invocation.Name}", SyntaxKind.StaticKeyword);

            if (!isPluginApi) field = field.AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword));
            else field = field.AddModifiers(Token(SyntaxKind.PublicKeyword));

            yield return field;
        }

        // // nested assemblies fields ???
        // foreach (var apiType in generation.AssemblyInfo.ReferencedAssemblies.Select(a => a.ApiType)) {
        //     foreach (var fieldSymbol in apiType.GetMembers().OfType<IFieldSymbol>()) {
        //         var field = Field(
        //             fieldSymbol.Type.ToDisplayString(),
        //             fieldSymbol.Name,
        //             isPluginApi ? null : $"null", SyntaxKind.StaticKeyword);
        //
        //         if (!isPluginApi) field = field.AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword));
        //         else field = field.AddModifiers(Token(SyntaxKind.PublicKeyword));
        //
        //         yield return field;
        //     }
        // }
    }

    private static IEnumerable<MethodDeclarationSyntax> GetMethods(Generation generation, bool isPluginApi) {
        for (var i = 0; i < commonConstantMethods.Length; i++) {
            var method = commonConstantMethods[i];
            if (isPluginApi) {
                var type = GetDelegatePointerType(method);
                method = method
                    .Attributes(AggressiveInlining)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .Body($"return __{method.Identifier.Text}_{type.GetHashCode():X}({string.Join(", ", method.ParameterList.Parameters.Select(FormatParameterPass))});");
            } else
                method = method.AddModifiers(Token(SyntaxKind.PrivateKeyword));
            yield return method;
        }

        foreach (var invocation in generation.Invocations) {
            var method = Method(invocation.ReturnType, invocation.Name, $"this World world, {invocation.ParameterList}", SyntaxKind.StaticKeyword);
            if (isPluginApi) {
                var type = GetDelegatePointerType(invocation);
                method = method
                    .Attributes(AggressiveInlining)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .Body($"return __{method.Identifier.Text}_{type.GetHashCode():X}({string.Join(", ", method.ParameterList.Parameters.Select(FormatParameterPass))});");
            } else
                method = method.AddModifiers(Token(SyntaxKind.PrivateKeyword))
                    .Attributes(AggressiveInlining)
                    .Body(Array.Empty<string>());
            yield return method;
        }
        yield break;

        static string FormatParameterPass(ParameterSyntax parameter) {
            var sb = new StringBuilder();
            foreach (var modifier in parameter.Modifiers) {
                switch (modifier.Text) {
                    case "ref":
                    case "out":
                        sb.Append(modifier.Text + " ");
                        break;
                }
            }
            sb.Append(parameter.Identifier.ToFullString());
            return sb.ToString();
        }
    }

    private static string GetDelegatePointerType(Invocation invocation) {
        var sb = new StringBuilder("delegate*<");
        sb.Append("World, ");
        sb.Append(string.Join(", ", invocation.Mask.ComponentArgs.Select(FormatArg)));
        sb.Append(", ");
        sb.Append(invocation.ReturnType);
        sb.Append(">");
        return sb.ToString();

        string FormatArg(Component arg) {
            var sb = new StringBuilder();
            switch (invocation.Name) {
                case "Create": sb.Append("in "); break;
                case "Add": sb.Append("in "); break;
                case "Remove": sb.Append("out "); break;
            }
            sb.Append(arg.TypeFullName);
            return sb.ToString();
        }
    }

    private static string GetDelegatePointerType(MethodDeclarationSyntax method) {
        var sb = new StringBuilder("delegate*<");
        sb.Append(string.Join(", ", method.ParameterList.Parameters.Select(FormatParameter)));
        sb.Append(", ");
        sb.Append(method.ReturnType.ToFullString());
        sb.Append(">");
        return sb.ToString();

        string FormatParameter(ParameterSyntax parameter) {
            var sb = new StringBuilder();
            foreach (var modifier in parameter.Modifiers) {
                switch (modifier.Text) {
                    case "ref":
                    case "in":
                    case "out":
                        sb.Append(modifier.Text + " ");
                        break;
                }
            }
            sb.Append(parameter.Type!.ToFullString());
            return sb.ToString();
        }
    }

    public static void GeneratePluginApi(Generation generation) {
        var root = CompilationUnit()
            .Usings("System.Runtime.CompilerServices")
            .AddMembers(Namespace(generation.AssemblyName).AddMembers(
                Class("Api", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.UnsafeKeyword)
                    .Fields(GetFields(generation, true).ToArray())
                    .Methods(GetMethods(generation, true).ToArray())
            ));

        generation.Add(root, "Api");
    }

    public static void GenerateEcsApi(Generation generation) {
        var root = CompilationUnit().AddMembers(
            Namespace(generation.AssemblyName).AddMembers(
                Class("Api", SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.UnsafeKeyword)
                    .Fields(GetFields(generation, true).ToArray())
                    .Constructors(
                        Constructor("Api", null, SyntaxKind.StaticKeyword)
                            .AddBodyStatements(generation.AssemblyInfo.ReferencedAssemblies.SelectMany(GetApiTypeInitializer).ToArray())
                    )
                    .Methods(GetMethods(generation, false).ToArray())
            ));

        generation.Add(root, "Api");
    }
    private static IEnumerable<StatementSyntax> GetApiTypeInitializer(AssemblyInfo assemblyInfo) {
        var apiType = assemblyInfo.ApiType;
        var fields = apiType.GetMembers().OfType<IFieldSymbol>();
        return fields.Select(f => ParseStatement($"{apiType.FullName()}.{f.Name} = ({f.Type.ToDisplayString()}){f.Name};"));
    }
}
