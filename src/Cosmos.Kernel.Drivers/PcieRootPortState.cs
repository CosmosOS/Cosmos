// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="PcieRootPortDriver"/> holds for one bound PCI
/// Express port with a hot-plug slot, hung off
/// <see cref="DeviceBinding.DriverState"/>: the slot's registers, the nodes
/// the port published for the functions behind it, the event the
/// <c>pcie-slot</c> thread waits on, and the counters the suites read. The
/// slot's registers are touched by the probe (before the thread exists),
/// the thread, and the detach hook (after the kit joined the thread), never
/// by two at once; the interrupt handler and the periodic poll only signal
/// the event, so no lock is needed.
/// </summary>
public sealed class PcieRootPortState
{
    // --- Constants ---

    /// <summary>Highest device number on a bus: the device field is five bits.</summary>
    private const byte MaxDevice = 31;

    /// <summary>Highest function number of a device: the function field is three bits.</summary>
    private const byte MaxFunction = 7;

    /// <summary>Base class of the bridge devices.</summary>
    private const byte BridgeClassCode = 0x06;

    /// <summary>Bridge subclass of a PCI-to-PCI bridge, a switch's upstream and downstream ports among them.</summary>
    private const byte PciToPciBridgeSubclass = 0x04;

    /// <summary>Header type of a PCI-to-PCI bridge.</summary>
    private const byte PciToPciBridgeHeaderType = 1;

    /// <summary>Milliseconds per second, for the deadline in <see cref="Stopwatch"/> ticks.</summary>
    private const long MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly PciAccess _port;
    private readonly byte _capability;
    private readonly bool _reportsCommandCompleted;

    /// <summary>The nodes this port published and has not retracted, in tree order. The probe (before the thread starts) and the thread alone touch it.</summary>
    private readonly List<DeviceNode> _children = new();

    private volatile bool _isPresent;
    private volatile bool _isPoweredOn;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile bool _hotPlugRunning;
    private volatile int _childCount;
    private volatile int _arrivalCount;
    private volatile int _removalCount;
    private volatile int _interruptCount;

    /// <summary>True once the thread logged that a present slot answered with no function, until the slot reads empty again.</summary>
    private bool _arrivalLogged;

    /// <summary>
    /// True once the slot was powered off on an attention button request
    /// with the card still present (a slot whose presence detect does not
    /// follow the power): the slot stays off until the card is pulled, the
    /// button is pressed again or presence changes (PCI Express Base 6.7.1.5
    /// and 6.7.3). The thread alone touches it.
    /// </summary>
    private bool _awaitingPull;

    // --- Constructor ---

    /// <summary>Takes the port and what the probe read from its slot. Thread context, from the probe.</summary>
    /// <param name="binding">The port's binding, for the log, the waits, and the children it publishes.</param>
    /// <param name="port">The port function's access: its slot registers and the describe of the functions behind it.</param>
    /// <param name="capability">The offset of the port's PCI Express capability.</param>
    /// <param name="slotNumber">The Physical Slot Number from Slot Capabilities.</param>
    /// <param name="secondaryBus">The port's secondary bus, where the slot's functions sit.</param>
    /// <param name="hasPowerController">True when the slot has a power controller.</param>
    /// <param name="hasDataLinkReporting">True when the port reports Data Link Layer Link Active.</param>
    /// <param name="reportsCommandCompleted">True when the port raises Command Completed after a Slot Control write (No Command Completed Support clear).</param>
    internal PcieRootPortState(DeviceBinding binding, PciAccess port, byte capability, uint slotNumber, byte secondaryBus, bool hasPowerController, bool hasDataLinkReporting, bool reportsCommandCompleted)
    {
        _binding = binding;
        _port = port;
        _capability = capability;
        _reportsCommandCompleted = reportsCommandCompleted;
        SlotNumber = slotNumber;
        SecondaryBus = secondaryBus;
        HasPowerController = hasPowerController;
        HasDataLinkReporting = hasDataLinkReporting;
    }

    // --- Properties the suites read ---

    /// <summary>The Physical Slot Number Slot Capabilities reports. Any context.</summary>
    public uint SlotNumber { get; }

