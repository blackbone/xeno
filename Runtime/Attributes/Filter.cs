using System;

namespace Xeno
{
    public static class Filter
    {
        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
        public sealed class IncludeDisabledAttribute : Attribute {}
        
        [AttributeUsage(AttributeTargets.Parameter)]
        public sealed class ChangedOnlyAttribute : Attribute {}
    }
}