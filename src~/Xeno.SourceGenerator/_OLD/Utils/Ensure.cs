using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator.Utils;

internal static class Ensure
{
    public static bool Type(Compilation compilation, string typeFullName, out INamedTypeSymbol type)
    {
        type = compilation.GetTypeByMetadataName(typeFullName);
        return type != null;
    }
}