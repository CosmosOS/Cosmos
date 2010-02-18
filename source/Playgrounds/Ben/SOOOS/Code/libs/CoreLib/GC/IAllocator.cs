using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CoreLib.GC
{


    //internal static class AllocatorConstants
    //{
    //    //TODO invesitgate using 6 bytes where we do not use Ref Counting .
    //    public const uint ObjectHeaderBytes = 8; // + x if ref counting. 

    //}


    /// as we dont have pages we dont need to be as concerned about data structure allignment
    /// 
    /// Normal offset is 8 bytes 
    /// 24 bits type id , 16 bit SYnclock Id 5 bytes
    /// We use 22 bits for ref counting and use 2 bits for flags. 
    /// 
    /// Arrays add a size field at the start !
    /// 
    //TODO check 
    [StructLayout(LayoutKind.Explicit)]
    public struct ObjectHeader
    { 

        [FieldOffset(0)]
        public uint TypeId;  // used all the time so has its own field though 24 bits should be enough

        [FieldOffset(4)]
        public ushort SyncLock; // Thread Id ( within Collector Domain) 

        [FieldOffset(6)]
        public ushort Flags;

#if REFCOUNTED        
        public const int RefCountOffset = -4;

        [FieldOffset(8)]
        internal uint RefCount;

#endif 
    } 




    [TrustedInterface]
    public interface IAllocator
    {
        



        ICollector Collector  { get; } 

        /// <summary>
        /// We allocate with no knowledge of the Object here 
        /// 
        /// 
        /// new allocation consists of inserting code if needed and executing any type initialization (static members and construction) 
        /// working out the size which is equal to the object size + an offset for the size of the object header
        /// creating the space  hopefully via an interlock 
        /// 
        /// we then do all the non zero initializer fields and constructor
        /// 
        /// zeroing is not needed but it is done by the GC when alloacting memory.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        UIntPtr Allocate(uint size);

        

    }
}
