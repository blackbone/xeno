# Xeno Source Generator

This project contains the Roslyn generator used by generated worlds.

`WorldSourceGenerator` looks for partial `World` subclasses marked with `[RegisterSystem]` and `[RegisterComponent]`.
It emits a typed world implementation with generated lifecycle methods and component storage.

Reference `Xeno.SourceGenerator` as an analyzer package together with the `Xeno` runtime package.
Generated worlds should not declare constructors because the generator emits the constructor that initializes
component storage, masks, and system delegates:

```csharp
[RegisterComponent(typeof(Position))]
[RegisterComponent(typeof(Velocity))]
public partial class NewWorld : World {
    public partial Entity CreateEntity(in Position position, in Velocity velocity);
}

var world = new NewWorld("main");
var entity = world.CreateEntity(new Position(), new Velocity());
world.Add(entity, new Velocity());
ref var position = ref world.RefPosition(entity);
```

Validate from the repository root:

```sh
./scripts/validate.sh
```
