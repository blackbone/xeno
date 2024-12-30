using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xeno.SourceGenerator.Collectors;

namespace Xeno.SourceGenerator;

internal class Generation : IDisposable {
    public readonly StringBuilder Logger;

    public readonly Compilation Compilation;
    public readonly SourceProductionContext Context;
    public readonly string AssemblyName;
    public readonly AssemblyInfo AssemblyInfo;
    public readonly ImmutableArray<Invocation> Invocations;
    public readonly ImmutableDictionary<string, ImmutableArray<Invocation>> InvocationsByName;

    private bool disposed;

    public Generation(SourceProductionContext context, Compilation compilation, ImmutableArray<InvocationCandidate> invocations) {
        Logger = new StringBuilder("/*\n");

        Compilation = compilation;
        Context = context;

        try {
            // this assembly generation info, there will be multiple during generation
            AssemblyInfo = new AssemblyInfo(this, compilation.Assembly, invocations);
            AssemblyInfo.Init();

            Invocations = PrepareInvocations(this, invocations);
            foreach (var invocation in Invocations) invocation.Init(this);
            InvocationsByName = Invocations.GroupBy(i => i.Name).ToImmutableDictionary(g => g.Key, g => g.ToImmutableArray());
            // at this stage we have collected all shit together

            AssemblyName = AssemblyInfo.AssemblyName;

            Log($"Assembly Name: {AssemblyName}");
            Log($"World: {AssemblyInfo.WorldType}");
            Log($"Entity: {AssemblyInfo.EntityType}");


            Log("Invocation Candidates:");
            foreach (var candidate in invocations) {
                Log($"\t{candidate.Target.Name}.{candidate.MethodName}({string.Join(", ", candidate.Args.Select(t => t.RefKind + " " + t.Type.Name))})");
            }
            Log("\n");

            Log("Invocations:");
            foreach (var invocation in Invocations) {
                Log($"\t{invocation.Target.Name}.{invocation.Name}({string.Join(", ", invocation.Args.Select(t => t.RefKind + " " + t.Type.Name))})");
            }
            Log("\n");

            Log("Components:");
            foreach (var component in AssemblyInfo.Components) {
                Log($"\t{component.TypeFullName} :: {component.Order} :: {component.Index} :: {component.FixedCapacity?.ToString() ?? "N/A"}");
            }
            AssemblyInfo.GenerateCommon(this);
            if (AssemblyInfo.IsEcsAssembly) {
                AssemblyInfo.GenerateEcs(this);
            }
            // else
            //     AssemblyInfo.GeneratePlugin(this);
        } catch (Exception e) {
            Log(e.Message + "\n" + e.StackTrace);
        } finally {
            Dispose();
        }
    }

    private static ImmutableArray<Invocation> PrepareInvocations(Generation generation, ImmutableArray<InvocationCandidate> invocations) {
        var result = new List<Invocation>();
        foreach (var inv in invocations.Where(iv => iv.IsValid(generation))) {
            result.Add(new Invocation(inv));
        }
        return result
            .Distinct(Invocation.Comparer)
            .ToImmutableArray();
    }

    public void Log(object message) => Logger.AppendLine(message.ToString());

    public void Dispose() {
        if (disposed) return;

        disposed = true;
        Logger.AppendLine("\n*/");
        Context.AddSource("Xeno/_log.cs", SourceText.From(Logger.ToString(), Encoding.UTF8));
    }

    private readonly Dictionary<Type, Dictionary<object, object>> cache = new();
    public T GetCached<T>(object key) {
        if (!cache.TryGetValue(typeof(T), out var typedCache))
            typedCache = cache[typeof(T)] = new Dictionary<object, object>();

        return typedCache.TryGetValue(key, out var value) ? (T)value : default;
    }

    public void SetCached<T>(object key, T value) {
        if (!cache.TryGetValue(typeof(T), out var typedCache))
            typedCache = cache[typeof(T)] = new Dictionary<object, object>();

        typedCache[key] = value;
    }

    public void Add(CompilationUnitSyntax unit, string hint)
        => Context.AddSource($"Xeno/{hint}.cs", SourceText.From(unit.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
}
