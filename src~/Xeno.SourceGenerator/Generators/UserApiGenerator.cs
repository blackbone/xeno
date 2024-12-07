using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Xeno.SourceGenerator;

internal static class UserApiGenerator {
    public static void Generate(GeneratorInfo info) {
        var text = new StringBuilder("/*\n");
        foreach (var userApiCall in info.UserApiCalls) {
            text.AppendLine(userApiCall.MethodSyntax.ToFullString());
        }
        text.AppendLine("*/");
        info.Context.AddSource("Xeno/_Log.cs", SourceText.From(text.ToString(), Encoding.UTF8));
    }
}
