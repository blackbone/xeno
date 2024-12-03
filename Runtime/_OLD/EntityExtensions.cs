using System.Runtime.CompilerServices;

namespace Xeno
{
    public static class EntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this ref Entity_Old entityOld) {
            return Worlds.TryGet(entityOld.WorldId, out var world) && world.IsEntityValid(entityOld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T1>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasComponent<T1>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllComponents<T1, T2>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAllComponents<T1, T2>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllComponents<T1, T2, T3>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAllComponents<T1, T2, T3>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllComponents<T1, T2, T3, T4>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAllComponents<T1, T2, T3, T4>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyComponents<T1, T2>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAnyComponents<T1, T2>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyComponents<T1, T2, T3>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAnyComponents<T1, T2, T3>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyComponents<T1, T2, T3, T4>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            => Worlds.TryGet(entityOld.WorldId, out var world) && world.HasAnyComponents<T1, T2, T3, T4>(entityOld);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponents<T1>(this ref Entity_Old entityOld, in T1 component1) where T1 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.AddComponents(entityOld, component1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponents<T1, T2>(this ref Entity_Old entityOld, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.AddComponents(entityOld, component1, component2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponents<T1, T2, T3>(this ref Entity_Old entityOld, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.AddComponents(entityOld, component1, component2, component3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponents<T1, T2, T3, T4>(this ref Entity_Old entityOld, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.AddComponents(entityOld, component1, component2, component3, component4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RefComponents<T1>(this ref Entity_Old entityOld, ref T1 component1)
            where T1 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return false;
            return world.RefComponents(entityOld, ref component1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RefComponentsAll<T1, T2, T3, T4>(this ref Entity_Old entityOld, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return false;
            return world.RefComponentsAll(entityOld, ref component1, ref component2, ref component3, ref component4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<T1>(this ref Entity_Old entityOld) where T1 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.RemoveComponents<T1>(entityOld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponents<T1>(this ref Entity_Old entityOld, ref T1 component1) where T1 : struct, IComponent
        {
            component1 = CI<T1>.Default;
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return default;
            return world.RemoveComponents(entityOld, ref component1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<T1, T2>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.RemoveComponents<T1, T2>(entityOld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, bool) RemoveComponents<T1, T2>(this ref Entity_Old entityOld, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            component1 = CI<T1>.Default;
            component2 = CI<T2>.Default;
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return default;
            return world.RemoveComponents(entityOld, ref component1, ref component2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<T1, T2, T3>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.RemoveComponents<T1, T2, T3>(entityOld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, bool, bool) RemoveComponents<T1, T2, T3>(this ref Entity_Old entityOld, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            component1 = CI<T1>.Default;
            component2 = CI<T2>.Default;
            component3 = CI<T3>.Default;
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return default;
            return world.RemoveComponents(entityOld, ref component1, ref component2, ref component3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<T1, T2, T3, T4>(this ref Entity_Old entityOld)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.RemoveComponents<T1, T2, T3, T4>(entityOld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, bool, bool, bool) RemoveComponents<T1, T2, T3, T4>(this ref Entity_Old entityOld, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            component1 = CI<T1>.Default;
            component2 = CI<T2>.Default;
            component3 = CI<T3>.Default;
            component4 = CI<T4>.Default;
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return default;
            return world.RemoveComponents(entityOld, ref component1, ref component2, ref component3, ref component4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this ref Entity_Old entityOld) {
            if (!Worlds.TryGet(entityOld.WorldId, out var world)) return;
            world.DestroyEntity(entityOld);
        }
    }
}
