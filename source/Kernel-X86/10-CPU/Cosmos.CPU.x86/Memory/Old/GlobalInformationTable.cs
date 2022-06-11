using System.Runtime.InteropServices;

namespace Cosmos.CPU.x86.Memory.Old;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GlobalInformationTable
{
    [FieldOffset(0)] public DataLookupTable* FirstDataLookupTable;
}
