using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GlobalInformationTable
    {
        [FieldOffset(0)]
        public DataLookupTable* FirstDataLookupTable;
    }
}
