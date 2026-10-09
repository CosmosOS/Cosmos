// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Storage;

namespace Cosmos.Kernel.System.FileSystem;

/// <summary>
/// Represents a mounted filesystem instance.
/// </summary>
public sealed class VfsMount
{
    /// <summary>Registered driver name (e.g. "fat").</summary>
    public string Name { get; }

    /// <summary>Driver-specific backing-store identifier passed to <see cref="VfsManager.TryMount(string, global::System.ReadOnlySpan{char}, MountFlags, string, out VfsMount)"/>. For the FAT driver this is the global partition index in <c>StorageManager.Partitions</c> as a decimal string.</summary>
    public string Source { get; }

    /// <summary>Absolute path the filesystem is mounted at (e.g. "/").</summary>
    public string MountPoint { get; }

    /// <summary>The filesystem driver that produced this mount.</summary>
    internal IVfsFileSystemType FileSystemType { get; }

    /// <summary>The mounted filesystem instance.</summary>
    public IVfsSuperblock Superblock { get; }

    /// <summary>
    /// The partition this mount was given, or <see langword="null"/> when it
    /// was mounted from a driver-specific source string instead.
    /// </summary>
    /// <remarks>
    /// Prefer this over parsing <see cref="Source"/> back into an index: a
    /// rescan renumbers <c>StorageManager.Partitions</c>, so the recorded
    /// index can come to name a different partition, while this keeps
    /// naming the same range on the same disk.
    /// </remarks>
    public Partition? Partition { get; }

    /// <summary>
    /// Creates a mount record.
    /// </summary>
    /// <param name="name">The registered driver name.</param>
    /// <param name="source">The driver-specific backing-store identifier.</param>
    /// <param name="mountPoint">The absolute path the filesystem is mounted at.</param>
    /// <param name="fileSystemType">The filesystem driver.</param>
    /// <param name="superblock">The mounted filesystem instance.</param>
    /// <param name="partition">The partition mounted, when the mount named one.</param>
    internal VfsMount(string name, string source, string mountPoint, IVfsFileSystemType fileSystemType, IVfsSuperblock superblock, Partition? partition)
    {
        Name = name;
        Source = source;
        MountPoint = mountPoint;
        FileSystemType = fileSystemType;
        Superblock = superblock;
        Partition = partition;
    }
}
