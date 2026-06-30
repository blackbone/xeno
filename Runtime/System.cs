using System;

namespace Xeno
{
    [Serializable]
    public abstract class System
    {
        protected World world;

        protected abstract bool IsWorldStartSystem { get; }
        protected abstract bool IsPreUpdateSystem { get; }
        protected abstract bool IsUpdateSystem { get; }
        protected abstract bool IsPostUpdateSystem { get; }
        protected abstract bool IsWordStopSystem { get; }

        protected abstract void Start();
        protected abstract void PreUpdate(in float delta);
        protected abstract void Update(in float delta);
        protected abstract void PostUpdate(in float delta);
        protected abstract void Stop();

        protected abstract void OnAfterAttachToWorld();
        protected abstract void OnBeforeDetachFromWorld();

        internal bool RunsOnWorldStart => IsWorldStartSystem;
        internal bool RunsOnPreUpdate => IsPreUpdateSystem;
        internal bool RunsOnUpdate => IsUpdateSystem;
        internal bool RunsOnPostUpdate => IsPostUpdateSystem;
        internal bool RunsOnWorldStop => IsWordStopSystem;

        internal void InvokeStart() => Start();
        internal void InvokePreUpdate(in float delta) => PreUpdate(delta);
        internal void InvokeUpdate(in float delta) => Update(delta);
        internal void InvokePostUpdate(in float delta) => PostUpdate(delta);
        internal void InvokeStop() => Stop();

        internal void AttachToWorld(in World worldToAttach)
        {
            if (world != null) throw new InvalidOperationException("Trying attach system instance to more than one world!");
            world = worldToAttach;
            OnAfterAttachToWorld();
        }

        internal void DetachFromWorld(in World worldToAttach)
        {
            if (world != worldToAttach) throw new InvalidOperationException("Trying detach system instance from world it not belongs to!");
            OnBeforeDetachFromWorld();
            world = null;
        }
    }
}
