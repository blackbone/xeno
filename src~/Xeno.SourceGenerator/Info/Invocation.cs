using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xeno.SourceGenerator.Collectors;

namespace Xeno.SourceGenerator;

internal class Invocation {
    public static readonly IEqualityComparer<Invocation> Comparer = new EqualityComparer();

    public readonly INamedTypeSymbol Target;
    public readonly string Name;
    public readonly ImmutableArray<InvocationCandidateArg> Args;
    public readonly Location Location;

    public ImmutableArray<(RefKind, Component)> Components { get; private set; }
    public Mask Mask { get; private set; }

    public Invocation(InvocationCandidate candidate) {
        Target = candidate.Target;
        Name = candidate.MethodName;
        Args = candidate.Args;
    }

    public void Init(Generation generation) {
        Components = GetComponentArgs(generation, Args);
        Mask = new Mask(generation, Args.Select(a => a.Type).ToImmutableArray());
    }

    public string ReturnType => Name switch {
        "Create" => "Entity",
        "Add" => "void",
        "Remove" => "void",
        _ => throw new InvalidOperationException()
    };

    public string ParameterList => Name switch {
        "Create" => string.Join(", ", Mask.ComponentArgs.Select(a => $"in {a.TypeFullName} {a.ArgName}")),
        "Add" => string.Join(", ", Mask.ComponentArgs.Select(a => $"in {a.TypeFullName} {a.ArgName}")),
        "Remove" => string.Join(", ", Mask.ComponentArgs.Select(a => $"out {a.TypeFullName} {a.ArgName}")),
        _ => throw new InvalidOperationException()
    };

    private static ImmutableArray<(RefKind, Component)> GetComponentArgs(Generation generation, ImmutableArray<InvocationCandidateArg> args) {
        if (generation.AssemblyInfo == null)
            throw new NullReferenceException("assembly");
        if (generation.AssemblyInfo.Components == null)
            throw new NullReferenceException("components");
        if (generation.AssemblyInfo.Components[0] == null)
            throw new NullReferenceException("component");

        var list = new List<(RefKind, Component)>();
        var hashset = new HashSet<Component>();
        foreach (var arg in args) {
            var component = generation.AssemblyInfo.Components.FirstOrDefault(c => c?.Type?.Equals(arg.Type, SymbolEqualityComparer.Default) ?? false);
            if (component == null)
                throw new NullReferenceException("1");

            if (hashset.Add(component))
                generation.Context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MultipleSameComponentTypes, arg.Location)); // TODO

            list.Add((arg.RefKind, component));
        }
        return list.ToImmutableArray();
    }

    private class EqualityComparer : IEqualityComparer<Invocation> {
        public bool Equals(Invocation x, Invocation y) {
            if (ReferenceEquals(x, y)) {
                return true;
            }
            if (x is null) {
                return false;
            }
            if (y is null) {
                return false;
            }
            if (x.GetType() != y.GetType()) {
                return false;
            }
            return SymbolEqualityComparer.Default.Equals(x.Target, y.Target)
                && x.Name == y.Name
                && x.Args.SequenceEqual(y.Args);
        }
        public int GetHashCode(Invocation obj) {
            return HashCode.Combine(obj.Target.Name, obj.Name, string.Join("-", obj.Args.Select(a => a.Type.ToDisplayString())));
        }
    }
}
