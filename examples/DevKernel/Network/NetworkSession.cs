using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;

namespace DevKernel.Network;

/// <summary>
/// The IPv4 configuration the shell applied to the primary NIC. One instance
/// lives for the whole shell session.
/// </summary>
internal sealed class NetworkSession
{
    /// <summary>First octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet1 = 10;

    /// <summary>Second octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet2 = 0;

    /// <summary>Third octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet3 = 2;

    /// <summary>Host octet of the default QEMU guest IP (10.0.2.15).</summary>
    private const byte QemuGuestHostOctet = 15;

    /// <summary>Host octet of the QEMU user-networking gateway IP (10.0.2.2).</summary>
    private const byte QemuGatewayHostOctet = 2;

    /// <summary>Fully-masked octet of the /24 subnet mask (255.255.255.0).</summary>
    private const byte SubnetMaskFullOctet = 255;

    /// <summary>Unmasked host octet of the /24 subnet mask (255.255.255.0).</summary>
    private const byte SubnetMaskHostOctet = 0;

    /// <summary>Address assigned to this machine, once configured.</summary>
    public Address? LocalIp { get; private set; }

    /// <summary>Default gateway, once configured.</summary>
    public Address? GatewayIp { get; private set; }

    /// <summary>True once either <see cref="ConfigureStatic"/> or <see cref="AdoptLease"/> has run.</summary>
    public bool IsConfigured { get; private set; }

    /// <summary>
    /// Brings up the stack with the static QEMU user-networking address plan.
    /// The subnet and gateway are passed through so <c>IPConfig.FindNetwork()</c>
    /// can route outbound packets.
    /// </summary>
    /// <returns>Whether an adapter took the configuration.</returns>
    public bool ConfigureStatic()
    {
        LocalIp = new Address(QemuNetOctet1, QemuNetOctet2, QemuNetOctet3, QemuGuestHostOctet);
        GatewayIp = new Address(QemuNetOctet1, QemuNetOctet2, QemuNetOctet3, QemuGatewayHostOctet);
        Address subnet = new(SubnetMaskFullOctet, SubnetMaskFullOctet, SubnetMaskFullOctet, SubnetMaskHostOctet);

        IsConfigured = IPConfig.Enable(LocalIp, subnet, GatewayIp);
        return IsConfigured;
    }

    /// <summary>Records the addresses a DHCP server handed out.</summary>
    public void AdoptLease(Address localIp, Address gatewayIp)
    {
        LocalIp = localIp;
        GatewayIp = gatewayIp;

        IsConfigured = true;
    }
}
