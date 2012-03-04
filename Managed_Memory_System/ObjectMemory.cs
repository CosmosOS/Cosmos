using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.Managed_Memory_System
{
    class ObjectMemory
    {
        protected uint startAddress = 0;
        protected uint endAddress = 0;
        protected bool full = false;
        protected bool aSmall = false;
        protected uint processId = 0; //only use when allocation memory in MangaedMemory.

        public ObjectMemory(uint thisStartAddress, uint thisEndAddress, bool thisSmall)
        {
            startAddress = thisStartAddress;
            endAddress = thisEndAddress;
            aSmall = thisSmall;
        }

        public uint ObjectStart
        {
            get
            {
                return startAddress;
            }
            set
            {
                startAddress = value;
            }
        }

        public bool MemorySmall
        {
            get
            {
                return aSmall; 
            }
        }

        public uint ObjectEndAddress
        {
            get
            {
                return endAddress;
            }
            set
            {
                endAddress = value;
            }
        }

        public bool MemoryFull
        {
            get
            {
                return full;
            }
            set
            {
                full = value;
            }
        }

        public uint Process
        {
            get
            {
                return processId;
            }
            set
            {
                processId = value;
            }
        }

    }
}
