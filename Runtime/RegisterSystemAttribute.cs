using System;

namespace Xeno {
    /// <summary>
    /// Mark required type to allow code generation for that type as component type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RegisterSystemAttribute : Attribute {
        public Type Type { get; }
        public RegisterSystemAttribute(Type type) => Type = type;
    }
}
