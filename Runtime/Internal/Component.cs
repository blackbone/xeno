using System;

namespace Xeno
{
    internal abstract class Component
    {
        protected internal static uint Index;
    }

    internal abstract class Component<T> : Component where T : struct, IComponent
    {
        private static T _defaultValue;

        public static Type Type { get; } = typeof(T);

        // ReSharper disable once StaticMemberInGenericType
        public new static readonly uint Index = Component.Index++;
        public static int Id => Type.MetadataToken;
        public static ref T Default => ref _defaultValue;
        private Component() { }
    }
}