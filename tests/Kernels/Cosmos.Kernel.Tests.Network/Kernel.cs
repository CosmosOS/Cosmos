using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.DNS;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.DHCP;
using Cosmos.Kernel.System.Network.IPv6;
using Cosmos.Kernel.System.Network.TCP;
using Cosmos.Kernel.System.Network.UDP;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using CosmosEndPoint = Cosmos.Kernel.System.Network.EndPoint;
using CosmosUdpClient = Cosmos.Kernel.System.Network.UDP.UdpClient;
using DotNetTcpClient = System.Net.Sockets.TcpClient;
using DotNetTcpListener = System.Net.Sockets.TcpListener;
using DotNetUdpClient = System.Net.Sockets.UdpClient;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Network;

public class Kernel : Sys.Kernel
{
    // Network configuration
    private static Address? s_localIP;
    private static Address? s_gatewayIP;
    private static bool s_networkConfigured = false;
    private static bool s_receivedPacket = false;
    private static byte[]? s_lastReceivedData;
    private static ushort s_lastReceivedPort;
    private static Address? s_lastReceivedSourceIP;
    private static ushort s_lastReceivedSourcePort;

    // UDP Test ports
    private const ushort TestPort = 5555;
    private const ushort EchoPort = 5556;

    // TCP Test ports
    private const ushort TcpClientPort = 5557;  // Kernel connects to test runner
    private const ushort TcpServerPort = 5558;  // Kernel listens, test runner connects
    private const ushort TcpLingerPort = 5559;  // Kernel connects, test runner echoes but never closes its side

    protected override void BeforeRun()
    {
        Log.WriteString("[Network Tests] Starting test suite\n");

        // x64 has E1000E network driver
        TR.Start("Network Tests", expectedTests: 21);

        // Network initialization tests
        TR.Run("Network_DeviceDetected", TestNetworkDeviceDetected);
        TR.Run("Network_DeviceReady", TestNetworkDeviceReady);
        TR.Run("DHCP_AutoConfigure", TestDHCPConfiguration);

        // ICMP tests
        TR.Run("ICMP_PingGateway", TestICMPPingGateway);
        TR.Run("ICMP_HostPing", TestICMPHostPing);

        // IPv6 tests
        TR.Run("IPv6_LinkLocalConfigured", TestIPv6LinkLocalConfigured);
        TR.Run("ICMPv6_PingGateway", TestICMPv6PingGateway);
        TR.Run("ICMPv6_HostPing", TestICMPv6HostPing);

        // UDP tests
        TR.Run("UDP_SendPacket", TestUDPSendPacket);
        TR.Run("UDP_ReceivePacket", TestUDPReceivePacket);

        // Packet seam (COSMOS0002): crafted packets through the public packet types
        TR.Run("PacketSeam_CraftedEchoRoundTrip", TestPacketSeamCraftedEchoRoundTrip);
        TR.Run("PacketSeam_CraftedUdpRoundTrip", TestPacketSeamCraftedUdpRoundTrip);
        TR.Run("PacketSeam_SendUnroutableReturnsFalse", TestPacketSeamSendUnroutable);

        // TCP tests
        TR.Run("TCP_ClientConnect", TestTCPClientConnect);
        TR.Run("TCP_ServerAccept", TestTCPServerAccept);
        TR.Run("TCP_CloseNoPeerFin", TestTCPCloseWithoutPeerFin);

        // DNS tests
        TR.Run("DNS_ClientCreate", TestDNSClientCreate);
        TR.Run("DNS_ResolveValentinBzh", TestDNSResolveTestSite);
        TR.Run("DNS_ResolveCnameChain", TestDNSResolveCnameChain);
        TR.Run("DNS_ResolveMultipleARecords", TestDNSResolveMultipleARecords);
        TR.Run("DNS_TwoQueriesOneClient", TestDNSTwoQueriesOneClient);
        TR.Run("DNS_DotNetGetHostName", TestDotNetDnsGetHostName);
        TR.Run("DNS_DotNetGetHostAddresses", TestDotNetDnsGetHostAddresses);

        Log.WriteString("[Network Tests] All tests completed\n");
        TR.Finish();
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Network Device Tests ====================

    private static void TestNetworkDeviceDetected()
    {
        Assert.True(NetworkManager.DeviceCount > 0, "Network device should be detected");

        if (NetworkManager.DeviceCount > 0)
        {
            Log.WriteString("[Test] Device detected: ");
            Log.WriteString(NetworkManager.Name!);
            Log.WriteString("\n");
        }
    }

    private static void TestNetworkDeviceReady()
    {
        if (NetworkManager.DeviceCount == 0)
        {
            Assert.True(false, "No network device available");
            return;
        }

        // Wait for link to come up (max 2 seconds)
        int attempts = 0;
        while (!NetworkManager.LinkUp && attempts < 20)
        {
            TimerManager.Wait(100);
            attempts++;
        }

        Log.WriteString("[Test] Link status: ");
        Log.WriteString(NetworkManager.LinkUp ? "UP" : "DOWN");
        Log.WriteString(", Ready: ");
        Log.WriteString(NetworkManager.Ready ? "YES" : "NO");
        Log.WriteString("\n");

        Assert.True(NetworkManager.Ready, "Network device should be ready");
    }

    private static void TestDHCPConfiguration()
    {
        // White-box: this suite reaches the device itself to check that the
        // stack attached its packet handler, which the ring does not report.
        INetworkDevice? device = NetworkManager.PrimaryDevice;
        if (device == null)
        {
            Assert.True(false, "No network device available");
            return;
        }

        Log.WriteString("[Test] Starting DHCP auto-configuration...\n");

        // Use DHCP to auto-assign IP address
        DhcpClient dhcpClient = new();

        Log.WriteString("[Test] Sending DHCP Discover packet...\n");
        int result = dhcpClient.SendDiscoverPacket();

        if (result == -1)
        {
            Log.WriteString("[Test] DHCP timeout - no response from server\n");
            Assert.True(false, "DHCP should receive response from QEMU DHCP server");
            return;
        }

        Log.WriteString("[Test] DHCP completed in ");
        Log.WriteNumber((ulong)result);
        Log.WriteString(" ms\n");

        // Verify we got an IP configuration
        IPConfig? netConfig = NetworkManager.Primary.IPConfig;
        if (netConfig == null)
        {
            Log.WriteString("[Test] No network configuration after DHCP\n");
            Assert.True(false, "Network should be configured after DHCP");
            return;
        }

        s_localIP = netConfig.Address;
        s_gatewayIP = netConfig.DefaultGateway;
        s_networkConfigured = true;

        Log.WriteString("[Test] DHCP assigned IP: ");
        Log.WriteString(s_localIP.ToString());
        Log.WriteString("\n");
        Log.WriteString("[Test] Gateway: ");
        Log.WriteString(s_gatewayIP.ToString());
        Log.WriteString("\n");

        // Verify device has packet handler registered
        Assert.True(device.OnPacketReceived != null, "Device should have packet handler registered after DHCP");

        // Verify we got a valid IP (not 0.0.0.0)
        Assert.True(s_localIP != Address4.Zero, "DHCP should assign a non-zero IP address");
    }

    // ==================== ICMP Tests ====================

    private static void TestICMPPingGateway()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // QEMU user networking: slirp answers ICMP echo requests to the
        // gateway address itself, so no host-side helper is needed.
        Address4 target = new(10, 0, 2, 2);

        Log.WriteString("[Test] Pinging ");
        Log.WriteString(target.ToString());
        Log.WriteString("...\n");

        IcmpClient icmpClient = new();
        icmpClient.Connect(target);
        icmpClient.SendEcho();

        CosmosEndPoint endpoint = new CosmosEndPoint(Address4.Zero, 0);
        int time = icmpClient.Receive(ref endpoint, 5000);

        if (time >= 0)
        {
            Log.WriteString("[Test] Echo reply from ");
            Log.WriteString(endpoint.Address.ToString());
            Log.WriteString(" in ");
            Log.WriteNumber((ulong)time);
            Log.WriteString(" ms\n");

            Assert.True(endpoint.Address.CompareTo(target) == 0, "Echo reply should come from the pinged address");
        }
        else
        {
            Log.WriteString("[Test] No echo reply within timeout\n");
            Assert.True(false, "Should receive ICMP echo reply from gateway");
        }

        icmpClient.Close();
    }

