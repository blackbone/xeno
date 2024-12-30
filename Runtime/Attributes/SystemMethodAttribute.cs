using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SystemMethodAttribute : Attribute
    {
        public SystemMethodAttribute(SystemType method, int order = 0) { }
    }
}
