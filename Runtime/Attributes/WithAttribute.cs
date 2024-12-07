using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Method)]
    public class WithAttribute : Attribute
    {
        public WithAttribute(params uint[] indices) { }
    }
}