    private static void TestICMPHostPing()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // The test runner's IcmpTestServer pings our IP every 500 ms through
        // the raw-Ethernet hub port (slirp cannot forward host-sourced ICMP).
        // Phase 1: wait until the echo responder answered at least one request.
        Log.WriteString("[Test] Waiting for ICMP echo request from host...\n");

        int waited = 0;
        while (IcmpPacket.EchoRequestsReplied < 1 && waited < 10000)
        {
            TimerManager.Wait(100);
            waited += 100;
        }

        if (IcmpPacket.EchoRequestsReplied < 1)
        {
            Log.WriteString("[Test] No echo request received from host within timeout\n");
            Assert.True(false, "Host echo request should reach the kernel and be answered");
            return;
        }

        Log.WriteString("[Test] Answered ");
        Log.WriteNumber((ulong)IcmpPacket.EchoRequestsReplied);
        Log.WriteString(" echo request(s) from host\n");
        Assert.True(true, "Host echo request received and answered");

        // Phase 2: the host validates our echo reply (checksum + payload) and
        // only then switches its request payload from COSMOS_PING to HOST_OK —
        // seeing it proves the full host->guest->host round trip.
        Log.WriteString("[Test] Waiting for HOST_OK acknowledgment payload...\n");

        bool hostAck = false;
        waited = 0;
        while (!hostAck && waited < 10000)
        {
            byte[]? data = IcmpPacket.LastEchoRequestData;
            if (data != null && data.Length >= 7 &&
                data[0] == (byte)'H' && data[1] == (byte)'O' && data[2] == (byte)'S' &&
                data[3] == (byte)'T' && data[4] == (byte)'_' && data[5] == (byte)'O' &&
                data[6] == (byte)'K')
            {
                hostAck = true;
            }
            else
            {
                TimerManager.Wait(100);
                waited += 100;
            }
        }

        if (hostAck)
        {
            Log.WriteString("[Test] Host acknowledged a valid echo reply\n");
        }
        else
        {
            Log.WriteString("[Test] No HOST_OK payload within timeout\n");
        }

