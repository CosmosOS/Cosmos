using System.Runtime.InteropServices;

namespace Cosmos.CPU.x86.Memory.Old;

// The DataLookupTable (DLT) basically is a double linked list.
[StructLayout(LayoutKind.Explicit)]
public unsafe struct DataLookupTable
{
    public const int EntriesPerTable = 170;

    [FieldOffset(0)] public DataLookupTable* Previous;
    [FieldOffset(4)] public DataLookupTable* Next;

    [FieldOffset(8)] public DataLookupEntry FirstEntry;

    public DataLookupEntry* GetEntry(uint index)
    {
        fixed (DataLookupEntry* xFirstEntryPtr = &FirstEntry)
        {
            if (index == 0)
            {
                return xFirstEntryPtr;
            }

            return &xFirstEntryPtr[index];
        }
    }
}