    /// <summary>The port's secondary bus, where the functions behind the slot sit. Any context.</summary>
    public byte SecondaryBus { get; }

    /// <summary>True when the slot has a power controller, which the port powers on for an arrival and off for a removal. Any context.</summary>
    public bool HasPowerController { get; }

    /// <summary>Presence Detect State as last read by the probe or the thread. Any context.</summary>
    public bool IsPresent
    {
        get => _isPresent;
        internal set => _isPresent = value;
    }

    /// <summary>Whether the slot is powered on, as last read or written by the probe or the thread; always true without a power controller. Any context.</summary>
    public bool IsPoweredOn
    {
        get => _isPoweredOn;
        internal set => _isPoweredOn = value;
    }

    /// <summary>True when the slot's events come through the port's message interrupt. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when the slot's events come from a periodic poll of Slot Status on the kit worker. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True when the <c>pcie-slot</c> thread started, so devices plugged or pulled after the probe are followed. Any context.</summary>
    public bool HotPlugRunning
    {
        get => _hotPlugRunning;
        internal set => _hotPlugRunning = value;
    }

    /// <summary>The nodes this port published and has not retracted. Any context.</summary>
    public int ChildCount => _childCount;

    /// <summary>Devices the thread powered on and published after the probe. Any context.</summary>
    public int ArrivalCount => _arrivalCount;

    /// <summary>Devices the thread retracted, by the attention button or a surprise removal. Any context.</summary>
    public int RemovalCount => _removalCount;

    /// <summary>Times the port's message interrupt fired. Any context.</summary>
    public int InterruptCount => _interruptCount;

    // --- Internal properties ---

    /// <summary>True when the port reports Data Link Layer Link Active, so the probe enables Data Link Layer State Changed. Any context.</summary>
    internal bool HasDataLinkReporting { get; }

    /// <summary>The event the handler and the poll signal and the thread waits on: set once by the probe before the thread starts; read in any context, including the interrupt handler.</summary>
    internal DeviceEvent? Event { get; set; }

    // --- The handler and the poll ---

