// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// What one addressed device needs: the control endpoint's ED and its
/// four TD slots (SETUP, DATA, STATUS and the dummy), one more pool slot
/// that holds the 8-byte SETUP packet, the bounce page of a control data
/// stage, and the event its control transfers wait on. Allocated once on
/// the host's binding, never freed early, and pooled by
/// <see cref="OhciState"/>: a released set goes back to the free list and
/// the next address path takes it, with a fresh <see cref="OhciDevice"/>
/// over it.
/// </summary>
internal sealed class OhciDeviceMemory : OhciDescriptorSet
{
    /// <summary>Takes the slots, the page and the event the host allocated. Thread context.</summary>
    /// <param name="endpoint">The control ED's slot.</param>
    /// <param name="transfers">Four TD slots.</param>
    /// <param name="setup">The slot the SETUP packet is written into.</param>
    /// <param name="controlBuffer">One page: the data stage bounce buffer.</param>
    /// <param name="controlEvent">The event a control transfer completion signals.</param>
    public OhciDeviceMemory(OhciDescriptorSlot endpoint, OhciDescriptorSlot[] transfers, OhciDescriptorSlot setup, DmaBuffer controlBuffer, DeviceEvent controlEvent)
        : base(endpoint, transfers)
    {
        SetupSlot = setup;
        ControlBuffer = controlBuffer;
        ControlEvent = controlEvent;
    }

    /// <summary>The pool slot holding the SETUP packet of the transfer in flight.</summary>
    public OhciDescriptorSlot SetupSlot { get; }

    /// <summary>The bounce page of a control transfer's data stage.</summary>
    public DmaBuffer ControlBuffer { get; }

    /// <summary>The event a control transfer's completion signals, created once per set.</summary>
    public DeviceEvent ControlEvent { get; }

    /// <summary>A control transfer without data takes a SETUP, a STATUS and the dummy.</summary>
    protected override int MinimumUsableSlots => OhciProtocol.MinimumDeviceTransferSlots;
}
