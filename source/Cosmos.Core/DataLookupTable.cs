using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    // The DataLookupTable (DLT) basically is a linked list.
    internal unsafe struct DataLookupTable
    {
        public const int EntriesPerTable = 169;

        public DataLookupTable* Previous;
        public DataLookupTable* Next;

        public DataLookupEntry FirstEntry;

        public unsafe DataLookupEntry* GetEntry(int index)
        {
            fixed (DataLookupEntry* xFirstEntryPtr = &FirstEntry)
            {
                return &xFirstEntryPtr[index];
            }
        }

        public void* GetFirstByteAfterTable()
        {
            fixed (DataLookupTable* xThisPtr = &this)
            {
                return (void*)(uint)(xThisPtr + GlobalSystemInfo.TotalDataLookupTableSize);
            }
        }
    }
}
