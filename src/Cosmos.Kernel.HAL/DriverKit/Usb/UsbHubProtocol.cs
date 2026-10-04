// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The protocol numbers of the hub class (USB 2.0 chapter 11, USB 3.2
/// chapter 10), so a hub driver carries numbers, not a copy of the kit's
/// state machine. Constants; any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public static class UsbHubProtocol
{
    /// <summary>Bytes of the hub descriptor read, through bPwrOn2PwrGood (USB 2.0 section 11.23.2.1; the SuperSpeed hub descriptor of USB 3.2 section 10.15.2.1 keeps these offsets).</summary>
    public const int HubDescriptorLength = 7;

    /// <summary>bNbrPorts in the hub descriptor.</summary>
    public const int PortCountOffset = 2;

    /// <summary>wHubCharacteristics in the hub descriptor.</summary>
    public const int CharacteristicsOffset = 3;

    /// <summary>wHubCharacteristics bits 6:5, TT think time: the shift.</summary>
    public const int ThinkTimeShift = 5;

    /// <summary>wHubCharacteristics bits 6:5, TT think time: the mask after the shift.</summary>
    public const int ThinkTimeMask = 0x3;

    /// <summary>bPwrOn2PwrGood in the hub descriptor.</summary>
    public const int PowerOnToPowerGoodOffset = 5;

    /// <summary>The unit of bPwrOn2PwrGood, in milliseconds.</summary>
    public const uint PowerOnToPowerGoodUnitMs = 2;

    /// <summary>SET_HUB_DEPTH (USB 3.2 section 10.16.2.9).</summary>
    public const byte SetHubDepthRequest = 0x0C;

    /// <summary>Port feature selector PORT_RESET (USB 2.0 table 11-17, USB 3.2 table 10-9).</summary>
    public const ushort PortFeatureReset = 4;

    /// <summary>Port feature selector PORT_POWER.</summary>
    public const ushort PortFeaturePower = 8;

    /// <summary>Port feature selector C_PORT_CONNECTION.</summary>
    public const ushort PortFeatureConnectionChange = 16;

    /// <summary>Port feature selector C_PORT_RESET.</summary>
    public const ushort PortFeatureResetChange = 20;

    /// <summary>wPortStatus bit PORT_CONNECTION (USB 2.0 section 11.24.2.7.1; bits 0, 1 and 4 mean the same on a SuperSpeed hub).</summary>
    public const ushort PortStatusConnection = 1 << 0;

    /// <summary>wPortStatus bit PORT_ENABLE.</summary>
    public const ushort PortStatusEnable = 1 << 1;

    /// <summary>wPortStatus bit PORT_RESET.</summary>
    public const ushort PortStatusReset = 1 << 4;

    /// <summary>wPortStatus bit PORT_LOW_SPEED (USB 2.0 hubs).</summary>
    public const ushort PortStatusLowSpeed = 1 << 9;

    /// <summary>wPortStatus bit PORT_HIGH_SPEED (USB 2.0 hubs).</summary>
    public const ushort PortStatusHighSpeed = 1 << 10;

    /// <summary>wPortChange bit C_PORT_CONNECTION (USB 2.0 section 11.24.2.7.2).</summary>
    public const ushort PortChangeConnection = 1 << 0;

    /// <summary>wPortChange bit C_PORT_ENABLE (not defined on a SuperSpeed hub).</summary>
    public const ushort PortChangeEnable = 1 << 1;

    /// <summary>wPortChange bit C_PORT_RESET.</summary>
    public const ushort PortChangeReset = 1 << 4;

    /// <summary>The request type of a hub class request to the hub itself (USB 2.0 section 11.24.2).</summary>
    public const UsbRequestType HubRecipient = UsbRequestType.Class | UsbRequestType.Device;

    /// <summary>The request type of a hub class request to one of its ports.</summary>
    public const UsbRequestType PortRecipient = UsbRequestType.Class | UsbRequestType.Other;

    /// <summary>GET_STATUS on the hub or a port returns the status word, then the change word.</summary>
    public const int StatusLength = 4;

    /// <summary>Status change bitmap bit 0 is the hub itself; bit N is port N (USB 2.0 section 11.12.4).</summary>
    public const uint HubChangeBit = 1;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 section 8.9), so higher ports are unreachable.</summary>
    public const int MaxRoutablePort = 15;

    /// <summary>TATTDB: a connection must be stable this long, in milliseconds, before the port is reset (USB 2.0 section 7.1.7.3).</summary>
    public const uint ConnectDebounceMs = 100;

    /// <summary>TRSTRCY: recovery after a reset, in milliseconds, before the device must answer (USB 2.0 section 7.1.7.5).</summary>
    public const uint ResetRecoveryMs = 10;

    /// <summary>How long a port reset may take to complete, in milliseconds.</summary>
    public const uint PortResetTimeoutMs = 500;

    /// <summary>How long to wait between two looks at a resetting port, in milliseconds.</summary>
    public const uint PortResetPollMs = 10;

    /// <summary>
    /// Feature cleared for each wPortChange bit a USB 2.0 hub sets:
    /// C_PORT_CONNECTION, C_PORT_ENABLE, C_PORT_SUSPEND, C_PORT_OVER_CURRENT,
    /// C_PORT_RESET. A change left set is reported again and again.
    /// </summary>
    public static ReadOnlySpan<byte> HighSpeedChangeFeatures => [16, 17, 18, 19, 20];

    /// <summary>
    /// The same for a SuperSpeed hub (USB 3.2 section 10.16.2.6.2):
    /// C_PORT_CONNECTION, none, none, C_PORT_OVER_CURRENT, C_PORT_RESET,
    /// C_BH_PORT_RESET, C_PORT_LINK_STATE, C_PORT_CONFIG_ERROR.
    /// </summary>
    public static ReadOnlySpan<byte> SuperSpeedChangeFeatures => [16, 0, 0, 19, 20, 29, 25, 26];

    /// <summary>
    /// Feature cleared for each wHubChange bit (USB 2.0 section 11.24.2.6):
    /// C_HUB_LOCAL_POWER, C_HUB_OVER_CURRENT.
    /// </summary>
    public static ReadOnlySpan<byte> HubChangeFeatures => [0, 1];
}
