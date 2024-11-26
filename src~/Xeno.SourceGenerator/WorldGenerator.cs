using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

[Generator]
public class WorldGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {

    }
}

[Generator]
public class StoreGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {

    }
}

[Generator]
public class ComponentGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context
    }
}
