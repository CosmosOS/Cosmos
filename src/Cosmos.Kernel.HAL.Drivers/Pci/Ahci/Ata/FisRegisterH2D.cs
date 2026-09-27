// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Ata;

/// <summary>
/// The Register Host to Device FIS at the start of a command table, which
/// carries the ATA command to the device (SATA 3.x s10.5.5). Creating one
/// clears it.
/// </summary>
internal sealed class FisRegisterH2D
{
    /// <summary>Number of 32-bit dwords in a Register H2D FIS (20 bytes / 4, SATA spec 10.5.4). Internal so <see cref="Sata"/> programs it into the command header CFL field.</summary>
    internal const int FisDwordCount = 5;

    /// <summary>Bytes in a Register H2D FIS.</summary>
    private const int FisBytes = FisDwordCount * sizeof(uint);

    /// <summary>FIS Type field offset (SATA spec 10.5.4).</summary>
    private const int FisTypeOffset = 0x00;

    /// <summary>Offset of the byte holding the C (Command) bit and PM Port field (SATA spec 10.5.4).</summary>
    private const int FlagsOffset = 0x01;

    /// <summary>Command register field offset (SATA spec 10.5.4).</summary>
    private const int CommandOffset = 0x02;

    /// <summary>LBA bits 7:0 field offset (SATA spec 10.5.4).</summary>
    private const int Lba0Offset = 0x04;

    /// <summary>LBA bits 15:8 field offset (SATA spec 10.5.4).</summary>
    private const int Lba1Offset = 0x05;

    /// <summary>LBA bits 23:16 field offset (SATA spec 10.5.4).</summary>
    private const int Lba2Offset = 0x06;

    /// <summary>Device register field offset (SATA spec 10.5.4).</summary>
    private const int DeviceOffset = 0x07;

    /// <summary>LBA bits 31:24 field offset (SATA spec 10.5.4).</summary>
    private const int Lba3Offset = 0x08;

    /// <summary>LBA bits 39:32 field offset (SATA spec 10.5.4).</summary>
    private const int Lba4Offset = 0x09;

    /// <summary>LBA bits 47:40 field offset (SATA spec 10.5.4).</summary>
    private const int Lba5Offset = 0x0A;

    /// <summary>Count register (low byte) field offset (SATA spec 10.5.4).</summary>
    private const int CountLowOffset = 0x0C;

    /// <summary>Count register (high byte) field offset (SATA spec 10.5.4).</summary>
    private const int CountHighOffset = 0x0D;

    /// <summary>C - Command bit position within the flags byte; set for a command FIS, clear for device control (SATA spec 10.5.4).</summary>
    private const int CommandBitShift = 7;

    private readonly DmaBuffer _memory;
    private readonly int _offset;

    /// <summary>FIS Type.</summary>
    internal byte FisType
    {
        get => Bytes[FisTypeOffset];
        set => Bytes[FisTypeOffset] = value;
    }

    /// <summary>C - the FIS carries a command. Setting it rewrites the flags byte, clearing PM Port.</summary>
    internal byte IsCommand
    {
        get => (byte)(Bytes[FlagsOffset] >> CommandBitShift);
        set => Bytes[FlagsOffset] = (byte)(value << CommandBitShift);
    }

    /// <summary>Command register.</summary>
    internal byte Command
    {
        get => Bytes[CommandOffset];
        set => Bytes[CommandOffset] = value;
    }

    /// <summary>LBA bits 7:0.</summary>
    internal byte LBA0
    {
        get => Bytes[Lba0Offset];
        set => Bytes[Lba0Offset] = value;
    }

    /// <summary>LBA bits 15:8.</summary>
    internal byte LBA1
    {
        get => Bytes[Lba1Offset];
        set => Bytes[Lba1Offset] = value;
    }

    /// <summary>LBA bits 23:16.</summary>
    internal byte LBA2
    {
        get => Bytes[Lba2Offset];
        set => Bytes[Lba2Offset] = value;
    }

    /// <summary>Device register.</summary>
    internal byte Device
    {
        get => Bytes[DeviceOffset];
        set => Bytes[DeviceOffset] = value;
    }

    /// <summary>LBA bits 31:24.</summary>
    internal byte LBA3
    {
        get => Bytes[Lba3Offset];
        set => Bytes[Lba3Offset] = value;
    }

    /// <summary>LBA bits 39:32.</summary>
    internal byte LBA4
    {
        get => Bytes[Lba4Offset];
        set => Bytes[Lba4Offset] = value;
    }

    /// <summary>LBA bits 47:40.</summary>
    internal byte LBA5
    {
        get => Bytes[Lba5Offset];
        set => Bytes[Lba5Offset] = value;
    }

    /// <summary>Count register, low byte.</summary>
    internal byte CountL
    {
        get => Bytes[CountLowOffset];
        set => Bytes[CountLowOffset] = value;
    }

    /// <summary>Count register, high byte.</summary>
    internal byte CountH
    {
        get => Bytes[CountHighOffset];
        set => Bytes[CountHighOffset] = value;
    }

    private Span<byte> Bytes => _memory.Span.Slice(_offset, FisBytes);

    /// <summary>Clears the FIS at <paramref name="offset"/> in <paramref name="memory"/>, and views it.</summary>
    /// <param name="memory">The controller's command region.</param>
    /// <param name="offset">Where the FIS starts in it: the command table's start.</param>
    internal FisRegisterH2D(DmaBuffer memory, int offset)
    {
        _memory = memory;
        _offset = offset;
        Bytes.Clear();
    }
}
