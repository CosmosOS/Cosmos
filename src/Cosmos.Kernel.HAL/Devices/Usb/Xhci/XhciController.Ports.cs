// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

internal sealed unsafe partial class XhciController
{
    // PORTSC (xHCI 1.2 §5.4.8).
    private const uint PortConnected = 1u << 0;
    private const uint PortEnabled = 1u << 1;
    private const uint PortReset = 1u << 4;
    private const uint PortPower = 1u << 9;
    private const int PortSpeedShift = 10;
    private const uint PortSpeedMask = 0xF;
    private const uint PortConnectChange = 1u << 17;
    private const uint PortEnableChange = 1u << 18;
    private const uint PortWarmResetChange = 1u << 19;
    private const uint PortResetChange = 1u << 21;
    private const uint PortWarmReset = 1u << 31;

    /// <summary>RW1C change bits 23:17: CSC, PEC, WRC, OCC, PRC, PLC, CEC.</summary>
    private const uint PortChangeBits = 0x7Fu << 17;

    /// <summary>
    /// PORTSC bits a write must carry back as read: the RO and RWS ones
    /// (Linux xhci_port_state_to_neutral). PED is left out on purpose:
    /// writing it as 1 disables the port.
    /// </summary>
    private const uint PortPreserveMask =
        (1u << 0) | (1u << 3) | (0xFu << 10) | (1u << 30)
        | (0xFu << 5) | (1u << 9) | (0x3u << 14) | (0x7u << 25);

    // Supported Protocol capability (xHCI 1.2 §7.2).
    private const byte SupportedProtocolCapabilityId = 2;
    private const int SupportedProtocolRevisionShift = 24;
    private const ulong SupportedProtocolPortsOffset = 8;
    private const uint CompatiblePortOffsetMask = 0xFF;
    private const int CompatiblePortCountShift = 8;
    private const uint CompatiblePortCountMask = 0xFF;
    private const byte UsbMajorRevision3 = 3;

    /// <summary>Port power-on to power-good wait when software switches port power.</summary>
    private const uint PortPowerSettleMs = 20;

    /// <summary>
    /// Wait after the controller starts: the reset dropped every device, so
    /// USB 2 ones have to reconnect and pass the connect debounce (TATTDB,
    /// USB 2.0 §7.1.7.3) and USB 3 links have to retrain.
    /// </summary>
    private const uint PortConnectSettleMs = 200;

