// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The root ports: the boot-time scan from the probe, the hot-plug thread
/// that handles port changes afterwards, the resets and the waits, each
/// written as a deadline loop of 1 ms steps that leaves when the host
/// binding begins detaching.
/// </summary>
public sealed partial class XhciState
{
    /// <summary>
    /// Records which root ports are USB 2 and which USB 3 from the
    /// Supported Protocol capabilities (xHCI 1.2 section 7.2). Thread
    /// context, from the probe.
    /// </summary>
    internal void ReadSupportedProtocols()
    {
        for (ulong capability = FindExtendedCapability(XhciProtocol.SupportedProtocolCapabilityId, 0);
             capability != 0;
             capability = FindExtendedCapability(XhciProtocol.SupportedProtocolCapabilityId, capability))
        {
            byte majorRevision = (byte)(_registers.Read32(capability) >> XhciProtocol.SupportedProtocolRevisionShift);
            uint ports = _registers.Read32(capability + XhciProtocol.SupportedProtocolPortsOffset);
            int firstPort = (int)(ports & XhciProtocol.CompatiblePortOffsetMask);
            int portCount = (int)((ports >> XhciProtocol.CompatiblePortCountShift) & XhciProtocol.CompatiblePortCountMask);
            for (int port = Math.Max(firstPort, 1); port < firstPort + portCount && port <= _portMajorRevision.Length; port++)
            {
                _portMajorRevision[port - 1] = majorRevision;
            }
        }
    }

    /// <summary>
    /// The boot-time scan: powers every port lacking PP when software
    /// switches port power and lets it settle, waits out the connect settle
    /// (the reset dropped every device), probes every port, then marks the
    /// scan done so later port changes count as plugs. Thread context, the
    /// probe on the kit worker; every device's interface nodes are published
    /// from here, during the driver stage.
    /// </summary>
    internal void ProbeRootPorts()
    {
        if (_hasPortPowerControl)
        {
            for (int port = 1; port <= _maxPorts; port++)
            {
                uint portsc = ReadPortSc((byte)port);
                if ((portsc & XhciProtocol.PortPower) == 0)
                {
                    WritePortSc((byte)port, Neutral(portsc) | XhciProtocol.PortPower);
                }
            }

            if (!SettleMilliseconds(XhciProtocol.PortPowerSettleMs))
            {
                return;
            }
        }

        if (!SettleMilliseconds(XhciProtocol.PortConnectSettleMs))
        {
            return;
        }

        for (int port = 1; port <= _maxPorts; port++)
        {
            ProbeRootPort((byte)port);
        }

        _rootPortsProbed = true;
    }

    /// <summary>
    /// The <c>xhci-hotplug</c> thread: until the host binding detaches,
    /// handles the port changes, then waits for the next port change event
    /// with an interrupt, or sleeps the polled drain interval and drains the
    /// event ring under the lock without one (the periodic drain for
    /// interrupt pipes and port events on a polled controller). Thread
    /// context, its own thread.
    /// </summary>
    internal void HotPlugMain()
    {
        while (!_binding.IsDetaching)
        {
            HandlePortChanges();
            if (_hasInterrupt)
            {
                _binding.Wait(_portChangeEvent, XhciProtocol.HotPlugWaitMs);
            }
            else
            {
                _binding.Sleep(XhciProtocol.PolledDrainIntervalMs);
                using (Lock.Acquire())
                {
                    DrainEvents(null);
                }
            }
        }
    }

    /// <summary>
    /// One pass over the root ports: a port with change bits has them
    /// written back first (so a change landing meanwhile raises an event
    /// of its own); a connect change, or a port the controller disabled on
    /// its own after a bus error, detaches whatever was on the port and,
    /// when a device is connected and stays so through the debounce, probes
    /// it. Every step of a port is inside a try/catch that logs. Thread
    /// context, the hot-plug thread.
    /// </summary>
    private void HandlePortChanges()
    {
        for (int i = 1; i <= _maxPorts; i++)
        {
            byte port = (byte)i;
            try
            {
                uint portsc = ReadPortSc(port);
                uint changes = portsc & XhciProtocol.PortChangeBits;
                if (changes == 0)
                {
                    continue;
                }

                WritePortSc(port, Neutral(portsc) | changes);

                bool disabledByError = (changes & XhciProtocol.PortEnableChange) != 0 && (portsc & XhciProtocol.PortEnabled) == 0;
                if ((changes & XhciProtocol.PortConnectChange) == 0 && !disabledByError)
                {
                    continue;
                }

                Bus.Detach(port);
                if ((portsc & XhciProtocol.PortConnected) != 0 && WaitForStableConnection(port))
                {
                    ProbeRootPort(port);
                }
            }
            catch (Exception exception)
            {
                _binding.Log($"root port {port}: {exception.Message}");
            }
        }
    }

