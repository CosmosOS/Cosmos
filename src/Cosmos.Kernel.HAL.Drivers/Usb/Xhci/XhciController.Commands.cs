// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Contexts;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

internal sealed partial class XhciController
{
    /// <summary>Budget for one command; Address Device includes the device's SET_ADDRESS (Linux XHCI_CMD_DEFAULT_TIMEOUT).</summary>
    private const long CommandTimeoutUs = 5_000_000;

    /// <summary>Granularity of the synchronous command and transfer waits.</summary>
    private const long WaitPollIntervalUs = 10;

    /// <summary>SET_ADDRESS recovery interval before the next request (USB 2.0 §9.2.6.3).</summary>
    private const uint SetAddressRecoveryMs = 2;

    /// <summary>The device descriptor's first 8 bytes carry bMaxPacketSize0, and any packet size moves them (USB 2.0 §9.6.1).</summary>
    private const int DeviceDescriptorHeadLength = 8;
    private const int MaxPacketSize0Offset = 7;

    /// <summary>GET_DESCRIPTOR (USB 2.0 table 9-4) and the device descriptor type in its wValue's high byte (table 9-5).</summary>
    private const byte GetDescriptorRequest = 0x06;
    private const ushort DeviceDescriptorValue = 0x01 << 8;

    /// <summary>SuperSpeed devices give bMaxPacketSize0 as a power of two; 2^15 is past any valid value.</summary>
    private const int MaxPacketSize0MaxExponent = 15;

    /// <summary>
    /// Controllers after 0.95 take a hub's Slot Context fields through
    /// Configure Endpoint, earlier ones through Evaluate Context (Linux
    /// xhci_update_hub_device).
    /// </summary>
    private const ushort HubConfigureEndpointMinVersion = 0x0096;

    /// <summary>Average TRB length the spec recommends for control endpoints (xHCI 1.2 §4.14.1.1).</summary>
    private const ushort ControlAverageTrbLength = 8;

    // Periodic endpoint interval bounds, in the Endpoint Context's 2^n x 125 µs encoding (xHCI 1.2 §6.2.3.6).
    private const int FullSpeedMinIntervalExponent = 3;
    private const int FullSpeedMaxIntervalExponent = 10;
    private const int HighSpeedMaxInterval = 16;

    /// <summary>
    /// Holds one signal while no command runs, so a wait on it takes the
    /// turn: enumeration issues commands, and so does the recovery of an
    /// endpoint, from whichever thread hit the error.
    /// </summary>
    private readonly DeviceEvent _commandTurn;

    // Synchronous command state, under _eventLock.
    private ulong _pendingCommand;
    private bool _commandCompleted;
    private CompletionCode _commandCode;
    private byte _commandSlotId;

    /// <summary>How long a synchronous wait sleeps between two looks at the event ring.</summary>
    private static TimeSpan WaitPollInterval => TimeSpan.FromMicroseconds(WaitPollIntervalUs);

