using System;
using System.Runtime.CompilerServices;
using ECS.Feature1;
using ECS.Feature2;
using ECS.Feature3;

namespace ECS.Impl
{
    internal static class ExtsStub {
        public static bool Match(this FilterReadOnly fro, in SetReadOnly mask) => true;
    }

    public readonly unsafe struct WorldHandle {
        public readonly Func<Entity> CreateEntity;

        public WorldHandle(World world) {
            CreateEntity = world.CreateEmpty;
        }
    }

    public partial class World {
        public unsafe WorldHandle GetHandle() {
            return new WorldHandle();
        }

        private int iterationI;
        private uint iterationEid;
        private Feature1SystemGroup f1sg;
        private Feature2SystemGroup f2sg;
        private Feature3SystemGroup f3sg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update2(in float delta)
        {
            // start of ECS.Feature1.Feature1SystemGroup.System(ref ECS.Feature1.Feature1Component)
            iterationCount = 0;
            iterationCurrent = archetypes.head;
            while (iterationCurrent != null)
            {
                if (!filter_2.Match(iterationCurrent.mask))
                    continue;
                Array.Copy(iterationCurrent.entities, 0, iterationBuffer, iterationCount, iterationCurrent.entitiesCount);
                iterationCount += iterationCurrent.entitiesCount;
            }

            for (iterationI = 0; iterationI < iterationCount; iterationI++) {
                iterationEid = iterationBuffer[iterationI];
                f1sg.System(ref s_3.data[s_3.sparse[iterationEid]]);
            }
            // end of ECS.Feature1.Feature1SystemGroup.System(ref ECS.Feature1.Feature1Component)


            // start of ECS.Feature2.Feature2.System(ref ECS.Feature2.Feature2Component, ref ECS.Feature1.Feature1Component)
            iterationCount = 0;
            iterationCurrent = archetypes.head;
            while (iterationCurrent != null)
            {
                if (!filter_5.Match(iterationCurrent.mask))
                    continue;
                Array.Copy(iterationCurrent.entities, 0, iterationBuffer, iterationCount, iterationCurrent.entitiesCount);
                iterationCount += iterationCurrent.entitiesCount;
            }

            for (iterationI = 0; iterationI < iterationCount; iterationI++) {
                iterationEid = iterationBuffer[iterationI];
                f2sg.System(
                    ref s_4.data[s_4.sparse[iterationEid]],
                    ref s_3.data[s_3.sparse[iterationEid]]
                    );
            }
            // end of ECS.Feature2.Feature2.System(ref ECS.Feature2.Feature2Component, ref ECS.Feature1.Feature1Component)


            // start of ECS.Feature3.Feature3SystemGroup.System(ref ECS.Feature3.Feature3Component, ref ECS.Feature2.Feature2Component, ref ECS.Feature1.Feature1Component)
            iterationCount = 0;
            iterationCurrent = archetypes.head;
            while (iterationCurrent != null)
            {
                if (!filter_8.Match(iterationCurrent.mask))
                    continue;
                Array.Copy(iterationCurrent.entities, 0, iterationBuffer, iterationCount, iterationCurrent.entitiesCount);
                iterationCount += iterationCurrent.entitiesCount;
            }

            for (iterationI = 0; iterationI < iterationCount; iterationI++) {
                iterationEid = iterationBuffer[iterationI];
                f3sg.System(
                    ref s_5.data[s_5.sparse[iterationEid]],
                    ref s_4.data[s_4.sparse[iterationEid]],
                    ref s_3.data[s_3.sparse[iterationEid]]
                );
            }
            // end of ECS.Feature3.Feature3SystemGroup.System(ref ECS.Feature3.Feature3Component, ref ECS.Feature2.Feature2Component, ref ECS.Feature1.Feature1Component)

        }
    }
}
