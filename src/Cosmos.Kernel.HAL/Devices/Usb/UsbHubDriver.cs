// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Hub class driver (USB 2.0 chapter 11, USB 3.2 chapter 10). Registers the
/// hub with the host controller, powers and resets every port, and
/// enumerates the devices behind them. The status-change endpoint is not
/// used: ports are probed once, at boot, by GET_STATUS on the default pipe.
/// </summary>
internal sealed class UsbHubDriver : UsbDriver
{
    /// <summary>SET_HUB_DEPTH (USB 3.2 §10.16.2.9).</summary>
    private const byte SetHubDepthRequest = 0x0C;

    // Port feature selectors (USB 2.0 table 11-17).
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

    /// <summary>wPortChange C_PORT_RESET (USB 2.0 §11.24.2.7.2).</summary>
    private const ushort PortChangeReset = 1 << 4;

    /// <summary>GET_STATUS on a port returns wPortStatus then wPortChange.</summary>
    private const int PortStatusLength = 4;

    // Hub descriptor (USB 2.0 §11.23.2.1; the SuperSpeed hub descriptor of
    // USB 3.2 §10.15.2.1 keeps these offsets). Only the fields through
    // bPwrOn2PwrGood are read.
    private const int HubDescriptorLength = 7;
    private const int PortCountOffset = 2;
    private const int CharacteristicsOffset = 3;
    private const int PowerOnToPowerGoodOffset = 5;

