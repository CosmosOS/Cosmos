// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Interface for block storage devices (SATA, NVMe, virtio-blk, etc.).
/// Block-level read/write only; partitioning and filesystems sit above this.
///
/// <para>Error contract: <see cref="ReadBlock"/>, <see cref="WriteBlock"/>
/// and <see cref="Flush"/> throw on failure (device error, timeout,
/// out-of-range request). A return without an exception means the
/// operation completed on the device.</para>
/// </summary>
public interface IBlockDevice
{
    /// <summary>
    /// Total number of addressable blocks on the device.
    /// </summary>
    ulong BlockCount { get; }

    /// <summary>
    /// Block size in bytes (typically 512 or 4096).
    /// </summary>
    ulong BlockSize { get; }

    /// <summary>
    /// Human-readable device name, unique per device instance
    /// (e.g. "sata0", "nvme0n1").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Read whole blocks starting at <paramref name="blockNo"/> into <paramref name="data"/>.
    /// </summary>
    /// <param name="blockNo">Logical block address to start reading from.</param>
    /// <param name="blockCount">Number of blocks to read.</param>
    /// <param name="data">Destination buffer; must be at least <paramref name="blockCount"/> * <see cref="BlockSize"/> bytes.</param>
    void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data);

    /// <summary>
    /// Write whole blocks starting at <paramref name="blockNo"/> from <paramref name="data"/>.
    /// Completion means the device accepted the write; durability against
    /// power loss is only guaranteed after <see cref="Flush"/> returns.
    /// </summary>
    /// <param name="blockNo">Logical block address to start writing to.</param>
    /// <param name="blockCount">Number of blocks to write.</param>
    /// <param name="data">Source buffer; must be at least <paramref name="blockCount"/> * <see cref="BlockSize"/> bytes.</param>
    void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data);

    /// <summary>
    /// Flush the device's volatile write cache so previously completed
    /// writes are durable. No-op on devices without a volatile cache.
    /// </summary>
    void Flush();
}
