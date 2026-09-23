// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

internal sealed unsafe partial class XhciController
{
    /// <summary>Budget for one command; Address Device includes the device's SET_ADDRESS (Linux XHCI_CMD_DEFAULT_TIMEOUT).</summary>
    private const uint CommandTimeoutMs = 5000;

    /// <summary>Granularity of the synchronous command and transfer waits.</summary>
    private const uint WaitPollIntervalUs = 10;
    private const uint MicrosecondsPerMillisecond = 1000;

    /// <summary>SET_ADDRESS recovery interval before the next request (USB 2.0 §9.2.6.3).</summary>
    private const uint SetAddressRecoveryMs = 2;

    /// <summary>The device descriptor's first 8 bytes carry bMaxPacketSize0, and any packet size moves them (USB 2.0 §9.6.1).</summary>
    private const int DeviceDescriptorHeadLength = 8;
    private const int MaxPacketSize0Offset = 7;

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

    // Synchronous command state, under _eventLock. One command at a time:
    // commands are only issued from the single-threaded enumeration.
    private ulong _pendingCommand;
    private bool _commandCompleted;
    private XhciCompletionCode _commandCode;
    private byte _commandSlotId;

    public override UsbDevice? AddressDevice(UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        XhciCompletionCode code = ExecuteCommand(0, XhciTrb.TypeField(XhciTrbType.EnableSlotCommand), out byte slotId);
        if (code != XhciCompletionCode.Success || slotId == 0 || slotId >= _devices.Length)
        {
            LogCommandFailure("Enable Slot", code);
            return null;
        }

        XhciDevice device = new(this, slotId, parentHub as XhciDevice, port, speed, _regs.ContextSize);
        using (_eventLock.AcquireIrqSafe())
        {
            _devices[slotId] = device;
            _deviceContextArray[slotId] = device.OutputContextAddress;
        }

        device.ClearInputContext();
        XhciContext.SetAddFlags(device.InputControlContext, XhciContext.SlotFlag | XhciContext.EndpointFlag(XhciDevice.ControlEndpointId));
        XhciContext.WriteSlot(device.InputSlotContext, device.RouteString, speed, XhciDevice.ControlEndpointId,
            device.RootPortNumber, device.TtHubSlotId, device.TtPortNumber);
        WriteControlEndpoint(device, device.MaxPacketSize0);

        code = ExecuteCommand(device.InputContextAddress, SlotCommand(XhciTrbType.AddressDeviceCommand, slotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Address Device", code);
            ReleaseDevice(device);
            return null;
        }

        UsbManager.DelayMilliseconds(SetAddressRecoveryMs);

        Span<byte> head = stackalloc byte[DeviceDescriptorHeadLength];
        if (device.GetDescriptor(UsbDescriptorType.Device, 0, head) != UsbTransferStatus.Success)
        {
            Serial.WriteString("[xHCI] Addressed device did not return its descriptor\n");
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
        XhciContext.SetAddFlags(device.InputControlContext, XhciContext.SlotFlag);
        XhciContext.CopySlot(device.OutputSlotContext, device.InputSlotContext);
        XhciContext.SetHub(device.InputSlotContext, portCount, thinkTime);

        XhciTrbType command = _regs.HciVersion >= HubConfigureEndpointMinVersion
            ? XhciTrbType.ConfigureEndpointCommand
            : XhciTrbType.EvaluateContextCommand;
        XhciCompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(command, device.SlotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Configure hub", code);
            return false;
        }

        return true;
    }

    /// <summary>Adds an interrupt IN endpoint to the device's slot and starts its transfers.</summary>
    internal bool OpenInterruptPipe(XhciDevice device, UsbEndpoint endpoint, UsbInterruptHandler handler)
    {
        if (endpoint.Type != UsbEndpointType.Interrupt || !endpoint.IsIn)
        {
            Serial.WriteString("[xHCI] Only interrupt IN endpoints can be opened as interrupt pipes\n");
            return false;
        }

        // Device Context Index: endpoint number x 2, plus 1 for IN (xHCI 1.2 §4.5.1).
        byte endpointId = (byte)((endpoint.Number * 2) + 1);

        // For a high-speed periodic endpoint, Max Burst is the number of
        // additional transactions per microframe (xHCI 1.2 §6.2.3.4).
        int maxEsitPayload = endpoint.MaxPacketSize * (endpoint.AdditionalTransactions + 1);
        XhciInterruptPipe pipe = new(endpointId, endpoint.Address, maxEsitPayload, handler);

        device.ClearInputContext();
        XhciContext.SetAddFlags(device.InputControlContext, XhciContext.SlotFlag | XhciContext.EndpointFlag(endpointId));
        XhciContext.CopySlot(device.OutputSlotContext, device.InputSlotContext);
        if (XhciContext.GetContextEntries(device.InputSlotContext) < endpointId)
        {
            XhciContext.SetContextEntries(device.InputSlotContext, endpointId);
        }

        XhciContext.WriteEndpoint(device.InputEndpointContext(endpointId), XhciEndpointType.InterruptIn,
            endpoint.MaxPacketSize, endpoint.AdditionalTransactions, InterruptInterval(device.Speed, endpoint.Interval),
            pipe.Ring.PhysicalAddress, pipe.Ring.CycleState, (ushort)maxEsitPayload, (uint)maxEsitPayload);

        XhciCompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, device.SlotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Configure Endpoint", code);
            pipe.Free();
            return false;
        }

        using (_eventLock.AcquireIrqSafe())
        {
            device.AddPipe(pipe);
            using (_ringLock.AcquireIrqSafe())
            {
                pipe.QueueAll();
            }
        }

        _regs.RingDoorbell(device.SlotId, endpointId);
        return true;
    }

    /// <summary>Tells the controller the default endpoint's real packet size once the device descriptor gave it.</summary>
    private bool UpdateControlMaxPacketSize(XhciDevice device, ushort maxPacketSize)
    {
        device.ClearInputContext();
        XhciContext.SetAddFlags(device.InputControlContext, XhciContext.EndpointFlag(XhciDevice.ControlEndpointId));
        WriteControlEndpoint(device, maxPacketSize);

        XhciCompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(XhciTrbType.EvaluateContextCommand, device.SlotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Evaluate Context", code);
            return false;
        }

        device.SetMaxPacketSize0(maxPacketSize);
        return true;
    }

    private static void WriteControlEndpoint(XhciDevice device, ushort maxPacketSize) =>
        XhciContext.WriteEndpoint(device.InputEndpointContext(XhciDevice.ControlEndpointId), XhciEndpointType.Control,
            maxPacketSize, 0, 0, device.ControlRing.PhysicalAddress, device.ControlRing.CycleState, ControlAverageTrbLength, 0);

    /// <summary>
    /// Queues one command, rings the command doorbell and waits for its
    /// completion event. Thread context only.
    /// </summary>
    /// <returns>The completion code, or <see cref="XhciCompletionCode.Invalid"/> on timeout.</returns>
    private XhciCompletionCode ExecuteCommand(ulong parameter, uint control, out byte slotId)
    {
        using (_eventLock.AcquireIrqSafe())
        {
            _commandCompleted = false;
            using (_ringLock.AcquireIrqSafe())
            {
                _pendingCommand = _commandRing.Enqueue(parameter, 0, control);
            }

            _regs.RingDoorbell(0, 0);
        }

        for (uint waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
        {
            using (_eventLock.AcquireIrqSafe())
            {
                DrainEvents();
                if (_commandCompleted || waitedUs >= CommandTimeoutMs * MicrosecondsPerMillisecond)
                {
                    _pendingCommand = 0;
                    slotId = _commandCompleted ? _commandSlotId : (byte)0;
                    return _commandCompleted ? _commandCode : XhciCompletionCode.Invalid;
                }
            }

            PlatformHAL.Initializer?.DelayMicroseconds(WaitPollIntervalUs);
        }
    }

    /// <summary>Control dword of a command that targets a slot.</summary>
    private static uint SlotCommand(XhciTrbType type, byte slotId) =>
        XhciTrb.TypeField(type) | ((uint)slotId << XhciTrb.SlotIdShift);

    /// <summary>Control dword of a command that targets one endpoint of a slot.</summary>
    private static uint EndpointCommand(XhciTrbType type, byte slotId, byte endpointId) =>
        SlotCommand(type, slotId) | ((uint)endpointId << XhciTrb.EndpointIdShift);

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

    private static void LogCommandFailure(string command, XhciCompletionCode code)
    {
        Serial.WriteString("[xHCI] ");
        Serial.WriteString(command);
        Serial.WriteString(code == XhciCompletionCode.Invalid ? " timed out\n" : " failed, completion code ");
        if (code != XhciCompletionCode.Invalid)
        {
            Serial.WriteNumber((uint)code);
            Serial.WriteString("\n");
        }
    }
}
