// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.Devices.Storage;

namespace Cosmos.Kernel.Tests.Iso9660;

/// <summary>
/// RAM-backed <see cref="IBlockDevice"/> holding a fixture image: a flat byte
/// array sliced by LBA, in the block size the cell asks for, so that the same
/// image is read as a CD (2048-byte sectors) and as a disk an image was
/// written to (512-byte sectors). Writes are accepted so that a cell can
/// corrupt a descriptor on purpose.
/// </summary>
internal sealed class MemoryBlockDevice : IBlockDevice
{
    private readonly byte[] _storage;

    public string Name { get; }

    public ulong BlockSize { get; }

    public ulong BlockCount { get; private set; }

    /// <summary>Number of <see cref="Flush"/> calls, so a cell can assert the driver never asks for one.</summary>
    public int FlushCount { get; private set; }

    /// <summary>Number of <see cref="ReadBlock"/> calls, so a cell can assert how many device reads a file read costs.</summary>
    public int ReadCount { get; private set; }

    public MemoryBlockDevice(string name, ulong blockSize, byte[] storage)
    {
        Name = name;
        BlockSize = blockSize;
        _storage = storage;
        BlockCount = (ulong)storage.Length / blockSize;
    }

    /// <summary>Shortens the device so that the volume it holds no longer fits.</summary>
    public void Truncate(ulong blockCount)
    {
        BlockCount = blockCount;
    }

    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfOutOfRange(blockNo, blockCount);
        ReadCount++;
        int byteOffset = (int)(blockNo * BlockSize);
        int byteLength = (int)(blockCount * BlockSize);
        _storage.AsSpan(byteOffset, byteLength).CopyTo(data);
    }

    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfOutOfRange(blockNo, blockCount);
        int byteOffset = (int)(blockNo * BlockSize);
        int byteLength = (int)(blockCount * BlockSize);
        data.Slice(0, byteLength).CopyTo(_storage.AsSpan(byteOffset, byteLength));
    }

    public void Flush()
    {
        FlushCount++;
    }

    // IBlockDevice is throw-on-failure: validate before the (int) casts
    // above truncate.
    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount)
    {
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo));
        }
    }
}
