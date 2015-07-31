using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct GlobalInformationTable
    {
        [FieldOffset(0)]
        public DataLookupTable* FirstDataLookupTable;
    }
}
