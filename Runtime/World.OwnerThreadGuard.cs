using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Xeno {
    public abstract partial class World {
#if DEBUG || XENO_OWNER_THREAD_GUARD_ASSERTS
        private int _ownerThreadId;
#endif

        [Conditional("DEBUG")]
        [Conditional("XENO_OWNER_THREAD_GUARD_ASSERTS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertOwnerThread() {
#if DEBUG || XENO_OWNER_THREAD_GUARD_ASSERTS
            var currentThreadId = Environment.CurrentManagedThreadId;
            var ownerThreadId = _ownerThreadId;

            if (ownerThreadId == 0) {
                ownerThreadId = Interlocked.CompareExchange(ref _ownerThreadId, currentThreadId, 0);
                if (ownerThreadId == 0)
                    ownerThreadId = currentThreadId;
            }

            if (ownerThreadId != currentThreadId)
                ThrowWrongThreadMutation(ownerThreadId, currentThreadId);
#endif
        }

#if DEBUG || XENO_OWNER_THREAD_GUARD_ASSERTS
        private static void ThrowWrongThreadMutation(int ownerThreadId, int currentThreadId) {
            throw new InvalidOperationException($"World mutation must stay on the owner thread. Owner={ownerThreadId}, current={currentThreadId}.");
        }
#endif
    }
}
