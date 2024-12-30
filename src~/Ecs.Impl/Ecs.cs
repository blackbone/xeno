using Xeno;
using Ecs.Feature3;

[assembly:EcsAssembly]
[assembly:RegisterSystemGroup(typeof(Feature3SystemGroup))]

namespace Ecs.Impl
{
    public partial class World { }
    public partial struct Entity { }
}
