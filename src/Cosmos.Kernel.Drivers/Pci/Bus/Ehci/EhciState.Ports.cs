// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The root ports: the boot-time scan from the probe, the hot-plug thread
/// that handles port changes and pipe recoveries afterwards, the resets,
/// the hand-off of a full or low-speed device to a companion controller,
/// and the waits, each written as a deadline loop of 1 ms steps that
/// leaves when the host binding begins detaching.
/// </summary>
internal sealed partial class EhciState
{
    /// <summary>
    /// The boot-time scan: powers every port lacking PP when software
    /// switches port power and lets it settle, waits out the connect
    /// settle (the reset dropped every device), probes every port, then
    /// marks the scan done so later port changes count as plugs. Thread
    /// context, the probe on the kit worker; every device's interface
    /// nodes are published from here, during the driver stage.
    /// </summary>
    internal void ProbeRootPorts()
    {
        if (_hasPortPowerControl)
        {
            for (int i = 1; i <= _portCount; i++)
            {
                byte port = (byte)i;
                uint portsc = ReadPortSc(port);
                if ((portsc & EhciProtocol.PortPower) == 0)
                {
                    WritePortSc(port, Neutral(portsc) | EhciProtocol.PortPower);
                }
            }

            if (!SettleMilliseconds(EhciProtocol.PortPowerSettleMs))
            {
                return;
            }
        }

        if (!SettleMilliseconds(EhciProtocol.PortConnectSettleMs))
        {
            return;
        }

        for (int i = 1; i <= _portCount; i++)
        {
            ProbeRootPort((byte)i);
        }

        _rootPortsProbed = true;
    }

    /// <summary>
    /// The <c>ehci-hotplug</c> thread: until the host binding detaches,
    /// handles the port changes and the pipe recoveries, then waits for
    /// the next port change event with an interrupt, or sleeps the polled
    /// drain interval without one, and services the status and the
    /// descriptors under the lock either way: the periodic drain of a
    /// polled controller, and the safety net for an edge lost on the
    /// legacy line. Thread context, its own thread.
    /// </summary>
    internal void HotPlugMain()
    {
        while (!_binding.IsDetaching)
        {
            HandlePortChanges();
            RecoverPipes();
            if (_hasInterrupt)
            {
                _binding.Wait(_portChangeEvent, EhciProtocol.HotPlugWaitMs);
            }
            else
            {
                _binding.Sleep(EhciProtocol.PolledDrainIntervalMs);
            }

            Poll();
        }
    }

