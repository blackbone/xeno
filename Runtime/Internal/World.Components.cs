using System.Runtime.CompilerServices;

namespace Xeno {
    public partial class World {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool ComponentIsReferenceOrContainsReferences<T>() {
            return CI<T>.IsReferenceOrContainsReferences;
        }
    }
}
