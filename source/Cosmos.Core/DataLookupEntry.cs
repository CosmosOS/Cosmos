namespace Cosmos.Core
{
    internal unsafe struct DataLookupEntry
    {
        public void* DataBlock;
        public uint Size;
        public uint Refcount;
    }
}
