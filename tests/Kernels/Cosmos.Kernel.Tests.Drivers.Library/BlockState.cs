// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// What <see cref="BlockDriver"/> holds for one bound device, hung off
/// <see cref="DeviceBinding.DriverState"/>: the block device contract it
/// publishes, a blank disk of <see cref="DeviceBlockCount"/> sectors held
/// in a byte array. A blank disk carries no partition table and no boot
/// sector, so the storage manager's scan finds nothing on it. The test
/// reads the state back through the node's binding and the ring's storage
/// manager. Written over the public seam only.
/// </summary>
public sealed class BlockState : IBlockDevice
{
    /// <summary>The name the device is published under.</summary>
    public const string DeviceName = "synthetic-block";

    /// <summary>Bytes per block of the disk.</summary>
    public const int DeviceBlockSize = 512;

    /// <summary>Blocks on the disk.</summary>
    public const int DeviceBlockCount = 64;

    private readonly DeviceBinding _binding;
    private readonly byte[] _store = new byte[DeviceBlockSize * DeviceBlockCount];
    private volatile int _flushCount;

    /// <summary>Keeps the binding the driver was probed with. Thread context, from the probe.</summary>
    /// <param name="binding">The device and the kit facilities for it.</param>
    internal BlockState(DeviceBinding binding)
    {
        _binding = binding;
    }

    /// <inheritdoc/>
    public string Name => DeviceName;

    /// <inheritdoc/>
    public ulong BlockSize => DeviceBlockSize;

    /// <inheritdoc/>
    public ulong BlockCount => DeviceBlockCount;

    /// <summary>The binding this state belongs to.</summary>
    public DeviceBinding Binding => _binding;

    /// <summary>How many times <see cref="Flush"/> was called.</summary>
    public int FlushCount => _flushCount;

    /// <summary>Copies blocks out of the store. Any context.</summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> blocks long.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range runs past the end of the disk, or <paramref name="data"/> is too short.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int offset = CheckRange(blockNo, blockCount, data.Length);
        int length = (int)blockCount * DeviceBlockSize;
        _store.AsSpan(offset, length).CopyTo(data);
    }

    /// <summary>Copies blocks into the store. Any context.</summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Where they come from; at least <paramref name="blockCount"/> blocks long.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range runs past the end of the disk, or <paramref name="data"/> is too short.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int offset = CheckRange(blockNo, blockCount, data.Length);
        int length = (int)blockCount * DeviceBlockSize;
        data.Slice(0, length).CopyTo(_store.AsSpan(offset, length));
    }

    /// <summary>Counts the call; the store is memory, so there is nothing to make durable. Any context.</summary>
    public void Flush() => _flushCount++;

    /// <summary>Checks a block range against the disk and the buffer, and turns it into a byte offset.</summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="bufferLength">The caller's buffer length in bytes.</param>
    /// <returns>The byte offset of <paramref name="blockNo"/> in the store.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The range runs past the end of the disk, or the buffer is too short.</exception>
    private static int CheckRange(ulong blockNo, ulong blockCount, int bufferLength)
    {
        // Overflow-safe: the sum of the two would wrap for a block number
        // near the top of the range and slip past a naive check.
        if (blockNo > DeviceBlockCount || blockCount > DeviceBlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "The range extends beyond the end of the synthetic disk.");
        }

        if ((ulong)bufferLength < blockCount * DeviceBlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount), "The buffer is shorter than the requested blocks.");
        }

        return (int)blockNo * DeviceBlockSize;
    }
}
