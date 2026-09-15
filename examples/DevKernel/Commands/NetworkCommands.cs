using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.DNS;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.DHCP;
using Cosmos.Kernel.System.Network.UDP;
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

    /// <summary>Sentinel returned by <see cref="DhcpClient.SendDiscoverPacket"/> when no server answered before the timeout.</summary>
    private const int DhcpTimeoutResult = -1;

    /// <summary>Octet value of the Cloudflare public DNS resolver 1.1.1.1.</summary>
    private const byte CloudflareDnsOctet = 1;

    /// <summary>Timeout (ms) when waiting for a DNS response.</summary>
    private const int DnsReceiveTimeoutMs = 5000;

    /// <summary>Payload sent by the netsend test packet.</summary>
    private const string TestPacketMessage = "Hello from CosmosOS!";

    /// <summary>Maximum UDP payload bytes echoed to the console per datagram.</summary>
    private const int UdpPreviewMaxBytes = 64;

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

    /// <summary>Reports the absence of a primary NIC, and whether one is present.</summary>
    private static bool RequireDevice()
    {
        if (NetworkManager.DeviceCount == 0)
        {
            Terminal.Error("No network device found");
            return false;
        }

        return true;
    }

    private static void ConfigureNetwork(NetworkSession session)
    {
        if (!RequireDevice())
        {
            return;
        }

        if (!session.ConfigureStatic())
        {
            Terminal.Error("No adapter took the configuration");
            return;
        }

        Terminal.Success("Network configured!\n");
        Terminal.InfoLine("IP", session.LocalIp!.ToString());
        Terminal.InfoLine("Gateway", session.GatewayIp!.ToString());
    }

    private static void ShowNetworkInfo(NetworkSession session)
    {
        if (!RequireDevice())
        {
            return;
        }

        Terminal.Header("Network Information:");

        Terminal.InfoLine("Device", NetworkManager.Name!);
        Terminal.InfoLine("MAC", NetworkManager.MacAddress!.ToString());
        Terminal.StatusLine("Link", NetworkManager.LinkUp ? "UP" : "DOWN", NetworkManager.LinkUp ? ConsoleColor.Green : ConsoleColor.Red);
        Terminal.StatusLine("Ready", NetworkManager.Ready ? "YES" : "NO", NetworkManager.Ready ? ConsoleColor.Green : ConsoleColor.Red);
        Terminal.StatusLine(
            "Configured",
            session.IsConfigured ? "YES" : "NO",
            session.IsConfigured ? ConsoleColor.Green : ConsoleColor.Red);

        if (NetworkManager.DeviceCount > 1)
        {
            Terminal.Header("Adapters:");
            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                NetworkAdapter adapter = NetworkManager.GetAdapter(i);
                string marker = adapter == NetworkManager.Primary ? " (primary)" : string.Empty;
                Terminal.InfoLine($"[{i}] {adapter.Name}", $"{adapter.MacAddress}{marker}");
            }
        }

        if (session.IsConfigured && session.LocalIp != null)
        {
            Terminal.InfoLine("IP Address", session.LocalIp.ToString());
        }
    }

    private static void SendTestPacket(NetworkSession session)
    {
        if (!RequireDevice())
        {
            return;
        }

        if (!NetworkManager.Ready)
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

        Terminal.Info("Sending UDP packet to " + session.GatewayIp!.ToString() + ":" + TestUdpPort + "...");

        using (UdpClient client = new(TestUdpPort))
        {
            client.Connect(session.GatewayIp!, TestUdpPort);
            client.Send(payload);
        }

        // UdpClient.Send is void: the datagram is handed to the outgoing
        // queue, which resolves ARP and hits the NIC without reporting back.
        // Say what we know rather than claiming delivery.
        Terminal.Success("Packet queued for transmission\n");
    }

    private static void StartListening(NetworkSession session)
    {
        if (!RequireDevice())
        {
            return;
        }

        if (!session.IsConfigured)
        {
            ConfigureNetwork(session);
        }

        Terminal.Info("Listening for UDP packets on port " + TestUdpPort + "... (Esc to stop)");
        Terminal.Hint("Send from host: echo 'test' | nc -u localhost " + TestUdpPort);

        using (UdpClient client = new(TestUdpPort))
        {
            EndPoint source = new(Address4.Zero, 0);

            while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                byte[]? data = client.Receive(ref source, timeoutMs: 0);
                if (data is not null)
                {
                    PrintDatagram(source, data);
                }
            }
        }
    }

    /// <summary>Logs the full payload to serial, and a printable preview to the console.</summary>
    private static void PrintDatagram(EndPoint source, byte[] data)
    {
        Log.Write("[UDP] Received datagram from ");
        Log.WriteString(source.Address.ToString());
        Log.Write(":");
        Log.WriteNumber((ulong)source.Port);
        Log.Write(" -> port ");
        Log.WriteNumber((ulong)TestUdpPort);
        Log.Write("\n");

        Log.Write("[UDP] Payload (");
        Log.WriteNumber((ulong)data.Length);
        Log.Write(" bytes): ");

        for (int i = 0; i < data.Length; i++)
        {
            char c = (char)data[i];
            if (Ascii.IsPrintable(c))
            {
                Log.Write(c.ToString());
            }
        }

        Log.Write("\n");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("[UDP] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(source.Address.ToString() + ":" + source.Port.ToString());
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" -> ");
        Console.ResetColor();

        for (int i = 0; i < data.Length && i < UdpPreviewMaxBytes; i++)
        {
            char c = (char)data[i];
            if (Ascii.IsPrintable(c))
            {
                Console.Write(c.ToString());
            }
        }

        Console.WriteLine();
    }

    private static void RunDhcp(NetworkSession session)
    {
        if (!RequireDevice())
        {
            return;
        }

        if (!NetworkManager.Ready)
        {
            Terminal.Error("Network device not ready");
            return;
        }

        Terminal.Info("Starting DHCP auto-configuration...");

        DhcpClient dhcpClient = new();
        if (dhcpClient.SendDiscoverPacket() == DhcpTimeoutResult)
        {
            Terminal.Error("DHCP timeout - no response from server");
            return;
        }

        IPConfig? netConfig = NetworkManager.Primary.IPConfig;
        if (netConfig == null)
        {
            Terminal.Error("No network configuration after DHCP");
            return;
        }

        session.AdoptLease(netConfig.Address, netConfig.DefaultGateway);

        Terminal.Success("DHCP configuration successful!");
        Terminal.InfoLine("IP Address", netConfig.Address.ToString());
        Terminal.InfoLine("Subnet", netConfig.SubnetMask.ToString());
        Terminal.InfoLine("Gateway", netConfig.DefaultGateway.ToString());
        Console.WriteLine();
    }

    private static void ResolveDns(NetworkSession session, string domain)
    {
        if (!RequireDevice())
        {
            return;
        }

        if (!session.IsConfigured)
        {
            Terminal.Error("Network not configured. Run 'dhcp' or 'netconfig' first.");
            return;
        }

        Terminal.Info("Resolving " + domain + "...");

        Address4 dnsServer = new(CloudflareDnsOctet, CloudflareDnsOctet, CloudflareDnsOctet, CloudflareDnsOctet);
        DnsConfig.Add(dnsServer);

        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);
        dnsClient.SendQuery(domain);

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
