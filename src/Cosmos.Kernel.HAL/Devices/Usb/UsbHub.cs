// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// One hub bound by <see cref="UsbHubDriver"/> (USB 2.0 chapter 11, USB 3.2
/// chapter 10): powers its ports, enumerates what is plugged into them, and
/// follows what is plugged in or pulled out afterwards. The hub reports
/// which ports changed on its status change endpoint, in interrupt context;
/// the ports themselves are only looked at from the hot-plug thread, by
/// GET_STATUS on the default pipe.
/// </summary>
internal sealed class UsbHub
{
    // Port feature selectors (USB 2.0 table 11-17, USB 3.2 table 10-9).
    private const ushort PortFeatureReset = 4;
    private const ushort PortFeaturePower = 8;
    private const ushort PortFeatureConnectionChange = 16;
    private const ushort PortFeatureResetChange = 20;

    // wPortStatus bits (USB 2.0 §11.24.2.7.1; bits 0, 1 and 4 mean the same on a SuperSpeed hub).
    private const ushort PortStatusConnection = 1 << 0;
    private const ushort PortStatusEnable = 1 << 1;
    private const ushort PortStatusReset = 1 << 4;
    private const ushort PortStatusLowSpeed = 1 << 9;
    private const ushort PortStatusHighSpeed = 1 << 10;

    // wPortChange bits (USB 2.0 §11.24.2.7.2; C_PORT_ENABLE is not defined on a SuperSpeed hub).
    private const ushort PortChangeConnection = 1 << 0;
    private const ushort PortChangeEnable = 1 << 1;
    private const ushort PortChangeReset = 1 << 4;

    // Hub class requests go to the hub itself or to one of its ports (USB 2.0 §11.24.2).
    private const UsbRequestType HubRecipient = UsbRequestType.Class | UsbRequestType.Device;
    private const UsbRequestType PortRecipient = UsbRequestType.Class | UsbRequestType.Other;

    /// <summary>GET_STATUS on the hub or a port returns the status word, then the change word.</summary>
    private const int StatusLength = 4;