    /// <summary>
    /// The port's message interrupt handler: counts and signals the slot
    /// thread's event, nothing else. Interrupt context; allocation-free;
    /// touches no register.
    /// </summary>
    /// <param name="context">The kit's context for the handler.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        _interruptCount++;
        DeviceEvent? evt = Event;
        if (evt is not null)
        {
            context.Signal(evt);
        }
    }

    /// <summary>
    /// The periodic poll's work item: signals the slot thread's event,
    /// nothing else, so it can never throw and be cancelled by the kit.
    /// Thread context on the kit worker.
    /// </summary>
    internal void Poll() => Event?.Signal();

    // --- The thread ---

    /// <summary>
    /// The <c>pcie-slot</c> thread: until the binding detaches, one pass
    /// over the slot inside a try/catch that logs the exception (so one
    /// failed event never silences the slot), then a wait of up to a second
    /// on the event, which returns at once once teardown cancelled it.
    /// Thread context, this thread.
    /// </summary>
    internal void ThreadMain()
    {
        while (!_binding.IsDetaching)
        {
            try
            {
                HandleSlot();
            }
            catch (Exception exception)
            {
                _binding.Log($"slot {SlotNumber}: {exception.GetType().Name}: {exception.Message}");
            }

            _binding.Wait(Event!, PcieSlotProtocol.SlotWaitMilliseconds);
        }
    }

    // --- The probe's and the thread's steps ---

    /// <summary>
    /// Describes device 0 of the secondary bus (placing the registers
    /// nobody assigned inside the port's windows) and publishes it, then
    /// functions 1 to 7 when function 0's header says multi-function; a
    /// published bridge that is no hot-plug slot (the upstream port of a
    /// switch present at boot) has its bus walked as well.
    /// Thread context: the probe (the offers queue behind it) or the slot
    /// thread (each offer completes before the publish returns).
    /// </summary>
    /// <returns>The number of functions published.</returns>
    internal int PublishChildren()
    {
        if (!_port.TryDescribeChild(0, 0, assignResources: true, out PciFunctionDescription first))
        {
            return 0;
        }

        int count = Publish(first);
        for (byte function = 1; first.IsMultiFunction && function <= MaxFunction; function++)
        {
            if (_port.TryDescribeChild(0, function, assignResources: true, out PciFunctionDescription description))
            {
                count += Publish(description);
            }
        }

        return count;
    }

    /// <summary>
    /// Retracts every node this port published, the last one first. The
    /// list is emptied first, so <see cref="ChildCount"/> is 0 before the
    /// first retraction; a node the kit did not retract (the port's own
    /// teardown released the wait, or the kit refused) is put back with the
    /// ones not yet reached, in tree order. Thread context, the slot thread.
    /// </summary>
    /// <param name="hardwarePresent">Whether the functions are still there to be quiesced.</param>
    /// <param name="retracted">The number of nodes retracted.</param>
    /// <returns>True when every node reached <see cref="NodeState.Retracted"/>.</returns>
    internal bool RetractChildren(bool hardwarePresent, out int retracted)
    {
        DeviceNode[] snapshot = new DeviceNode[_children.Count];
        for (int i = 0; i < snapshot.Length; i++)
        {
            snapshot[i] = _children[_children.Count - 1 - i];
        }

        _children.Clear();
        _childCount = 0;
        retracted = 0;
        for (int i = 0; i < snapshot.Length; i++)
        {
            DeviceNode node = snapshot[i];
            _binding.RetractChild(node, hardwarePresent);
            if (node.State != NodeState.Retracted)
            {
                // The snapshot runs last to first: this node and the ones
                // after it in the snapshot go back in tree order.
                for (int j = snapshot.Length - 1; j >= i; j--)
                {
                    _children.Add(snapshot[j]);
                }

                _childCount = _children.Count;
                return false;
            }

            retracted++;
        }

        return true;
    }

    /// <summary>
    /// Powers the slot on with the power indicator on, waits for Command
    /// Completed, then lets the device settle. Without a power controller
    /// only the indicator is written and nothing settles: the slot was
    /// never off. The settle is skipped once the port is being torn down.
    /// Thread context, the slot thread.
    /// </summary>
    internal void PowerOn()
    {
        ushort control = ReadSlotControl();
        ushort powerBits = HasPowerController ? PcieSlotProtocol.SlotControlPowerOff : (ushort)0;
        control = (ushort)((control & ~(powerBits | PcieSlotProtocol.SlotControlPowerIndicatorMask)) | PcieSlotProtocol.SlotControlPowerIndicatorOn);
        WriteSlotControl(control);
        WaitCommandCompleted(stopWhenDetaching: true);
        if (HasPowerController && !_binding.IsDetaching)
        {
            _binding.Sleep(PcieSlotProtocol.PowerSettleMilliseconds);
        }

        IsPoweredOn = true;
    }

    /// <summary>
    /// Powers the slot off with the power indicator off, waits for Command
    /// Completed, then re-reads Presence Detect State (QEMU finishes a
    /// device_del inside the write that powered the slot off). Without a
    /// power controller only the indicator is written and the slot stays
    /// powered, as <see cref="IsPoweredOn"/> says. Thread context, the slot
    /// thread.
    /// </summary>
    internal void PowerOff()
    {
        ushort control = ReadSlotControl();
        ushort powerBits = HasPowerController ? PcieSlotProtocol.SlotControlPowerOff : (ushort)0;
        control = (ushort)((control & ~PcieSlotProtocol.SlotControlPowerIndicatorMask) | powerBits | PcieSlotProtocol.SlotControlPowerIndicatorOff);
        WriteSlotControl(control);
        WaitCommandCompleted(stopWhenDetaching: true);
        IsPoweredOn = !HasPowerController;
        IsPresent = (ReadSlotStatus() & PcieSlotProtocol.SlotStatusPresent) != 0;
    }

    /// <summary>
    /// Clears every event enable in Slot Control, leaving the indicators
    /// and the power bit as they are. Thread context: the probe, or the
    /// detach hook once the thread was joined.
    /// </summary>
    internal void ClearEnables() =>
        WriteSlotControl((ushort)(ReadSlotControl() & ~PcieSlotProtocol.SlotControlEnableMask));

    /// <summary>Reads Slot Control. Thread context; allocation-free.</summary>
    internal ushort ReadSlotControl() => _port.ReadConfig16((ushort)(_capability + PcieSlotProtocol.SlotControlOffset));

    /// <summary>Writes Slot Control. Thread context; allocation-free.</summary>
    /// <param name="value">The new value.</param>
    internal void WriteSlotControl(ushort value) => _port.WriteConfig16((ushort)(_capability + PcieSlotProtocol.SlotControlOffset), value);

    /// <summary>Reads Slot Status. Thread context; allocation-free.</summary>
    internal ushort ReadSlotStatus() => _port.ReadConfig16((ushort)(_capability + PcieSlotProtocol.SlotStatusOffset));

    /// <summary>Writes Slot Status: a one clears that write-one-to-clear bit. Thread context; allocation-free.</summary>
    /// <param name="value">The bits to clear.</param>
    internal void WriteSlotStatus(ushort value) => _port.WriteConfig16((ushort)(_capability + PcieSlotProtocol.SlotStatusOffset), value);

    // --- Private helpers ---

    /// <summary>
    /// One pass over the slot: clears the change bits it saw before acting,
    /// then handles an arrival, a removal request by the attention button,
    /// a surprise removal, or an empty slot left powered on. Thread
    /// context, the slot thread.
    /// </summary>
    private void HandleSlot()
    {
        // 1. Write-one-to-clear what was seen, before acting, so an event
        //    raised during the handling is not lost.
        ushort status = ReadSlotStatus();
        ushort events = (ushort)(status & PcieSlotProtocol.SlotStatusEvents);
        if (events != 0)
        {
            WriteSlotStatus(events);
        }

        // 2. The slot as it is now.
        bool present = (status & PcieSlotProtocol.SlotStatusPresent) != 0;
        bool poweredOn = !HasPowerController || (ReadSlotControl() & PcieSlotProtocol.SlotControlPowerOff) == 0;
        IsPresent = present;
        IsPoweredOn = poweredOn;
        if (!present)
        {
            _arrivalLogged = false;
            _awaitingPull = false;
        }

        // 3. Decide. QEMU raises Attention Button Pressed with Presence
        //    Detect Changed when a device is added to a powered-off slot,
        //    so a button with no children is an arrival.
        if (present && _children.Count == 0)
        {
            if (_awaitingPull)
            {
                // Powered off on request with the card still in: off until
                // the button is pressed again or presence changes, never
                // on the next pass alone.
                if ((events & (PcieSlotProtocol.SlotStatusAttentionButton | PcieSlotProtocol.SlotStatusPresenceChanged)) == 0)
                {
                    return;
                }

                _awaitingPull = false;
            }

            if (!poweredOn)
            {
                PowerOn();
            }

            int published = PublishChildren();
            if (published > 0)
            {
                _arrivalCount++;
                _binding.Log($"slot {SlotNumber}: device arrived, {published} functions published");
            }
            else if (!_arrivalLogged)
            {
                _arrivalLogged = true;
                _binding.Log($"slot {SlotNumber}: present but no function answered");
            }

            return;
        }

        if (present && _children.Count > 0 && (events & PcieSlotProtocol.SlotStatusAttentionButton) != 0)
        {
            // A removal request (QEMU's device_del on a powered-on slot):
            // the functions are quiesced while still there, then the slot
            // is powered off, which completes the removal.
            if (!RetractChildren(hardwarePresent: true, out int count))
            {
                return;
            }

            PowerOff();
            _awaitingPull = IsPresent;
            _removalCount++;
            _binding.Log($"slot {SlotNumber}: attention button, {count} functions retracted, slot powered off");
            return;
        }

        if (!present && _children.Count > 0)
        {
            // A surprise removal: the functions are gone already.
            if (!RetractChildren(hardwarePresent: false, out int count))
            {
                return;
            }

            if (poweredOn)
            {
                PowerOff();
            }

            _removalCount++;
            _binding.Log($"slot {SlotNumber}: device removed, {count} functions retracted");
            return;
        }

        if (!present && _children.Count == 0 && poweredOn && HasPowerController)
        {
            // An empty slot left powered on: off, so the next arrival takes
            // the power-on path QEMU expects.
            PowerOff();
            _binding.Log($"slot {SlotNumber}: empty slot powered off");
        }
    }

    /// <summary>
    /// Logs the registers the kit could not place, then publishes the
    /// function as a child of the port and lists it; for a PCI-to-PCI
    /// bridge that is no hot-plug slot, the functions on its bus follow.
    /// Thread context: the probe or the slot thread.
    /// </summary>
    /// <param name="description">What the describe built for the function.</param>
    /// <returns>The number of functions published: this one and those behind it.</returns>
    private int Publish(PciFunctionDescription description)
    {
        ReadOnlySpan<PciBar> bars = description.Access.Bars;
        for (int i = 0; i < bars.Length; i++)
        {
            PciBar bar = bars[i];
            if (!bar.IsAssigned && bar.Length != 0)
            {
                _binding.Log($"{description.Identity.Address} bar {bar.Index} ({bar.Length} bytes{(bar.IsIo ? ", I/O" : "")}) not placed: no room in the port's windows");
            }
        }

        _children.Add(_binding.PublishChild(description.Identity, description.Resources, description.Interrupts, description.Access));
        _childCount = _children.Count;

        PciIdentity identity = description.Identity;
        if (identity.ClassCode != BridgeClassCode
            || identity.Subclass != PciToPciBridgeSubclass
            || identity.HeaderType != PciToPciBridgeHeaderType
            || description.Access.IsHotPlugSlot)
        {
            // A hot-plug slot behind this one has its own port driver.
            return 1;
        }

        return 1 + PublishBehind(description.Access);
    }

    /// <summary>
    /// Walks the bus behind a bridge this port published that is no
    /// hot-plug slot, the host's walk rule (every device, functions 1 to 7
    /// when function 0 says multi-function), and publishes each function as
    /// a child of this port, so the slot's removal retracts them too. The
    /// registers stay as firmware assigned them: a bus only firmware
    /// numbered is walked. Thread context: the probe or the slot thread.
    /// </summary>
    /// <param name="bridge">The bridge's access.</param>
    /// <returns>The number of functions published.</returns>
    private int PublishBehind(PciAccess bridge)
    {
        int count = 0;
        for (byte device = 0; device <= MaxDevice; device++)
        {
            if (!bridge.TryDescribeChild(device, 0, assignResources: false, out PciFunctionDescription first))
            {
                continue;
            }

            count += Publish(first);
            for (byte function = 1; first.IsMultiFunction && function <= MaxFunction; function++)
            {
                if (bridge.TryDescribeChild(device, function, assignResources: false, out PciFunctionDescription description))
                {
                    count += Publish(description);
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Waits for Command Completed after a Slot Control write and clears
    /// it, so the next write is never issued while the port still works on
    /// this one; logs a miss after <see cref="PcieSlotProtocol.CommandCompletedTimeoutMilliseconds"/>
    /// and carries on. Returns at once on a port with No Command Completed
    /// Support. Thread context: the probe, the slot thread, or the detach
    /// hook.
    /// </summary>
    /// <param name="stopWhenDetaching">True on the slot thread: the wait ends as soon as the port's teardown begins, so the kit's join of the thread is not outlasted.</param>
    internal void WaitCommandCompleted(bool stopWhenDetaching)
    {
        if (!_reportsCommandCompleted)
        {
            return;
        }

        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * PcieSlotProtocol.CommandCompletedTimeoutMilliseconds;
        while (true)
        {
            ushort status = ReadSlotStatus();
            if ((status & PcieSlotProtocol.SlotStatusCommandCompleted) != 0)
            {
                WriteSlotStatus(PcieSlotProtocol.SlotStatusCommandCompleted);
                return;
            }

            if (stopWhenDetaching && _binding.IsDetaching)
            {
                return;
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                _binding.Log($"slot {SlotNumber}: command completed not seen within {PcieSlotProtocol.CommandCompletedTimeoutMilliseconds} ms");
                return;
            }

            _binding.Sleep(PcieSlotProtocol.CommandPollMilliseconds);
        }
    }
}
