using System;

namespace Xeno {
    /// <summary>
    /// Marker attribute to make marked assembly an ECS assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class EcsAssemblyAttribute : Attribute { }
}