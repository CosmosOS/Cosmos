using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;

namespace MM
{
    [IPCStruct]
    [Immutable]
    public struct MemoryRegion 
    {

        public static MemoryRegion Empty = new MemoryRegion(UIntPtr.Zero, 0); 

        readonly UIntPtr address;
        readonly uint size;

        public MemoryRegion( UIntPtr baseAddress , uint sizeInBytes)
        {
            address = baseAddress;
            size = sizeInBytes;
        }


        public UIntPtr Address
        {
            get { return address; }
        }


        public uint Size
        {
            get { return size; }
        }


        public override string ToString()
        {
            return string.Format("Memory Start:{0} Size:{1}", address, size);
        }

        public static Boolean operator ==(MemoryRegion v1, MemoryRegion v2)
        {

            return (v1.Equals(v2));
        }
        public static Boolean operator !=(MemoryRegion v1, MemoryRegion v2)
        {

            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MemoryRegion);


        }


        public override bool Equals(MemoryRegion obj)
        {
            return (this == obj);


        }
    }
}
