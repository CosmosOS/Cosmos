// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// One legacy receive descriptor of the 82574, 16 bytes, as the controller
/// reads and writes it in the receive ring: the driver fills the buffer
/// address, the controller fills the rest when a frame lands. Reached
/// through <c>MemoryMarshal.Cast</c> over the ring's DMA memory; the fields
/// are the hardware layout, so they stay public fields.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct E1000EReceiveDescriptor
{
    /// <summary>Physical address of the buffer the frame lands in.</summary>
    public ulong BufferAddress;

    /// <summary>Bytes the controller wrote into the buffer.</summary>
    public ushort Length;

    /// <summary>The packet checksum the controller computed.</summary>
    public ushort Checksum;

    /// <summary>Status: Descriptor Done, End Of Packet and the rest.</summary>
    public byte Status;

    /// <summary>Errors: non-zero when the frame is bad.</summary>
    public byte Errors;

    /// <summary>VLAN tag and priority.</summary>
    public ushort Special;
}
