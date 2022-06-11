/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.System.Network.IPv4.UDP.DNS;

namespace Cosmos.System.Network.IPv4.UDP;

/// <summary>
///     UDPPacket class.
/// </summary>
public class UDPPacket : IPPacket
{
    /// <summary>
    ///     UDP CRC.
    /// </summary>
    private ushort udpCRC;

    /// <summary>
    ///     Create new instance of the <see cref="UDPPacket" /> class.
    /// </summary>
    internal UDPPacket()
    {
    }

    /// <summary>
    ///     Create new instance of the <see cref="UDPPacket" /> class.
    /// </summary>
    /// <param name="rawData">Raw data.</param>
    public UDPPacket(byte[] rawData)
        : base(rawData)
    {
    }

    public UDPPacket(Address source, Address dest, ushort srcport, ushort destport, ushort datalength)
        : base((ushort)(datalength + 8), 17, source, dest, 0x00)
    {
        MakePacket(srcport, destport, datalength);
        InitFields();
    }

    public UDPPacket(Address source, Address dest, ushort srcport, ushort destport, ushort datalength,
        MACAddress destmac)
        : base((ushort)(datalength + 8), 17, source, dest, 0x00, destmac)
    {
        MakePacket(srcport, destport, datalength);
        InitFields();
    }

    /// <summary>
    ///     Create new instance of the <see cref="UDPPacket" /> class.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="srcPort">Source port.</param>
    /// <param name="destPort">Destination port.</param>
    /// <param name="data">Data array.</param>
    /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
    /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
    public UDPPacket(Address source, Address dest, ushort srcPort, ushort destPort, byte[] data)
        : base((ushort)(data.Length + 8), 17, source, dest, 0x00)
    {
        MakePacket(srcPort, destPort, (ushort)data.Length);

        for (var b = 0; b < data.Length; b++)
        {
            RawData[DataOffset + 8 + b] = data[b];
        }

        InitFields();
    }

    /// <summary>
    ///     Get destination port.
    /// </summary>
    public ushort DestinationPort { get; private set; }

    /// <summary>
    ///     Get source port.
    /// </summary>
    public ushort SourcePort { get; private set; }

    /// <summary>
    ///     Get UDP length.
    /// </summary>
    public ushort UDP_Length { get; private set; }

    /// <summary>
    ///     Get UDP data lenght.
    /// </summary>
    public ushort UDP_DataLength => (ushort)(UDP_Length - 8);

    /// <summary>
    ///     Get UDP data.
    /// </summary>
    /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
    internal byte[] UDP_Data
    {
        get
        {
            var data = new byte[UDP_DataLength];

            for (var b = 0; b < data.Length; b++)
            {
                data[b] = RawData[DataOffset + 8 + b];
            }

            return data;
        }
    }

    /// <summary>
    ///     UDP handler.
    /// </summary>
    /// <param name="packetData">Packet data.</param>
    /// <exception cref="OverflowException">Thrown if UDP_Data array length is greater than Int32.MaxValue.</exception>
    /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
    internal static void UDPHandler(byte[] packetData)
    {
        var udp_packet = new UDPPacket(packetData);

        Global.mDebugger.Send("[Received] UDP packet from " + udp_packet.SourceIP + ":" + udp_packet.SourcePort);

        if (udp_packet.SourcePort == 67)
        {
            DHCPPacket.DHCPHandler(packetData);
            return;
        }

        if (udp_packet.SourcePort == 53)
        {
            DNSPacket.DNSHandler(packetData);
            return;
        }

        var receiver = UdpClient.GetClient(udp_packet.DestinationPort);
        if (receiver != null)
        {
            receiver.ReceiveData(udp_packet);
        }
    }

    private void MakePacket(ushort srcport, ushort destport, ushort length)
    {
        RawData[DataOffset + 0] = (byte)((srcport >> 8) & 0xFF);
        RawData[DataOffset + 1] = (byte)((srcport >> 0) & 0xFF);
        RawData[DataOffset + 2] = (byte)((destport >> 8) & 0xFF);
        RawData[DataOffset + 3] = (byte)((destport >> 0) & 0xFF);
        UDP_Length = (ushort)(length + 8);

        RawData[DataOffset + 4] = (byte)((UDP_Length >> 8) & 0xFF);
        RawData[DataOffset + 5] = (byte)((UDP_Length >> 0) & 0xFF);

        RawData[DataOffset + 6] = (0 >> 8) & 0xFF;
        RawData[DataOffset + 7] = (0 >> 0) & 0xFF;
    }

    /// <summary>
    ///     Init UDPPacket fields.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
    protected override void InitFields()
    {
        base.InitFields();
        SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
        DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
        UDP_Length = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
        udpCRC = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
    }

    /// <summary>
    ///     To string.
    /// </summary>
    /// <returns>string value.</returns>
    public override string ToString() =>
        "UDP Packet Src=" + SourceIP + ":" + SourcePort + "," +
        "Dest=" + DestinationIP + ":" + DestinationPort + ", DataLen=" + UDP_DataLength;
}
