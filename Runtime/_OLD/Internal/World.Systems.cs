using System;
using System.Collections.Generic;

namespace Xeno {
    public sealed partial class World_Old
    {
         private readonly SystemGroup defaultSystemGroup = new("Default");
         private readonly LinkedList<SystemGroup> systemGroups = new();

         internal event Action Started;
         internal event UpdateDelegate PreUpdate;
         internal event UpdateDelegate Update;
         internal event UpdateDelegate PostUpdate;
         internal event Action Stopped;
    }
}