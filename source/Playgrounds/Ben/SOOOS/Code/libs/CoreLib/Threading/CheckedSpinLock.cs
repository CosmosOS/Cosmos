

using CoreLib.ProcessModel;

namespace CoreLib.Threading
{
    /// <summary>
    /// wraps a spinlock with checked methods , statistics and deadlock detection
    /// </summary>
    /// 

    //TODO counters 
    //TODO Dead lock detection
    //TODO lock levels
    public struct CheckedSpinLock
    {
        private SpinLock spinLock;
        private SpinLockType type;


        public CheckedSpinLock(int spinType)
            : this(new SpinLockType(spinType))
        {


        }


        public CheckedSpinLock(SpinLockType spinType)
        {
            spinLock = new SpinLock();
            type = spinType;

        }


        public bool TryAcquire()
        {
            int threadId = (int) STP.Current.Thread.Id ;
            return spinLock.TryAcquire(threadId); 
           
        }


        [Inline]
        public void Acquire()
        {
            int threadId = (int) STP.Current.Thread.Id ;
            spinLock.Acquire(threadId); 
         
        }

        [Inline]
        public void Release()
        {
            int threadId = (int) STP.Current.Thread.Id ;
            spinLock.Release(threadId); 
        
        }


    }
}
