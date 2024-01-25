namespace Xeno
{
    public static class EntityExtensions
    {
        public static void Enable(this Entity entity)
            => Worlds.ExistingWorlds[entity.WorldId].Enable(entity.Id);
        
        public static void Disable(this Entity entity)
            => Worlds.ExistingWorlds[entity.WorldId].Disable(entity.Id);
        
        public static bool HasComponent<T>(this Entity entity) where T : unmanaged, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Has(entity.Id);

        public static void AddComponent<T>(this Entity entity, in T component) where T : unmanaged, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Add(entity.Id, component);

        public static void AddComponents<T1, T2>(this Entity entity, in T1 component1, in T2 component2)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
        {
            Worlds.ExistingWorlds[entity.WorldId].Components<T1>().Add(entity.Id, component1);
            Worlds.ExistingWorlds[entity.WorldId].Components<T2>().Add(entity.Id, component2);
        }

        public static void AddComponents<T1, T2, T3>(this Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
        {
            Worlds.ExistingWorlds[entity.WorldId].Components<T1>().Add(entity.Id, component1);
            Worlds.ExistingWorlds[entity.WorldId].Components<T2>().Add(entity.Id, component2);
            Worlds.ExistingWorlds[entity.WorldId].Components<T3>().Add(entity.Id, component3);
        }

        public static void AddComponents<T1, T2, T3, T4>(this Entity entity, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
        {
            Worlds.ExistingWorlds[entity.WorldId].Components<T1>().Add(entity.Id, component1);
            Worlds.ExistingWorlds[entity.WorldId].Components<T2>().Add(entity.Id, component2);
            Worlds.ExistingWorlds[entity.WorldId].Components<T3>().Add(entity.Id, component3);
            Worlds.ExistingWorlds[entity.WorldId].Components<T4>().Add(entity.Id, component4);
        }

        public static void SetComponent<T>(this Entity entity, in T component) where T : unmanaged, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Set(entity.Id, component);

        public static T GetComponent<T>(this Entity entity) where T : unmanaged, IComponent
            => Worlds.ExistingWorlds[entity.WorldId].Components<T>().Get(entity.Id);

        public static ref T AccessComponent<T>(this Entity entity) where T : unmanaged, IComponent
            => ref Worlds.ExistingWorlds[entity.WorldId].Components<T>().Ref(entity.Id);

        public static void RemoveComponent<T>(this Entity entity, out T component) where T : unmanaged, IComponent
            => component = Worlds.ExistingWorlds[entity.WorldId].Components<T>().Remove(entity.Id);
    }
}