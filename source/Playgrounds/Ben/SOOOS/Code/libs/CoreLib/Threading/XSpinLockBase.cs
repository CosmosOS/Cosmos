using System;
using CoreLib.Locking;

namespace CoreLib.Threading
{
   
    namespace System.Threading
    {
        ///
        /// <summary>
        ///     Spinlock Base - actual implementation of spinlock mechanism
        /// </summary>
        ///
        [NoCCtor]
        [CLSCompliant(false)]
        public struct SpinLockBase
        {
            ///
            /// <summary>
            ///     Construct a SpinLockBase and initialize its type
            /// </summary>
            ///
            /// <param name="type">Type of the spin lock. See KernelSpinLock.cs for details</param>
            ///
            [NoHeapAllocation]
            public SpinLockBase(int type)
            {
                this.type = type;
                this.ownerId = 0;
            }

            ///
            /// <summary>
            ///    Assert if the specified thread actual owner of the spinlock
            /// </summary>
            ///
            /// <param name="ownerId">Owner Id that request is using to get spinlock</param>
            ///
            [System.Diagnostics.Conditional("DEBUG")]
            [NoHeapAllocation]
            public void AssertHeldBy(int ownerId)
            {
                VTable.Assert(IsHeldBy(ownerId));
            }

            ///
            /// <summary>
            ///     Internal method to find out if spinlock is held by any thread
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            [NoHeapAllocation]
            public bool IsHeld()
            {
                return this.ownerId != 0;
            }

            ///
            /// <summary>
            ///     Method to find out if spinlock is held by specific thread
            /// </summary>
            /// <returns> true if the spin lock is acquired by specific thread. </returns>
            ///
            /// <param name="ownerId">Owner Id that request is using to check for spinlock ownership</param>
            ///
            [NoHeapAllocation]
            public bool IsHeldBy(int ownerId)
            {
                return (this.ownerId == ownerId);
            }

            ///
            /// <summary>
            ///     Try to acquire the spin lock. Always return immediately.
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            [Inline]
            [NoHeapAllocation]
            public bool TryAcquire(int ownerId)
            {
                bool result;

                // Assert preconditions SpinLocks are *NOT* re-entrant, thread can't hold spinlocks of
                // lower rank
                VTable.Assert(ownerId != this.ownerId);

                // Try to acquire the spin lock
                result = (Interlocked.CompareExchange(ref this.ownerId,
                                                ownerId,
                                                0) == 0);

                return result;
            }

            ///
            /// <summary>
            ///    Acquire a lock
            /// </summary>
            ///
            /// <param name="ownerId">Owner Id that request is using to acquire spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Acquire(int ownerId)
            {
                // Thead Id can't be null
                VTable.Assert(ownerId != 0);

                // Try to acquire spinlock
                if (!TryAcquire(ownerId))
                {
                    // We failed to acquire just go ahead and dive into deeper spin loop with attempts
                    // to acquire lock
                    SpinToAcquire(ownerId);
                }
            }

            ///
            /// <summary>
            ///    Release a lock
            /// </summary>
            ///
            /// <param name="ownerId">Spinlock owner</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Release(int ownerId)
            {
                // Assert preconditions: Thread should own spinlock
                VTable.Assert(this.ownerId == ownerId);

                Interlocked.Exchange(ref this.ownerId, 0);
            }

            ///
            /// <summary>
            ///     Type of a spinlock
            ///</summary>
            ///
            internal int Type
            {
                [NoHeapAllocation]
                [Inline]
                get
                {
                    return this.type;
                }
            }

            ///
            /// <summary>
            ///    Spin until we can actually acquire spinlock
            /// </summary>
            ///
            /// <param name="ownerId">Owner Id that request is using to acquire spinlock</param>
            ///
            [NoHeapAllocation]
            private void SpinToAcquire(int ownerId)
            {
                int iSpin;
                int backoffs = 0;

                // Assert preconditions: thread's id and passed in id's should be the same
                VTable.Assert(ownerId != 0);

                while (true)
                {

                    // It is assumed this routine is only called after the inline
                    // method has failed the interlocked spinlock test. Therefore we
                    // retry using the safe test only after cheaper, unsafe test
                    // succeeds.
                    for (iSpin = 0; (this.ownerId != 0 && iSpin < MaxSpinLimit); iSpin++)
                    {
                        // Hopefully ownerId will not be enregistered, and this read will
                        // always hit memory, if it does then we are in trouble

                        // Perform HT friendly pause:
                        Thread.NativeNoOp();
                    }

                    // If we exited the loop prematurely, then try to get the lock
                    if (iSpin < MaxSpinLimit)
                    {

                        // Attempt to grab the spinlock
                        if (TryAcquire(ownerId))
                        {
                            break;
                        }

                        // If we couldn't get the lock, at least we know someone did,
                        // and the system is making progress; there is no need to
                        // back off.
                        backoffs = 0;
                        continue;
                    }

                    // Increment back off stats
                    backoffs++;
                }
            }

            /// <summary> A timer for short back off in ms </summary>
            const int MaxSpinLimit = 10000;

            /// <summary> A counter for short back off in ms </summary>
            const long ShortBackOffLimit = 8;

            /// <summary> A counter for short back off in ms </summary>
#if DEBUG
            const long LongBackOffLimit = 1000;
#else
        const long                      LongBackOffLimit = 10000;
#endif
            /// <summary> A timer for short back off in ms </summary>
            const long ShortBackOff = 1;

            /// <summary> A timer for long back off in ms </summary>
            const long LongBackOff = 5000;

            ///<summary> Thread owner </summary>
            private int ownerId;

            /// <summary>Type of a spinlock</summary>
            private readonly int type;

        }
    }

}
