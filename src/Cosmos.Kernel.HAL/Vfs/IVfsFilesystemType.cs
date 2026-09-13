// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Registered filesystem driver entry point.
/// </summary>
public interface IVfsFilesystemType
{
    /// <summary>
    /// Mount the filesystem found on <paramref name="source"/> and produce
    /// its superblock.
    /// </summary>
    /// <param name="source">Optional backing store identifier (device path, image id, etc.).</param>
    /// <param name="flags">Mount flags (<see cref="MountFlags"/>).</param>
    /// <param name="superblock">Populated superblock on success.</param>
    /// <returns>true when a filesystem was recognized and mounted.</returns>
    bool TryMount(ReadOnlySpan<char> source, MountFlags flags, [NotNullWhen(true)] out IVfsSuperblock? superblock);

    /// <summary>
    /// Lay down a fresh on-disk filesystem on the backing store identified by
    /// <paramref name="source"/>. Drivers cast <paramref name="options"/> to
    /// their own concrete <see cref="IVfsFormatOptions"/> type; passing
    /// <c>null</c> selects driver defaults. The source must not be mounted:
    /// <c>VfsManager.TryFormat</c>/<c>TryDestroy</c> refuse a source with a
    /// live mount, since rewriting the volume underneath a superblock's
    /// cached geometry corrupts it.
    /// </summary>
    /// <param name="source">Backing store identifier (device path, partition index, etc.).</param>
    /// <param name="options">Driver-specific format options, or null for the driver's defaults.</param>
    /// <returns>true when the volume was formatted.</returns>
    bool TryFormat(ReadOnlySpan<char> source, IVfsFormatOptions? options);

    /// <summary>
    /// Wipe the filesystem signature on the backing store so it no longer
    /// mounts. The underlying device is not zeroed in full.
    /// </summary>
    /// <param name="source">Backing store identifier (device path, partition index, etc.).</param>
    /// <returns>true when the signature was wiped.</returns>
    bool TryDestroy(ReadOnlySpan<char> source);
}
