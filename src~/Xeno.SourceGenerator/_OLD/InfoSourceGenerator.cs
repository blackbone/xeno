// using System;
// using System.Linq;
// using System.Text;
// using Microsoft.CodeAnalysis;
// using Xeno.SourceGenerator.SyntaxReceivers;
//
// namespace Xeno.SourceGenerator;
//
// [Generator]
// public class InfoSourceGenerator : ISourceGenerator
// {
//     public void Initialize(GeneratorInitializationContext context)
//     {
//         return;
//         context.RegisterForSyntaxNotifications(() => new CompositeSyntaxReceiver<
//             ComponentCollectorSyntaxReceiver,
//             SystemCollectorSyntaxReceiver>());
//     }
//
//     public void Execute(GeneratorExecutionContext context)
//     {
//         return;
//
//         var receiver = context.SyntaxReceiver as CompositeSyntaxReceiver<
//             ComponentCollectorSyntaxReceiver,
//             SystemCollectorSyntaxReceiver>;
//         if (receiver == null) throw new InvalidOperationException();
//
//         receiver.Receiver1.VerifyComponents(context.Compilation);
//         receiver.Receiver2.VerifySystems(context.Compilation);
//
//         var source = new StringBuilder("/*\n");
//
//         source.AppendLine("\nFound components:");
//         foreach (var componentInfo in receiver.Receiver1.Value)
//             source.AppendLine($"\t{componentInfo.FullName} :: [{componentInfo.Guid ?? componentInfo.FullName}] :: {componentInfo.Hash:X}");
//
//         source.AppendLine("\nFound systems:");
//         foreach (var systemInfo in receiver.Receiver2.Value)
//         {
//             source.AppendLine($"\t{systemInfo.Symbol.ContainingNamespace}.{systemInfo.Symbol.Name}");
//             foreach (var group in systemInfo.Methods
//                          .GroupBy(smi => smi.InvocationGroup)
//                          .OrderBy(g => g.Key))
//             {
//                 source.AppendLine($"\t\t[{group.Key}]");
//                 foreach (var systemMethodInfo in group.OrderBy(smi => smi.Order))
//                     source.AppendLine($"\t\t\t[{systemMethodInfo.Order}] {systemMethodInfo.Symbol.Name} ({string.Join(", ", systemMethodInfo.ComponentParameters.Select(t => t.Type.Name))})");
//
//             }
//         }
//         source.AppendLine();
//
//         source.AppendLine("*/");
//
//         context.AddSource("Info.g.cs", source.ToString());
//     }
// }
