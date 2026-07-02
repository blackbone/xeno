<p align="center">
  <img src="https://github.com/blackbone/xeno/assets/19610247/7a99bd7a-57af-4b42-8041-701a9f55e3a1" width="256" height="256" border="5"/>
</p>

# XENO

Xenomorp ECS is just another ECS for C# and unity utilizing some experimental and weird approaches such as:
* Source Generators
* IL Weaver
* a lot of unsafe code
* direct memory manipulations and types blitting

## Release 0.2.1 Highlights

Version `0.2.1` focuses on generated-world hot paths, query iteration, and storage cleanup:

* Add materialized generated query page masks for bake-query systems.
* Inline generated query add/remove/update paths and reuse computed page ids and slot bits.
* Replace archetype lookup variants with an on-demand transition graph and LRU transition cache.
* Add opt-in inline component pages and fixed-size page array pooling.
* Fix full bitset equality/matching semantics and remove hash-only matching from archetype lookup.
* Remove the old generic storage layer and keep generated worlds on direct page storage.

# Repository Layout

The package is split across Unity-facing runtime files and .NET development projects:

* `Runtime/` contains the ECS runtime API and internal storage/archetype implementation.
* `Editor/` contains the Unity editor assembly and UI assets.
* `src~/Xeno.sln` is the .NET development solution for tests, source generator work, and samples.
* `src~/Xeno.Tests/` contains the NUnit regression suite.
* `src~/Xeno.SourceGenerator/` contains the Roslyn generator and analyzer project.
* `src~/Xeno.SourceGenerator.Sample/` contains a sample project that consumes the generator.

# Validation

Run the repository validation entrypoint from the repo root:

```sh
./scripts/validate.sh
```

The script runs the .NET solution test suite at `src~/Xeno.sln`.

GitHub Actions runs the same validation on pushes to `main` and pull requests.

# Package Metadata

Unity package metadata lives in `package.json`. NuGet package metadata lives in `Xeno.csproj` and
`src~/Xeno.SourceGenerator/Xeno.SourceGenerator.csproj`. Version `0.2.1` is the current release and
all three package versions should stay aligned before cutting the next one.

# Generated Worlds

World-specific code generation is driven by attributes on a partial `World` subtype:

```csharp
[RegisterComponent(typeof(Position))]
[RegisterComponent(typeof(Velocity))]
[RegisterSystem(typeof(MovementSystem))]
public partial class GameWorld : World {
    public partial Entity CreateEntity(in Position position, in Velocity velocity);
}
```

The generator emits the world constructor, typed store/page caches, system delegates, and direct `Start`,
`Tick`, and `Stop` overrides. Components can be registered explicitly or inferred from registered system
method parameters. Generated worlds are instantiated directly and own the full component API:

```csharp
var world = new GameWorld("main");
var entity = world.CreateEntity(new Position(), new Velocity());
ref var position = ref world.RefPosition(entity);
var positions = world.CountPosition();
```

Do not declare constructors in partial worlds and do not use `Worlds.Create(...)` or manual runtime
system registration for new code.

# Roadmap

- [ ] Base core implementation wich includes:
    - [X] Components
    - [X] Systems
    - [X] Entities
    - [X] Api for .NET
    - [ ] Api for Unity
    - [X] NuGet package of v1
    - [ ] Documentation
- [ ] Unity package validation in CI
- [ ] Public API documentation
- [ ] Source generator analyzer coverage