    /// <inheritdoc />
    protected override UsbHostDevice? AddressDevice(UsbHostDevice? parentHub, byte port, UsbSpeed speed)
    {
        CompletionCode code = ExecuteCommand(0, Trb.TypeField(TrbType.EnableSlotCommand), out byte slotId);
        if (code != CompletionCode.Success || slotId == 0 || slotId >= _devices.Length)
        {
            LogCommandFailure("Enable Slot", code);
            return null;
        }

        XhciDevice device;
        try
        {
            device = XhciDevice.Create(this, _memory, slotId, parentHub as XhciDevice, port, speed, _registers.ContextSize);
        }
        catch (InvalidOperationException exception)
        {
            WriteLog($"slot {slotId}: {exception.Message}");
            ExecuteCommand(0, SlotCommand(TrbType.DisableSlotCommand, slotId), out _);
            return null;
        }

        using (_eventLock.EnterScope())
        {
            _devices[slotId] = device;
            WriteDeviceContextPointer(slotId, device.OutputContextAddress);
        }

        device.ClearInputContext();
        ContextLayout.SetAddFlags(device.InputControlContext, ContextLayout.SlotFlag | ContextLayout.EndpointFlag(XhciDevice.ControlEndpointId));
        ContextLayout.WriteSlot(device.InputSlotContext, device.RouteString, speed, XhciDevice.ControlEndpointId,
            device.RootPort, device.TtHubSlotId, device.TtPortNumber);
        WriteControlEndpoint(device, device.MaxPacketSize0);

        code = ExecuteCommand(device.InputContextAddress, SlotCommand(TrbType.AddressDeviceCommand, slotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Address Device", code);
            ReleaseDevice(device);
            return null;
        }

        DelayMilliseconds(SetAddressRecoveryMs);

        Span<byte> head = stackalloc byte[DeviceDescriptorHeadLength];
        UsbSetupPacket getDescriptor = new(UsbDirection.In, UsbRequestKind.Standard, UsbRecipient.Device, GetDescriptorRequest,
            DeviceDescriptorValue, 0, DeviceDescriptorHeadLength);
        if (ControlTransfer(device, getDescriptor, head, out int received) != UsbTransferStatus.Success || received < DeviceDescriptorHeadLength)
        {
            WriteLog($"slot {slotId}: the addressed device did not return its descriptor");
            ReleaseDevice(device);
            return null;
        }

        int packetSizeField = head[MaxPacketSize0Offset];
        ushort maxPacketSize = speed >= UsbSpeed.Super
            ? (ushort)(packetSizeField <= MaxPacketSize0MaxExponent ? 1 << packetSizeField : 0)
            : (ushort)packetSizeField;
        if (maxPacketSize != 0 && maxPacketSize != device.MaxPacketSize0 && !UpdateControlMaxPacketSize(device, maxPacketSize))
        {
            ReleaseDevice(device);
            return null;
        }

        return device;
    }

    /// <summary>Registers <paramref name="device"/> as a hub in its Slot Context.</summary>
    internal bool ConfigureHub(XhciDevice device, byte portCount, byte thinkTime)
    {
        device.ClearInputContext();
        ContextLayout.SetAddFlags(device.InputControlContext, ContextLayout.SlotFlag);
        ContextLayout.CopySlot(device.OutputSlotContext, device.InputSlotContext);
        ContextLayout.SetHub(device.InputSlotContext, portCount, thinkTime);

        TrbType command = _registers.HciVersion >= HubConfigureEndpointMinVersion
            ? TrbType.ConfigureEndpointCommand
            : TrbType.EvaluateContextCommand;
        CompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(command, device.SlotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Configure hub", code);
            return false;
        }

        return true;
    }

    /// <summary>Adds an interrupt IN endpoint to the device's slot and starts its transfers.</summary>
    internal bool OpenInterruptPipe(XhciDevice device, UsbEndpointInfo endpoint, UsbReportHandler handler)
    {
        if (endpoint.Type != UsbEndpointType.Interrupt || endpoint.Direction != UsbDirection.In)
        {
            WriteLog($"slot {device.SlotId}: endpoint 0x{endpoint.Address:X2} is not an interrupt IN endpoint");
            return false;
        }

        byte endpointId = XhciDevice.EndpointId(endpoint);

        // For a high-speed periodic endpoint, Max Burst is the number of
        // additional transactions per microframe (xHCI 1.2 §6.2.3.4).
        int maxEsitPayload = endpoint.MaxPacketSize * (endpoint.AdditionalTransactions + 1);
        InterruptPipe pipe;
        try
        {
            pipe = InterruptPipe.Create(endpointId, endpoint.Address, maxEsitPayload, handler, _memory);
        }
        catch (InvalidOperationException exception)
        {
            WriteLog($"slot {device.SlotId}: {exception.Message}");
            return false;
        }

        PrepareEndpointInput(device, endpointId, reinitialize: false);
        ContextLayout.WriteEndpoint(device.InputEndpointContext(endpointId), EndpointType.InterruptIn,
            endpoint.MaxPacketSize, endpoint.AdditionalTransactions, InterruptInterval(device.Speed, endpoint.Interval),
            pipe.Ring.DeviceAddress, pipe.Ring.CycleState, (ushort)maxEsitPayload, (uint)maxEsitPayload);

        CompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(TrbType.ConfigureEndpointCommand, device.SlotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Configure Endpoint", code);
            pipe.Free(_memory);
            return false;
        }

        using (_eventLock.EnterScope())
        {
            device.AddPipe(pipe);
            using (_ringLock.EnterScope())
            {
                pipe.QueueAll();
            }
        }

        _registers.RingDoorbell(device.SlotId, endpointId);
        return true;
    }

    /// <summary>
    /// Fills the input context of a Configure Endpoint command that adds the
    /// endpoint at <paramref name="endpointId"/>, or drops and adds it back
    /// when <paramref name="reinitialize"/>: the Slot Context copied from the
    /// controller's, its Context Entries covering the endpoint. The caller
    /// writes the Endpoint Context.
    /// </summary>
    private static void PrepareEndpointInput(XhciDevice device, byte endpointId, bool reinitialize)
    {
        uint endpointFlag = ContextLayout.EndpointFlag(endpointId);
        device.ClearInputContext();
        ContextLayout.SetFlags(device.InputControlContext, reinitialize ? endpointFlag : 0, ContextLayout.SlotFlag | endpointFlag);
        ContextLayout.CopySlot(device.OutputSlotContext, device.InputSlotContext);
        if (ContextLayout.GetContextEntries(device.InputSlotContext) < endpointId)
        {
            ContextLayout.SetContextEntries(device.InputSlotContext, endpointId);
        }
    }

    /// <summary>Tells the controller the default endpoint's real packet size once the device descriptor gave it.</summary>
    private bool UpdateControlMaxPacketSize(XhciDevice device, ushort maxPacketSize)
    {
        device.ClearInputContext();
        ContextLayout.SetAddFlags(device.InputControlContext, ContextLayout.EndpointFlag(XhciDevice.ControlEndpointId));
        WriteControlEndpoint(device, maxPacketSize);

        CompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(TrbType.EvaluateContextCommand, device.SlotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Evaluate Context", code);
            return false;
        }

        device.SetMaxPacketSize0(maxPacketSize);
        return true;
    }

    private static void WriteControlEndpoint(XhciDevice device, ushort maxPacketSize) =>
        ContextLayout.WriteEndpoint(device.InputEndpointContext(XhciDevice.ControlEndpointId), EndpointType.Control,
            maxPacketSize, 0, 0, device.ControlRing.DeviceAddress, device.ControlRing.CycleState, ControlAverageTrbLength, 0);

    /// <summary>
    /// Queues one command, rings the command doorbell and waits for its
    /// completion event. Thread context only, once the binding is Bound.
    /// </summary>
    /// <returns>The completion code, or <see cref="CompletionCode.Invalid"/> on timeout.</returns>
    private CompletionCode ExecuteCommand(ulong parameter, uint control, out byte slotId)
    {
        slotId = 0;

        // Refused only once the binding is gone, which a PCI one never is.
        if (!_commandTurn.Wait())
        {
            return CompletionCode.Invalid;
        }

        try
        {
            using (_eventLock.EnterScope())
            {
                _commandCompleted = false;
                using (_ringLock.EnterScope())
                {
                    _pendingCommand = _commandRing.Enqueue(parameter, 0, control);
                }

                _registers.RingDoorbell(0, 0);
            }

            for (long waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
            {
                using (_eventLock.EnterScope())
                {
                    DrainEvents();
                    if (_commandCompleted || waitedUs >= CommandTimeoutUs)
                    {
                        _pendingCommand = 0;
                        slotId = _commandCompleted ? _commandSlotId : (byte)0;
                        return _commandCompleted ? _commandCode : CompletionCode.Invalid;
                    }
                }

                _context.Delay(WaitPollInterval);
            }
        }
        finally
        {
            _commandTurn.Signal();
        }
    }

    /// <summary>Control dword of a command that targets a slot.</summary>
    private static uint SlotCommand(TrbType type, byte slotId) =>
        Trb.TypeField(type) | ((uint)slotId << Trb.SlotIdShift);

    /// <summary>Control dword of a command that targets one endpoint of a slot.</summary>
    private static uint EndpointCommand(TrbType type, byte slotId, byte endpointId) =>
        SlotCommand(type, slotId) | ((uint)endpointId << Trb.EndpointIdShift);

    /// <summary>
    /// Converts bInterval into the Endpoint Context's 2^n x 125 µs
    /// encoding. Full/low-speed devices count 1 ms frames, rounded down to a
    /// power of two; faster ones already use 2^(bInterval-1) microframes.
    /// </summary>
    private static byte InterruptInterval(UsbSpeed speed, byte interval)
    {
        if (speed is UsbSpeed.Low or UsbSpeed.Full)
        {
            int exponent = FullSpeedMinIntervalExponent;
            for (int frames = Math.Max((int)interval, 1); frames > 1; frames >>= 1)
            {
                exponent++;
            }

            return (byte)Math.Min(exponent, FullSpeedMaxIntervalExponent);
        }

        return (byte)(Math.Clamp((int)interval, 1, HighSpeedMaxInterval) - 1);
    }

    private void LogCommandFailure(string command, CompletionCode code) =>
        WriteLog(code == CompletionCode.Invalid ? $"{command} timed out" : $"{command} failed, completion code {(byte)code}");
}
