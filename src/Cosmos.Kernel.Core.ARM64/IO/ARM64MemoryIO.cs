using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.ARM64.IO;

/// <summary>
/// ARM64 uses memory-mapped I/O instead of port I/O.
/// Native imports live in Cosmos.Kernel.Core.ARM64/Bridge/Import/ARM64MmioNative.cs.
/// </summary>
internal class ARM64MemoryIO : IPortIO
{
    // ARM64 uses memory-mapped I/O instead of port I/O
    private const ulong MMIO_BASE = 0x3F000000; // Example base for Raspberry Pi 3/4

    private static ulong PortToAddress(ushort port)
    {
        // Map legacy x86 port numbers to ARM64 MMIO addresses
        return MMIO_BASE + port;
    }

    public byte ReadByte(ushort port) => ARM64MmioNative.ReadByte(PortToAddress(port));
    public ushort ReadWord(ushort port) => ARM64MmioNative.ReadWord(PortToAddress(port));
    public uint ReadDWord(ushort port) => ARM64MmioNative.ReadDWord(PortToAddress(port));

    public void WriteByte(ushort port, byte value) => ARM64MmioNative.WriteByte(PortToAddress(port), value);
    public void WriteWord(ushort port, ushort value) => ARM64MmioNative.WriteWord(PortToAddress(port), value);
    public void WriteDWord(ushort port, uint value) => ARM64MmioNative.WriteDWord(PortToAddress(port), value);
}
