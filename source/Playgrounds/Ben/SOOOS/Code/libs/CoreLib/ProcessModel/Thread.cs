

using CoreLib.GC;
namespace CoreLib.ProcessModel
{
    public class Thread
    {
        public static Thread None = new Thread(); // id 0 


        /// <summary>
        /// We can used unsigned here 0 is invalid 
        /// 
        /// High bits is the CollectorDomainId
        /// Low 16 bits are the thread wihtin 
        /// 
        /// So we allow 65K threads per apps and 65K apps.
        /// 
        /// </summary>
        public uint Id { get; set; } // remove set


        //TODO set is trusted and internal 
        [Trusted]
        public IAllocator Allocator { get; set; }


        internal int State;  //Threadstate for Synch status
        /// <summary>
        /// emmit noop used for HT friendly code
        /// </summary>
        [Inline]
        public static void NoOp()
        {
            //TODO 
        }
    }
}
