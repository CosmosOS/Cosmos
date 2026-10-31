using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.RISCV64.IO;

/// <summary>
/// Port I/O on RISC-V, which has none: every device is memory-mapped. Reads
/// return 0 and writes are dropped, so the shared code that probes x86 ports
/// finds nothing there.
/// </summary>
internal class RISCV64PortIO : IPortIO
{
    public byte ReadByte(ushort port) => 0;
    public ushort ReadWord(ushort port) => 0;
    public uint ReadDWord(ushort port) => 0;

    public void WriteByte(ushort port, byte value)
    {
    }

    public void WriteWord(ushort port, ushort value)
    {
    }

    public void WriteDWord(ushort port, uint value)
    {
    }
}