    /// <summary>wHubCharacteristics bits 6:5, TT think time.</summary>
    private const int ThinkTimeShift = 5;
    private const int ThinkTimeMask = 0x3;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 §8.9), so higher ports are unreachable.</summary>
    private const int MaxRoutablePort = 15;

    private const uint PowerOnToPowerGoodUnitMs = 2;

    /// <summary>TATTDB: connect debounce before a port may be reset (USB 2.0 §7.1.7.3).</summary>
    private const uint ConnectDebounceMs = 100;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 §7.1.7.5).</summary>
    private const uint ResetRecoveryMs = 10;

    private const uint PortResetTimeoutMs = 500;
    private const uint PortResetPollMs = 10;

    public override string Name => "hub";

    public override bool TryBind(UsbDevice device, UsbInterface usbInterface)
    {
        if (usbInterface.Class != UsbClassCode.Hub)
        {
            return false;
        }

        bool superSpeed = device.Speed >= UsbSpeed.Super;
        UsbDescriptorType descriptorType = superSpeed ? UsbDescriptorType.SuperSpeedHub : UsbDescriptorType.Hub;
        Span<byte> descriptor = stackalloc byte[HubDescriptorLength];
        UsbTransferStatus status = device.ControlIn(UsbRequestType.Class | UsbRequestType.Device,
            (byte)UsbStandardRequest.GetDescriptor, (ushort)((byte)descriptorType << 8), 0, descriptor);
        if (status != UsbTransferStatus.Success)
        {
            Log(device, "could not read the hub descriptor\n");
            return false;
        }

        byte portCount = descriptor[PortCountOffset];
        int characteristics = descriptor[CharacteristicsOffset] | (descriptor[CharacteristicsOffset + 1] << 8);
        byte thinkTime = superSpeed ? (byte)0 : (byte)((characteristics >> ThinkTimeShift) & ThinkTimeMask);

        if (!device.ConfigureAsHub(portCount, thinkTime))
        {
            Log(device, "host controller refused the hub configuration\n");
            return false;
        }

        // A SuperSpeed hub routes by the route-string nibble of its own tier,
        // which it has to be told before any downstream traffic.
        if (superSpeed && device.ControlOut(UsbRequestType.Class | UsbRequestType.Device, SetHubDepthRequest, (ushort)device.HubDepth, 0) != UsbTransferStatus.Success)
        {
            Log(device, "SET_HUB_DEPTH failed\n");
            return false;
        }

        Log(device, "");
        Serial.WriteNumber((uint)portCount);
        Serial.WriteString(" port(s)\n");

        for (int port = 1; port <= portCount; port++)
        {
            SetPortFeature(device, (byte)port, PortFeaturePower);
        }

        UsbManager.DelayMilliseconds((descriptor[PowerOnToPowerGoodOffset] * PowerOnToPowerGoodUnitMs) + ConnectDebounceMs);

        int lastPort = Math.Min((int)portCount, MaxRoutablePort);
        for (int port = 1; port <= lastPort; port++)
        {
            ProbePort(device, (byte)port, superSpeed);
        }

        return true;
    }

    private static void ProbePort(UsbDevice hub, byte port, bool superSpeed)
    {
        if (!TryGetPortStatus(hub, port, out ushort status, out _) || (status & PortStatusConnection) == 0)
        {
            return;
        }

        ClearPortFeature(hub, port, PortFeatureConnectionChange);

        // A USB 2.0 port only enables through a reset; a SuperSpeed port
        // trains its link on connect and needs one only when it did not.
        if ((!superSpeed || (status & PortStatusEnable) == 0) && !ResetPort(hub, port, out status))
        {
            Log(hub, "port reset timed out\n");
            return;
        }

        if ((status & PortStatusEnable) == 0)
        {
            Log(hub, "port did not enable after reset\n");
            return;
        }

        UsbSpeed speed = superSpeed ? UsbSpeed.Super
            : (status & PortStatusLowSpeed) != 0 ? UsbSpeed.Low
            : (status & PortStatusHighSpeed) != 0 ? UsbSpeed.High
            : UsbSpeed.Full;

        UsbManager.DelayMilliseconds(ResetRecoveryMs);
        UsbManager.EnumerateDevice(hub.HostController, hub, port, speed);
    }

    private static bool ResetPort(UsbDevice hub, byte port, out ushort status)
    {
        status = 0;
        if (SetPortFeature(hub, port, PortFeatureReset) != UsbTransferStatus.Success)
        {
            return false;
        }

        for (uint elapsedMs = 0; elapsedMs < PortResetTimeoutMs; elapsedMs += PortResetPollMs)
        {
            UsbManager.DelayMilliseconds(PortResetPollMs);
            if (!TryGetPortStatus(hub, port, out status, out ushort change))
            {
                return false;
            }

            if ((change & PortChangeReset) != 0 && (status & PortStatusReset) == 0)
            {
                ClearPortFeature(hub, port, PortFeatureResetChange);
                return true;
            }
        }

        return false;
    }

    private static bool TryGetPortStatus(UsbDevice hub, byte port, out ushort status, out ushort change)
    {
        Span<byte> data = stackalloc byte[PortStatusLength];
        if (hub.ControlIn(UsbRequestType.Class | UsbRequestType.Other, (byte)UsbStandardRequest.GetStatus, 0, port, data) != UsbTransferStatus.Success)
        {
            status = 0;
            change = 0;
            return false;
        }

        status = (ushort)(data[0] | (data[1] << 8));
        change = (ushort)(data[2] | (data[3] << 8));
        return true;
    }

    private static UsbTransferStatus SetPortFeature(UsbDevice hub, byte port, ushort feature) =>
        hub.ControlOut(UsbRequestType.Class | UsbRequestType.Other, (byte)UsbStandardRequest.SetFeature, feature, port);

    private static UsbTransferStatus ClearPortFeature(UsbDevice hub, byte port, ushort feature) =>
        hub.ControlOut(UsbRequestType.Class | UsbRequestType.Other, (byte)UsbStandardRequest.ClearFeature, feature, port);

    private static void Log(UsbDevice hub, string message)
    {
        Serial.WriteString("[USB] hub ");
        Serial.WriteHex((uint)hub.VendorId);
        Serial.WriteString(":");
        Serial.WriteHex((uint)hub.ProductId);
        Serial.WriteString(": ");
        Serial.WriteString(message);
    }
}
