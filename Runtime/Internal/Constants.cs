namespace Xeno
{
    internal static class Constants {
        internal const int INT_DIVIDER = 5;
        internal const uint INT_SIZE = 1u << INT_DIVIDER;
        internal const int INT_DIVISION_MASK = (1 << (INT_DIVIDER + 1)) - 1;

        internal const int LONG_DIVIDER = 6;
        internal const uint LONG_SIZE = 1u << LONG_DIVIDER;
        internal const int LONG_DIVISION_MASK = (1 << (LONG_DIVIDER + 1)) - 1;

        public const int DefaultStep = 4;
        public const int DefaultCapacity = 0;
        public const int DefaultCapacityGrow = 32;
    }
}