using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Xeno.SourceGenerator.SyntaxReceivers;

namespace Xeno.SourceGenerator;

internal sealed class System(SystemGroup group, SystemMethodType type, int order, IMethodSymbol method) {
    public readonly SystemGroup Group = group;
    public readonly SystemMethodType Type = type;
    public readonly int Order = order;
    public readonly IMethodSymbol Method = method;
    public bool IsStatic => Method.IsStatic;

    public IEnumerable<IParameterSymbol> Parameters => Method.Parameters;
    public string Invocation() => IsStatic
        ? $"{Group.TypeFullName}.{Method.Name}();"
        : $"{Group.FieldName}.{Method.Name}();";
}