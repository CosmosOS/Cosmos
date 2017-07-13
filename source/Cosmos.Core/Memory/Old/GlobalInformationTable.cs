using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cosmos.Core.Memory.Old
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct GlobalInformationTable
    {
        [FieldOffset(0)]
        public DataLookupTable* FirstDataLookupTable;
    }
}
