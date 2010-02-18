
using CoreLib.ProcessModel;
using CoreLib.Locking;
using System.Diagnostics;

namespace CoreLib.Threading
{
    //unchecked spinlock
    public struct SpinLock
    {


        /// <summary> A timer for short back off in ms </summary>
        const int MaxSpinLimit = 10000;

        ///// <summary> A counter for short back off in ms </summary>
        //const long ShortBackOffLimit = 8;

        ///// <summary> A timer for short back off in ms </summary>
        //const long ShortBackOff = 1;

        ///// <summary> A timer for long back off in ms </summary>
        //const long LongBackOff = 5000;

        /////<summary> Thread owner </summary>
        private int ownerId;

        ///// <summary>Type of a spinlock</summary>
        //private readonly SpinLockType type;



        //public SpinLock()
        //{
        // //   type = new SpinLockType(spinType);
        //    this.ownerId = 0; 
        //}

        //public SpinLock(SpinLockType.Types spinType , STP owner) : this( (int) spinType ,owner)
        //{
            
        //}



        public bool TryAcquire(int ownerId)
        {
            bool result;

            Debug.Assert(ownerId != this.ownerId);

            // Try to acquire the spin lock
            result = (Interlocked.CompareExchange(ref this.ownerId,
                                            ownerId,
                                            0) == 0);

            return result;
        }


        [Inline]
        public void Acquire(int ownerId)
        {
            // Thead Id can't be null
            Debug.Assert(ownerId != 0);

            // Try to acquire spinlock
            if (!TryAcquire(ownerId))
            {
                // We failed to acquire just go ahead and dive into deeper spin loop with attempts
                // to acquire lock
                SpinToAcquire(ownerId);
            }
        }

        [Inline]
        public void Release(int ownerId)
        {
            Debug.Assert(this.ownerId == ownerId);

            Interlocked.Exchange(ref this.ownerId, 0);
        }

        /////
        ///// <summary>
        /////     Type of a spinlock
        /////</summary>
        /////
        //internal int Type
        //{
        //    [Inline]
        //    get
        //    {
        //        return this.type;
        //    }
        //}

  
        private void SpinToAcquire(int ownerId)
        {
            int iSpin;
            int backoffs = 0;

            // Assert preconditions: thread's id and passed in id's should be the same
            Debug.Assert(ownerId != 0);

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
                    Thread.NoOp();
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

    }
}
