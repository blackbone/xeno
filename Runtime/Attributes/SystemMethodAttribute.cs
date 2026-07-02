using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SystemMethodAttribute : Attribute
    {
        public SystemMethodAttribute(SystemMethodType method, int order = 0, bool pure = false) {
            Pure = pure;
        }

        public bool Pure { get; set; }
    }
    
    public enum SystemMethodType
    {
        Startup,
        PreUpdate,
        Update,
        PostUpdate,
        Shutdown
    }
}
