using System;

namespace Xeno {
    /// <summary>
    /// Mark required type to allow code generation for that type as component type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RegisterComponentAttribute : Attribute {
        public Type Type { get; }
        public RegisterComponentAttribute(Type type, int order = 0) => Type = type;
    }
}