    /// <summary>
    /// Brings up one connected root port and attaches its device: a USB 3
    /// port enables by itself once its link trains and needs a warm reset
    /// only when it did not; a USB 2 port only enables through a port reset
    /// (xHCI 1.2 section 4.3.1). The change bits are cleared, the speed read,
    /// the reset recovery waited out, then the bus attaches the port. Thread
    /// context, the probe or the hot-plug thread.
    /// </summary>
    private void ProbeRootPort(byte port)
    {
        uint portsc = ReadPortSc(port);
        if ((portsc & XhciProtocol.PortConnected) == 0)
        {
            return;
        }

        bool enabled = _portMajorRevision[port - 1] >= XhciProtocol.UsbMajorRevision3
            ? WaitForLinkTraining(port) || ResetPort(port, warm: true)
            : ResetPort(port, warm: false);

        portsc = ReadPortSc(port);
        if (!enabled || (portsc & XhciProtocol.PortEnabled) == 0)
        {
            _binding.Log($"root port {port}: device connected but the port did not enable");
            return;
        }

        WritePortSc(port, Neutral(portsc) | XhciProtocol.PortChangeBits);
        UsbSpeed speed = (UsbSpeed)((portsc >> XhciProtocol.PortSpeedShift) & XhciProtocol.PortSpeedMask);

        if (!SettleMilliseconds(XhciProtocol.PortResetRecoveryMs))
        {
            return;
        }

        Bus.Attach(port, speed);
    }

    /// <summary>Waits out the connect debounce; false when the device went away meanwhile or the binding detaches. Thread context.</summary>
    private bool WaitForStableConnection(byte port)
    {
        if (!SettleMilliseconds(XhciProtocol.ConnectDebounceMs))
        {
            return false;
        }

        return (ReadPortSc(port) & XhciProtocol.PortConnected) != 0;
    }

    /// <summary>
    /// Makes the devices behind a root port that lost its connection fail
    /// their transfers at once, instead of when the hot-plug thread gets to
    /// the port: a thread waiting on one of them would otherwise wait out
    /// its whole timeout. In the handler or under the lock; allocation-free.
    /// </summary>
    private void MarkRootPortDisconnected(byte port)
    {
        if (port == 0 || port > _maxPorts || (ReadPortSc(port) & XhciProtocol.PortConnected) != 0)
        {
            return;
        }

        for (int slotId = 1; slotId < _slots.Length; slotId++)
        {
            XhciSlot? slot = _slots[slotId];
            if (slot is not null && slot.RootPortNumber == port)
            {
                slot.MarkDisconnected();
            }
        }
    }

    /// <summary>
    /// Resets a port, warm or hot, and waits for the change bit that ends
    /// it, polling every 1 ms up to <see cref="XhciProtocol.PortResetTimeoutMs"/>;
    /// both reset change bits are cleared when it lands. Thread context.
    /// </summary>
    /// <returns>True when the port came out of the reset enabled; false on a timeout (logged), a disabled port or a detaching binding.</returns>
    private bool ResetPort(byte port, bool warm)
    {
        WritePortSc(port, Neutral(ReadPortSc(port)) | (warm ? XhciProtocol.PortWarmReset : XhciProtocol.PortReset));
        uint completion = warm ? XhciProtocol.PortWarmResetChange : XhciProtocol.PortResetChange;
        for (uint waitedMs = 0; waitedMs < XhciProtocol.PortResetTimeoutMs; waitedMs++)
        {
            if (!SettleMilliseconds(1))
            {
                return false;
            }

            uint portsc = ReadPortSc(port);
            if ((portsc & completion) != 0)
            {
                WritePortSc(port, Neutral(portsc) | XhciProtocol.PortResetChange | XhciProtocol.PortWarmResetChange);
                return (portsc & XhciProtocol.PortEnabled) != 0;
            }
        }

        _binding.Log($"root port {port}: port reset timed out");
        return false;
    }

    /// <summary>Waits for a USB 3 port's link to train (PED set), polling every 1 ms up to <see cref="XhciProtocol.LinkTrainingTimeoutMs"/>. Thread context.</summary>
    private bool WaitForLinkTraining(byte port)
    {
        for (uint waitedMs = 0; waitedMs < XhciProtocol.LinkTrainingTimeoutMs; waitedMs++)
        {
            if ((ReadPortSc(port) & XhciProtocol.PortEnabled) != 0)
            {
                return true;
            }

            if (!SettleMilliseconds(1))
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>PORTSC as a write must carry it: the RO and RWS bits alone, so no change bit is cleared and PED is not written.</summary>
    private static uint Neutral(uint portsc) => portsc & XhciProtocol.PortPreserveMask;
}
