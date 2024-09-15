namespace Xeno.Collections {
    internal sealed class Archetype {
        public readonly bool floating;
        public FixedBitSet mask;
        public SwapBackListUInt entities;
        public Archetype prev;
        public Archetype next;

        public Archetype(in bool floating, in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity) {
            this.floating = floating;
            entities = new SwapBackListUInt(step, capacity);
        }

        public override string ToString() => $"{mask} ({entities.count})";
    }
}