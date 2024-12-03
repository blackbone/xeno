using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xeno {
    public class EntityNotValidException : Exception {
        private new const string Message = "Entity you trying to access E_{0}({1}) of world [{3}]{2} has is not valid. It has beed destroyed or world has been destroyed.";
        private EntityNotValidException((uint Id, uint Version, (string Name, ushort Id) w) e)
            : base(string.Format(Message, e.Id, e.Version, e.w.Name, e.w.Id)) {}

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf(bool condition, (uint Id, uint Version, (string Name, ushort Id) w) e) {
            if (condition) throw new EntityNotValidException(e);
        }
    }
}
