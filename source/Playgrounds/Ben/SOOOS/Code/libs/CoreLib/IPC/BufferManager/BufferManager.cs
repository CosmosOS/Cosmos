using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC
{
    /// <summary>
    /// Manages a large amount of byte[] buffers
    /// 
    /// Can be used by Drivers so pointers of sub blocks can be passed around
    /// with zero copy. The region is marked in use unless explicityly released 
    /// 
    /// Buffers are trusted and have pointers to globalImmutableObjects
    /// 
    /// Note the memory used is always fixed and taken directly from the Memory Manager. 
    /// Though you could create a fixed buffer and pass it in. 
    /// 
    /// It MUST be fixed since we will pass the pointer to other STPs which will have a different GC.
    /// 
    /// Some apps should use small , medium and large buffers this just involves a different buffer manager . 
    /// 
    /// </summary>
    public class BufferManager<T>
    {

        public BufferManager()
        {

        }


        /// <summary>
        /// Passed in Memory location 
        /// </summary>
        /// <param name="baseLocation"></param>
        /// <param name="memSize"></param>
        /// <param name="bufferSize"></param>
        public void  Init( UIntPtr baseLocation , int memSize , int bufferSize) 
        {
            //var xxx = Buffer.BlockCopy(
        }
    }

   // public struct  Holder




}