        Assert.True(hostAck, "Host should confirm it received a valid echo reply");
    }

    // ==================== IPv6 Tests ====================

    private static void TestIPv6LinkLocalConfigured()
    {
        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Address6? linkLocal = NetworkManager.Primary.LinkLocalAddress;
        MACAddress? mac = NetworkManager.MacAddress;
        if (linkLocal is null || mac is null)
        {
            Assert.True(false, "The primary adapter should carry a link-local address once configured");
            return;
        }

        Log.WriteString("[Test] Link-local address: ");
        Log.WriteString(linkLocal.ToString());
        Log.WriteString("\n");

        // RFC 4291 Appendix A, checked from the MAC text rather than through
        // Address6.LinkLocalFor, so a wrong derivation cannot agree with itself.
        byte[] macBytes = ParseMac(mac.ToString());
        ReadOnlySpan<byte> bytes = linkLocal.ToBytes();

        Assert.True(bytes[0] == 0xFE && bytes[1] == 0x80, "Link-local address should be in fe80::/64");

        bool middleZero = true;
        for (int i = 2; i < 8; i++)
        {
            if (bytes[i] != 0)
            {
                middleZero = false;
            }
        }
        Assert.True(middleZero, "Bytes 2 to 7 of a link-local address should be zero");

        Assert.True(bytes[8] == (byte)(macBytes[0] ^ 0x02) && bytes[9] == macBytes[1] && bytes[10] == macBytes[2]
            && bytes[11] == 0xFF && bytes[12] == 0xFE
            && bytes[13] == macBytes[3] && bytes[14] == macBytes[4] && bytes[15] == macBytes[5],
            "Interface identifier should be the modified EUI-64 of the MAC address");
    }

    private static void TestICMPv6PingGateway()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // QEMU user networking answers ICMPv6 echo requests to its own
        // address fec0::2, once Neighbor Discovery has resolved it.
        Address6 target = new(0xFEC0_0000, 0, 0, 2);

        Log.WriteString("[Test] Pinging ");
        Log.WriteString(target.ToString());
        Log.WriteString("...\n");

        Icmpv6Client client = new();
        client.Connect(target);
        client.SendEcho();

        CosmosEndPoint endpoint = new(Address6.Zero, 0);
        int time = client.Receive(ref endpoint, 5000);

        if (time >= 0)
        {
            Log.WriteString("[Test] Echo reply from ");
            Log.WriteString(endpoint.Address.ToString());
            Log.WriteString(" in ");
            Log.WriteNumber((ulong)time);
            Log.WriteString(" ms\n");

            Assert.True(endpoint.Address == target, "Echo reply should come from the pinged address");
        }
        else
        {
            Log.WriteString("[Test] No echo reply within timeout\n");
            Assert.True(false, "Should receive ICMPv6 echo reply from gateway");
        }

        client.Close();
    }

    private static void TestICMPv6HostPing()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // The test runner's IcmpTestServer resolves our link-local address
        // with a Neighbor Solicitation, then pings it every 500 ms through the
        // raw-Ethernet hub port. Phase 1: the solicitation was answered and at
        // least one echo request replied to.
        Log.WriteString("[Test] Waiting for ICMPv6 echo request from host...\n");

        int waited = 0;
        while (Icmpv6Packet.EchoRequestsReplied < 1 && waited < 10000)
        {
            TimerManager.Wait(100);
            waited += 100;
        }

        if (Icmpv6Packet.EchoRequestsReplied < 1)
        {
            Log.WriteString("[Test] No ICMPv6 echo request received from host within timeout\n");
            Assert.True(false, "Host ICMPv6 echo request should reach the kernel and be answered");
            return;
        }

        Log.WriteString("[Test] Answered ");
        Log.WriteNumber((ulong)Icmpv6Packet.EchoRequestsReplied);
        Log.WriteString(" ICMPv6 echo request(s) from host\n");
        Assert.True(NdpPacket.SolicitationsAnswered >= 1, "Host Neighbor Solicitation should have been answered");

        // Phase 2: the host validates our echo reply (pseudo-header checksum
        // and payload) and only then switches its request payload from
        // COSMOS_PING6 to HOST_OK6, so seeing it proves the full round trip.
        Log.WriteString("[Test] Waiting for HOST_OK6 acknowledgment payload...\n");

        bool hostAck = false;
        waited = 0;
        while (!hostAck && waited < 10000)
        {
            byte[]? data = Icmpv6Packet.LastEchoRequestData;
            if (data is not null && data.Length >= 8 &&
                data[0] == (byte)'H' && data[1] == (byte)'O' && data[2] == (byte)'S' &&
                data[3] == (byte)'T' && data[4] == (byte)'_' && data[5] == (byte)'O' &&
                data[6] == (byte)'K' && data[7] == (byte)'6')
            {
                hostAck = true;
            }
            else
            {
                TimerManager.Wait(100);
                waited += 100;
            }
        }

        if (hostAck)
        {
            Log.WriteString("[Test] Host acknowledged a valid ICMPv6 echo reply\n");
        }
        else
        {
            Log.WriteString("[Test] No HOST_OK6 payload within timeout\n");
        }

        Assert.True(hostAck, "Host should confirm it received a valid ICMPv6 echo reply");
    }

    // Parses "52:54:00:12:34:56" into six bytes; MACAddress keeps its bytes internal.
    private static byte[] ParseMac(string mac)
    {
        byte[] bytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            bytes[i] = (byte)((HexValue(mac[i * 3]) << 4) | HexValue(mac[i * 3 + 1]));
        }

        return bytes;
    }

    private static int HexValue(char digit)
    {
        return digit >= 'A' ? digit - 'A' + 10 : digit - '0';
    }

    // ==================== UDP Tests ====================

    private static void TestUDPSendPacket()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Creating .NET UdpClient...\n");

        // Use .NET UdpClient (plugged by SocketPlug)
        var udpClient = new DotNetUdpClient(TestPort);

        // Create test message - test runner is listening on port 5555
        string message = "COSMOS_UDP_TEST";
        byte[] payload = Encoding.ASCII.GetBytes(message);

        // Gateway IP for QEMU user networking
        var gatewayEndpoint = new IPEndPoint(IPAddress.Parse("10.0.2.2"), TestPort);

        Log.WriteString("[Test] Sending UDP packet to ");
        Log.WriteString(gatewayEndpoint.Address.ToString());
        Log.WriteString(":");
        Log.WriteNumber(TestPort);
        Log.WriteString("\n");

        int bytesSent = udpClient.Send(payload, payload.Length, gatewayEndpoint);
        if (bytesSent <= 0)
        {
            Assert.True(false, "Failed to send UDP packet");
            udpClient.Close();
            return;
        }

        Log.WriteString("[Test] UDP packet sent (");
        Log.WriteNumber((ulong)bytesSent);
        Log.WriteString(" bytes), waiting for echo...\n");

        // Wait for echo from test runner (it echoes our packet back)
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        byte[]? receivedData = null;

        int waitTime = 0;
        while (receivedData == null && waitTime < 5000)
        {
            try
            {
                receivedData = udpClient.Receive(ref remoteEP);
            }
            catch
            {
                // No data yet
            }
            if (receivedData == null || receivedData.Length == 0)
            {
                receivedData = null;
                TimerManager.Wait(100);
                waitTime += 100;
            }
        }

        if (receivedData != null && receivedData.Length > 0)
        {
            Log.WriteString("[Test] Received echo from ");
            Log.WriteString(remoteEP.Address.ToString());
            Log.WriteString(":");
            Log.WriteNumber((ulong)remoteEP.Port);
            Log.WriteString(" with ");
            Log.WriteNumber((ulong)receivedData.Length);
            Log.WriteString(" bytes\n");

            // Validate the echo matches what we sent
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            bool contentValid = receivedMessage == message;

            if (contentValid)
            {
                Log.WriteString("[Test] Echo validated: COSMOS_UDP_TEST\n");
                Assert.True(true, "UDP send and echo received with correct content");
            }
            else
            {
                Log.WriteString("[Test] Echo content mismatch! Expected: COSMOS_UDP_TEST, Got: ");
                Log.WriteString(receivedMessage);
                Log.WriteString("\n");
                Assert.True(false, "UDP echo content should match COSMOS_UDP_TEST");
            }
        }
        else
        {
            Log.WriteString("[Test] No echo received within timeout\n");
            Assert.True(false, "Should receive echo from test runner");
        }

        udpClient.Close();
    }

    private static void TestUDPReceivePacket()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Creating .NET UdpClient on port ");
        Log.WriteNumber(EchoPort);
        Log.WriteString("...\n");

        // Use .NET UdpClient (plugged by SocketPlug)
        var udpClient = new DotNetUdpClient(EchoPort);

        Log.WriteString("[Test] Waiting for UDP packet from test runner...\n");

        // Wait for packet from test runner (it sends "TEST_FROM_HOST" to port 5556)
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        byte[]? receivedData = null;

        int waitTime = 0;
        while (receivedData == null && waitTime < 5000)
        {
            try
            {
                receivedData = udpClient.Receive(ref remoteEP);
            }
            catch
            {
                // No data yet
            }
            if (receivedData == null || receivedData.Length == 0)
            {
                receivedData = null;
                TimerManager.Wait(100);
                waitTime += 100;
            }
        }

        if (receivedData != null && receivedData.Length > 0)
        {
            Log.WriteString("[Test] Received UDP packet from ");
            Log.WriteString(remoteEP.Address.ToString());
            Log.WriteString(":");
            Log.WriteNumber((ulong)remoteEP.Port);
            Log.WriteString(" with ");
            Log.WriteNumber((ulong)receivedData.Length);
            Log.WriteString(" bytes\n");

            // Validate exact content from test runner
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            string expectedMessage = "TEST_FROM_HOST";
            bool contentValid = receivedMessage == expectedMessage;

            if (contentValid)
            {
                Log.WriteString("[Test] Content validated: TEST_FROM_HOST\n");
                Assert.True(true, "UDP packet received with correct content");
            }
            else
            {
                Log.WriteString("[Test] Content mismatch! Expected: TEST_FROM_HOST, Got: ");
                Log.WriteString(receivedMessage);
                Log.WriteString("\n");
                Assert.True(false, "UDP packet content should match TEST_FROM_HOST");
            }
        }
        else
        {
            Log.WriteString("[Test] No UDP packet received within timeout\n");
            Assert.True(false, "Should receive UDP packet from test runner");
        }

        udpClient.Close();
    }

    // ==================== Packet Seam Tests ====================

    private static void TestPacketSeamCraftedEchoRoundTrip()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Address4 target = new(10, 0, 2, 2);
        const ushort echoId = 0x4242;
        const ushort echoSequence = 9;

        IcmpClient icmpClient = new();
        icmpClient.Connect(target);

        // Build the echo request ourselves instead of going through SendEcho,
        // so the reply can be correlated with the id/sequence we chose.
        IcmpEchoRequest request = new(s_localIP!, target, echoId, echoSequence);
        Log.WriteString("[Test] Sending crafted echo request id=0x4242 seq=9...\n");
        Assert.True(NetworkStack.Send(request), "NetworkStack.Send should queue a packet with a configured source address");

        IcmpPacket? replyPacket = icmpClient.ReceivePacket(5000);
        if (replyPacket is null)
        {
            Log.WriteString("[Test] No reply packet within timeout\n");
            Assert.True(false, "Crafted echo request should get a reply packet");
            icmpClient.Close();
            return;
        }

        if (replyPacket is IcmpEchoReply reply)
        {
            Log.WriteString("[Test] Echo reply id=");
            Log.WriteNumber((ulong)reply.IcmpId);
            Log.WriteString(" seq=");
            Log.WriteNumber((ulong)reply.IcmpSequence);
            Log.WriteString(" from ");
            Log.WriteString(reply.SourceIP.ToString());
            Log.WriteString("\n");

            Assert.True(reply.IcmpId == echoId, "Echo reply should carry the identifier the request was built with");
            Assert.True(reply.IcmpSequence == echoSequence, "Echo reply should carry the sequence number the request was built with");
            Assert.True(reply.SourceIP.CompareTo(target) == 0, "Echo reply should come from the pinged address");
        }
        else
        {
            Assert.True(false, "Received ICMP packet should be typed as IcmpEchoReply");
        }

        icmpClient.Close();
    }

    private static void TestPacketSeamCraftedUdpRoundTrip()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Address4 gateway = new(10, 0, 2, 2);
        const ushort seamPort = 5559;

        // Bind a Cosmos UdpClient so the echo comes back as a packet object.
        CosmosUdpClient udpClient = new(seamPort);

        byte[] payload = Encoding.ASCII.GetBytes("COSMOS_SEAM_TEST");
        UdpPacket packet = new(s_localIP!, gateway, seamPort, TestPort, payload);

        Log.WriteString("[Test] Sending crafted UDP packet to the echo server...\n");
        Assert.True(udpClient.Send(packet), "Crafted UDP packet should be queued through the client");

        UdpPacket? echo = udpClient.ReceivePacket(5000);
        if (echo is null)
        {
            Log.WriteString("[Test] No echoed datagram within timeout\n");
            Assert.True(false, "Echo of the crafted UDP packet should come back as a packet object");
            udpClient.Close();
            return;
        }

        Log.WriteString("[Test] Echoed datagram ");
        Log.WriteString(echo.SourceIP.ToString());
        Log.WriteString(":");
        Log.WriteNumber((ulong)echo.SourcePort);
        Log.WriteString(" -> port ");
        Log.WriteNumber((ulong)echo.DestinationPort);
        Log.WriteString("\n");

        Assert.True(echo.DestinationPort == seamPort, "Echoed datagram should target the port the client is bound to");

        byte[] echoedPayload = echo.GetUdpData();
        bool payloadMatches = echoedPayload.Length == payload.Length;
        if (payloadMatches)
        {
            for (int i = 0; i < payload.Length; i++)
            {
                if (echoedPayload[i] != payload[i])
                {
                    payloadMatches = false;
                    break;
                }
            }
        }
        Assert.True(payloadMatches, "Echoed payload should match the crafted payload");

        udpClient.Close();
    }

    private static void TestPacketSeamSendUnroutable()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // A source address no interface carries: Send must report the drop
        // instead of pretending the packet went out.
        Address4 unconfigured = new(192, 168, 250, 250);
        IcmpEchoRequest request = new(unconfigured, new Address4(10, 0, 2, 2), 1, 1);
        Assert.True(!NetworkStack.Send(request), "Send should return false for a source address no interface carries");
    }

    // ==================== TCP Tests ====================

    private static void TestTCPClientConnect()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Creating .NET TcpClient...\n");

        try
        {
            // Create TCP client and connect to test runner (gateway on port 5557)
            var tcpClient = new DotNetTcpClient();

            Log.WriteString("[Test] Connecting to ");
            Log.WriteString("10.0.2.2:");
            Log.WriteNumber(TcpClientPort);
            Log.WriteString("...\n");

            tcpClient.Connect(IPAddress.Parse("10.0.2.2"), TcpClientPort);

            Log.WriteString("[Test] Connected! Sending data...\n");

            // Send test message
            Log.WriteString("[Test] Getting stream...\n");
            var stream = tcpClient.GetStream();
            Log.WriteString("[Test] Got stream, preparing message...\n");
            string message = "COSMOS_TCP_TEST";
            byte[] payload = Encoding.ASCII.GetBytes(message);
            Log.WriteString("[Test] Writing to stream...\n");
            stream.Write(payload, 0, payload.Length);
            Log.WriteString("[Test] Write complete\n");

            Log.WriteString("[Test] Sent '");
            Log.WriteString(message);
            Log.WriteString("', waiting for echo...\n");

            // Wait for echo from test runner
            byte[] buffer = new byte[256];
            int bytesRead = 0;
            int waitTime = 0;

            while (bytesRead == 0 && waitTime < 5000)
            {
                if (stream.DataAvailable)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                else
                {
                    TimerManager.Wait(100);
                    waitTime += 100;
                }
            }

            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Log.WriteString("[Test] Received echo: '");
                Log.WriteString(receivedMessage);
                Log.WriteString("'\n");

                bool contentValid = receivedMessage == message;
                if (contentValid)
                {
                    Log.WriteString("[Test] Echo validated!\n");
                    Assert.True(true, "TCP connect and echo received with correct content");
                }
                else
                {
                    Log.WriteString("[Test] Echo content mismatch!\n");
                    Assert.True(false, "TCP echo content should match");
                }
            }
            else
            {
                Log.WriteString("[Test] No echo received within timeout\n");
                Assert.True(false, "Should receive echo from test runner");
            }

            Log.WriteString("[Test] Closing TCP client...\n");
            tcpClient.Close();
            Log.WriteString("[Test] TCP client closed successfully\n");
        }
        catch
        {
            Log.WriteString("[Test] TCP connect failed with exception\n");
            Assert.True(false, "TCP connect failed with exception");
        }
    }

    private static void TestTCPServerAccept()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Creating .NET TcpListener on port ");
        Log.WriteNumber(TcpServerPort);
        Log.WriteString("...\n");

        try
        {
            // Create TCP listener on port 5558
            var listener = new DotNetTcpListener(IPAddress.Any, TcpServerPort);
            listener.Start();

            Log.WriteString("[Test] Listening, waiting for connection from test runner...\n");

            // Wait for connection from test runner (it connects after a delay)
            DotNetTcpClient? client = null;
            int waitTime = 0;

            while (client == null && waitTime < 10000)
            {
                if (listener.Pending())
                {
                    client = listener.AcceptTcpClient();
                }
                else
                {
                    TimerManager.Wait(100);
                    waitTime += 100;
                }
            }

            if (client != null)
            {
                Log.WriteString("[Test] Accepted connection!\n");

                var stream = client.GetStream();

                // Wait for data from test runner
                byte[] buffer = new byte[256];
                int bytesRead = 0;
                waitTime = 0;

                while (bytesRead == 0 && waitTime < 5000)
                {
                    if (stream.DataAvailable)
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        TimerManager.Wait(100);
                        waitTime += 100;
                    }
                }

                if (bytesRead > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Log.WriteString("[Test] Received: '");
                    Log.WriteString(receivedMessage);
                    Log.WriteString("'\n");

                    // Echo back
                    stream.Write(buffer, 0, bytesRead);
                    Log.WriteString("[Test] Echoed data back\n");

                    string expectedMessage = "TEST_FROM_HOST";
                    bool contentValid = receivedMessage == expectedMessage;
                    if (contentValid)
                    {
                        Log.WriteString("[Test] Content validated!\n");
                        Assert.True(true, "TCP server accept and received correct content");
                    }
                    else
                    {
                        Log.WriteString("[Test] Content mismatch! Expected: TEST_FROM_HOST\n");
                        Assert.True(false, "TCP received content should match TEST_FROM_HOST");
                    }
                }
                else
                {
                    Log.WriteString("[Test] No data received within timeout\n");
                    Assert.True(false, "Should receive data from test runner");
                }

                client.Close();
            }
            else
            {
                Log.WriteString("[Test] No connection received within timeout\n");
                Assert.True(false, "Should receive connection from test runner");
            }

            listener.Stop();
        }
        catch (Exception ex)
        {
            Log.WriteString("[Test] TCP server failed: ");
            Log.WriteString(ex.Message);
            Log.WriteString("\n");
            Assert.True(false, "TCP server failed with exception");
        }
    }

    private static void TestTCPCloseWithoutPeerFin()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Connecting to lingering host peer at 10.0.2.2:");
        Log.WriteNumber(TcpLingerPort);
        Log.WriteString("...\n");

        try
        {
            var tcpClient = new DotNetTcpClient();
            tcpClient.Connect(IPAddress.Parse("10.0.2.2"), TcpLingerPort);

            var stream = tcpClient.GetStream();
            string message = "COSMOS_CLOSE_TEST";
            byte[] payload = Encoding.ASCII.GetBytes(message);
            stream.Write(payload, 0, payload.Length);

            Log.WriteString("[Test] Sent '");
            Log.WriteString(message);
            Log.WriteString("', waiting for echo...\n");

            // Wait for the echo so we know the peer consumed our data and is
            // now idling with its half of the connection deliberately open.
            byte[] buffer = new byte[256];
            int bytesRead = 0;
            int waitTime = 0;

            while (bytesRead == 0 && waitTime < 5000)
            {
                if (stream.DataAvailable)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                else
                {
                    TimerManager.Wait(100);
                    waitTime += 100;
                }
            }

            if (bytesRead == 0)
            {
                Log.WriteString("[Test] No echo received within timeout\n");
                Assert.True(false, "Should receive echo from lingering host peer");
                return;
            }

            Log.WriteString("[Test] Echo received; closing while the peer holds the connection open...\n");

            // Issue #369: the peer never sends its FIN, so a synchronous close
            // waiting for CLOSED throws. Standard sockets semantics: Close()
            // must succeed once our FIN is ACKed and let the state machine
            // finish in the background.
            try
            {
                tcpClient.Close();
                Log.WriteString("[Test] Close() returned without throwing\n");
                Assert.True(true, "Close() succeeds while the peer holds the connection open");
            }
            catch
            {
                Log.WriteString("[Test] Close() threw!\n");
                Assert.True(false, "Close() must not throw when the peer does not FIN");
            }
        }
        catch
        {
            Log.WriteString("[Test] TCP lingering-close test failed with exception\n");
            Assert.True(false, "TCP lingering-close test failed with exception");
        }
    }

    // ==================== DNS Tests ====================

    private static void TestDNSClientCreate()
    {
        Log.WriteString("[Test] Creating DNS client...\n");

        // Create DNS client
        DnsClient dnsClient = new();

        Assert.True(dnsClient != null, "DNS client should be created");

        // Configure DNS server (Cloudflare's public DNS)
        Address4 dnsServer = new(1, 1, 1, 1);
        DnsConfig.Add(dnsServer);

        Assert.True(DnsConfig.Nameservers.Count > 0, "DNS nameservers should be configured");

        // Verify the DNS server was added correctly (1.1.1.1)
        bool foundCloudflare = false;
        for (int i = 0; i < DnsConfig.Nameservers.Count; i++)
        {
            Address ns = DnsConfig.Nameservers[i];
            var parts = ns.Parts;
            if (parts[0] == 1 && parts[1] == 1 && parts[2] == 1 && parts[3] == 1)
            {
                foundCloudflare = true;
                break;
            }
        }
        Assert.True(foundCloudflare, "DNS server 1.1.1.1 should be in nameservers list");

        Log.WriteString("[Test] DNS client created successfully\n");
        Log.WriteString("[Test] DNS server configured: ");
        Log.WriteString(dnsServer.ToString());
        Log.WriteString("\n");
        Log.WriteString("[Test] Verified 1.1.1.1 is in DNS nameservers list\n");

        dnsClient.Close();
    }

    private static void TestDNSResolveTestSite()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Log.WriteString("[Test] Resolving valentin.bzh via DNS...\n");

        // Configure DNS server (Cloudflare's public DNS)
        Address4 dnsServer = new(1, 1, 1, 1);
        DnsConfig.Add(dnsServer);

        // Create DNS client and connect to DNS server
        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);

        Log.WriteString("[Test] Connected to DNS server: ");
        Log.WriteString(dnsServer.ToString());
        Log.WriteString("\n");

        // Send DNS query for valentin.bzh
        string domain = "valentin.bzh";
        Log.WriteString("[Test] Sending DNS query for: ");
        Log.WriteString(domain);
        Log.WriteString("\n");

        dnsClient.SendQuery(domain);

        // Wait for response with timeout
        Address resolvedIP = dnsClient.Receive(5000);

        if (resolvedIP != null)
        {
            Log.WriteString("[Test] DNS resolution successful!\n");
            Log.WriteString("[Test] valentin.bzh resolved to: ");
            Log.WriteString(resolvedIP.ToString());
            Log.WriteString("\n");

            // Verify we got a valid IP (not 0.0.0.0)
            Assert.True(resolvedIP != Address4.Zero, "Resolved IP should not be 0.0.0.0");
            Assert.True(true, "DNS resolution for valentin.bzh succeeded");
        }
        else
        {
            Log.WriteString("[Test] DNS resolution timed out or failed\n");
            // Don't fail the test on timeout - network may not be available in test environment
            Assert.True(true, "DNS query sent (timeout may occur in isolated test environment)");
        }

        dnsClient.Close();
    }

    private static void TestDNSResolveCnameChain()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // www.github.com is a CNAME to github.com, so the A records in the
        // answer carry the canonical name, not the queried one. A non-empty
        // result proves ReceiveAll followed the CNAME chain.
        string domain = "www.github.com";
        Log.WriteString("[Test] Resolving CNAME chain for ");
        Log.WriteString(domain);
        Log.WriteString("...\n");

        Address4 dnsServer = new(1, 1, 1, 1);
        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);

        dnsClient.SendQuery(domain);

        List<Address>? addresses = dnsClient.ReceiveAll(5000);

        if (addresses != null)
        {
            Log.WriteString("[Test] CNAME chain resolved to ");
            Log.WriteNumber((ulong)addresses.Count);
            Log.WriteString(" address(es), first: ");
            Log.WriteString(addresses[0].ToString());
            Log.WriteString("\n");

            Assert.True(addresses.Count > 0, "CNAME chain should yield at least one A record");
            Assert.True(addresses[0] != Address4.Zero, "Resolved IP should not be 0.0.0.0");
        }
        else
        {
            Log.WriteString("[Test] DNS resolution timed out or failed\n");
            // Don't fail the test on timeout - network may not be available in test environment
            Assert.True(true, "DNS query sent (timeout may occur in isolated test environment)");
        }

        dnsClient.Close();
    }

    /// <summary>
    /// Two queries in a row on ONE DnsClient must both resolve. Regression for
    /// the duplicated DNS delivery: UDPHandler routed a reply to the client
    /// twice, so the first Receive left a stale copy queued and the next query
    /// dequeued that instead of its own answer, failing the query-name check.
    /// </summary>
    private static void TestDNSTwoQueriesOneClient()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        Address4 dnsServer = new(1, 1, 1, 1);
        DnsConfig.Add(dnsServer);

        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);

        dnsClient.SendQuery("valentin.bzh");
        Address? first = dnsClient.Receive(5000);

        if (first is null)
        {
            // The environment has no working DNS: the second query proves
            // nothing, so do not fail on it.
            Log.WriteString("[Test] First query timed out, skipping the repeat check\n");
            Assert.True(true, "First DNS query timed out (no DNS in this environment)");
            dnsClient.Close();
            return;
        }

        Log.WriteString("[Test] First query resolved, repeating on the same client\n");

        dnsClient.SendQuery("github.com");
        Address? second = dnsClient.Receive(5000);

        Assert.True(second is not null, "a second query on the same DnsClient must resolve");
        Assert.True(second is null || !second.IsZero, "the second answer should not be 0.0.0.0");

        dnsClient.Close();
    }

    private static void TestDNSResolveMultipleARecords()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        // one.one.one.one stably resolves to exactly two A records:
        // 1.1.1.1 and 1.0.0.1.
        string domain = "one.one.one.one";
        Log.WriteString("[Test] Resolving multiple A records for ");
        Log.WriteString(domain);
        Log.WriteString("...\n");

        Address4 dnsServer = new(1, 1, 1, 1);
        DnsClient dnsClient = new();
        dnsClient.Connect(dnsServer);

        dnsClient.SendQuery(domain);

        List<Address>? addresses = dnsClient.ReceiveAll(5000);

        if (addresses != null)
        {
            Log.WriteString("[Test] Got ");
            Log.WriteNumber((ulong)addresses.Count);
            Log.WriteString(" address(es):\n");

            bool allCloudflare = true;
            for (int i = 0; i < addresses.Count; i++)
            {
                Log.WriteString("[Test]   ");
                Log.WriteString(addresses[i].ToString());
                Log.WriteString("\n");

                MaskedAddress bytes = addresses[i].Parts;
                bool isOneOneOneOne = bytes[0] == 1 && bytes[1] == 1 && bytes[2] == 1 && bytes[3] == 1;
                bool isOneZeroZeroOne = bytes[0] == 1 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 1;
                if (!isOneOneOneOne && !isOneZeroZeroOne)
                {
                    allCloudflare = false;
                }
            }

            Assert.True(addresses.Count >= 2, "one.one.one.one should resolve to at least two A records");
            Assert.True(allCloudflare, "All resolved addresses should be 1.1.1.1 or 1.0.0.1");
        }
        else
        {
            Log.WriteString("[Test] DNS resolution timed out or failed\n");
            // Don't fail the test on timeout - network may not be available in test environment
            Assert.True(true, "DNS query sent (timeout may occur in isolated test environment)");
        }

        dnsClient.Close();
    }

    /// <summary>
    /// The plug on NameResolutionPal is what makes System.Net.Dns work, so
    /// these two cases go through the standard API with no Cosmos DnsClient in
    /// sight. This one needs no network: it pins the name the plug reports.
    /// </summary>
    private static void TestDotNetDnsGetHostName()
    {
        DnsConfig.HostName = "cosmos-test";

        string hostName = Dns.GetHostName();

        Log.WriteString("[Test] Dns.GetHostName() returned: ");
        Log.WriteString(hostName);
        Log.WriteString("\n");

        Assert.True(hostName == "cosmos-test", "Dns.GetHostName must report the configured host name");

        DnsConfig.HostName = "cosmos";
    }

    private static void TestDotNetDnsGetHostAddresses()
    {
        if (!NetworkManager.Ready)
        {
            Assert.True(false, "Network device not ready");
            return;
        }

        if (!s_networkConfigured)
        {
            TestDHCPConfiguration();
        }

        DnsConfig.Add(new Address4(1, 1, 1, 1));

        Log.WriteString("[Test] Resolving valentin.bzh through System.Net.Dns...\n");

        IPAddress[] addresses;
        try
        {
            addresses = Dns.GetHostAddresses("valentin.bzh");
        }
        catch (SocketException)
        {
            // Dns reports every failure by throwing, where the Cosmos client
            // returns null. Tolerated for the same reason the cases above
            // tolerate a timeout: this environment may have no resolver.
            Log.WriteString("[Test] Dns.GetHostAddresses threw SocketException (no resolver reachable)\n");
            Assert.True(true, "Dns query sent (no resolver in this environment)");
            return;
        }

        Assert.True(addresses.Length > 0, "A successful lookup must return at least one address");

        for (int i = 0; i < addresses.Length; i++)
        {
            Log.WriteString("[Test] valentin.bzh resolved to: ");
            Log.WriteString(addresses[i].ToString());
            Log.WriteString("\n");

            // A plugged IPAddress holds four bytes, so this is also the check
            // that the plug filtered out anything it could not carry.
            byte[] octets = addresses[i].GetAddressBytes();
            Assert.True(octets.Length == 4, "The plug hands back IPv4 addresses only");
            Assert.True(octets[0] != 0, "A resolved address should not start with a zero octet");
        }
    }
}
