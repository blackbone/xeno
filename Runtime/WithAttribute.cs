using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WithAttribute : Attribute
    {
        public WithAttribute(params uint[] componentIds) { }
    }
}