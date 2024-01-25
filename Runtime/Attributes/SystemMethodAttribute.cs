using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SystemMethodAttribute : Attribute
    {
        public SystemMethodType Method { get; }
        public int Order { get; }

        public SystemMethodAttribute(SystemMethodType method, int order = 0)
        {
            Method = method;
            Order = order;
        }
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