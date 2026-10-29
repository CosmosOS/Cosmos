// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.System.Storage;

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// ISO 9660 driver entry point, for CD-ROM images and the volumes they are
/// written to. Pluggable into the VFS via <see cref="VfsManager.RegisterFileSystem"/>;
/// mirrors <see cref="Fat.FatFileSystemType"/> and <see cref="Ext2.Ext2FileSystemType"/>
/// construction. The volume is read-only: a mount made through it refuses
/// every mutation, and formatting or destroying one is refused.
/// </summary>
public sealed class Iso9660FileSystemType : IVfsFileSystemType
{
    /// <summary>Radix used to parse the decimal partition-index mount source string.</summary>
    private const int DecimalRadix = 10;

    private readonly IBlockDevice? _injectedDevice;

    /// <summary>
    /// Creates a driver that resolves its device from the mount source string.
    /// </summary>
    public Iso9660FileSystemType()
    {
    }

    /// <summary>
    /// Creates a driver bound to a fixed device, used when the mount source
    /// is empty: a RAM disk holding an image, or a CD-ROM device the kernel
    /// has in hand.
    /// </summary>
    /// <param name="device">The block device holding the volume.</param>
    public Iso9660FileSystemType(IBlockDevice device)
    {
        _injectedDevice = device;
    }

    /// <inheritdoc />
    public bool TryMount(ReadOnlySpan<char> source, MountFlags flags, [NotNullWhen(true)] out IVfsSuperblock? superblock)
    {
        superblock = null;
        IBlockDevice? device = ResolveDevice(source);
        if (device is null)
        {
            return false;
        }

        if (!Iso9660Superblock.TryCreate(device, out Iso9660Superblock? volume))
        {
            return false;
        }

        superblock = volume;
        return true;
    }

    /// <summary>
    /// Refused: the driver writes no volume.
    /// </summary>
    /// <param name="source">Backing store identifier.</param>
    /// <param name="options">Ignored.</param>
    /// <returns>Always <see langword="false"/>.</returns>
    public bool TryFormat(ReadOnlySpan<char> source, IVfsFormatOptions? options)
    {
        return false;
    }

    /// <summary>
    /// Refused: the driver writes no volume.
    /// </summary>
    /// <param name="source">Backing store identifier.</param>
    /// <returns>Always <see langword="false"/>.</returns>
    public bool TryDestroy(ReadOnlySpan<char> source)
    {
        return false;
    }

    /// <summary>
    /// Resolve the mount source to a device: empty selects the injected
    /// device, otherwise a decimal partition index.
    /// </summary>
    /// <param name="source">Mount source string.</param>
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

    /// <summary>
    /// Parse a decimal partition index without allocating.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="value">Parsed index on success.</param>
    /// <returns><see langword="true"/> when the string is a non-negative integer without overflow.</returns>
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
            if (result > (int.MaxValue - digit) / DecimalRadix)
            {
                return false;
            }

            result = (result * DecimalRadix) + digit;
        }

        value = result;
        return true;
    }
}
