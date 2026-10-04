// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="UsbHubDriver"/> holds for one bound hub (USB 2.0
/// chapter 11, USB 3.2 chapter 10), hung off
/// <see cref="DeviceBinding.DriverState"/>: the port count, the speed class,
/// the pending change bitmap the status change pipe fills, the event the
/// <c>usb-hub</c> thread waits on, and the counters the suites read. The
/// hub reports which ports changed on its status change endpoint, in the
/// <see cref="UsbReportHandler"/> contexts; the ports themselves are only
/// looked at from the hub's thread or the probe, by GET_STATUS on the
/// default pipe through the kit's <see cref="UsbAccess"/>.
/// </summary>
public sealed class UsbHubState
{
    // --- Constants ---

    /// <summary>How long the thread waits for a port change before it looks again, in milliseconds.</summary>
    private const uint PortChangeWaitMs = 1000;

    /// <summary>Bytes of the status change bitmap folded into the pending word.</summary>
    private const int PendingChangeBytes = sizeof(uint);

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly UsbAccess _usb;
    private readonly DeviceEvent _portChange;
    private readonly byte _portCount;
    private readonly bool _isSuperSpeed;

    /// <summary>Ports the status change endpoint reported and the thread has not looked at yet, as its bitmap. Written with Interlocked.Or by the report handler, read with Interlocked.Exchange by the thread.</summary>
    private uint _pendingChanges;

    private volatile int _childrenAttached;
    private volatile int _childrenDetached;
    private volatile bool _hotPlugRunning;

    // --- Constructor ---

    /// <summary>Takes the access, the hub descriptor's facts and the event the probe created. Thread context, from the probe.</summary>
    /// <param name="binding">The hub interface's binding, for the log, the sleeps and the thread's wait.</param>
    /// <param name="usb">The kit's access to the hub interface.</param>
    /// <param name="portCount">bNbrPorts from the hub descriptor.</param>
    /// <param name="isSuperSpeed">True for a hub attached at SuperSpeed or above.</param>
    /// <param name="portChange">The event the report handler signals and <see cref="ThreadMain"/> waits on.</param>
    internal UsbHubState(DeviceBinding binding, UsbAccess usb, byte portCount, bool isSuperSpeed, DeviceEvent portChange)
    {
        _binding = binding;
        _usb = usb;
        _portCount = portCount;
        _isSuperSpeed = isSuperSpeed;
        _portChange = portChange;
    }

    // --- Properties the suites read ---

    /// <summary>bNbrPorts: how many downstream ports the hub has. Any context.</summary>
    public byte PortCount => _portCount;

    /// <summary>True for a hub attached at SuperSpeed or above, which reads the SuperSpeed hub descriptor and feature tables. Any context.</summary>
    public bool IsSuperSpeed => _isSuperSpeed;

    /// <summary>How many times a device on one of the hub's ports was attached through the kit, by the probe and the thread. Any context.</summary>
    public int ChildrenAttached => _childrenAttached;

    /// <summary>How many times the thread asked the kit to detach a port's device after a connection change. Any context.</summary>
    public int ChildrenDetached => _childrenDetached;

    /// <summary>True when the <c>usb-hub</c> thread started, so ports plugged after the probe are followed. Any context.</summary>
    public bool HotPlugRunning
    {
        get => _hotPlugRunning;
        internal set => _hotPlugRunning = value;
    }

    // --- Internal properties ---

    /// <summary>The event the status change handler signals; the thread waits on it between passes.</summary>
    internal DeviceEvent PortChange => _portChange;

    /// <summary>Last port a device can be enumerated on: a route string holds one 4-bit port number per tier.</summary>
    private int LastPort => Math.Min((int)_portCount, UsbHubProtocol.MaxRoutablePort);

    // --- The report handler ---

    /// <summary>
    /// The status change pipe's handler: folds up to four bytes of the
    /// bitmap into the pending word with Interlocked.Or and signals the
    /// thread when any bit is set. The <see cref="UsbReportHandler"/>
    /// contexts: interrupt context on a controller with a message
    /// interrupt, otherwise whichever thread drains the controller's events
    /// under its lock, which may be this hub's own thread inside a control
    /// transfer. Allocation-free; calls nothing on the access or the
    /// binding.
    /// </summary>
    /// <param name="bitmap">The bytes the hub sent: bit 0 the hub itself, bit N port N.</param>
    internal void OnStatusChange(ReadOnlySpan<byte> bitmap)
    {
        uint changed = 0;
        for (int i = 0; i < bitmap.Length && i < PendingChangeBytes; i++)
        {
            changed |= (uint)bitmap[i] << (i * 8);
        }

        if (changed != 0)
        {
            Interlocked.Or(ref _pendingChanges, changed);
            _portChange.Signal();
        }
    }

    // --- The thread ---

