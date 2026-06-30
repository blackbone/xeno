using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RegisterComponentAttribute : Attribute {
        public RegisterComponentAttribute(Type type) { }
    }
}
