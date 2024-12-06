namespace Xeno
{
    internal static class Constants {
        // for bit operation
        internal const int LONG_DIVIDER = 6;
        internal const int LONG_DIVISION_MASK = 0b111111;
        public const int LongBitSize = 64;

        // modifiers
        private const int WarmUpModifier = 1024;

        // entities and archetypes
        public const int PreInitializedArchetypesCount = 4;
        public const int DefaultEntityCount = WarmUpModifier * 16; // 16k will be enough for most games
        public const int DefaultArchetypeEntityCount = WarmUpModifier * 16;
        public const int MaxArchetypeComponents = 1024;
        public const int DefaultComponentTypesCount = 128;
        public const int InitialComponentsCapacity = 128;
    }
}
