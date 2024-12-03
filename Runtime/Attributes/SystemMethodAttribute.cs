using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SystemMethodAttribute : Attribute
    {
        public SystemMethodAttribute(SystemMethodKind method, int order = 0) { }
    }
}
