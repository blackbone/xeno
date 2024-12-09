using System;

namespace Xeno.SourceGenerator.Sample {
    public class XenoBaseContext : IDisposable
    {
        public struct Component1 : IComponent
        {
            public int Value;
        }

        public struct Component2 : IComponent
        {
            public int Value;
        }

        public struct Component3 : IComponent
        {
            public int Value;
        }

        public World World { get; }

        public XenoBaseContext()
        {
            World = Worlds.Create("Xeno");
        }

        public virtual void Dispose()
        {
            World.Dispose();
        }
    }
}

