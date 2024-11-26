using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator.SyntaxReceivers;

public class CompositeSyntaxReceiver<T1, T2> : ISyntaxReceiver
    where T1 : ISyntaxReceiver, new()
    where T2 : ISyntaxReceiver, new()
    
{
    public readonly T1 Receiver1 = new();
    public readonly T2 Receiver2 = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        Receiver1.OnVisitSyntaxNode(syntaxNode);
        Receiver2.OnVisitSyntaxNode(syntaxNode);
    }
}

public class CompositeSyntaxReceiver<T1, T2, T3> : ISyntaxReceiver
    where T1 : ISyntaxReceiver, new()
    where T2 : ISyntaxReceiver, new()
    where T3 : ISyntaxReceiver, new()
    
{
    public readonly T1 Receiver1 = new();
    public readonly T2 Receiver2 = new();
    public readonly T3 Receiver3 = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        Receiver1.OnVisitSyntaxNode(syntaxNode);
        Receiver2.OnVisitSyntaxNode(syntaxNode);
        Receiver3.OnVisitSyntaxNode(syntaxNode);
    }
}

public class CompositeSyntaxReceiver<T1, T2, T3, T4> : ISyntaxReceiver
    where T1 : ISyntaxReceiver, new()
    where T2 : ISyntaxReceiver, new()
    where T3 : ISyntaxReceiver, new()
    where T4 : ISyntaxReceiver, new()
    
{
    public readonly T1 Receiver1 = new();
    public readonly T2 Receiver2 = new();
    public readonly T3 Receiver3 = new();
    public readonly T4 Receiver4 = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        Receiver1.OnVisitSyntaxNode(syntaxNode);
        Receiver2.OnVisitSyntaxNode(syntaxNode);
        Receiver3.OnVisitSyntaxNode(syntaxNode);
        Receiver4.OnVisitSyntaxNode(syntaxNode);
    }
}