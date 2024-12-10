using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal class Helpers {
    public static readonly AttributeListSyntax StructLayoutKind_Sequential = AttributeList().AddAttributes(Attribute(ParseName(nameof(StructLayoutAttribute)), AttributeArgumentList().AddArguments(AttributeArgument(ParseExpression($"{nameof(LayoutKind)}.{nameof(LayoutKind.Sequential)}")))));
    public static readonly AttributeListSyntax AggressiveInlining = AttributeList().AddAttributes(Attribute(ParseName(nameof(MethodImplAttribute)), AttributeArgumentList().AddArguments(AttributeArgument(ParseExpression($"{nameof(MethodImplOptions)}.{nameof(MethodImplOptions.AggressiveInlining)}")))));


    public static FieldDeclarationSyntax PublicField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

    public static FieldDeclarationSyntax PublicStaticField(string type, string name, string initializer)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name).WithInitializer(EqualsValueClause(Token(SyntaxKind.EqualsToken), ParseExpression(initializer))))))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)));

    public static FieldDeclarationSyntax PublicReadOnlyField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

    public static FieldDeclarationSyntax InternalField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)));

    public static FieldDeclarationSyntax InternalReadOnlyField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

    public static FieldDeclarationSyntax PrivateField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

    public static FieldDeclarationSyntax PrivateReadOnlyField(string type, string name)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

    public static MemberDeclarationSyntax PrivateConstant(string type, string name, string initializer)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name).WithInitializer(EqualsValueClause(Token(SyntaxKind.EqualsToken), ParseExpression(initializer))))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ConstKeyword)));

    public static FieldDeclarationSyntax PrivateStaticField(string type, string name, string initializer)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name).WithInitializer(EqualsValueClause(Token(SyntaxKind.EqualsToken), ParseExpression(initializer))))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)));

    public static FieldDeclarationSyntax PrivateStaticReadOnlyField(string type, string name, string initializer)
        => FieldDeclaration(VariableDeclaration(ParseTypeName(type)).WithVariables(SingletonSeparatedList(VariableDeclarator(name).WithInitializer(EqualsValueClause(Token(SyntaxKind.EqualsToken), ParseExpression(initializer))))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

    public static MethodDeclarationSyntax PublicMethod(string type, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

    public static MethodDeclarationSyntax PublicVoidMethod(string name, string parameterList = null)
        => PublicMethod("void", name, parameterList);

    public static MethodDeclarationSyntax InternalMethod(string type, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)));

    public static MethodDeclarationSyntax InternalStaticMethod(string type, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)));

    public static MethodDeclarationSyntax InternalStaticVoidMethod(string name, string parameterList = null)
        => InternalStaticMethod("void", name, parameterList);

    public static MethodDeclarationSyntax GenericInternalStaticMethod(string type, string name, string typeParameters, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithTypeParameterList(TypeParameterList(SeparatedList(typeParameters.Split(",").Select(s => s.Trim()).Select(TypeParameter))))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)));

    public static MethodDeclarationSyntax GenericInternalStaticVoidMethod(string name, string typeParameters, string parameterList = null)
        => GenericInternalStaticMethod("void", name, typeParameters, parameterList);

    public static MethodDeclarationSyntax InternalVoidMethod(string name, string parameterList = null)
        => InternalMethod("void", name, parameterList);

    public static MethodDeclarationSyntax GenericExtensionMethod(string returnType, string typeArguments, string target, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .WithTypeParameterList(TypeParameterList(SeparatedList(typeArguments.Split(",").Select(TypeParameter).ToArray())))
            .WithParameterList(ParseParameterList($"(this {target}{(string.IsNullOrEmpty(parameterList) ? "" : $", {parameterList}")})"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)));

    public static MethodDeclarationSyntax GenericVoidExtensionMethod(string typeArguments, string target, string name, string parameterList = null)
        => GenericExtensionMethod("void", typeArguments, target, name, parameterList);

    public static MethodDeclarationSyntax ExtensionMethod(string returnType, string target, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .WithParameterList(ParseParameterList($"(this {target}{(string.IsNullOrEmpty(parameterList) ? "" : $", {parameterList}")})"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)));

    public static MethodDeclarationSyntax ExtensionVoidMethod(string target, string name, string parameterList = null)
        => ExtensionMethod("void", target, name, parameterList);

    public static MethodDeclarationSyntax PrivateMethod(string type, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

    public static MethodDeclarationSyntax PrivateVoidMethod(string name, string parameterList = null)
        => PrivateMethod("void", name, parameterList);

    public static MethodDeclarationSyntax PartialMethod(string type, string name, string parameterList = null)
        => MethodDeclaration(ParseTypeName(type), Identifier(name))
            .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"))
            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)));

    public static MethodDeclarationSyntax PartialVoidMethod(string name, string parameterList = null)
        => PartialMethod("void", name, parameterList);

    public static ConstructorDeclarationSyntax PrivateConstructor(string name, string parameterList = null) => ConstructorDeclaration(name)
        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
        .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"));

    public static ConstructorDeclarationSyntax PublicConstructor(string name, string parameterList = null) => ConstructorDeclaration(name)
        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
        .WithParameterList(ParseParameterList($"({parameterList ?? string.Empty})"));

    public static CompilationUnitSyntax StaticClass(string name, IEnumerable<MemberDeclarationSyntax> members, GeneratorInfo info) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration(name)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                    .WithMembers(List(members))
                ));
    }

    public static CompilationUnitSyntax InternalStaticClass(string name, IEnumerable<MemberDeclarationSyntax> members, GeneratorInfo info) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration(name)
                    .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
                    .WithMembers(List(members))
                ));
    }

    public static CompilationUnitSyntax ExtensionsClass(IEnumerable<MemberDeclarationSyntax> members, GeneratorInfo info) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration("Extensions")
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                    .WithMembers(List(members))
                ));
    }

    public static CompilationUnitSyntax InternalExtensionsClass(IEnumerable<MemberDeclarationSyntax> members, GeneratorInfo info) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
            )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration("InternalExtensions")
                .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                .WithMembers(List(members))
            ));
    }
}
