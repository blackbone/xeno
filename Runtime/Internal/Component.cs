using System;

namespace Xeno
{
    internal abstract class Component
    {
        protected internal static uint Index;
    }

    internal abstract class Component<T> : Component where T : unmanaged, IComponent
    {
        private static T _defaultValue;

        public static Type Type { get; } = typeof(T);

        // ReSharper disable once StaticMemberInGenericType
        public new static uint Index { get; } = Component.Index++;
        public static int Id => Type.MetadataToken;
        public static ref T Default => ref _defaultValue;
        private Component() { }
    }
}