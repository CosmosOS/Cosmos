// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Block-device view of a single partition on a host disk. Read/Write are
/// translated to the host LBA space by adding <see cref="StartSector"/>.
/// </summary>
public sealed class Partition : IBlockDevice
{
    private readonly IBlockDevice _host;
    private readonly string _name;

    /// <summary>The disk this partition lives on.</summary>
    public IBlockDevice Host => _host;

    /// <summary>Absolute LBA on the host where the partition begins.</summary>
    public ulong StartSector { get; }

    /// <inheritdoc />
    public ulong BlockCount { get; }

    /// <inheritdoc />
    public ulong BlockSize { get; }

    /// <inheritdoc />
    public string Name => _name;

    /// <summary>
    /// Index-based naming ctor: builds "&lt;host&gt;p&lt;index&gt;" with the
    /// index formatted digit by digit, so naming stays safe if registration
    /// ever moves earlier in boot.
    /// </summary>
    /// <param name="host">The disk this partition lives on.</param>
    /// <param name="startSector">Absolute LBA on the host where the partition begins.</param>
    /// <param name="sectorCount">Length of the partition in sectors.</param>
    /// <param name="index">Zero-based partition index on the host disk.</param>
    /// <remarks>
    /// Internal: the index is the scanner's own numbering, so the only caller
    /// is <see cref="StorageManager"/> walking a partition table. A kernel that
    /// builds a partition view by hand names it instead.
    /// </remarks>
    internal Partition(IBlockDevice host, ulong startSector, ulong sectorCount, uint index)
        : this(host, startSector, sectorCount, BuildName(host.Name, index))
    {
    }

    /// <summary>Creates a block-device view of a partition on a host disk.</summary>
    /// <param name="host">The disk this partition lives on.</param>
    /// <param name="startSector">Absolute LBA on the host where the partition begins.</param>
    /// <param name="sectorCount">Length of the partition in sectors.</param>
    /// <param name="name">Display name for the partition.</param>
    /// <exception cref="ArgumentOutOfRangeException">The partition would extend past the end of <paramref name="host"/>.</exception>
    public Partition(IBlockDevice host, ulong startSector, ulong sectorCount, string name)
    {
        // Overflow-safe, and the only containment in the stack: CheckBounds
        // measures a request against BlockCount, which is whatever was passed
        // here, so an oversized view turns in-bounds-looking calls into host
        // I/O off the end of the disk. Neither the AHCI port nor the NVMe
        // namespace the kit's drivers publish range-checks the LBA it is
        // handed.
        if (startSector > host.BlockCount || sectorCount > host.BlockCount - startSector)
        {
            throw new ArgumentOutOfRangeException(nameof(sectorCount), "Partition extends beyond the end of its host device.");
        }

        _host = host;
        _name = name;
        StartSector = startSector;
        BlockCount = sectorCount;
        BlockSize = host.BlockSize;
    }

    /// <inheritdoc />
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        CheckBounds(blockNo, blockCount);
        _host.ReadBlock(StartSector + blockNo, blockCount, data);
    }

    /// <inheritdoc />
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        CheckBounds(blockNo, blockCount);
        _host.WriteBlock(StartSector + blockNo, blockCount, data);
    }

    /// <inheritdoc />
    public void Flush()
    {
        _host.Flush();
    }

    private void CheckBounds(ulong blockNo, ulong blockCount)
    {
        // Overflow-safe: `blockNo + blockCount` would wrap for a blockNo near
        // ulong.MaxValue and slip past a naive `sum > BlockCount` check.
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "Partition I/O extends beyond partition end.");
        }
    }

    /// <summary>Separator between the host name and the partition index ("sata0" + "p" + "1").</summary>
    private const string PartitionInfix = "p";

    /// <summary>Decimal digits in <see cref="uint.MaxValue"/> (4294967295), the longest index.</summary>
    private const int MaxIndexDigits = 10;

    /// <summary>Radix used when converting the index to its decimal digits.</summary>
    private const uint DecimalBase = 10;

    // Builds "<host>p<index>" without CoreLib int formatting (uint.ToString,
    // "" + uint, $""), which reproducibly triple-faulted in device
    // constructors that ran before the exception handlers and the late
    // module initializers; the index is formatted by hand in case partition
    // registration ever moves that early. The host name is copied whole.
    private static string BuildName(string hostName, uint index)
    {
        Span<char> digits = stackalloc char[MaxIndexDigits];
        int start = digits.Length;
        do
        {
            digits[--start] = (char)('0' + index % DecimalBase);
            index /= DecimalBase;
        }
        while (index != 0);

        return string.Concat(hostName, PartitionInfix, new string(digits[start..]));
    }
}
