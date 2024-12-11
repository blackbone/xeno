using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WithoutAttribute : Attribute
    {
        public WithoutAttribute(params Type[] types) { }
    }
}