    /// <summary>
    /// The <c>usb-hub</c> thread: until the binding detaches, takes the
    /// pending bitmap, clears the hub's own change bits when bit 0 is set
    /// (the hub is port 0 of its own status requests), handles every
    /// reported port (its changes cleared, the device that was on it
    /// detached, one now on it probed), then waits up to a second for the
    /// next signal. Every step is inside a try/catch that logs
    /// <c>port {n}: {message}</c>, so a publish refused by a detaching
    /// binding is logged, not fatal. The thread never retracts from the
    /// detach hook: the kit's child step tears the hub's children down
    /// first. Thread context, this thread.
    /// </summary>
    internal void ThreadMain()
    {
        while (!_binding.IsDetaching)
        {
            uint changed = Interlocked.Exchange(ref _pendingChanges, 0);
            for (int port = 0; port <= LastPort; port++)
            {
                if ((changed & (1u << port)) == 0)
                {
                    continue;
                }

                try
                {
                    if (port == 0)
                    {
                        ClearHubChanges();
                    }
                    else
                    {
                        HandlePortChange((byte)port);
                    }
                }
                catch (Exception exception)
                {
                    _binding.Log($"port {port}: {exception.Message}");
                }
            }

            _binding.Wait(_portChange, PortChangeWaitMs);
        }
    }

    // --- The probe's steps ---

    /// <summary>
    /// Powers every port, then waits out bPwrOn2PwrGood and the connect
    /// debounce so the connections have settled. Thread context, the probe.
    /// </summary>
    /// <param name="powerOnToPowerGoodMs">bPwrOn2PwrGood from the hub descriptor, in milliseconds.</param>
    internal void PowerPorts(uint powerOnToPowerGoodMs)
    {
        for (int port = 1; port <= _portCount; port++)
        {
            SetPortFeature((byte)port, UsbHubProtocol.PortFeaturePower);
        }

        _binding.Sleep(powerOnToPowerGoodMs + UsbHubProtocol.ConnectDebounceMs);
    }

    /// <summary>
    /// Probes every port a device can be enumerated on, each inside a
    /// try/catch that logs <c>port {n}: {message}</c>. Thread context, the
    /// probe; the children it attaches are published from the probe and
    /// offered after it.
    /// </summary>
    internal void ProbePorts()
    {
        for (int port = 1; port <= LastPort; port++)
        {
            try
            {
                ProbePort((byte)port);
            }
            catch (Exception exception)
            {
                _binding.Log($"port {port}: {exception.Message}");
            }
        }
    }

    // --- Port handling ---

    /// <summary>
    /// One reported port: GET_STATUS, every set change bit cleared through
    /// the speed's feature table, then, on a connection change or a port the
    /// hub disabled on its own after a bus error (USB 2.0 section
    /// 11.24.2.7.2.2), the device that was on it is detached and, when a
    /// device is connected and stays so through the debounce, probed.
    /// Thread context, the hub's thread.
    /// </summary>
    /// <param name="port">The 1-based port number.</param>
    private void HandlePortChange(byte port)
    {
        if (!TryGetStatus(UsbHubProtocol.PortRecipient, port, out ushort status, out ushort change))
        {
            return;
        }

        ClearPortChanges(port, change);

        bool disabledByHub = (change & UsbHubProtocol.PortChangeEnable) != 0 && (status & UsbHubProtocol.PortStatusEnable) == 0;
        if ((change & UsbHubProtocol.PortChangeConnection) == 0 && !disabledByHub)
        {
            return;
        }

        _usb.DetachChild(_binding, port);
        _childrenDetached++;

        if ((status & UsbHubProtocol.PortStatusConnection) == 0)
        {
            return;
        }

        _binding.Sleep(UsbHubProtocol.ConnectDebounceMs);
        if (TryGetStatus(UsbHubProtocol.PortRecipient, port, out status, out _) && (status & UsbHubProtocol.PortStatusConnection) != 0)
        {
            ProbePort(port);
        }
    }

    /// <summary>
    /// Enumerates the device on one port when one is connected: the
    /// connection change cleared, the port reset (a USB 2.0 port only
    /// enables through a reset; a SuperSpeed port trains its link on connect
    /// and needs one only when it did not), the speed read from the status
    /// bits, the reset recovery waited out, then the kit's attach with this
    /// hub as the parent. Thread context, the probe or the hub's thread.
    /// </summary>
    /// <param name="port">The 1-based port number.</param>
    private void ProbePort(byte port)
    {
        if (!TryGetStatus(UsbHubProtocol.PortRecipient, port, out ushort status, out _) || (status & UsbHubProtocol.PortStatusConnection) == 0)
        {
            return;
        }

        ClearPortFeature(port, UsbHubProtocol.PortFeatureConnectionChange);

        if ((!_isSuperSpeed || (status & UsbHubProtocol.PortStatusEnable) == 0) && !ResetPort(port, out status))
        {
            _binding.Log($"port {port}: port reset timed out");
            return;
        }

        if ((status & UsbHubProtocol.PortStatusEnable) == 0)
        {
            _binding.Log($"port {port}: port did not enable after reset");
            return;
        }

        UsbSpeed speed = _isSuperSpeed ? UsbSpeed.Super
            : (status & UsbHubProtocol.PortStatusLowSpeed) != 0 ? UsbSpeed.Low
            : (status & UsbHubProtocol.PortStatusHighSpeed) != 0 ? UsbSpeed.High
            : UsbSpeed.Full;

        _binding.Sleep(UsbHubProtocol.ResetRecoveryMs);
        if (_usb.AttachChild(_binding, port, speed))
        {
            _childrenAttached++;
        }
    }

