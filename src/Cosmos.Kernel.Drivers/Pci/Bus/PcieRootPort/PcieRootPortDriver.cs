// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers.Pci.Bus.PcieRootPort;

/// <summary>
/// A PCI Express root port (or downstream port) with a hot-plug slot:
/// publishes the functions behind it at boot, follows the slot's presence
/// detect and attention button events on a driver thread, powers the slot
/// on for a device that arrives (placing its registers inside the port's
/// windows through <see cref="PciAccess.TryDescribeChild"/>) and off for
/// one that is pulled, and retracts the nodes of a device that left. The
/// PCI host driver does not walk the bus behind such a slot. The slot's
/// events come through the port's message interrupt where the platform
/// routes it, else a periodic poll of Slot Status; without an interrupt and
/// without a timer the slot thread's one-second wait is the poll; without a
/// scheduler the cold-plugged functions are still published and nothing
/// follows afterwards. <see cref="Probe"/> and <see cref="OnDetach"/> run in
/// thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Pci)]
public sealed class PcieRootPortDriver : Driver
{
    // --- Constants ---

    /// <summary>
    /// The port's message interrupt source: interrupts[1], message 0 of its
    /// one-entry MSI-X table. interrupts[0] is the legacy line, never
    /// requested: the ports' lines collide with other functions' on q35 and
    /// are refused on ARM64.
    /// </summary>
    private const int MessageInterruptIndex = 1;

    /// <summary>How often the kit worker signals the slot thread when the port has no message interrupt, in milliseconds.</summary>
    private const uint SlotPollMilliseconds = 500;

    /// <summary>The slot thread's name, for the log.</summary>
    private const string ThreadName = "pcie-slot";

