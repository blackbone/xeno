using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xeno.SourceGenerator.Utils;

using static SyntaxFactory;

public static class SyntaxHelpers {
    public static readonly AttributeListSyntax Serializable = AttributeList().AddAttributes(Attribute(ParseName(nameof(SerializableAttribute))));
    public static readonly AttributeListSyntax StructLayoutSequential = AttributeList().AddAttributes(Attribute(ParseName(nameof(StructLayoutAttribute)), AttributeArgumentList().AddArguments(AttributeArgument(ParseExpression($"{nameof(LayoutKind)}.{nameof(LayoutKind.Sequential)}")))));
    public static readonly AttributeListSyntax AggressiveInlining = AttributeList().AddAttributes(Attribute(ParseName(nameof(MethodImplAttribute)), AttributeArgumentList().AddArguments(AttributeArgument(ParseExpression($"{nameof(MethodImplOptions)}.{nameof(MethodImplOptions.AggressiveInlining)}")))));
    public static readonly StatementSyntax DisposeCheck = ParseStatement("Xeno.WorldDisposedException.ThrowIf(isDisposed, this.Describe());");

    public static UsingDirectiveSyntax Using(string name)
        => UsingDirective(ParseName(name));

    public static CompilationUnitSyntax Usings(this CompilationUnitSyntax unit, params string[] names)
        => unit.AddUsings(names.Select(Using).ToArray());

    public static NamespaceDeclarationSyntax Namespace(string namespaceName)
        => NamespaceDeclaration(ParseName(namespaceName));

    public static InterfaceDeclarationSyntax Interface(string name, params SyntaxKind[] modifiers)
        => InterfaceDeclaration(name)
            .WithModifiers(TokenList(modifiers.Select(Token)));

    public static ClassDeclarationSyntax Class(string name, params SyntaxKind[] modifiers)
        => ClassDeclaration(name)
            .WithModifiers(TokenList(modifiers.Select(Token)));

    public static StructDeclarationSyntax Struct(string name, params SyntaxKind[] modifiers)
        => StructDeclaration(name)
            .WithModifiers(TokenList(modifiers.Select(Token)));

    public static T Constructors<T>(this T type, params ConstructorDeclarationSyntax[] constructors)
        where T : TypeDeclarationSyntax
        => (T)type.AddMembers(constructors);

    public static ConstructorDeclarationSyntax Constructor(string identifier, string parameterList = null, params SyntaxKind[] modifiers) {
        var constructor = ConstructorDeclaration(identifier)
                .WithModifiers(TokenList(modifiers.Select(Token)));
        if (!string.IsNullOrEmpty(parameterList))
            constructor = constructor.WithParameterList(ParseParameterList($"({parameterList})"));
        return constructor;
    }

    public static T Attributes<T>(this T member, params AttributeListSyntax[] attributeLists)
        where T : MemberDeclarationSyntax {
        return (T)member.AddAttributeLists(attributeLists);
    }

    public static T Fields<T>(this T type, params FieldDeclarationSyntax[] methods)
        where T : TypeDeclarationSyntax
        => (T)type.AddMembers(methods);

    public static T Line<T>(this T node)
        where T : SyntaxNode
        => node.WithTrailingTrivia(Comment("\n"));

    public static FieldDeclarationSyntax Field(string type, string name, string initializer = null, params SyntaxKind[] modifiers) {
        var variableDeclarator = VariableDeclarator(name);
        if (!string.IsNullOrEmpty(initializer))
            variableDeclarator = variableDeclarator.WithInitializer(EqualsValueClause(ParseExpression(initializer)));

        return FieldDeclaration(VariableDeclaration(ParseTypeName(type)))
            .AddDeclarationVariables(variableDeclarator)
            .WithModifiers(TokenList(modifiers.Select(Token)));
    }

    public static T Methods<T>(this T type, params MethodDeclarationSyntax[] methods)
        where T : TypeDeclarationSyntax
        => (T)type.AddMembers(methods);

    public static MethodDeclarationSyntax Method(string returnType, string name, string parameterList = null, params SyntaxKind[] modifiers) {
        var method = MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .WithModifiers(TokenList(modifiers.Select(Token)));
        if (!string.IsNullOrEmpty(parameterList))
            method = method.WithParameterList(ParseParameterList($"({parameterList})"));
        return method;
    }

    public static T Inner<T>(this T type, params TypeDeclarationSyntax[] types)
        where T : TypeDeclarationSyntax
        => (T)type.AddMembers(types);

    public static StatementSyntax Statement(string text)
        => ParseStatement(text);

    public static BlockSyntax Block(params string[] statements)
        => SyntaxFactory.Block(statements.Select(static s => ParseStatement(s)));

    public static T Body<T>(this T method, params string[] statements)
        where T : BaseMethodDeclarationSyntax
        => (T)method.WithBody(SyntaxFactory.Block(statements.Select(static s => ParseStatement(s))));

    public static T Body<T>(this T method, params StatementSyntax[] statements)
        where T : BaseMethodDeclarationSyntax
        => (T)method.WithBody(SyntaxFactory.Block(statements));

    public static T ExtendBody<T>(this T method, params string[] statements)
        where T : BaseMethodDeclarationSyntax
        => (T)method.AddBodyStatements(statements.Select(static s => ParseStatement(s)).ToArray());

    public static T ExtendBody<T>(this T method, params StatementSyntax[] statements)
        where T : BaseMethodDeclarationSyntax
        => (T)method.AddBodyStatements(statements);

    public static T BodyExpression<T>(this T method, string expression)
        where T : BaseMethodDeclarationSyntax
        => (T)method.WithExpressionBody(ArrowExpressionClause(ParseExpression(expression.EndsWith(";") ? expression : expression + ";"))).WithTrailingTrivia(Comment(";\n"));

    public static MethodDeclarationSyntax Extension(string returnType, string typeArguments, string target, string name, string parameterList = null, params string[] typeConstraints)
        => MethodDeclaration(ParseTypeName(returnType), Identifier(name))
            .WithTypeParameterList(TypeParameterList(SeparatedList(typeArguments.Split(",").Select(TypeParameter).ToArray())))
            .WithConstraintClauses(ParseConstraints(typeConstraints))
            .WithParameterList(ParseParameterList($"(this {target}{(string.IsNullOrEmpty(parameterList) ? "" : $", {parameterList}")})"))
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)));

    private static SyntaxList<TypeParameterConstraintClauseSyntax> ParseConstraints(string[] typeConstraints) {
        if ((typeConstraints?.Length ?? 0) == 0) return new SyntaxList<TypeParameterConstraintClauseSyntax>();

        return List(typeConstraints.Select(tc => {
            var split1 = tc.Split(":");
            return TypeParameterConstraintClause(IdentifierName(split1[0])).WithTrailingTrivia(Comment(split1[1]));
        }));
    }

    public static T WithBase<T>(this T type, params INamedTypeSymbol[] baseTypes)
        where T : TypeDeclarationSyntax {
        if (baseTypes is { Length: > 0 }) {
            var bases = baseTypes.Select(t => SimpleBaseType(ParseTypeName(t.FullName())));
            return (T)type.WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(bases)));
        }

        return type;
    }
}
