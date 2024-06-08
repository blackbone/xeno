namespace Xeno
{
    public static class EntityExtensions
    {
        public static void Enable(this Entity entity)
            => Worlds.ExistingWorlds[entity.WorldId].Enable(entity.Id);
        
        public static void Disable(this Entity entity)
            => Worlds.ExistingWorlds[entity.WorldId].Disable(entity.Id);
        
        public static bool HasComponent<T>(this Entity entity) where T : struct, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Has(entity.Id);

        public static void AddComponent<T>(this Entity entity, in T component) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.Components<T>().Add(entity.Id, component);
            world.AddToArchetype<T>(entity.Id);
        }

        public static void AddComponents<T1, T2>(this Entity entity, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.Components<T1>().Add(entity.Id, component1);
            world.AddToArchetype<T1>(entity.Id);
            world.Components<T2>().Add(entity.Id, component2);
            world.AddToArchetype<T2>(entity.Id);
        }

        public static void AddComponents<T1, T2, T3>(this Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.Components<T1>().Add(entity.Id, component1);
            world.AddToArchetype<T1>(entity.Id);
            world.Components<T2>().Add(entity.Id, component2);
            world.AddToArchetype<T2>(entity.Id);
            world.Components<T3>().Add(entity.Id, component3);
            world.AddToArchetype<T3>(entity.Id);
        }

        public static void AddComponents<T1, T2, T3, T4>(this Entity entity, in T1 component1, in T2 component2,
            in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.Components<T1>().Add(entity.Id, component1);
            world.AddToArchetype<T1>(entity.Id);
            world.Components<T2>().Add(entity.Id, component2);
            world.AddToArchetype<T2>(entity.Id);
            world.Components<T3>().Add(entity.Id, component3);
            world.AddToArchetype<T3>(entity.Id);
            world.Components<T4>().Add(entity.Id, component4);
            world.AddToArchetype<T4>(entity.Id);
        }

        public static void SetComponent<T>(this Entity entity, in T component) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.Components<T>().Add(entity.Id, component);
            world.AddToArchetype<T>(entity.Id);
        }

        public static T GetComponent<T>(this Entity entity) where T : struct, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Get(entity.Id);

        public static ref T AccessComponent<T>(this Entity entity) where T : struct, IComponent
            => ref Worlds.ExistingWorlds[entity.WorldId].Components<T>().Ref(entity.Id);

        public static void RemoveComponent<T>(this Entity entity, out T component) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            component = world.Components<T>().Remove(entity.Id);
            world.RemoveFromArchetype<T>(entity.Id);
        }

        public static void RemoveComponents<T1, T2>(this Entity entity, out T1 component1, out T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            component1 = world.Components<T1>().Remove(entity.Id);
            world.RemoveFromArchetype<T1>(entity.Id);
            component2 = world.Components<T2>().Remove(entity.Id);
            world.RemoveFromArchetype<T2>(entity.Id);
        }
        
        public static void RemoveComponents<T1, T2, T3>(this Entity entity, out T1 component1, out T2 component2, out T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            component1 = world.Components<T1>().Remove(entity.Id);
            world.RemoveFromArchetype<T1>(entity.Id);
            component2 = world.Components<T2>().Remove(entity.Id);
            world.RemoveFromArchetype<T2>(entity.Id);
            component3 = world.Components<T3>().Remove(entity.Id);
            world.RemoveFromArchetype<T3>(entity.Id);
        }
        
        public static void RemoveComponents<T1, T2, T3, T4>(this Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            component1 = world.Components<T1>().Remove(entity.Id);
            world.RemoveFromArchetype<T1>(entity.Id);
            component2 = world.Components<T2>().Remove(entity.Id);
            world.RemoveFromArchetype<T2>(entity.Id);
            component3 = world.Components<T3>().Remove(entity.Id);
            world.RemoveFromArchetype<T3>(entity.Id);
            component4 = world.Components<T4>().Remove(entity.Id);
            world.RemoveFromArchetype<T4>(entity.Id);
        }
    }
}