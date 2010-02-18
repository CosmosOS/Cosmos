using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Threading
{

    /// <summary>
    /// From Singularity to keep compatability with driver API...
    /// </summary>
    public struct SpinLockType
    {
        internal const int RankShift = 0x10;
        internal const int TypeMask = 0xFFFF;

        private int type;

     
        /// <summary>
        ///  Spinlocks cant aquire higher ranls    
        /// /// </summary>
        public enum Ranks : short
        {
         
            NoRank = 0x0,

         //   MemoryManager = 0x1,

            Hal = 0x2,

            Scheduler = 0x4,

            TrustedService = 0x8, // special services 

            Application = 0x10, // services device dirvers etc 

        };

        /// <summary>
        /// enumerator used for statistics and tracking spinlocks
        /// 
        /// TODO  all but drivers needs to change.
        /// </summary>
        public enum Types : int
        {
            NoType = 0,
            Scheduler = (Ranks.Scheduler << RankShift) | 1,
            //InterruptMutex = (Ranks.Dispatcher << RankShift) | 1,
            //InterruptAutoResetEvent = (Ranks.Dispatcher << RankShift) | 2,
            //AutoResetEvent = (Ranks.Passive << RankShift) | 3,
            //Mutex = (Ranks.Passive << RankShift) | 4,
            //ManualResetEvent = (Ranks.Passive << RankShift) | 5,
            //Timer = (Ranks.Hal << RankShift) | 6,
            //IoApic = (Ranks.Hal << RankShift) | 7,
            //MpHalClock = (Ranks.Hal << RankShift) | 8,
            //RTClock = (Ranks.Hal << RankShift) | 9,
            //HalScreen = (Ranks.Hal << RankShift) | 10,
            //FlatPages = (Ranks.FlatPages << RankShift) | 11,
            //VirtualMemoryRange = (Ranks.Dispatcher << RankShift) | 12,
            //VMManager = (Ranks.Dispatcher << RankShift) | 13,
            //VMKernelMapping = (Ranks.Dispatcher << RankShift) | 14,
            //ProtectionDomainTable = (Ranks.Dispatcher << RankShift) | 15,
            //ProtectionDomainInit = (Ranks.Dispatcher << RankShift) | 16,
            //ProtectionDomainMapping = (Ranks.Dispatcher << RankShift) | 17,
            //SharedHeapAllocationOwner = (Ranks.Service << RankShift) | 18,
            //PhysicalHeap = (Ranks.Dispatcher << RankShift) | 19,
            //PhysicalPages = (Ranks.Dispatcher << RankShift) | 20,
            //PageManager = (Ranks.Dispatcher << RankShift) | 21,
            //IoResources = (Ranks.Dispatcher << RankShift) | 22,
            //Finalizer = (Ranks.Dispatcher << RankShift) | 23,
            //MpExecutionFreeze = (Ranks.Dispatcher << RankShift) | 24,
            //MpExecutionMpCall = (Ranks.Dispatcher << RankShift) | 25,

            //GCTracing = (Ranks.Dispatcher << RankShift) | 27,
            //IoIrq = (Ranks.Dispatcher << RankShift) | 28,
            //ServiceQueue = (Ranks.Dispatcher << RankShift) | 29,
            //EndpointCore = (Ranks.Dispatcher << RankShift) | 30,
            //KernelTestCase = (Ranks.Dispatcher << RankShift) | 31,
            //HandleTable = (Ranks.Passive << RankShift) | 32,
            //ThreadTable = (Ranks.Passive << RankShift) | 33,
            //ProcessTable = (Ranks.Passive << RankShift) | 34,
            Thread = (Ranks.Application << RankShift) | 35,
            MaxTypeId = (Thread & TypeMask) + 1
        };


        public SpinLockType(int type)
        {
            this.type = type;
        }


        public SpinLockType(Types type)
        {
            this.type = (int)type;
        }


        // support assign.
        //public static implicit operator SpinLockType(Int32 intValue)
        //{
        //    return new SpinLockType(intValue);
        //}
        public static implicit operator SpinLockType(Types types)
        {
            return new SpinLockType(types);
        }



        public int GetTypeVal()
        {
            return (type & (int)TypeMask);
        }



        public int GetRank()
        {
            return (type >> RankShift);
        }

        
        public Types Type
        {
            get { return (Types)type; }
        } 


       
    }
}
