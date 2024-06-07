using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator.SyntaxReceivers;

internal readonly struct ComponentInfo
{
    public readonly StructDeclarationSyntax Declaration;
    public readonly INamedTypeSymbol Symbol;
    public readonly string FullName;
    public readonly string Guid;
    public readonly uint Hash;

    public ComponentInfo(StructDeclarationSyntax declaration, INamedTypeSymbol symbol, string fullName, string guid)
    {
        Declaration = declaration;
        Symbol = symbol;
        FullName = fullName;
        Guid = guid;
        Hash = string.IsNullOrEmpty(guid)
            ? MurMur.Hash32(Encoding.UTF8.GetBytes(fullName), 1)
            : MurMur.Hash32(Encoding.UTF8.GetBytes(guid), 1);
    }
}

internal class ComponentCollectorSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<StructDeclarationSyntax> candidates = new();
    private readonly List<ComponentInfo> value = new();
    public IReadOnlyList<ComponentInfo> Value => value;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not StructDeclarationSyntax structDeclaration) return;
        candidates.Add(structDeclaration);
    }

    public void VerifyComponents(Compilation compilation)
    {
        if (!Ensure.Type(compilation, "Xeno.IComponent", out var componentInterfaceType))
            throw new InvalidOperationException();
        
        if (!Ensure.Type(compilation, "System.Runtime.InteropServices.GuidAttribute", out var guidAttributeType))
            throw new InvalidOperationException();

        for (var i = candidates.Count - 1; i >= 0; i--)
        {
            var semanticModel = compilation.GetSemanticModel(candidates[i].SyntaxTree, true);
            var symbol = semanticModel.GetDeclaredSymbol(candidates[i]);
            if (symbol == null)
            {
                candidates.RemoveAt(i);
                continue;
            }

            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                candidates.RemoveAt(i);
                continue;
            }

            if (!namedTypeSymbol.AllInterfaces.Contains(componentInterfaceType))
            {
                candidates.RemoveAt(i);
                continue;
            }

            var guidAttribute = namedTypeSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Equals(guidAttributeType, SymbolEqualityComparer.Default) ?? false);
            var guid = (string)guidAttribute?.ConstructorArguments[0].Value;
            value.Add(new ComponentInfo(candidates[i], namedTypeSymbol, $"{namedTypeSymbol.ContainingNamespace}.{namedTypeSymbol.Name}", guid));
            value.Sort((a, b) => (int)((long)a.Hash - b.Hash));
        }
    }
}