    private const uint PortResetTimeoutMs = 500;
    private const uint LinkTrainingTimeoutMs = 500;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 §7.1.7.5).</summary>
    private const uint PortResetRecoveryMs = 10;

    /// <summary>TATTDB: a connection must be stable this long before the port is reset (USB 2.0 §7.1.7.3).</summary>
    private const uint ConnectDebounceMs = 100;

    /// <summary>Set once the boot probe is done: port changes before it are the probe's own.</summary>
    private bool _rootPortsProbed;

    public override void ProbeRootPorts()
    {
        if (_regs.HasPortPowerControl)
        {
            for (int port = 1; port <= _regs.MaxPorts; port++)
            {
                uint portsc = _regs.ReadPortSc((byte)port);
                if ((portsc & PortPower) == 0)
                {
                    _regs.WritePortSc((byte)port, Neutral(portsc) | PortPower);
                }
            }

            UsbManager.DelayMilliseconds(PortPowerSettleMs);
        }

        UsbManager.DelayMilliseconds(PortConnectSettleMs);
        for (int port = 1; port <= _regs.MaxPorts; port++)
        {
            ProbeRootPort((byte)port);
        }

        _rootPortsProbed = true;
    }

    public override void HandlePortChanges()
    {
        for (int i = 1; i <= _regs.MaxPorts; i++)
        {
            byte port = (byte)i;
            uint portsc = _regs.ReadPortSc(port);
            uint changes = portsc & PortChangeBits;
            if (changes == 0)
            {
                continue;
            }

            // Cleared before the port is looked at, so a change landing
            // meanwhile raises an event of its own.
            _regs.WritePortSc(port, Neutral(portsc) | changes);

            // A port the controller disabled on its own, after an error on
            // the bus, lost its device as surely as one that was unplugged.
            bool disabledByError = (changes & PortEnableChange) != 0 && (portsc & PortEnabled) == 0;
            if ((changes & PortConnectChange) == 0 && !disabledByError)
            {
                continue;
            }

            UsbManager.DisconnectPort(this, null, port);
            if ((portsc & PortConnected) != 0 && WaitForStableConnection(port))
            {
                ProbeRootPort(port);
            }
        }
    }

    /// <summary>Records which root ports are USB 2 and which USB 3 (xHCI 1.2 §7.2).</summary>
    private void ReadSupportedProtocols()
    {
        for (ulong capability = FindExtendedCapability(SupportedProtocolCapabilityId, 0);
             capability != 0;
             capability = FindExtendedCapability(SupportedProtocolCapabilityId, capability))
        {
            byte majorRevision = (byte)(_regs.ReadCapability(capability) >> SupportedProtocolRevisionShift);
            uint ports = _regs.ReadCapability(capability + SupportedProtocolPortsOffset);
            int firstPort = (int)(ports & CompatiblePortOffsetMask);
            int portCount = (int)((ports >> CompatiblePortCountShift) & CompatiblePortCountMask);
            for (int port = Math.Max(firstPort, 1); port < firstPort + portCount && port <= _portMajorRevision.Length; port++)
            {
                _portMajorRevision[port - 1] = majorRevision;
            }
        }
    }

    private void ProbeRootPort(byte port)
    {
        uint portsc = _regs.ReadPortSc(port);
        if ((portsc & PortConnected) == 0)
        {
            return;
        }

        // A USB 2 port only enables through a port reset. A USB 3 port
        // enables by itself once its link trains, and needs a warm reset
        // only when it did not (xHCI 1.2 §4.3.1).
        bool enabled = _portMajorRevision[port - 1] >= UsbMajorRevision3
            ? WaitForLinkTraining(port) || ResetPort(port, warm: true)
            : ResetPort(port, warm: false);

        portsc = _regs.ReadPortSc(port);
        if (!enabled || (portsc & PortEnabled) == 0)
        {
            WritePortPrefix(port);
            Serial.WriteString("device connected but the port did not enable\n");
            return;
        }

        _regs.WritePortSc(port, Neutral(portsc) | PortChangeBits);
        UsbSpeed speed = (UsbSpeed)((portsc >> PortSpeedShift) & PortSpeedMask);

        UsbManager.DelayMilliseconds(PortResetRecoveryMs);
        UsbManager.EnumerateDevice(this, null, port, speed);
    }

    /// <summary>Waits out the connect debounce; false when the device went away meanwhile.</summary>
    private bool WaitForStableConnection(byte port)
    {
        UsbManager.DelayMilliseconds(ConnectDebounceMs);
        return (_regs.ReadPortSc(port) & PortConnected) != 0;
    }

    /// <summary>
    /// Makes the devices behind a root port that lost its connection fail
    /// their transfers at once, instead of when the hot-plug thread gets to
    /// the port: a thread waiting on one of them would otherwise wait out
    /// its whole timeout. Interrupt context, under the event lock.
    /// </summary>
    private void MarkRootPortDisconnected(byte port)
    {
        if (port == 0 || port > _regs.MaxPorts || (_regs.ReadPortSc(port) & PortConnected) != 0)
        {
            return;
        }

        foreach (XhciDevice? device in _devices)
        {
            if (device is not null && device.RootPortNumber == port)
            {
                device.MarkDisconnected();
            }
        }
    }

    private bool ResetPort(byte port, bool warm)
    {
        _regs.WritePortSc(port, Neutral(_regs.ReadPortSc(port)) | (warm ? PortWarmReset : PortReset));
        uint completion = warm ? PortWarmResetChange : PortResetChange;
        for (uint waitedMs = 0; waitedMs < PortResetTimeoutMs; waitedMs++)
        {
            UsbManager.DelayMilliseconds(1);
            uint portsc = _regs.ReadPortSc(port);
            if ((portsc & completion) != 0)
            {
                _regs.WritePortSc(port, Neutral(portsc) | PortResetChange | PortWarmResetChange);
                return (portsc & PortEnabled) != 0;
            }
        }

        WritePortPrefix(port);
        Serial.WriteString("port reset timed out\n");
        return false;
    }

    private bool WaitForLinkTraining(byte port)
    {
        for (uint waitedMs = 0; waitedMs < LinkTrainingTimeoutMs; waitedMs++)
        {
            if ((_regs.ReadPortSc(port) & PortEnabled) != 0)
            {
                return true;
            }

            UsbManager.DelayMilliseconds(1);
        }

        return false;
    }

    private static uint Neutral(uint portsc) => portsc & PortPreserveMask;

    private static void WritePortPrefix(byte port)
    {
        Serial.WriteString("[xHCI] Root port ");
        Serial.WriteNumber((uint)port);
        Serial.WriteString(": ");
    }
}
