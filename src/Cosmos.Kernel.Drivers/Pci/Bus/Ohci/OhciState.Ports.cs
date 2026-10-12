// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The root ports: the boot-time scan from the probe, the hot-plug thread
/// that handles port changes and pipe recoveries afterwards, the resets
/// and the waits, each written as a deadline loop of 1 ms steps that
/// leaves when the host binding begins detaching.
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>
    /// The boot-time scan: powers the ports when the root hub switches
    /// power (globally, and per port when PSM says so) and lets them
    /// settle for the power-good time, waits out the connect settle (the
    /// reset dropped every device), writes back the root hub's own change
    /// bit and every port's change bits, empty ports included (a power-on
    /// CSC left on an empty port would hold a level-triggered RHSC
    /// asserted for good once enabled), probes every port, then marks the
    /// scan done so later port changes count as plugs. Thread context, the
    /// probe on the kit worker, before the interrupt enables are written;
    /// every device's interface nodes are published from here, during the
    /// driver stage.
    /// </summary>
    internal void ProbeRootPorts()
    {
        if (!_noPowerSwitching)
        {
            Write(OhciProtocol.HcRhStatus, OhciProtocol.SetGlobalPower);
            if (_perPortPowerSwitching)
            {
                for (int port = 1; port <= _portCount; port++)
                {
                    WritePortStatus((byte)port, OhciProtocol.PortPowerStatus);
                }
            }

            if (!SettleMilliseconds(Math.Max(_powerOnToPowerGoodMs, OhciProtocol.PortPowerSettleMs)))
            {
                return;
            }
        }

        if (!SettleMilliseconds(OhciProtocol.PortConnectSettleMs))
        {
            return;
        }

        ClearRootHubOverCurrentChange();
        for (int port = 1; port <= _portCount; port++)
        {
            uint changes = ReadPortStatus((byte)port) & OhciProtocol.PortChangeBits;
            if (changes != 0)
            {
                WritePortStatus((byte)port, changes);
            }
        }

        for (int port = 1; port <= _portCount; port++)
        {
            ProbeRootPort((byte)port);
        }

        _rootPortsProbed = true;
    }

    /// <summary>
    /// The <c>ohci-hotplug</c> thread: until the host binding detaches,
    /// handles the port changes and recovers the pipes the drain asked for,
    /// then waits for the next port change event with an interrupt and
    /// drains the controller under the lock once (the safety net for an
    /// edge lost on the legacy line), or sleeps the polled drain interval
    /// and drains without one (the periodic drain for interrupt pipes and
    /// port events on a polled controller). Thread context, its own thread.
    /// </summary>
    internal void HotPlugMain()
    {
        while (!_binding.IsDetaching)
        {
            HandlePortChanges();
            RecoverPipes();
            if (_hasInterrupt)
            {
                _binding.Wait(_portChangeEvent, OhciProtocol.HotPlugWaitMs);
            }
            else
            {
                _binding.Sleep(OhciProtocol.PolledDrainIntervalMs);
            }

            using (Lock.Acquire())
            {
                DrainCompletions(null);
            }
        }
    }

    /// <summary>
    /// One pass over the root ports, nothing once the controller stopped:
    /// the root hub's own change bit is written back first (logged: an
    /// over-current), then a port with change bits has them written back
    /// before anything else (so a change landing meanwhile raises an event
    /// of its own); a connect change, or a port the controller disabled on
    /// its own after a bus error, detaches whatever was on the port and,
    /// when a device is connected and stays so through the debounce,
    /// probes it. Every step of a port is inside a try/catch that logs.
    /// After the pass RHSC, which the handler disabled, is enabled again.
    /// Thread context, the hot-plug thread.
    /// </summary>
    private void HandlePortChanges()
    {
        if (!_running)
        {
            return;
        }

        if (ClearRootHubOverCurrentChange())
        {
            _binding.Log("root hub: over-current");
        }

        for (int i = 1; i <= _portCount; i++)
        {
            byte port = (byte)i;
            try
            {
                uint status = ReadPortStatus(port);
                uint changes = status & OhciProtocol.PortChangeBits;
                if (changes == 0)
                {
                    continue;
                }

                WritePortStatus(port, changes);

                bool disabledByError = (changes & OhciProtocol.PortEnableStatusChange) != 0 && (status & OhciProtocol.PortEnableStatus) == 0;
                if ((changes & OhciProtocol.ConnectStatusChange) == 0 && !disabledByError)
                {
                    continue;
                }

                Bus.Detach(port);
                if ((status & OhciProtocol.CurrentConnectStatus) != 0 && WaitForStableConnection(port))
                {
                    ProbeRootPort(port);
                }
            }
            catch (Exception exception)
            {
                _binding.Log($"root port {port}: {exception.Message}");
            }
        }

        if (_hasInterrupt)
        {
            Write(OhciProtocol.HcInterruptEnable, OhciProtocol.RootHubStatusChange);
        }
    }

    /// <summary>
    /// Writes back the root hub's own change bit, HcRhStatus.OCIC (the
    /// over-current indicator changed; W1C, OHCI 1.0a section 7.4.3): it
    /// raises RHSC as a port's change bits do, and a level-triggered RHSC
    /// would stay asserted for good with it set, since the port writes
    /// never touch it. The over-current itself is only reported. Review-only
    /// on QEMU, which never sets it. Thread context.
    /// </summary>
    /// <returns>True when an over-current change was pending.</returns>
    private bool ClearRootHubOverCurrentChange()
    {
        if ((Read(OhciProtocol.HcRhStatus) & OhciProtocol.OverCurrentIndicatorChange) == 0)
        {
            return false;
        }

        Write(OhciProtocol.HcRhStatus, OhciProtocol.OverCurrentIndicatorChange);
        return true;
    }

    /// <summary>
    /// Brings up one connected root port and attaches its device: the port
    /// reset (SetPortReset, PRSC polled every 1 ms up to
    /// <see cref="OhciProtocol.PortResetTimeoutMs"/>), the change bits
    /// cleared, PES required, the speed from LSDA, the reset recovery
    /// waited out, then the bus attaches the port (OHCI 1.0a section
    /// 7.4.4). Thread context, the probe or the hot-plug thread.
    /// </summary>
    private void ProbeRootPort(byte port)
    {
        uint status = ReadPortStatus(port);
        if ((status & OhciProtocol.CurrentConnectStatus) == 0)
        {
            return;
        }

        WritePortStatus(port, OhciProtocol.PortResetStatus);
        bool resetDone = false;
        for (uint waitedMs = 0; waitedMs < OhciProtocol.PortResetTimeoutMs; waitedMs++)
        {
            if (!SettleMilliseconds(1))
            {
                return;
            }

            status = ReadPortStatus(port);
            if ((status & OhciProtocol.PortResetStatusChange) != 0)
            {
                resetDone = true;
                break;
            }
        }

        if (!resetDone)
        {
            _binding.Log($"root port {port}: port reset timed out");
            return;
        }

        WritePortStatus(port, OhciProtocol.PortResetStatusChange | OhciProtocol.ConnectStatusChange | OhciProtocol.PortEnableStatusChange);
        status = ReadPortStatus(port);
        if ((status & OhciProtocol.PortEnableStatus) == 0)
        {
            _binding.Log($"root port {port}: device connected but the port did not enable");
            return;
        }

        UsbSpeed speed = (status & OhciProtocol.LowSpeedDeviceAttached) != 0 ? UsbSpeed.Low : UsbSpeed.Full;
        if (!SettleMilliseconds(OhciProtocol.PortResetRecoveryMs))
        {
            return;
        }

        Bus.Attach(port, speed);
    }

    /// <summary>Waits out the connect debounce; false when the device went away meanwhile or the binding detaches. Thread context.</summary>
    private bool WaitForStableConnection(byte port)
    {
        if (!SettleMilliseconds(OhciProtocol.ConnectDebounceMs))
        {
            return false;
        }

        return (ReadPortStatus(port) & OhciProtocol.CurrentConnectStatus) != 0;
    }

    /// <summary>
    /// Makes the devices behind a root port that lost its connection fail
    /// their transfers at once, instead of when the hot-plug thread gets to
    /// the port: a thread waiting on one of them would otherwise wait out
    /// its whole timeout. In the handler or under the lock; allocation-free.
    /// </summary>
    private void MarkRootPortDisconnected(byte port)
    {
        if (port == 0 || port > _portCount || (ReadPortStatus(port) & OhciProtocol.CurrentConnectStatus) != 0)
        {
            return;
        }

        for (int address = 1; address < _devices.Length; address++)
        {
            OhciDevice? device = _devices[address];
            if (device is not null && device.RootPortNumber == port)
            {
                device.MarkDisconnected();
            }
        }
    }
}