    /// <summary>
    /// SET_FEATURE(PORT_RESET), then GET_STATUS every 10 ms for up to 500 ms
    /// until C_PORT_RESET is set with PORT_RESET clear, which is then
    /// cleared. Thread context.
    /// </summary>
    /// <param name="port">The 1-based port number.</param>
    /// <param name="status">wPortStatus as last read; 0 when the request failed.</param>
    /// <returns>False when the request failed or the reset did not complete in time.</returns>
    private bool ResetPort(byte port, out ushort status)
    {
        status = 0;
        if (SetPortFeature(port, UsbHubProtocol.PortFeatureReset) != UsbTransferStatus.Success)
        {
            return false;
        }

        for (uint elapsedMs = 0; elapsedMs < UsbHubProtocol.PortResetTimeoutMs; elapsedMs += UsbHubProtocol.PortResetPollMs)
        {
            _binding.Sleep(UsbHubProtocol.PortResetPollMs);
            if (!TryGetStatus(UsbHubProtocol.PortRecipient, port, out status, out ushort change))
            {
                return false;
            }

            if ((change & UsbHubProtocol.PortChangeReset) != 0 && (status & UsbHubProtocol.PortStatusReset) == 0)
            {
                ClearPortFeature(port, UsbHubProtocol.PortFeatureResetChange);
                return true;
            }
        }

        return false;
    }

    /// <summary>Clears every set change bit of a port through the speed's feature table, so the hub stops reporting it. Thread context.</summary>
    /// <param name="port">The 1-based port number.</param>
    /// <param name="change">wPortChange as read.</param>
    private void ClearPortChanges(byte port, ushort change)
    {
        ReadOnlySpan<byte> features = _isSuperSpeed ? UsbHubProtocol.SuperSpeedChangeFeatures : UsbHubProtocol.HighSpeedChangeFeatures;
        for (int bit = 0; bit < features.Length; bit++)
        {
            if ((change & (1 << bit)) != 0 && features[bit] != 0)
            {
                ClearPortFeature(port, features[bit]);
            }
        }
    }

    /// <summary>GET_STATUS on the hub itself, then CLEAR_FEATURE of each set wHubChange bit (local power, over-current). Thread context.</summary>
    private void ClearHubChanges()
    {
        if (!TryGetStatus(UsbHubProtocol.HubRecipient, 0, out _, out ushort change))
        {
            return;
        }

        ReadOnlySpan<byte> features = UsbHubProtocol.HubChangeFeatures;
        for (int bit = 0; bit < features.Length; bit++)
        {
            if ((change & (1 << bit)) != 0)
            {
                _usb.ControlOut(UsbHubProtocol.HubRecipient, (byte)UsbStandardRequest.ClearFeature, features[bit], 0);
            }
        }
    }

    // --- Hub class requests ---

    /// <summary>GET_STATUS on the hub (recipient the hub, index 0) or a port: the status word, then the change word. Thread context.</summary>
    /// <param name="recipient">The hub or the port recipient.</param>
    /// <param name="port">The 1-based port number, or 0 for the hub.</param>
    /// <param name="status">wPortStatus or wHubStatus.</param>
    /// <param name="change">wPortChange or wHubChange.</param>
    /// <returns>False when the request did not succeed; both words are then 0.</returns>
    private bool TryGetStatus(UsbRequestType recipient, byte port, out ushort status, out ushort change)
    {
        Span<byte> data = stackalloc byte[UsbHubProtocol.StatusLength];
        if (_usb.ControlIn(recipient, (byte)UsbStandardRequest.GetStatus, 0, port, data) != UsbTransferStatus.Success)
        {
            status = 0;
            change = 0;
            return false;
        }

        status = (ushort)(data[0] | (data[1] << 8));
        change = (ushort)(data[2] | (data[3] << 8));
        return true;
    }

    /// <summary>SET_FEATURE on a port. Thread context.</summary>
    /// <param name="port">The 1-based port number.</param>
    /// <param name="feature">The port feature selector.</param>
    private UsbTransferStatus SetPortFeature(byte port, ushort feature) =>
        _usb.ControlOut(UsbHubProtocol.PortRecipient, (byte)UsbStandardRequest.SetFeature, feature, port);

    /// <summary>CLEAR_FEATURE on a port. Thread context.</summary>
    /// <param name="port">The 1-based port number.</param>
    /// <param name="feature">The port feature selector.</param>
    private UsbTransferStatus ClearPortFeature(byte port, ushort feature) =>
        _usb.ControlOut(UsbHubProtocol.PortRecipient, (byte)UsbStandardRequest.ClearFeature, feature, port);
}
