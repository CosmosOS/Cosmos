// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Hub class driver (USB 2.0 chapter 11, USB 3.2 chapter 10). Registers the
/// hub with the host controller, then hands it to a <see cref="UsbHub"/>,
/// which powers and resets every port, enumerates the devices behind them,
/// and follows the ports that change afterwards.
/// </summary>
internal sealed class UsbHubDriver : UsbClassDriver
{
    /// <summary>SET_HUB_DEPTH (USB 3.2 §10.16.2.9).</summary>
    private const byte SetHubDepthRequest = 0x0C;

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

    private const uint PowerOnToPowerGoodUnitMs = 2;

    /// <summary>Hubs bound so far. Changed by the boot path, then by the hot-plug thread only.</summary>
    private static List<UsbHub>? s_hubs;

    /// <summary>
    /// The driver's <see cref="Name"/>, which the driver kit also refuses as
    /// a registration name: the device list names an interface's owner by it.
    /// </summary>
    internal const string DriverName = "hub";

    public override string Name => DriverName;

    /// <summary>
    /// Lets every hub handle the ports its status change endpoint reported.
    /// Hot-plug thread only.
    /// </summary>
    public static void HandlePortChanges()
    {
        if (s_hubs is null)
        {
            return;
        }

        // A hub handling its ports may enumerate another hub or disconnect
        // one, which changes the list; work from a copy.
        foreach (UsbHub hub in s_hubs.ToArray())
        {
            if (!hub.Device.IsDisconnected)
            {
                hub.HandlePortChanges();
            }
        }
    }

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

        UsbHub hub = new(device, portCount);
        hub.PowerPorts(descriptor[PowerOnToPowerGoodOffset] * PowerOnToPowerGoodUnitMs);
        hub.ProbePorts();

        // Listening only after the probe keeps the changes it made (the
        // resets, the connections it cleared) from coming back as reports;
        // a device plugged in meanwhile leaves its change bit set, which the
        // hub reports as soon as the endpoint is polled.
        if (!hub.ListenForChanges(usbInterface))
        {
            Log(device, "status change endpoint unavailable, ports will not be followed\n");
        }

        (s_hubs ??= []).Add(hub);
        return true;
    }

    public override void Disconnect(UsbDevice device, UsbInterface usbInterface)
    {
        if (s_hubs is null)
        {
            return;
        }

        for (int i = 0; i < s_hubs.Count; i++)
        {
            if (s_hubs[i].Device == device)
            {
                s_hubs.RemoveAt(i);
                return;
            }
        }
    }

    internal static void Log(UsbDevice hub, string message)
    {
        Serial.WriteString("[USB] hub ");
        Serial.WriteHex((uint)hub.VendorId);
        Serial.WriteString(":");
        Serial.WriteHex((uint)hub.ProductId);
        Serial.WriteString(": ");
        Serial.WriteString(message);
    }
}
