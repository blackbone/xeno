# Xeno Source Generator

This project contains the Roslyn generator used by generated worlds.

`WorldSourceGenerator` looks for partial `World` subclasses marked with `[RegisterSystem]` and `[RegisterComponent]`.
It emits a typed world implementation with generated lifecycle methods and component storage.

Validate from the repository root:

```sh
./scripts/validate.sh
```
