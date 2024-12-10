using Xeno;

// // mark this assembly as ecs assembly
[assembly:EcsAssembly]

// register required component types
[assembly:RegisterComponent(typeof(int), 1)]
[assembly:RegisterComponent(typeof(float), 2)]
[assembly:RegisterComponent(typeof(string), 3)]
[assembly:RegisterComponent(typeof(ECS.Feature1.Feature1Component), 4)]
[assembly:RegisterComponent(typeof(ECS.Feature2.Feature2Component), 4)]
[assembly:RegisterComponent(typeof(ECS.Feature3.Feature3Component), 4)]

// register required system types
[assembly:RegisterSystem(typeof(ECS.Feature1.Feature1SystemGroup))]
[assembly:RegisterSystem(typeof(ECS.Feature2.Feature2SystemGroup))]
[assembly:RegisterSystem(typeof(ECS.Feature3.Feature3SystemGroup))]
