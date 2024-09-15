namespace Xeno
{
    public static class EntityExtensions
    {
        public static bool HasComponent<T>(this ref Entity entity) where T : struct, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Has(entity.Id);

        public static void AddComponent<T>(this ref Entity entity, in T component) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.AddComponent(entity, component);
        }

        public static void AddComponents<T1, T2>(this ref Entity entity, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.AddComponents(entity, component1, component2);
        }

        public static void AddComponents<T1, T2, T3>(this ref Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.AddComponents(entity, component1, component2, component3);
        }

        public static void AddComponents<T1, T2, T3, T4>(this ref Entity entity, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.AddComponents(entity, component1, component2, component3, component4);
        }

        public static ref T RefComponent<T>(this ref Entity entity) where T : struct, IComponent {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            var cs = world.Components<T>();
            return ref cs.components.data[cs.mapping.sparse.data[entity.Id]];
        }

        public static void RemoveComponent<T>(this ref Entity entity) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.RemoveComponent<T>(entity);
        }

        public static bool RemoveComponent<T>(this ref Entity entity, ref T component) where T : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            return world.RemoveComponent(entity, ref component);
        }

        public static void RemoveComponents<T1, T2>(this ref Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.RemoveComponents<T1, T2>(entity);
        }

        public static (bool, bool) RemoveComponents<T1, T2>(this ref Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            return world.RemoveComponents(entity, ref component1, ref component2);
        }

        public static void RemoveComponents<T1, T2, T3>(this ref Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.RemoveComponents<T1, T2, T3>(entity);
        }

        public static (bool, bool, bool) RemoveComponents<T1, T2, T3>(this ref Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            return world.RemoveComponents(entity, ref component1, ref component2, ref component3);
        }

        public static void RemoveComponents<T1, T2, T3, T4>(this ref Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            world.RemoveComponents<T1, T2, T3, T4>(entity);
        }

        public static (bool, bool, bool, bool) RemoveComponents<T1, T2, T3, T4>(this ref Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var world = Worlds.ExistingWorlds[entity.WorldId];
            return world.RemoveComponents(entity, ref component1, ref component2, ref component3, ref component4);
        }
    }
}