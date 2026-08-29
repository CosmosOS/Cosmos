using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// An Ethernet frame, the root of the packet hierarchy. The frame lives in
/// <see cref="RawData"/>; every header property is a snapshot parsed from
/// that buffer at construction time and is not refreshed by later writes to
/// the buffer. Derived types lay their headers into the same buffer from
/// their constructors, including checksums: there is no separate
/// finalize-before-send step.
/// </summary>
// For more info, refer to http://standards.ieee.org/about/get/802/802.3.html
[Experimental(Experimentals.PacketSeamDiagId)]
public class EthernetPacket
{
    /// <summary>Parsed source MAC address backing <see cref="SourceMac"/>.</summary>
    protected MACAddress srcMAC = null!;

    /// <summary>Parsed destination MAC address backing <see cref="DestinationMac"/>.</summary>
    protected MACAddress destMAC = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetPacket"/> class
    /// over existing frame bytes. The array is stored by reference, not
    /// copied: the caller must not reuse the buffer while the packet is
    /// alive.
    /// </summary>
    /// <param name="rawData">The raw data of the packet.</param>
    public EthernetPacket(byte[] rawData)
    {
        RawData = rawData;
        InitializeFields();
    }

    /// <summary>
    /// Parses the header fields from <see cref="RawData"/> into the typed
    /// properties. Runs from the constructors (including the base
    /// constructor, before derived-type state exists) and again whenever a
    /// MAC address setter rewrites the buffer.
    /// </summary>
    protected virtual void InitializeFields()
    {
        destMAC = new MACAddress(RawData, 0);
        srcMAC = new MACAddress(RawData, 6);
        EthernetType = (ushort)((RawData[12] << 8) | RawData[13]);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetPacket"/> class, with specified type and size.
    /// </summary>
    /// <param name="type">EtherType of the frame.</param>
    /// <param name="packetSize">Total frame size in bytes; the buffer is allocated here.</param>
    protected EthernetPacket(ushort type, int packetSize)
        : this(MACAddress.None, MACAddress.None, type, packetSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetPacket"/> class, with specified destination, source, type and size.
    /// </summary>
    /// <param name="dest">Destination MAC address.</param>
    /// <param name="src">Source MAC address.</param>
    /// <param name="type">EtherType of the frame.</param>
    /// <param name="packetSize">Total frame size in bytes; the buffer is allocated here.</param>
    protected EthernetPacket(MACAddress dest, MACAddress src, ushort type, int packetSize)
    {
        RawData = new byte[packetSize];
        for (int i = 0; i < 6; i++)
        {
            RawData[i] = dest._bytes[i];
            RawData[6 + i] = src._bytes[i];
        }

        RawData[12] = (byte)(type >> 8);
        RawData[13] = (byte)(type >> 0);
        InitializeFields();
    }

    /// <summary>
    /// The complete wire image of the frame. The property is get-only but
    /// the array contents are mutable; header properties parsed from it do
    /// not track direct writes, and checksums computed at construction are
    /// not recomputed.
    /// </summary>
    public byte[] RawData { get; }

    /// <summary>
    /// The source MAC address. The setter (used by the transmit path when
    /// it stamps the sending device's address) rewrites the buffer and
    /// re-parses the whole packet.
    /// </summary>
    public MACAddress SourceMac
    {
        get => srcMAC;
        internal set
        {
            for (int i = 0; i < 6; i++)
            {
                RawData[6 + i] = value._bytes[i];
            }
            InitializeFields();
        }
    }

    /// <summary>
    /// The destination MAC address. The setter (used by the transmit path
    /// once ARP resolution completes) rewrites the buffer and re-parses the
    /// whole packet.
    /// </summary>
    public MACAddress DestinationMac
    {
        get => destMAC;
        internal set
        {
            for (int i = 0; i < 6; i++)
            {
                RawData[i] = value._bytes[i];
            }

            InitializeFields();
        }
    }

    /// <summary>
    /// The EtherType of the frame (0x0800 IPv4, 0x0806 ARP).
    /// </summary>
    public ushort EthernetType { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + EthernetType;
    }
}
