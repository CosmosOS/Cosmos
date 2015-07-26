namespace Cosmos.Core
{
    internal unsafe struct DataLookupEntry
    {
        public void* DataBlock;
        public uint Size;
        // Refcount will be UInt32.MaxValue (0xFFFFFFFF) in case the block has been freed, but the memory hasn't been compacted yet
        public uint Refcount;
    }
}
