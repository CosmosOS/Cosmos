using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Threading
{
    //Wrapper with statistics
        public struct XSpinLock
        {
            ///
            /// <summary>
            ///     Spinlock Rank is enumerator used to enumerate spinlock  ranks. The rule is:
            ///     holders of lower rank spinlocks can't acquire spinlocks with higher ranks.
            ///     Note this needs to be a bitmask enum because of the validation logic in Thread.cs
            /// </summary>
            ///
            internal enum Ranks : short
            {
                /// <summary> Placeholder. eventually all spinlocks should be ranked </summary>
                NoRank = 0x0,

                /// <summary>
                ///     Used by flag pages (low level memory management). This is nested
                ///     within several spin locks with Dispatcher rank, including
                ///     PageManager (GC) and linked stack.
                /// </summary>
                FlatPages = 0x1,

                /// <summary>
                ///     Use by Hal. This can be nested within other
                ///     spin locks with Dispatcher rank.
                /// </summary>
                Hal = 0x2,

                /// <summary>
                ///     Used by scheduler and dispatcher code, and other code run with
                ///     interrupt disabled.
                /// </summary>
                Dispatcher = 0x4,

                /// <summary>
                ///     The lowest rank above Dispatcher. A thread holding a lock with this rank
                ///     cannot acquire another spinlock, unless it enters scheduling code through
                ///     Yield or blocking. Most code above dispatcher level should use this rank.
                ///
                ///     Add higher ranks only if the code needs to acquire multiple spinlocks,
                ///     which should be avoided unless absolutely necessary.
                /// </summary>
                Passive = 0x8,

                /// <summary>
                ///     Used by services implemented in kernel
                /// </summary>
                Service = 0x10,
            };

            ///
            /// <summary>
            ///     Spinlock Type is enumerator used to enumerate spinlock types so that we can keep
            /// proper statistic for each spinlock. Statistic will allow us to identify spinlock problems
            /// </summary>
            ///
            public enum Types : int
            {
                NoType = 0,
                InterruptMutex = (Ranks.Dispatcher << RankShift) | 1,
                InterruptAutoResetEvent = (Ranks.Dispatcher << RankShift) | 2,
                AutoResetEvent = (Ranks.Passive << RankShift) | 3,
                Mutex = (Ranks.Passive << RankShift) | 4,
                ManualResetEvent = (Ranks.Passive << RankShift) | 5,
                Timer = (Ranks.Hal << RankShift) | 6,
                IoApic = (Ranks.Hal << RankShift) | 7,
                MpHalClock = (Ranks.Hal << RankShift) | 8,
                RTClock = (Ranks.Hal << RankShift) | 9,
                HalScreen = (Ranks.Hal << RankShift) | 10,
                FlatPages = (Ranks.FlatPages << RankShift) | 11,
                VirtualMemoryRange = (Ranks.Dispatcher << RankShift) | 12,
                VMManager = (Ranks.Dispatcher << RankShift) | 13,
                VMKernelMapping = (Ranks.Dispatcher << RankShift) | 14,
                ProtectionDomainTable = (Ranks.Dispatcher << RankShift) | 15,
                ProtectionDomainInit = (Ranks.Dispatcher << RankShift) | 16,
                ProtectionDomainMapping = (Ranks.Dispatcher << RankShift) | 17,
                SharedHeapAllocationOwner = (Ranks.Service << RankShift) | 18,
                PhysicalHeap = (Ranks.Dispatcher << RankShift) | 19,
                PhysicalPages = (Ranks.Dispatcher << RankShift) | 20,
                PageManager = (Ranks.Dispatcher << RankShift) | 21,
                IoResources = (Ranks.Dispatcher << RankShift) | 22,
                Finalizer = (Ranks.Dispatcher << RankShift) | 23,
                MpExecutionFreeze = (Ranks.Dispatcher << RankShift) | 24,
                MpExecutionMpCall = (Ranks.Dispatcher << RankShift) | 25,
                Scheduler = (Ranks.Dispatcher << RankShift) | 26,
                GCTracing = (Ranks.Dispatcher << RankShift) | 27,
                IoIrq = (Ranks.Dispatcher << RankShift) | 28,
                ServiceQueue = (Ranks.Dispatcher << RankShift) | 29,
                EndpointCore = (Ranks.Dispatcher << RankShift) | 30,
                KernelTestCase = (Ranks.Dispatcher << RankShift) | 31,
                HandleTable = (Ranks.Passive << RankShift) | 32,
                ThreadTable = (Ranks.Passive << RankShift) | 33,
                ProcessTable = (Ranks.Passive << RankShift) | 34,
                Thread = (Ranks.Passive << RankShift) | 35,
                MaxTypeId = (Thread & TypeMask) + 1
            };

            ///
            /// <summary>
            ///    Static initializer
            /// </summary>
            ///
            static public void StaticInitialize()
            {
            }

            ///
            /// <summary>
            ///     Init spinlock
            /// </summary>
            /// <param name="type">Type of SpinLock</param>
            ///
            public XSpinLock(Types type)
            {
                baseLock = new SpinLockBase((int)type);
            }

            ///
            /// <summary>
            ///     SpinLock Rank
            /// </summary>
            ///
            internal int Rank
            {
                [NoHeapAllocation]
                get
                {
                    return baseLock.Type >> RankShift;
                }
            }

            ///
            /// <summary>
            ///     SpinLock Type
            /// </summary>
            ///
            internal int Type
            {
                [NoHeapAllocation]
                get
                {
                    return baseLock.Type & TypeMask;
                }
            }

            ///
            /// <summary>
            ///     Acquire spinlock
            /// </summary>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Acquire()
            {
                Thread thread = Thread.CurrentThread;
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();

                AcquireInternal(thread, threadId);
            }

            ///
            /// <summary>
            ///     Acquire spinlock
            /// </summary>
            ///
            /// <param name="threadId">Thread's Id acquiring spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Acquire(int threadId)
            {
                AcquireInternal(Thread.GetThreadFromThreadId(threadId), threadId);
            }

            ///
            /// <summary>
            ///     Acquire spinlock
            /// </summary>
            ///
            /// <param name="thread">Thread acquiring spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Acquire(Thread thread)
            {
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();

                AcquireInternal(thread, threadId);
            }

            ///
            /// <summary>
            ///     Release spinlock
            /// </summary>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Release()
            {
                Thread thread = Thread.CurrentThread;
                int threadId = (thread == null) ? InitialThreadId :
                                                thread.GetThreadId();
                // Release spinlock
                ReleaseInternal(thread, threadId);
            }

            ///
            /// <summary>
            ///     Release spinlock
            /// </summary>
            ///
            /// <param name="threadId">Thread's Id releasing spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Release(int threadId)
            {
                // Release spinlock
                ReleaseInternal(Thread.GetThreadFromThreadId(threadId), threadId);
            }

            ///
            /// <summary>
            ///     Release spinlock
            /// </summary>
            ///
            /// <param name="thread">Thread releasing spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public void Release(Thread thread)
            {
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();

                // Release spinlock
                ReleaseInternal(thread, threadId);
            }

            ///
            /// <summary>
            ///     Try to acquire the spin lock. Always return immediately.
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            [NoHeapAllocation]
            [Inline]
            public bool TryAcquire()
            {
                Thread thread = Thread.CurrentThread;
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();

                return TryAcquireInternal(thread, threadId);
            }

            ///
            /// <summary>
            ///     Try to acquire the spin lock. Always return immediately.
            /// </summary>
            ///
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            /// <param name="thread">Thread acquiring spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            public bool TryAcquire(Thread thread)
            {
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();

                return TryAcquireInternal(thread, thread.GetThreadId());
            }

            ///
            /// <summary>
            ///     Method to find out if spinlock is held by specified thread
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            /// <param name="thread">Thread to verify possible spinlock's ownership</param>
            ///
            [NoHeapAllocation]
            public bool IsHeldBy(Thread thread)
            {
                int threadId = (thread == null) ? InitialThreadId : thread.GetThreadId();
                return baseLock.IsHeldBy(threadId + 1);
            }

            ///
            /// <summary>
            ///     Method to find out if spinlock is held by specified thread
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            /// <param name="threadId">Thread's Id to verify possible spinlock's ownership</param>
            ///
            [NoHeapAllocation]
            public bool IsHeldBy(int threadId)
            {
                return baseLock.IsHeldBy(threadId + 1);
            }

            ///
            /// <summary>
            ///     Assert thatf spinlock is held by specified thread
            /// </summary>
            ///
            /// <param name="thread">Thread to verify possible spinlock's ownership</param>
            ///
            [System.Diagnostics.Conditional("DEBUG")]
            [NoHeapAllocation]
            public void AssertHeldBy(Thread thread)
            {
                VTable.Assert(IsHeldBy(thread));
            }

            ///
            /// <summary>
            ///     Assert thatf spinlock is held by specified thread
            /// </summary>
            ///
            /// <param name="threadId">Thread's Id to verify possible spinlock's ownership</param>
            ///
            [System.Diagnostics.Conditional("DEBUG")]
            [NoHeapAllocation]
            public void AssertHeldBy(int threadId)
            {
                VTable.Assert(IsHeldBy(threadId));
            }

            ///
            /// <summary>
            ///     Given integer : derive type of a lock
            /// </summary>
            ///
            /// <param name="type">Parameter from which we can derive actual type of spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            internal static int DeriveType(int type)
            {
                // Type has to be in correct range
                VTable.Assert((type & TypeMask) >= (int)Types.NoType &&
                            (type & TypeMask) < (int)Types.MaxTypeId);

                return (type & (int)TypeMask);
            }

            ///
            /// <summary>
            ///     Given integer : derive type of a lock
            /// </summary>
            ///
            /// <param name="type">Parameter from which we can derive actual rank of spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            internal static int DeriveRank(int type)
            {
                // Type has to be in correct range
                VTable.Assert((type & TypeMask) >= (int)Types.NoType &&
                            (type & TypeMask) < (int)Types.MaxTypeId);

                return (type >> RankShift);
            }

            ///
            /// <summary>
            ///     Try to acquire the spin lock. Always return immediately.
            /// </summary>
            /// <returns> true if the spin lock is acquired. </returns>
            ///
            /// <param name="thread">Thread acquiring spinlock</param>
            /// <param name="threadId">Thread's Id acquiring spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            private bool TryAcquireInternal(Thread thread, int threadId)
            {
                bool result;

                // Assert preconditions:for spinlock with a rank = DisabledInterrupts and below
                // interrupts have to be disabled.
                VTable.Assert(Rank == (int)Ranks.NoRank ||
                            Rank > (int)Ranks.Dispatcher ||
                            Processor.InterruptsDisabled());


                // Notify thread that we are about to acquire spinlock
                if (thread != null)
                {
                    thread.NotifySpinLockAboutToAcquire(this.baseLock.Type);
                }

                result = baseLock.TryAcquire(threadId + 1);

                // If we didn't acquire spinlock -we should notify thread about it: Just use release
                // notification
                if (thread != null && !result)
                {
                    thread.NotifySpinLockReleased(this.baseLock.Type);
                }

                return result;
            }

            ///
            /// <summary>
            ///     Acquire the spin lock.
            /// </summary>
            ///
            /// <param name="thread">Thread acquiring spinlock</param>
            /// <param name="threadId">Thread's Id acquiring spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            private void AcquireInternal(Thread thread, int threadId)
            {
                // Assert preconditions:for spinlock with a rank = DisabledInterrupts and below
                // interrupts have to be disabled.
                VTable.Assert(Rank == (int)Ranks.NoRank ||
                            Rank > (int)Ranks.Dispatcher ||
                            Processor.InterruptsDisabled());


                // Thread has to be notified if we are about to acquire spinlock
                if (thread != null)
                {
                    thread.NotifySpinLockAboutToAcquire(this.baseLock.Type);
                }

                // Get lock
                baseLock.Acquire(threadId + 1);
            }

            ///
            /// <summary>
            ///     Release the spin lock.
            /// </summary>
            ///
            /// <param name="thread">Thread releasing spinlock</param>
            /// <param name="threadId">Thread's Id releasing spinlock</param>
            ///
            [NoHeapAllocation]
            [Inline]
            private void ReleaseInternal(Thread thread, int threadId)
            {
                // Assert preconditions:for spinlock with a rank = DisabledInterrupts and below
                // interrupts have to be disabled.
                VTable.Assert(Rank == (int)Ranks.NoRank ||
                            Rank > (int)Ranks.Dispatcher ||
                            Processor.InterruptsDisabled());


                // Release spinlock
                baseLock.Release(threadId + 1);

                if (thread != null)
                {
                    // Don't forget to notify thread that it just released spinlock
                    thread.NotifySpinLockReleased(this.baseLock.Type);
                }
            }

            /// <summary> Constant defines shift of the rank</summary>
            internal const int RankShift = 0x10;

            /// <summary> Constant defines mask for getting type of gompound spinlock type </summary>
            internal const int TypeMask = 0xFFFF;

            /// <summary> Id of a initial thread </summary>
            private const int InitialThreadId = -2;

            /// <summary> Actual mechanism implementing spinlock</summary>
            private SpinLockBase baseLock;
        }

        /// <summary> Attribute to mark methods that stop lock rank verification </summary>
        [Layer(0)]
        public class IgnoreLockRankAttribute : Attribute
        {
            public IgnoreLockRankAttribute() { }
        }

        /// <summary> Attribute to mark classes / methods that hold FlatPages locks </summary>
        [Layer(1)]
        public class FlatPagesLockAttribute : Attribute
        {
            public FlatPagesLockAttribute() { }
        }

        /// <summary> Attribute to mark classes / methods that hold Hal locks </summary>
        [Layer(2)]
        public class HalLockAttribute : Attribute
        {
            public HalLockAttribute() { }
        }

        /// <summary> Attribute to mark classes / methods that hold Dispatcher locks </summary>
        [Layer(3)]
        public class DispatcherLockAttribute : Attribute
        {
            public DispatcherLockAttribute() { }
        }

        /// <summary> Attribute to mark classes / methods that hold Passive locks </summary>
        [Layer(4)]
        public class PassiveLockAttribute : Attribute
        {
            public PassiveLockAttribute() { }
        }

        /// <summary> Attribute to mark classes / methods that hold Service locks </summary>
        [Layer(5)]
        public class ServiceLockAttribute : Attribute
        {
            public ServiceLockAttribute() { }
        }
    }

}
