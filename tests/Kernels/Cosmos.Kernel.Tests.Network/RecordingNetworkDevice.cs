using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.System.Network;

namespace Cosmos.Kernel.Tests.Network;

/// <summary>
/// A second network device for the multi-adapter DHCP cells: registered
/// beside the NIC the profile attached, it counts the frames the stack sends
/// through it and carries none anywhere. White-box: <see cref="INetworkDevice"/>
/// is the stack's internal device contract, which this suite reaches through
/// InternalsVisibleTo.
/// </summary>
internal sealed class RecordingNetworkDevice : INetworkDevice
{
    public string Name { get; }

    public MacAddress MacAddress { get; }

    public bool LinkUp => true;

    public bool Ready => true;

    public PacketReceivedHandler? OnPacketReceived { get; set; }

    /// <summary>How many frames the stack handed to <see cref="Send"/>.</summary>
    public int FramesSent { get; private set; }

    public RecordingNetworkDevice(string name, MacAddress mac)
    {
        Name = name;
        MacAddress = mac;
    }

    public void Initialize()
    {
    }

    public void Enable()
    {
    }

    public void Disable()
    {
    }

    public bool Send(byte[] data, int length)
    {
        FramesSent++;
        return true;
    }
}
