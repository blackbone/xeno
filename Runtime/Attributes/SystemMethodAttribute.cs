using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SystemMethodAttribute : Attribute
    {
        public SystemMethodAttribute(SystemMethodType method, int order = 0) { }

        public bool NoFuse { get; set; }
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
