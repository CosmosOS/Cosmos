// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Network.E1000E;

/// <summary>
/// One legacy transmit descriptor of the 82574, 16 bytes, as the driver
/// fills it in the transmit ring and the controller reads it. Reached
/// through <c>MemoryMarshal.Cast</c> over the ring's DMA memory; the fields
/// are the hardware layout, so they stay public fields.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct E1000ETransmitDescriptor
{
    /// <summary>Physical address of the buffer holding the frame.</summary>
    public ulong BufferAddress;

    /// <summary>Bytes to send from the buffer.</summary>
    public ushort Length;

    /// <summary>Checksum Offset: where the controller inserts a checksum; unused.</summary>
    public byte ChecksumOffset;

    /// <summary>Command: End Of Packet, Insert FCS, Report Status.</summary>
    public byte Command;

    /// <summary>Status the controller writes back when Report Status is set.</summary>
    public byte Status;

    /// <summary>Checksum Start: where the checksum computation starts; unused.</summary>
    public byte ChecksumStart;

    /// <summary>VLAN tag and priority.</summary>
    public ushort Special;
}
