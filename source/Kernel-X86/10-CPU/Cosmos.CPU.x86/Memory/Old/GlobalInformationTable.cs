using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cosmos.CPU.x86.Memory.Old {
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GlobalInformationTable {
        [FieldOffset(0)]
        public DataLookupTable* FirstDataLookupTable;
    }
}
