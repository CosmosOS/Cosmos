using System;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.UDP;
using Cosmos.Kernel.System.Network.IPv4.UDP.DHCP;
using Cosmos.Kernel.System.Network.IPv4.UDP.DNS;
using DevKernel.Network;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Bring-up and smoke tests for the IPv4 stack on the primary NIC.
/// </summary>
internal static class NetworkCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Network";

    /// <summary>UDP port used for the netsend/netlisten test traffic.</summary>
    private const ushort TestUdpPort = 5555;

    /// <summary>Sentinel returned by <see cref="DHCPClient.SendDiscoverPacket"/> when no server answered before the timeout.</summary>
    private const int DhcpTimeoutResult = -1;

    /// <summary>Octet value of the Cloudflare public DNS resolver 1.1.1.1.</summary>
    private const byte CloudflareDnsOctet = 1;

    /// <summary>Timeout (ms) when waiting for a DNS response.</summary>
    private const int DnsReceiveTimeoutMs = 5000;

    /// <summary>Payload sent by the netsend test packet.</summary>
    private const string TestPacketMessage = "Hello from CosmosOS!";

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "netconfig",
                Usage = "netconfig",
                Description = "Configure network stack",
                Execute = static (context, args) => ConfigureNetwork(context.Network),
            },
            new ShellCommand
            {
                Name = "netinfo",
                Usage = "netinfo",
                Description = "Show network device info",
                Execute = static (context, args) => ShowNetworkInfo(context.Network),
            },
            new ShellCommand
            {
                Name = "netsend",
                Usage = "netsend",
                Description = "Send UDP test packet",
                Execute = static (context, args) => SendTestPacket(context.Network),
            },
            new ShellCommand
            {
                Name = "netlisten",
                Usage = "netlisten",
                Description = "Listen for UDP packets",
                Execute = static (context, args) => StartListening(context.Network),
            },
            new ShellCommand
            {
                Name = "dhcp",
                Usage = "dhcp",
                Description = "Auto-configure network via DHCP",
                Execute = static (context, args) => RunDhcp(context.Network),
            },
            new ShellCommand
            {
                Name = "dns",
                Usage = "dns <domain>",
                Description = "Resolve domain name to IP",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => ResolveDns(context.Network, args[0]),
            });
    }

    /// <summary>Returns the primary NIC, reporting its absence when there is none.</summary>
    private static INetworkDevice? RequireDevice()
    {
        INetworkDevice? device = NetworkManager.PrimaryDevice;
        if (device == null)
        {
            Terminal.Error("No network device found");
        }

        return device;
    }

    private static void ConfigureNetwork(NetworkSession session)
    {
        INetworkDevice? device = RequireDevice();
        if (device == null)
        {
            return;
        }

        session.ConfigureStatic(device);

        Terminal.Success("Network configured!\n");
        Terminal.InfoLine("IP", session.LocalIp!.ToString());
        Terminal.InfoLine("Gateway", session.GatewayIp!.ToString());
    }

    private static void ShowNetworkInfo(NetworkSession session)
    {
        INetworkDevice? device = RequireDevice();
        if (device == null)
        {
            return;
        }

        Terminal.Header("Network Information:");

        Terminal.InfoLine("Device", device.Name);
        Terminal.InfoLine("MAC", device.MacAddress.ToString());
        Terminal.StatusLine("Link", device.LinkUp ? "UP" : "DOWN", device.LinkUp ? ConsoleColor.Green : ConsoleColor.Red);
        Terminal.StatusLine("Ready", device.Ready ? "YES" : "NO", device.Ready ? ConsoleColor.Green : ConsoleColor.Red);
        Terminal.StatusLine(
            "Configured",
            session.IsConfigured ? "YES" : "NO",
            session.IsConfigured ? ConsoleColor.Green : ConsoleColor.Red);

        if (session.IsConfigured && session.LocalIp != null)
        {
            Terminal.InfoLine("IP Address", session.LocalIp.ToString());
        }
    }

    private static void SendTestPacket(NetworkSession session)
    {
        INetworkDevice? device = RequireDevice();
        if (device == null)
        {
            return;
        }

        if (!device.Ready)
        {
            Terminal.Error("Network device not ready");
            return;
        }

        if (!session.IsConfigured)
        {
            ConfigureNetwork(session);
        }

        byte[] payload = new byte[TestPacketMessage.Length];
        for (int i = 0; i < TestPacketMessage.Length; i++)
        {
            payload[i] = (byte)TestPacketMessage[i];
        }

        // Broadcast MAC stands in for the ARP resolution the stack does not do yet.
        UDPPacket packet = new(
            session.LocalIp!,
            session.GatewayIp!,
            TestUdpPort,
            TestUdpPort,
            payload,
            MACAddress.Broadcast);

        Terminal.Info("Sending UDP packet to " + session.GatewayIp!.ToString() + ":" + TestUdpPort + "...");
        bool sent = device.Send(packet.RawData, packet.RawData.Length);

        if (sent)
        {
            Terminal.Success("Packet sent!\n");
        }
        else
        {
            Terminal.Error("Failed to send packet\n");
        }
    }

    private static void StartListening(NetworkSession session)
    {
        INetworkDevice? device = RequireDevice();
        if (device == null)
        {
            return;
        }

        if (!session.IsConfigured)
        {
            ConfigureNetwork(session);
        }

        Terminal.Info("Listening for UDP packets on port " + TestUdpPort + "...");
        Terminal.Hint("Send from host: echo 'test' | nc -u localhost " + TestUdpPort);
    }

    private static void RunDhcp(NetworkSession session)
    {
        INetworkDevice? device = RequireDevice();
        if (device == null)
        {
            return;
        }

        if (!device.Ready)
        {
            Terminal.Error("Network device not ready");
            return;
        }

        Terminal.Info("Starting DHCP auto-configuration...");

        NetworkStack.Initialize();

        DHCPClient dhcpClient = new();
        if (dhcpClient.SendDiscoverPacket() == DhcpTimeoutResult)
        {
            Terminal.Error("DHCP timeout - no response from server");
            return;
        }

        IPConfig? netConfig = NetworkConfigManager.Get(device);
        if (netConfig == null)
        {
            Terminal.Error("No network configuration after DHCP");
            return;
        }

        session.AdoptLease(netConfig.IPAddress, netConfig.DefaultGateway);

        Terminal.Success("DHCP configuration successful!");
        Terminal.InfoLine("IP Address", netConfig.IPAddress.ToString());
        Terminal.InfoLine("Subnet", netConfig.SubnetMask.ToString());
        Terminal.InfoLine("Gateway", netConfig.DefaultGateway.ToString());
        Console.WriteLine();
    }

    private static void ResolveDns(NetworkSession session, string domain)
    {
        if (RequireDevice() == null)
        {
            return;
        }

        if (!session.IsConfigured)
        {
            Terminal.Error("Network not configured. Run 'dhcp' or 'netconfig' first.");
            return;
        }

        Terminal.Info("Resolving " + domain + "...");

        Address dnsServer = new(CloudflareDnsOctet, CloudflareDnsOctet, CloudflareDnsOctet, CloudflareDnsOctet);
        DNSConfig.Add(dnsServer);

        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);
        dnsClient.SendAsk(domain);

        Address? resolvedIP = dnsClient.Receive(DnsReceiveTimeoutMs);

        // TODO support IPv6
        if (resolvedIP != null && resolvedIP != Address4.Zero)
        {
            Terminal.Success(domain + " -> " + resolvedIP.ToString());
        }
        else
        {
            Terminal.Error("DNS resolution failed or timed out");
        }

        dnsClient.Close();
        Console.WriteLine();
    }
}
