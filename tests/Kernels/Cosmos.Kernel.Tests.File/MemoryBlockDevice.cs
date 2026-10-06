namespace Cosmos.Kernel.Tests.File;

// global:: because a plain "System" binds to Cosmos.Kernel.System from
// inside this namespace.
using global::System;
using Cosmos.Kernel.HAL.Devices;

/// <summary>
/// RAM-backed <see cref="IBlockDevice"/> for the System.IO tests. No
/// partitioning, no DMA, just a flat byte array sliced by LBA (same shape as
/// the Fat suite's device).
/// </summary>
internal sealed class MemoryBlockDevice : IBlockDevice
{
    private readonly byte[] _storage;

    public MemoryBlockDevice(string name, ulong blockSize, ulong blockCount)
    {
        Name = name;
        BlockSize = blockSize;
        BlockCount = blockCount;
        _storage = new byte[blockSize * blockCount];
    }

    public string Name { get; }
    public ulong BlockSize { get; }
    public ulong BlockCount { get; }

    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfOutOfRange(blockNo, blockCount);
        int byteOffset = (int)(blockNo * BlockSize);
        int byteLen = (int)(blockCount * BlockSize);
        _storage.AsSpan(byteOffset, byteLen).CopyTo(data);
    }

    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfOutOfRange(blockNo, blockCount);
        int byteOffset = (int)(blockNo * BlockSize);
        int byteLen = (int)(blockCount * BlockSize);
        data.Slice(0, byteLen).CopyTo(_storage.AsSpan(byteOffset, byteLen));
    }

    // IBlockDevice is throw-on-failure: validate before the (int) casts above
    // truncate.
    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount)
    {
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo));
        }
    }

    public void Flush()
    {
        // RAM-backed store: nothing volatile to make durable.
    }
}
