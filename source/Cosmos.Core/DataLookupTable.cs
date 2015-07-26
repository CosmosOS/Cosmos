namespace Cosmos.Core
{
    // The DataLookupTable (DLT) basically is a linked list.
    internal unsafe struct DataLookupTable
    {
        public const int EntriesPerTable = 170;

        public DataLookupTable* Previous;
        public DataLookupTable* Next;

        public DataLookupEntry* Entries;

        public void* FirstByteAfterTable;
    }
}