    /// <summary>Status change bitmap bit 0 is the hub itself; bit N is port N (USB 2.0 §11.12.4).</summary>
    private const uint HubChangeBit = 1;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 §8.9), so higher ports are unreachable.</summary>
    private const int MaxRoutablePort = 15;

    /// <summary>TATTDB: a connection must be stable this long before the port is reset (USB 2.0 §7.1.7.3).</summary>
    private const uint ConnectDebounceMs = 100;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 §7.1.7.5).</summary>
    private const uint ResetRecoveryMs = 10;

    private const uint PortResetTimeoutMs = 500;
    private const uint PortResetPollMs = 10;

    /// <summary>
    /// Feature cleared for each wPortChange bit a USB 2.0 hub sets:
    /// C_PORT_CONNECTION, C_PORT_ENABLE, C_PORT_SUSPEND, C_PORT_OVER_CURRENT,
    /// C_PORT_RESET. A change left set is reported again and again.
    /// </summary>
    private static ReadOnlySpan<byte> HighSpeedChangeFeatures => [16, 17, 18, 19, 20];

    /// <summary>
    /// The same for a SuperSpeed hub (USB 3.2 §10.16.2.6.2): C_PORT_CONNECTION,
    /// none, none, C_PORT_OVER_CURRENT, C_PORT_RESET, C_BH_PORT_RESET,
    /// C_PORT_LINK_STATE, C_PORT_CONFIG_ERROR.
    /// </summary>
    private static ReadOnlySpan<byte> SuperSpeedChangeFeatures => [16, 0, 0, 19, 20, 29, 25, 26];

    /// <summary>
    /// Feature cleared for each wHubChange bit (USB 2.0 §11.24.2.6):
    /// C_HUB_LOCAL_POWER, C_HUB_OVER_CURRENT.
    /// </summary>
    private static ReadOnlySpan<byte> HubChangeFeatures => [0, 1];

    /// <summary>Ports the status change endpoint reported and the hot-plug thread has not looked at yet, as its bitmap.</summary>
    private uint _pendingChanges;

    /// <param name="device">The hub.</param>
    /// <param name="portCount">bNbrPorts from its hub descriptor.</param>
    public UsbHub(UsbDevice device, byte portCount)
    {
        Device = device;
        PortCount = portCount;
        IsSuperSpeed = device.Speed >= UsbSpeed.Super;
    }

    public UsbDevice Device { get; }
    public byte PortCount { get; }
    public bool IsSuperSpeed { get; }

    /// <summary>Last port a device can be enumerated on.</summary>
    private int LastPort => Math.Min((int)PortCount, MaxRoutablePort);

    /// <summary>Powers every port, waits for power to be good and the connections to settle.</summary>
    /// <param name="powerOnToPowerGoodMs">bPwrOn2PwrGood from the hub descriptor, in milliseconds.</param>
    public void PowerPorts(uint powerOnToPowerGoodMs)
    {
        for (int port = 1; port <= PortCount; port++)
        {
            SetPortFeature((byte)port, PortFeaturePower);
        }

        UsbManager.DelayMilliseconds(powerOnToPowerGoodMs + ConnectDebounceMs);
    }

    /// <summary>Enumerates the device on every connected port.</summary>
    public void ProbePorts()
    {
        for (int port = 1; port <= LastPort; port++)
        {
            ProbePort((byte)port);
        }
    }

    /// <summary>
    /// Starts listening on the status change endpoint, so ports that change
    /// from now on are handled by the hot-plug thread.
    /// </summary>
    /// <returns><see langword="false"/> when the hub has no such endpoint or it could not be opened.</returns>
    public bool ListenForChanges(UsbInterface usbInterface)
    {
        UsbEndpoint? endpoint = usbInterface.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        return endpoint is not null && Device.OpenInterruptPipe(endpoint, OnStatusChange);
    }

    /// <summary>
    /// Handles every port the status change endpoint reported since the
    /// last call: the device that was on it is disconnected, and one now on
    /// it is enumerated. Hot-plug thread only.
    /// </summary>
    public void HandlePortChanges()
    {
        uint changed = Interlocked.Exchange(ref _pendingChanges, 0);
        if ((changed & HubChangeBit) != 0)
        {
            ClearHubChanges();
        }

        for (int port = 1; port <= LastPort && !Device.IsDisconnected; port++)
        {
            if ((changed & (1u << port)) != 0)
            {
                HandlePortChange((byte)port);
            }
        }
    }

    /// <summary>Status change endpoint handler; runs in interrupt context.</summary>
    private void OnStatusChange(ReadOnlySpan<byte> bitmap)
    {
        uint changed = 0;
        for (int i = 0; i < bitmap.Length && i < sizeof(uint); i++)
        {
            changed |= (uint)bitmap[i] << (i * 8);
        }

        if (changed != 0)
        {
            Interlocked.Or(ref _pendingChanges, changed);
            UsbManager.NotifyPortChange();
        }
    }

    private void HandlePortChange(byte port)
    {
        if (!TryGetStatus(PortRecipient, port, out ushort status, out ushort change))
        {
            return;
        }

        ClearPortChanges(port, change);

        // A port the hub disabled on its own, after an error on the bus, lost
        // its device as surely as one that was unplugged (USB 2.0 §11.24.2.7.2.2).
        bool disabledByHub = (change & PortChangeEnable) != 0 && (status & PortStatusEnable) == 0;
        if ((change & PortChangeConnection) == 0 && !disabledByHub)
        {
            return;
        }

        UsbManager.DisconnectPort(Device.HostController, Device, port);
        if ((status & PortStatusConnection) != 0 && WaitForStableConnection(port))
        {
            ProbePort(port);
        }
    }

    /// <summary>Waits out the connect debounce; false when the device went away meanwhile.</summary>
    private bool WaitForStableConnection(byte port)
    {
        UsbManager.DelayMilliseconds(ConnectDebounceMs);
        return TryGetStatus(PortRecipient, port, out ushort status, out _) && (status & PortStatusConnection) != 0;
    }

    private void ProbePort(byte port)
    {
        if (!TryGetStatus(PortRecipient, port, out ushort status, out _) || (status & PortStatusConnection) == 0)
        {
            return;
        }

        ClearPortFeature(port, PortFeatureConnectionChange);

        // A USB 2.0 port only enables through a reset; a SuperSpeed port
        // trains its link on connect and needs one only when it did not.
        if ((!IsSuperSpeed || (status & PortStatusEnable) == 0) && !ResetPort(port, out status))
        {
            Log("port reset timed out\n");
            return;
        }

        if ((status & PortStatusEnable) == 0)
        {
            Log("port did not enable after reset\n");
            return;
        }

        UsbSpeed speed = IsSuperSpeed ? UsbSpeed.Super
            : (status & PortStatusLowSpeed) != 0 ? UsbSpeed.Low
            : (status & PortStatusHighSpeed) != 0 ? UsbSpeed.High
            : UsbSpeed.Full;

        UsbManager.DelayMilliseconds(ResetRecoveryMs);
        UsbManager.EnumerateDevice(Device.HostController, Device, port, speed);
    }

    private bool ResetPort(byte port, out ushort status)
    {
        status = 0;
        if (SetPortFeature(port, PortFeatureReset) != UsbTransferStatus.Success)
        {
            return false;
        }

        for (uint elapsedMs = 0; elapsedMs < PortResetTimeoutMs; elapsedMs += PortResetPollMs)
        {
            UsbManager.DelayMilliseconds(PortResetPollMs);
            if (!TryGetStatus(PortRecipient, port, out status, out ushort change))
            {
                return false;
            }

            if ((change & PortChangeReset) != 0 && (status & PortStatusReset) == 0)
            {
                ClearPortFeature(port, PortFeatureResetChange);
                return true;
            }
        }

        return false;
    }

    /// <summary>Clears every change bit of a port, so the hub stops reporting it.</summary>
    private void ClearPortChanges(byte port, ushort change)
    {
        ReadOnlySpan<byte> features = IsSuperSpeed ? SuperSpeedChangeFeatures : HighSpeedChangeFeatures;
        for (int bit = 0; bit < features.Length; bit++)
        {
            if ((change & (1 << bit)) != 0 && features[bit] != 0)
            {
                ClearPortFeature(port, features[bit]);
            }
        }
    }

    /// <summary>Clears the hub's own change bits (local power, over-current).</summary>
    private void ClearHubChanges()
    {
        if (!TryGetStatus(HubRecipient, 0, out _, out ushort change))
        {
            return;
        }

        ReadOnlySpan<byte> features = HubChangeFeatures;
        for (int bit = 0; bit < features.Length; bit++)
        {
            if ((change & (1 << bit)) != 0)
            {
                Device.ControlOut(HubRecipient, (byte)UsbStandardRequest.ClearFeature, features[bit], 0);
            }
        }
    }

    private bool TryGetStatus(UsbRequestType recipient, byte port, out ushort status, out ushort change)
    {
        Span<byte> data = stackalloc byte[StatusLength];
        if (Device.ControlIn(recipient, (byte)UsbStandardRequest.GetStatus, 0, port, data) != UsbTransferStatus.Success)
        {
            status = 0;
            change = 0;
            return false;
        }

        status = (ushort)(data[0] | (data[1] << 8));
        change = (ushort)(data[2] | (data[3] << 8));
        return true;
    }

    private UsbTransferStatus SetPortFeature(byte port, ushort feature) =>
        Device.ControlOut(PortRecipient, (byte)UsbStandardRequest.SetFeature, feature, port);

    private UsbTransferStatus ClearPortFeature(byte port, ushort feature) =>
        Device.ControlOut(PortRecipient, (byte)UsbStandardRequest.ClearFeature, feature, port);

    private void Log(string message) => UsbHubDriver.Log(Device, message);
}
