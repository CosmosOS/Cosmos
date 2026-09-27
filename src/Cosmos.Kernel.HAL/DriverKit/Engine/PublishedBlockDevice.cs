// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// A disk a registered driver published, as the storage manager sees it: an
/// <see cref="IBlockDevice"/> in front of the driver's own, which forwards
/// every call to it while the disk is live. It also tells the storage
/// manager where the disk sits, which its primary-disk rule reads: whether
/// its device can leave the bus, and which PCI function carries it. A USB
/// driver's disk is withdrawn when its device leaves the bus, and every
/// read, write and flush through it throws from then on, as the
/// <see cref="IBlockDevice"/> contract asks of a device that can no longer
/// complete one.
/// </summary>
internal sealed class PublishedBlockDevice : IBlockDevice
{
    // Where the disk stands. It starts Queued and moves forward only; Gone
    // and Withdrawn are final. An int for Interlocked: the delivering thread
    // and the USB hot-plug thread can race on a work item's publication.
    private const int Queued = 0;
    private const int Live = 1;
    private const int Gone = 2;
    private const int Withdrawn = 3;

    private readonly DeviceContext _context;
    private readonly IBlockDevice _device;
    private int _state = Queued;

    /// <summary>The driver's disk, which does the I/O.</summary>
    internal IBlockDevice Device => _device;

    /// <summary>
    /// True for a disk whose device can leave the bus, which is every USB
    /// driver's; a PCI function never leaves it in this version.
    /// </summary>
    internal bool IsRemovable => _context is UsbDeviceContext;

    /// <summary>The PCI function behind the disk, or null for a USB driver's.</summary>
    internal PciFunction? Function => (_context as PciDeviceContext)?.Function;

    /// <summary>True once the kit withdrew the disk; every I/O through it throws.</summary>
    internal bool IsWithdrawn => Volatile.Read(ref _state) == Withdrawn;

    /// <summary>The driver's name for the disk, read once when it was published.</summary>
    public string Name { get; }

    /// <summary>The driver's block count.</summary>
    public ulong BlockCount => _device.BlockCount;

    /// <summary>The driver's block size.</summary>
    public ulong BlockSize => _device.BlockSize;

    /// <summary>Creates the disk <paramref name="context"/>'s driver publishes. Thread context.</summary>
    /// <param name="context">The binding that published it, which names its log lines and tells where it sits.</param>
    /// <param name="device">The driver's disk.</param>
    internal PublishedBlockDevice(DeviceContext context, IBlockDevice device)
    {
        _context = context;
        _device = device;
        Name = device.Name;
    }

    /// <summary>Reads through the driver's disk while it is live.</summary>
    /// <exception cref="IOException">The disk is not live: it was withdrawn, or never delivered.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfNotLive();
        _device.ReadBlock(blockNo, blockCount, data);
    }

    /// <summary>Writes through the driver's disk while it is live.</summary>
    /// <exception cref="IOException">The disk is not live: it was withdrawn, or never delivered.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfNotLive();
        _device.WriteBlock(blockNo, blockCount, data);
    }

    /// <summary>Flushes the driver's disk while it is live.</summary>
    /// <exception cref="IOException">The disk is not live: it was withdrawn, or never delivered.</exception>
    public void Flush()
    {
        ThrowIfNotLive();
        _device.Flush();
    }

    /// <summary>
    /// Makes a queued disk live, as the kit delivers it to the storage
    /// manager: from then on its I/O reaches the driver, which the manager's
    /// partition scan needs right away.
    /// </summary>
    /// <returns>False when the disk was dropped or withdrawn first, and must not be delivered.</returns>
    internal bool TryGoLive() => Interlocked.CompareExchange(ref _state, Live, Queued) == Queued;

    /// <summary>
    /// Drops a queued disk with the attempt that published it, which was
    /// declined or failed: it never reaches the storage manager.
    /// </summary>
    internal void Drop() => Interlocked.CompareExchange(ref _state, Gone, Queued);

    /// <summary>
    /// Withdraws the disk whose USB device left the bus, before the kit asks
    /// the storage manager to let go of it: an I/O that starts from here on
    /// throws, even meanwhile. One already in the driver finishes there, and
    /// fails on its own as the device is gone.
    /// </summary>
    internal void Withdraw() => Volatile.Write(ref _state, Withdrawn);

    private void ThrowIfNotLive()
    {
        int state = Volatile.Read(ref _state);
        if (state == Live)
        {
            return;
        }

        throw new IOException(state == Withdrawn
            ? $"{Name} was withdrawn: its device left the bus."
            : $"{Name} is not live: the kit never delivered it.");
    }
}
