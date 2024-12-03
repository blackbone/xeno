using System;
using System.Collections.Generic;

namespace Xeno
{
    public sealed class SystemGroup
    {
        private readonly string friendlyName;
        private readonly LinkedList<System> systems = new();
        private World_Old _worldOld;
        
        private event Action started;
        private event UpdateDelegate preUpdate; 
        private event UpdateDelegate update; 
        private event UpdateDelegate postUpdate;
        private event Action stopped;

        public SystemGroup(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }
        
        internal void AttachToWorld(in World_Old worldOld)
        {
            if (this._worldOld != null) throw new InvalidOperationException("Trying attach system instance to more than one world!");
            
            this._worldOld = worldOld;
            
            if (systems.Count > 0)
            {
                var current = systems.First;
                do
                {
                    current.Value.AttachToWorld(worldOld);
                    current = current.Next;
                } while (current != null);
            }
            
            worldOld.Started += Start;
            worldOld.PreUpdate += PreUpdate;
            worldOld.Update += Update;
            worldOld.PostUpdate += PostUpdate;
            worldOld.Stopped += Stop;
        }

        internal void DetachFromWorld(in World_Old worldOld)
        {
            if (this._worldOld != worldOld) throw new InvalidOperationException("Trying detach system instance from world it not belongs to!");
            
            worldOld.Started -= Start;
            worldOld.PreUpdate -= PreUpdate;
            worldOld.Update -= Update;
            worldOld.PostUpdate -= PostUpdate;
            worldOld.Stopped -= Stop;
            
            if (systems.Count > 0)
            {
                var current = systems.Last;
                do
                {
                    current.Value.DetachFromWorld(worldOld);
                    current = current.Previous;
                } while (current != null);
            }
            
            this._worldOld = null;
        }
        
        public void AddSystem(in System system)
        {
            systems.AddLast(system);
            
            if (system.IsWorldStartSystem) started += system.Start;
            if (system.IsPreUpdateSystem) preUpdate += system.PreUpdate;
            if (system.IsUpdateSystem) update += system.Update;
            if (system.IsPostUpdateSystem) postUpdate += system.PostUpdate;
            if (system.IsWordStopSystem) stopped += system.Stop;
        }
        
        public void RemoveSystem(in System system)
        {
            if (system.IsWorldStartSystem) started -= system.Start;
            if (system.IsPreUpdateSystem) preUpdate -= system.PreUpdate;
            if (system.IsUpdateSystem) update -= system.Update;
            if (system.IsPostUpdateSystem) postUpdate -= system.PostUpdate;
            if (system.IsWordStopSystem) stopped -= system.Stop;
            
            systems.Remove(system);
        }

        private void Start() => started?.Invoke();
        private void PreUpdate(in float delta) => preUpdate?.Invoke(delta);
        private void Update(in float delta) => update?.Invoke(delta);
        private void PostUpdate(in float delta) => postUpdate?.Invoke(delta);
        private void Stop() => stopped?.Invoke();
    }
}