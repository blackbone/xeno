using System;
using System.Collections.Generic;

namespace Xeno
{
    public sealed class SystemGroup
    {
        private readonly string friendlyName;
        private readonly LinkedList<System> systems = new();
        private World world;
        
        private event Action started;
        private event UpdateDelegate preUpdate; 
        private event UpdateDelegate update; 
        private event UpdateDelegate postUpdate;
        private event Action stopped;

        public SystemGroup(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }
        
        internal void AttachToWorld(in World world)
        {
            if (this.world != null) throw new InvalidOperationException("Trying attach system instance to more than one world!");
            
            this.world = world;
            
            if (systems.Count > 0)
            {
                var current = systems.First;
                do
                {
                    current.Value.AttachToWorld(world);
                    current = current.Next;
                } while (current != null);
            }
            
            // world.started += Start;
            // world.preUpdate += PreUpdate;
            // world.update += Update;
            // world.postUpdate += PostUpdate;
            // world.stopped += Stop;
        }

        internal void DetachFromWorld(in World world)
        {
            if (this.world != world) throw new InvalidOperationException("Trying detach system instance from world it not belongs to!");
            
            // world.started -= Start;
            // world.preUpdate -= PreUpdate;
            // world.update -= Update;
            // world.postUpdate -= PostUpdate;
            // world.stopped -= Stop;
            
            if (systems.Count > 0)
            {
                var current = systems.Last;
                do
                {
                    current.Value.DetachFromWorld(world);
                    current = current.Previous;
                } while (current != null);
            }
            
            this.world = null;
        }
        
        public void AddSystem(in System system)
        {
            systems.AddLast(system);
            
            // if (system.IsWorldStartSystem) started += system.Start;
            // if (system.IsPreUpdateSystem) preUpdate += system.PreUpdate;
            // if (system.IsUpdateSystem) update += system.Update;
            // if (system.IsPostUpdateSystem) postUpdate += system.PostUpdate;
            // if (system.IsWordStopSystem) stopped += system.Stop;
        }
        
        public void RemoveSystem(in System system)
        {
            // if (system.IsWorldStartSystem) started -= system.Start;
            // if (system.IsPreUpdateSystem) preUpdate -= system.PreUpdate;
            // if (system.IsUpdateSystem) update -= system.Update;
            // if (system.IsPostUpdateSystem) postUpdate -= system.PostUpdate;
            // if (system.IsWordStopSystem) stopped -= system.Stop;
            
            systems.Remove(system);
        }

        private void Start() => started?.Invoke();
        private void PreUpdate(in float delta) => preUpdate?.Invoke(delta);
        private void Update(in float delta) => update?.Invoke(delta);
        private void PostUpdate(in float delta) => postUpdate?.Invoke(delta);
        private void Stop() => stopped?.Invoke();
    }
}