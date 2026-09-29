// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// What a transport driver implements over its registers so the kit's
/// <see cref="VirtioAccess"/> can run the status handshake, the feature
/// negotiation and the virtqueues once, for every transport. The kit calls
/// these members; a leaf driver never sees the transport, only the access.
/// A transport driver in Cosmos.Kernel.Drivers derives from this, so the
/// constructor is protected and the members are public abstract. The
/// execution context of each member is in its summary.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class VirtioTransport
{
    /// <summary>For the derived transport.</summary>
    protected VirtioTransport()
    {
    }

    /// <summary>The device type the transport read from the function or the slot. Any context.</summary>
    public abstract VirtioDeviceType DeviceType { get; }

    /// <summary>
    /// True for a virtio 1.x transport (PCI, MMIO version 2), where the
    /// FEATURES_OK step exists; false for legacy MMIO (version 1), where
    /// it does not. Any context.
    /// </summary>
    public abstract bool SupportsFeaturesOk { get; }

    /// <summary>
    /// How many interrupt entries the transport delivers to
    /// <see cref="VirtioAccess.Dispatch"/>: the MSI-X messages it connected
    /// over PCI, one for the MMIO line, 0 when the device has no interrupt
    /// path and the leaf polls. Read by the access at each handshake start
    /// and at each queue creation, so a transport sets it before publishing
    /// the node. Any context.
    /// </summary>
    public abstract int InterruptEntryCount { get; }

    /// <summary>Reads the device status register. Thread context.</summary>
    public abstract byte ReadStatus();

    /// <summary>Writes the device status register. Thread context.</summary>
    /// <param name="status">The whole register.</param>
    public abstract void WriteStatus(byte status);

    /// <summary>Reads both 32-bit halves of the device features. Thread context.</summary>
    public abstract ulong ReadDeviceFeatures();

    /// <summary>Writes both 32-bit halves of the driver features. Thread context.</summary>
    /// <param name="features">The features the driver accepts.</param>
    public abstract void WriteDriverFeatures(ulong features);

    /// <summary>Reads the largest size the device supports for a queue: 0 when the queue does not exist. Thread context.</summary>
    /// <param name="index">The queue index.</param>
    public abstract ushort ReadQueueMaxSize(ushort index);

    /// <summary>True when the queue is already enabled: a non-zero PFN on legacy MMIO, the ready or enable bit set on a modern transport. Thread context.</summary>
    /// <param name="index">The queue index.</param>
    public abstract bool IsQueueReady(ushort index);

    /// <summary>
    /// Programs the ring addresses (or the PFN and the alignment on legacy
    /// MMIO) and enables the queue. Thread context.
    /// </summary>
    /// <param name="index">The queue index.</param>
    /// <param name="layout">Where the kit placed the rings.</param>
    /// <returns>False when the transport refuses the index or the layout; the kit then frees the rings through the binding's unwind.</returns>
    public abstract bool ActivateQueue(ushort index, in VirtqueueLayout layout);

    /// <summary>The doorbell: tells the device the available ring of the queue has new buffers. Interrupt context allowed; allocation-free.</summary>
    /// <param name="index">The queue index.</param>
    public abstract void NotifyQueue(ushort index);

    /// <summary>
    /// Reads and, where the transport is level-signalled, acknowledges the
    /// interrupt status. A message-signalled transport that cannot read a
    /// per-delivery status returns <see cref="VirtioInterruptStatus.Queue"/>
    /// and <see cref="VirtioInterruptStatus.Config"/> together. Interrupt
    /// context; allocation-free.
    /// </summary>
    public abstract VirtioInterruptStatus ReadAndAcknowledgeInterrupt();

    /// <summary>
    /// Tells the device which interrupt entry signals a configuration
    /// change: PCI writes the MSI-X configuration vector and reads it back,
    /// MMIO does nothing and returns true. Thread context.
    /// </summary>
    /// <param name="entry">The entry, from 0.</param>
    /// <returns>False when the device refused the entry (PCI answers NO_VECTOR).</returns>
    public abstract bool AssignConfigInterrupt(int entry);

    /// <summary>
    /// Tells the device which interrupt entry signals a queue: PCI writes
    /// the queue's MSI-X vector and reads it back, MMIO does nothing and
    /// returns true. Thread context.
    /// </summary>
    /// <param name="index">The queue index.</param>
    /// <param name="entry">The entry, from 0.</param>
    /// <returns>False when the device refused the entry (PCI answers NO_VECTOR).</returns>
    public abstract bool AssignQueueInterrupt(ushort index, int entry);

    /// <summary>What the transport programs right after the status read back as 0: the guest page size on legacy MMIO, nothing elsewhere. Thread context.</summary>
    public abstract void AfterReset();

    /// <summary>Reads one byte of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public abstract byte ReadConfig8(uint offset);

    /// <summary>Reads one word of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public abstract ushort ReadConfig16(uint offset);

    /// <summary>Reads one dword of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public abstract uint ReadConfig32(uint offset);

    /// <summary>Writes one byte of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    /// <param name="value">The value.</param>
    public abstract void WriteConfig8(uint offset, byte value);
}
