using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cosmos.CPU.x86.Memory.Old {
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct DataLookupEntry {
        [FieldOffset(0)]
        public void* DataBlock;
        [FieldOffset(4)]
        public uint Size;
        // Refcount will be UInt32.MaxValue (0xFFFFFFFF) in case the block has been freed, but the memory hasn't been compacted yet
        [FieldOffset(8)]
        public uint Refcount;
    }
}
