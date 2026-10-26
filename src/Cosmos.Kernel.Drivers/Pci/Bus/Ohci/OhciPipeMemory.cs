// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// What one open pipe needs: its ED, its TD slots (one per buffer and the
/// dummy for an interrupt pipe, one transfer and the dummy for a bulk
/// pipe), its data buffer (one page split into report buffers, or the
/// 64 KiB bounce of a bulk pipe) and, for a bulk pipe, the completion
/// event. Allocated once on the host's binding, never freed early, and
/// pooled by <see cref="OhciState"/> per kind: a closed pipe's set goes
/// back to its free list and the next open takes it, resets it and puts a
/// fresh <see cref="OhciInterruptPipe"/> or <see cref="OhciBulkPipe"/>
/// over it.
/// </summary>
internal sealed class OhciPipeMemory : OhciDescriptorSet
{
    /// <summary>Takes the slots, the buffer and the event the host allocated. Thread context.</summary>
    /// <param name="endpoint">The ED's slot.</param>
    /// <param name="transfers">The TD slots.</param>
    /// <param name="buffer">The data buffer: one page for an interrupt pipe, 64 KiB for a bulk pipe.</param>
    /// <param name="deviceEvent">The completion event of a bulk pipe; null for an interrupt pipe.</param>
    public OhciPipeMemory(OhciDescriptorSlot endpoint, OhciDescriptorSlot[] transfers, DmaBuffer buffer, DeviceEvent? deviceEvent)
        : base(endpoint, transfers)
    {
        Buffer = buffer;
        BufferLength = buffer.Length;
        Event = deviceEvent;
    }

    /// <summary>The data buffer.</summary>
    public DmaBuffer Buffer { get; }

    /// <summary>Bytes in <see cref="Buffer"/>.</summary>
    public int BufferLength { get; }

    /// <summary>The event a bulk transfer's completion signals; null on an interrupt pipe's set.</summary>
    public DeviceEvent? Event { get; }

    /// <summary>True for a bulk set: it carries an event and the 64 KiB bounce.</summary>
    public bool IsBulk => Event is not null;
}
