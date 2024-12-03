using Xeno;

// // mark this assembly as ecs assembly
[assembly:EcsAssembly]

// register required component types
[assembly:RegisterComponent(typeof(int), 1)]
[assembly:RegisterComponent(typeof(float), 2)]
[assembly:RegisterComponent(typeof(string), 3)]

// register required system types
[assembly:RegisterSystem(typeof(ECS.Feature1.Feature1SystemGroup))]
[assembly:RegisterSystem(typeof(ECS.Feature2.Feature2SystemGroup), true)]
