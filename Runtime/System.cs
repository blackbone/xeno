using System;

namespace Xeno
{
    [Serializable]
    public abstract class System
    {
        protected World world;

        protected internal abstract bool IsWorldStartSystem { get; }
        protected internal abstract bool IsPreUpdateSystem { get; }
        protected internal abstract bool IsUpdateSystem { get; }
        protected internal abstract bool IsPostUpdateSystem { get; }
        protected internal abstract bool IsWordStopSystem { get; }

        protected internal abstract void Start();
        protected internal abstract void PreUpdate(in float delta);
        protected internal abstract void Update(in float delta);
        protected internal abstract void PostUpdate(in float delta);
        protected internal abstract void Stop();

        internal void AttachToWorld(in World world)
        {
            if (this.world != null) throw new InvalidOperationException("Trying attach system instance to more than one world!");
            this.world = world;

            if (IsWorldStartSystem) world.Started += Start;
            if (IsPreUpdateSystem) world.PreUpdate += PreUpdate;
            if (IsUpdateSystem) world.Update += Update;
            if (IsPostUpdateSystem) world.PostUpdate += PostUpdate;
            if (IsWordStopSystem) world.Stopped += Stop;
        }

        internal void DetachFromWorld(in World world)
        {
            if (this.world != world) throw new InvalidOperationException("Trying detach system instance from world it not belongs to!");
            this.world = null;
            
            if (IsWorldStartSystem) world.Started -= Start;
            if (IsPreUpdateSystem) world.PreUpdate -= PreUpdate;
            if (IsUpdateSystem) world.Update -= Update;
            if (IsPostUpdateSystem) world.PostUpdate -= PostUpdate;
            if (IsWordStopSystem) world.Stopped -= Stop;
        }
    }
}