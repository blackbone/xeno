using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xeno {
    public class NoComponentException : Exception {
        private static readonly string Message = "Component {0} you're trying to access not presented on E_{1}({2}) of world [{4}]{3}.";
        private NoComponentException(string type, (uint Id, uint Version, (string Name, ushort Id) w) e)
            : base(string.Format(Message, type, e.Id, e.Version, e.w.Name, e.w.Id)) { }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf<T>(bool condition, (uint Id, uint Version, (string Name, ushort Id) w) e) {
            if (condition) throw new NoComponentException(typeof(T).FullName, e);
        }
    }
}
