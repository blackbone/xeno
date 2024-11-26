using System.Collections.Generic;
using System.Numerics;
using Xeno;

// mark this assembly as ecs assembly
[assembly:EcsAssembly]

// register required component types
[assembly:RegisterComponent(typeof(ECS.Feature1.Feature1Component), 0)]
[assembly:RegisterComponent(typeof(ECS.Feature2.Feature2Component), 1)]
[assembly:RegisterComponent(typeof(int), 2)]
[assembly:RegisterComponent(typeof(float), 3)]
[assembly:RegisterComponent(typeof(string), 10)]
[assembly:RegisterComponent(typeof(Vector2), 120)]
[assembly:RegisterComponent(typeof(List<uint>), 255)]


// register required system types
[assembly:RegisterSystem(typeof(ECS.Feature1.Feature1System))]
[assembly:RegisterSystem(typeof(ECS.Feature2.Feature2System))]