    /// <summary>
    /// One pass over the root ports, nothing once the controller stopped
    /// running: a port a companion owns is skipped (the hardware clears
    /// Port Owner when its device leaves, and the next plug shows a
    /// connect change here again); a port with change bits has them
    /// written back first (so a change landing meanwhile raises an
    /// interrupt of its own); a connect change, or a port the controller
    /// disabled on its own after a bus error, detaches whatever was on the
    /// port and, when a device is connected and stays so through the
    /// debounce, probes it. Every step of a port is inside a try/catch
    /// that logs. Thread context, the hot-plug thread.
    /// </summary>
    private void HandlePortChanges()
    {
        if (!_running)
        {
            return;
        }

        for (int i = 1; i <= _portCount; i++)
        {
            byte port = (byte)i;
            try
            {
                uint portsc = ReadPortSc(port);
                if ((portsc & EhciProtocol.PortOwner) != 0)
                {
                    continue;
                }

                uint changes = portsc & EhciProtocol.PortChangeBits;
                if (changes == 0)
                {
                    continue;
                }

                WritePortSc(port, Neutral(portsc) | changes);

                bool disabledByError = (changes & EhciProtocol.PortEnableChange) != 0 && (portsc & EhciProtocol.PortEnabled) == 0;
                if ((changes & EhciProtocol.PortConnectChange) == 0 && !disabledByError)
                {
                    continue;
                }

                Bus.Detach(port);
                if ((portsc & EhciProtocol.PortConnected) != 0 && WaitForStableConnection(port))
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
    /// Brings up one connected root port (EHCI 1.0 section 4.2.2): a
    /// K-state line is a low-speed device, handed to the companion; else
    /// the port is reset and, enabled afterwards, holds a high-speed
    /// device whose change bits are cleared and which the bus attaches
    /// after the reset recovery; a port not enabled after the reset holds
    /// a full-speed device, handed to the companion. Thread context, the
    /// probe or the hot-plug thread.
    /// </summary>
    private void ProbeRootPort(byte port)
    {
        uint portsc = ReadPortSc(port);
        if ((portsc & EhciProtocol.PortConnected) == 0)
        {
            return;
        }

        if (((portsc >> EhciProtocol.PortLineStatusShift) & EhciProtocol.PortLineStatusMask) == EhciProtocol.PortLineStatusKState)
        {
            HandToCompanion(port, "low-speed");
            return;
        }

        if (!ResetPort(port))
        {
            return;
        }

        portsc = ReadPortSc(port);
        if ((portsc & EhciProtocol.PortEnabled) == 0)
        {
            HandToCompanion(port, "full-speed");
            return;
        }

        WritePortSc(port, Neutral(portsc) | (portsc & EhciProtocol.PortChangeBits));
        if (!SettleMilliseconds(EhciProtocol.PortResetRecoveryMs))
        {
            return;
        }

        Bus.Attach(port, UsbSpeed.High);
    }

    /// <summary>
    /// Resets a port: PR set with PED written 0 (the spec wants the port
    /// disabled before a reset), asserted for <see cref="EhciProtocol.PortResetAssertMs"/>,
    /// then PR cleared and polled every 1 ms up to <see cref="EhciProtocol.PortResetCompleteMs"/>
    /// for it to read 0. Every write is composed from a value read right
    /// before it. Thread context.
    /// </summary>
    /// <returns>True when the reset completed; false on a timeout (logged) or a detaching binding.</returns>
    private bool ResetPort(byte port)
    {
        uint portsc = ReadPortSc(port);
        WritePortSc(port, (Neutral(portsc) & ~EhciProtocol.PortEnabled) | EhciProtocol.PortReset);
        if (!SettleMilliseconds(EhciProtocol.PortResetAssertMs))
        {
            return false;
        }

        WritePortSc(port, Neutral(ReadPortSc(port)));
        for (uint waitedMs = 0; waitedMs < EhciProtocol.PortResetCompleteMs; waitedMs++)
        {
            if ((ReadPortSc(port) & EhciProtocol.PortReset) == 0)
            {
                return true;
            }

            if (!SettleMilliseconds(1))
            {
                return false;
            }
        }

        if ((ReadPortSc(port) & EhciProtocol.PortReset) == 0)
        {
            return true;
        }

        _binding.Log($"root port {port}: port reset timed out");
        return false;
    }

    /// <summary>
    /// Hands a port holding a full or low-speed device to its companion
    /// controller (EHCI 1.0 section 4.2): Port Owner written, and the
    /// release counted only once PORTSC reads it back set, since the
    /// hardware ignores the write on a port without a companion. The
    /// companion's own driver enumerates the device on its own bus.
    /// Thread context.
    /// </summary>
    /// <param name="port">The 1-based root port.</param>
    /// <param name="speed">The device's speed, for the log.</param>
    private void HandToCompanion(byte port, string speed)
    {
        if (!HasCompanion(port))
        {
            _binding.Log($"root port {port}: {speed} device but no companion controller");
            return;
        }

        WritePortSc(port, Neutral(ReadPortSc(port)) | EhciProtocol.PortOwner);
        uint portsc = ReadPortSc(port);
        if ((portsc & EhciProtocol.PortOwner) == 0)
        {
            _binding.Log($"root port {port}: {speed} device but the port could not be released");
            return;
        }

        WritePortSc(port, Neutral(portsc) | (portsc & EhciProtocol.PortChangeBits));
        _companionPortsReleased++;
        _binding.Log($"root port {port}: {speed} device released to the companion controller");
    }

    /// <summary>Waits out the connect debounce; false when the device went away meanwhile or the binding detaches. Thread context.</summary>
    private bool WaitForStableConnection(byte port)
    {
        if (!SettleMilliseconds(EhciProtocol.ConnectDebounceMs))
        {
            return false;
        }

        return (ReadPortSc(port) & EhciProtocol.PortConnected) != 0;
    }

    /// <summary>
    /// Makes the devices behind a root port that lost its connection fail
    /// their transfers at once, instead of when the hot-plug thread gets to
    /// the port: a thread waiting on one of them would otherwise wait out
    /// its whole timeout. In the handler or under the lock; allocation-free.
    /// </summary>
    private void MarkRootPortDisconnected(byte port)
    {
        if (port == 0 || port > _portCount || (ReadPortSc(port) & EhciProtocol.PortConnected) != 0)
        {
            return;
        }

        for (int address = 1; address < _devices.Length; address++)
        {
            EhciDevice? device = _devices[address];
            if (device is not null && device.RootPortNumber == port)
            {
                device.MarkDisconnected();
            }
        }
    }
}
