using System;

namespace Xeno {
    /// <summary>
    /// Mark required type to allow code generation for that type as component type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterComponentAttribute : Attribute {
        public RegisterComponentAttribute(Type type, int order = 0, uint fixedCapacity = 0) { }
    }
}
