using System;
using Cosmos.Kernel.HAL.Devices;

namespace Cosmos.Kernel.Tests.Fat;

/// <summary>
/// RAM-backed <see cref="IBlockDevice"/> for FAT tests. No partitioning,
/// no DMA, just a flat byte array sliced by LBA.
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

    public string Name { get; private set; }
    public ulong BlockSize { get; private set; }
    public ulong BlockCount { get; private set; }

    /// <summary>
    /// Re-purposes this device's backing array as a fresh zeroed disk with
    /// new geometry (which must fit the capacity allocated at construction).
    /// The kernel heap has no compacting collector to hand back per-test
    /// device arrays, so cells recycle one scratch device instead of
    /// allocating their own — the accumulation is what exhausted the ARM64
    /// CI heap. Returns <c>this</c> so calls can slot in where a
    /// constructor call used to be.
    /// </summary>
    public MemoryBlockDevice Reconfigure(string name, ulong blockSize, ulong blockCount)
    {
        ulong bytes = blockSize * blockCount;
        if (blockSize == 0 || blockCount > ulong.MaxValue / blockSize || bytes > (ulong)_storage.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount));
        }

        Name = name;
        BlockSize = blockSize;
        BlockCount = blockCount;
        _storage.AsSpan(0, (int)bytes).Clear();
        FlushCount = 0;
        return this;
    }

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

    // IBlockDevice is throw-on-failure: validate before the (int) casts
    // below truncate — a byte offset that is a multiple of 2^32 would
    // otherwise silently alias sector 0, hiding exactly the wild-sector
    // driver bugs this device exists to expose.
    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount)
    {
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo));
        }
    }

    /// <summary>Number of <see cref="Flush"/> calls — lets tests assert durability points.</summary>
    public int FlushCount { get; private set; }

    public void Flush()
    {
        // RAM-backed store: nothing volatile to make durable; count the
        // calls so tests can assert the contract's durability points.
        FlushCount++;
    }
}
