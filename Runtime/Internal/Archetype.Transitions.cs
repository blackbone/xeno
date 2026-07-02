namespace Xeno {
    internal struct ArchetypeTransition {
        public byte Kind;
        public int Key;
        public BitSetReadOnly Mask;
        public Archetype Target;
    }

    internal struct ArchetypeTransitionCache {
        public Archetype From;
        public byte Kind;
        public int Key;
        public BitSetReadOnly Mask;
        public Archetype Target;
    }

    internal sealed partial class Archetype {
        internal ArchetypeTransition[] transitions;
        internal int transitionsCount;
    }
}
