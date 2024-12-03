using System;

namespace Xeno {
    /// <summary>
    /// Mark required type to allow code generation for that type as component type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterSystemAttribute : Attribute {
        public RegisterSystemAttribute(Type type, bool requiresInstance = false) { }
    }
}
