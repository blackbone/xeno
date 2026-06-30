using System.Runtime.CompilerServices;

namespace Xeno
{
    public static class EntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this ref Entity entity) {
            return Worlds.TryGet(entity.WorldId, out var world) && world.IsEntityValid(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this ref Entity entity) {
            if (!Worlds.TryGet(entity.WorldId, out var world)) return;
            world.DestroyEntity(entity);
        }
    }
}