    /// <summary>
    /// How many times the probe asks for the slot thread. A start fails at
    /// once without a scheduler, but also when the scheduler did not switch
    /// to the new thread within the kit's start window, which a slow
    /// emulated machine early in the boot occasionally misses; the later
    /// attempts cover that.
    /// </summary>
    private const int ThreadStartAttempts = 3;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: 0x06, subclass: 0x04),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(PcieRootPortDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Takes a PCI-to-PCI bridge that is a hot-plug slot: quiets and clears
    /// the slot, turns the port's decoding and bus mastering on, connects the
    /// message interrupt or the periodic poll, publishes the functions a
    /// powered-on slot holds at boot, enables the slot's events and starts
    /// the slot thread. Thread context on the kit worker; the children are
    /// offered once this probe returns.
    /// </summary>
    /// <param name="binding">The port's node and the kit facilities for it.</param>
    /// <returns>Bound with the slot followed; declined for a bridge that is no hot-plug slot or has no usable secondary bus.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The access.
        if (!binding.Node.TryGetAccess(out PciAccess? pci))
        {
            return ProbeResult.Declined("the node carries no PCI access");
        }

        // 2. A hot-plug slot.
        if (!pci.IsHotPlugSlot)
        {
            return ProbeResult.Declined("not a hot-plug slot");
        }

        // 3. A secondary bus the host decodes.
        PciIdentity identity = (PciIdentity)binding.Node.Identity;
        byte secondary = pci.SecondaryBus;
        if (secondary == 0 || secondary <= identity.Bus)
        {
            return ProbeResult.Declined("no secondary bus assigned to the slot");
        }

        if (binding.Node.Parent is { } parent && parent.TryGetAccess(out PciHostAccess? host) && secondary > host.EndBus)
        {
            return ProbeResult.Declined("no secondary bus assigned to the slot");
        }

        // 4. The slot's and the link's capabilities.
        byte cap = pci.FindCapability(PcieSlotProtocol.ExpressCapabilityId);
        uint slotCapabilities = pci.ReadConfig32((ushort)(cap + PcieSlotProtocol.SlotCapabilitiesOffset));
        uint slotNumber = slotCapabilities >> PcieSlotProtocol.SlotCapabilityNumberShift;
        bool hasPowerController = (slotCapabilities & PcieSlotProtocol.SlotCapabilityPowerController) != 0;
        bool reportsCommandCompleted = (slotCapabilities & PcieSlotProtocol.SlotCapabilityNoCommandCompleted) == 0;
        bool hasDataLinkReporting = (pci.ReadConfig32((ushort)(cap + PcieSlotProtocol.LinkCapabilitiesOffset)) & PcieSlotProtocol.LinkCapabilityDataLinkActiveReporting) != 0;

        // 5. The state and its event.
        PcieRootPortState state = new(binding, pci, cap, slotNumber, secondary, hasPowerController, hasDataLinkReporting, reportsCommandCompleted);
        binding.DriverState = state;
        state.Event = binding.CreateEvent();

        // 6. Quiet and clear: no event enabled (and the command completed
        //    before the next Slot Control write), the change bits of
        //    firmware's time cleared. Only the bits that are set are
        //    written back: QEMU restores every change bit when a write
        //    clears one that was not set.
        state.ClearEnables();
        state.WaitCommandCompleted(stopWhenDetaching: false);
        state.WriteSlotStatus((ushort)(state.ReadSlotStatus() & PcieSlotProtocol.SlotStatusEvents));

        // 7. The port's Command: memory decoding for the MSI-X table in
        //    BAR0, and bus mastering, which the kit's quiesce cleared and
        //    without which a bridge forwards no upstream memory request (the
        //    DMA of the functions behind it). Then the message interrupt,
        //    else the periodic poll.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);
        bool connected = binding.Node.Interrupts.Count > MessageInterruptIndex
            && binding.TryRequestInterrupt(binding.Node.Interrupts[MessageInterruptIndex], state.OnInterrupt, out _);
        state.HasInterrupt = connected;
        if (!connected)
        {
            WorkItem poll = binding.CreateWorkItem(state.Poll);
            state.IsPolling = binding.TrySchedulePeriodic(SlotPollMilliseconds, poll);
            if (!state.IsPolling)
            {
                binding.Log("slot events polled by the thread alone (no interrupt and no timer)");
            }
        }

        // 8. The cold-plugged device: a populated slot resets powered on,
        //    and its functions are published from here, offered after this
        //    probe. Powered off or empty: the thread's first pass takes the
        //    arrival path.
        ushort status = state.ReadSlotStatus();
        state.IsPresent = (status & PcieSlotProtocol.SlotStatusPresent) != 0;
        state.IsPoweredOn = !hasPowerController || (state.ReadSlotControl() & PcieSlotProtocol.SlotControlPowerOff) == 0;
        if (state.IsPresent && state.IsPoweredOn)
        {
            state.PublishChildren();
        }

        // 9. The events: presence, the attention button and, when the link
        //    reports it, Data Link Layer State Changed. Command Completed
        //    stays off; the thread polls it after its own writes.
        ushort enables = (ushort)(PcieSlotProtocol.SlotControlHotPlugInterruptEnable
            | PcieSlotProtocol.SlotControlPresenceChangedEnable
            | PcieSlotProtocol.SlotControlAttentionButtonEnable
            | (hasDataLinkReporting ? PcieSlotProtocol.SlotControlDataLinkChangedEnable : 0));
        state.WriteSlotControl((ushort)(state.ReadSlotControl() | enables));
        state.WaitCommandCompleted(stopWhenDetaching: false);

        // 10. The thread; without a scheduler, the boot-time publish only.
        bool started = false;
        for (int attempt = 0; attempt < ThreadStartAttempts && !started; attempt++)
        {
            started = binding.TryStartThread(ThreadName, state.ThreadMain, out _);
        }

        state.HotPlugRunning = started;
        if (!state.HotPlugRunning)
        {
            binding.Log("hot-plug off (no scheduler)");
        }

        // 11.
        string events = connected ? "message interrupt" : state.IsPolling ? $"polled every {SlotPollMilliseconds} ms" : "thread polled";
        binding.Log($"slot {slotNumber}, bus {secondary}, {(state.IsPresent ? "occupied" : "empty")}, {(state.IsPoweredOn ? "powered on" : "powered off")}, {events}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// With the port still there, clears the slot's event enables, leaving
    /// the indicators and the power bit as they are, and waits for the
    /// command to complete. Thread context on the
    /// kit worker; the kit tore the children down, disconnected the
    /// interrupt, cancelled the poll and joined the slot thread before
    /// this, and quiesces the port function afterwards.
    /// </summary>
    /// <param name="binding">The port's binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not PcieRootPortState state || !reason.HardwarePresent)
        {
            return;
        }

        state.ClearEnables();
        state.WaitCommandCompleted(stopWhenDetaching: false);
    }
}
