using System;

namespace Xeno
{
    [Serializable]
    public abstract class System
    {
        protected World_Old WorldOld;

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

        internal void AttachToWorld(in World_Old worldOldToAttach)
        {
            if (WorldOld != null) throw new InvalidOperationException("Trying attach system instance to more than one world!");
            WorldOld = worldOldToAttach;
        }

        internal void DetachFromWorld(in World_Old worldOldToAttach)
        {
            if (WorldOld != worldOldToAttach) throw new InvalidOperationException("Trying detach system instance from world it not belongs to!");
            WorldOld = null;
        }
    }
}