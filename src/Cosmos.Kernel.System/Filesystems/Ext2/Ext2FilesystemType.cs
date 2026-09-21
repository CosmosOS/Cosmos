// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Storage;
using global::System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// ext2 driver entry point. Pluggable into the VFS via <c>VfsManager.RegisterFilesystem</c>.
/// Mirrors <c>FatFilesystemType</c> construction.
/// </summary>
public sealed class Ext2FilesystemType : IVfsFilesystemType
{
    /// <summary>Radix used to parse the decimal partition-index mount source string.</summary>
    private const int DecimalRadix = 10;

    private readonly IBlockDevice? _injectedDevice;

    /// <summary>
    /// Creates a driver that resolves its device from the mount source string.
    /// </summary>
    public Ext2FilesystemType()
    {
    }

    /// <summary>
    /// Creates a driver bound to a fixed device, used when the mount source is
    /// empty (test seam).
    /// </summary>
    /// <param name="device">The block device holding the volume.</param>
    public Ext2FilesystemType(IBlockDevice device)
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

        if (!Ext2Superblock.TryCreate(device, out Ext2Superblock? sb))
        {
            return false;
        }

        superblock = sb;
        return true;
    }

    /// <inheritdoc />
    public bool TryFormat(ReadOnlySpan<char> source, IVfsFormatOptions? options)
    {
        IBlockDevice? device = ResolveDevice(source);
        if (device is null)
        {
            return false;
        }

        Ext2FormatOptions? ext2Options = options as Ext2FormatOptions;
        if (options is not null && ext2Options is null)
        {
            return false;
        }

        return Ext2Formatter.Format(device, ext2Options);
    }

    /// <inheritdoc />
    public bool TryDestroy(ReadOnlySpan<char> source)
    {
        IBlockDevice? device = ResolveDevice(source);
        if (device is null)
        {
            return false;
        }

        return Ext2Formatter.Destroy(device);
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
    /// <returns>true when the string is a non-negative integer without overflow.</returns>
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
