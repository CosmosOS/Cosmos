// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Storage;

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>
/// FAT12 / FAT16 / FAT32 driver entry point. Pluggable into the VFS via
/// <c>VfsManager.RegisterFilesystem</c>. Source strings:
/// <list type="bullet">
///   <item><description>Empty: use the injected device passed to the constructor (test seam).</description></item>
///   <item><description><c>"&lt;index&gt;"</c>: partition index in <see cref="StorageManager.Partitions"/>.</description></item>
/// </list>
/// </summary>
public sealed class FatFilesystemType : IVfsFilesystemType
{
    /// <summary>Number of blocks the BPB probe reads: the boot sector occupies exactly one sector (fatgen103 section 3.1).</summary>
    private const ulong BootSectorBlockCount = 1;

    /// <summary>Radix used to parse the decimal partition-index mount source string.</summary>
    private const int DecimalRadix = 10;

    private readonly IBlockDevice? _injectedDevice;

    /// <summary>
    /// Creates a driver that resolves its device from the mount source string.
    /// </summary>
    public FatFilesystemType()
    {
    }

    /// <summary>
    /// Creates a driver bound to a fixed device, used when the mount source is
    /// empty (test seam).
    /// </summary>
    /// <param name="device">The block device holding the FAT volume.</param>
    public FatFilesystemType(IBlockDevice device)
    {
        _injectedDevice = device;
    }

    /// <inheritdoc />
    public bool TryMount(ReadOnlySpan<char> source, MountFlags flags, [NotNullWhen(true)] out IVfsSuperblock? superblock)
    {
        superblock = null;

        IBlockDevice? device = ResolveDevice(source);
        if (device == null)
        {
            return false;
        }

        Span<byte> bpb = new byte[device.BlockSize];
        device.ReadBlock(FatBootSector.BootSectorLba, BootSectorBlockCount, bpb);

        if (!FatBootSector.TryParse(bpb, out FatBootSector? boot))
        {
            return false;
        }

        if (boot.BytesPerSector != device.BlockSize)
        {
            return false;
        }
        // The parser never sees the device: reject volumes whose claimed
        // geometry leaves it, and FAT32 root clusters outside the data
        // area (0 underflows ClusterToLba's cluster - 2), before any
        // FAT or cluster I/O can hit the device's range guard.
        if (boot.TotalSectorCount > device.BlockCount)
        {
            return false;
        }
        if (boot.Type == FatType.Fat32
            && (boot.RootCluster < FatTable.FirstDataCluster
                || boot.RootCluster >= boot.ClusterCount + FatTable.FirstDataCluster))
        {
            return false;
        }

        superblock = new FatSuperblock(device, boot);
        return true;
    }

    /// <inheritdoc />
    public bool TryFormat(ReadOnlySpan<char> source, IVfsFormatOptions? options)
    {
        IBlockDevice? device = ResolveDevice(source);
        if (device == null)
        {
            return false;
        }

        FatFormatOptions? fatOptions = options as FatFormatOptions;
        if (options != null && fatOptions == null)
        {
            return false;
        }

        return FatFormatter.Format(device, fatOptions);
    }

    /// <inheritdoc />
    public bool TryDestroy(ReadOnlySpan<char> source)
    {
        IBlockDevice? device = ResolveDevice(source);
        if (device == null)
        {
            return false;
        }
        return FatFormatter.Destroy(device);
    }

    private IBlockDevice? ResolveDevice(ReadOnlySpan<char> source)
    {
        if (source.IsEmpty || source.IsWhiteSpace())
        {
            return _injectedDevice;
        }

        if (!TryParseInt(source, out int partitionIndex))
        {
            return null;
        }

        IReadOnlyList<Partition> partitions = StorageManager.Partitions;
        if (partitionIndex < 0 || partitionIndex >= partitions.Count)
        {
            return null;
        }

        return partitions[partitionIndex];
    }

    private static bool TryParseInt(ReadOnlySpan<char> source, out int value)
    {
        value = 0;
        if (source.Length == 0)
        {
            return false;
        }

        int result = 0;
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c < '0' || c > '9')
            {
                return false;
            }
            int digit = c - '0';
            // Unchecked accumulation wraps: "4294967297" would parse as 1
            // and alias destructive operations (format/destroy) onto a
            // valid low partition index.
            if (result > (int.MaxValue - digit) / DecimalRadix)
            {
                return false;
            }
            result = result * DecimalRadix + digit;
        }

        value = result;
        return true;
    }
}
