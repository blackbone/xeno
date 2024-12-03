using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xeno {
    public class WorldDisposedException : Exception {
        private new const string Message = "World you trying to access [{1}]{0} has beed disposed.";
        private WorldDisposedException((string Name, ushort Id) w)
            : base(string.Format(Message, w.Name, w.Id)) { }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf(bool condition, in (string Name, ushort Id) w) {
            if (condition) throw new WorldDisposedException(w);
        }
    }
}
