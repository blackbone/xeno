using System;

namespace Xeno {
    /// <summary>
    /// Mark required type to allow code generation for that type as component type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterSystemGroupAttribute : Attribute {
        public RegisterSystemGroupAttribute(Type type, bool requiresInstance = false) { }
    }
}
