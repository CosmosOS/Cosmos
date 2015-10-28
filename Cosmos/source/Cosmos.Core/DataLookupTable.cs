﻿using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    // The DataLookupTable (DLT) basically is a linked list.
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct DataLookupTable
    {
        public const int EntriesPerTable = 170;

        [FieldOffset(0)]
        public DataLookupTable* Previous;
        [FieldOffset(4)]
        public DataLookupTable* Next;

        [FieldOffset(8)]
        public DataLookupEntry FirstEntry;

        public unsafe DataLookupEntry* GetEntry(int index)
        {
            fixed (DataLookupEntry* xFirstEntryPtr = &FirstEntry)
            {
                if (index == 0)
                {
                    return xFirstEntryPtr;
                }
                else
                {
                    return &xFirstEntryPtr[index];
                }
            }
        }
    }
}
