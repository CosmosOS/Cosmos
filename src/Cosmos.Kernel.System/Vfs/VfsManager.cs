// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Storage;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Central entry point for registering filesystem drivers and resolving VFS paths.
/// Path-level operations (current directory, create/unlink/rename/remove,
/// virtual root) live in the <c>VfsManager.Paths.cs</c> partial.
/// </summary>
public static partial class VfsManager
{
    private sealed class VfsOpenFile : IVfsOpenFile
    {
        public VfsOpenFile(string name, IVfsInode inode, IFileOperations operations)
        {
            Name = name;
            Inode = inode;
            Operations = operations;
            Position = 0;
        }

        public string Name { get; }

        public IVfsInode Inode { get; }

        public IFileOperations Operations { get; }

        public long Position { get; set; }
    }

    /// <summary>
    /// Represents a mounted filesystem instance.
    /// </summary>
    public sealed class VfsMount
    {
        /// <summary>
        /// Creates a mount record.
        /// </summary>
        /// <param name="name">The registered driver name.</param>
        /// <param name="source">The driver-specific backing-store identifier.</param>
        /// <param name="mountPoint">The absolute path the filesystem is mounted at.</param>
        /// <param name="filesystemType">The filesystem driver.</param>
        /// <param name="superblock">The mounted filesystem instance.</param>
        /// <param name="partition">The partition mounted, when the mount named one.</param>
        internal VfsMount(string name, string source, string mountPoint, IVfsFilesystemType filesystemType, IVfsSuperblock superblock, Partition? partition)
        {
            Name = name;
            Source = source;
            MountPoint = mountPoint;
            FilesystemType = filesystemType;
            Superblock = superblock;
            Partition = partition;
        }

        /// <summary>Registered driver name (e.g. "fat").</summary>
        public string Name { get; }

        /// <summary>Driver-specific backing-store identifier passed to <see cref="TryMount(string, global::System.ReadOnlySpan{char}, Cosmos.Kernel.HAL.Vfs.MountFlags, string, out VfsMount)"/>. For the FAT driver this is the global partition index in <c>StorageManager.Partitions</c> as a decimal string.</summary>
        public string Source { get; }

        /// <summary>Absolute path the filesystem is mounted at (e.g. "/").</summary>
        public string MountPoint { get; }

        /// <summary>The filesystem driver that produced this mount.</summary>
        internal IVfsFilesystemType FilesystemType { get; }

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
    }

    private static readonly Dictionary<string, IVfsFilesystemType> s_registeredTypes = new(StringComparer.Ordinal);
    private static readonly List<VfsMount> s_mounts = new();
    private static readonly string s_directorySeparatorString = Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// All currently active mounts in registration order.
    /// </summary>
    public static IReadOnlyList<VfsMount> Mounts => s_mounts;

    /// <summary>
    /// Register a filesystem driver by name.
    /// </summary>
    /// <returns><c>true</c> when registration succeeds; <c>false</c> if name is invalid, driver is null, or already registered.</returns>
    public static bool RegisterFilesystem(string name, IVfsFilesystemType filesystemType)
    {
        return !string.IsNullOrWhiteSpace(name)
            && filesystemType is not null
            && s_registeredTypes.TryAdd(name, filesystemType);
    }

    /// <summary>
    /// Resolves a registered driver by name. Guards the key the way
    /// <see cref="RegisterFilesystem"/> does, because the backing dictionary
    /// throws on a null key and no member here may throw for a name that
    /// simply names no driver.
    /// </summary>
    private static bool TryGetFilesystem(string name, [NotNullWhen(true)] out IVfsFilesystemType? filesystemType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            filesystemType = null;
            return false;
        }

        return s_registeredTypes.TryGetValue(name, out filesystemType);
    }

    /// <summary>
    /// Mount a registered filesystem driver at a mount point.
    /// </summary>
    /// <param name="name">Registered filesystem name.</param>
    /// <param name="source">Driver-specific backing store identifier.</param>
    /// <param name="flags">Mount flags.</param>
    /// <param name="mountPoint">Mount point (normalized to leading /, no trailing /).</param>
    /// <param name="mount">Resulting mount data.</param>
    /// <returns><c>true</c> on success, <c>false</c> if driver is missing or mount fails.</returns>
    public static bool TryMount(string name, ReadOnlySpan<char> source, MountFlags flags, string mountPoint, [NotNullWhen(true)] out VfsMount? mount)
    {
        return TryMount(name, source, flags, mountPoint, null, out mount);
    }

    /// <summary>
    /// Mount a registered filesystem driver on <paramref name="partition"/>.
    /// </summary>
    /// <param name="name">Registered filesystem name.</param>
    /// <param name="partition">The partition to mount, from <see cref="StorageManager.Partitions"/>.</param>
    /// <param name="flags">Mount flags.</param>
    /// <param name="mountPoint">Mount point (normalized to leading /, no trailing /).</param>
    /// <param name="mount">Resulting mount data.</param>
    /// <returns><c>true</c> on success, <c>false</c> if the driver is missing, the partition is not registered, or the mount fails.</returns>
    /// <remarks>
    /// The driver still receives the index as its source string, because
    /// <see cref="IVfsFilesystemType"/> lives in the HAL and cannot name a
    /// <see cref="Partition"/>. Resolving it here rather than at the call site
    /// is what closes the window in which a rescan renumbers the list.
    /// </remarks>
    public static bool TryMount(string name, Partition partition, MountFlags flags, string mountPoint, [NotNullWhen(true)] out VfsMount? mount)
    {
        mount = null;

        int index = IndexOfPartition(partition);
        if (index < 0)
        {
            return false;
        }

        return TryMount(name, index.ToString(), flags, mountPoint, partition, out mount);
    }

    private static bool TryMount(string name, ReadOnlySpan<char> source, MountFlags flags, string mountPoint, Partition? partition, [NotNullWhen(true)] out VfsMount? mount)
    {
        mount = null;

        if (!TryGetFilesystem(name, out IVfsFilesystemType? filesystemType))
        {
            return false;
        }

        if (!filesystemType.TryMount(source, flags, out IVfsSuperblock? superblock))
        {
            return false;
        }

        string normalizedMountPoint = NormalizeMountPoint(mountPoint);
        mount = new VfsMount(name, source.ToString(), normalizedMountPoint, filesystemType, superblock, partition);
        s_mounts.Add(mount);

        return true;
    }

    /// <summary>
    /// The position of <paramref name="partition"/> in
    /// <see cref="StorageManager.Partitions"/>, or -1 when it is not
    /// registered. That position is the source string the drivers parse.
    /// </summary>
    private static int IndexOfPartition(Partition partition)
    {
        IReadOnlyList<Partition> partitions = StorageManager.Partitions;
        for (int i = 0; i < partitions.Count; i++)
        {
            if (ReferenceEquals(partitions[i], partition))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Format the backing store for a registered driver. The driver decides
    /// what <paramref name="source"/> means (partition index, injected device,
    /// etc.) and casts <paramref name="options"/> to its own option type.
    /// </summary>
    /// <param name="name">Registered filesystem name.</param>
    /// <param name="source">Driver-specific backing-store identifier.</param>
    /// <param name="options">Driver-specific format options, or null for its defaults.</param>
    /// <returns><see langword="false"/> when no driver is registered under
    /// <paramref name="name"/>, when that driver already has
    /// <paramref name="source"/> mounted, or when the driver itself refuses
    /// the format.</returns>
    public static bool TryFormat(string name, ReadOnlySpan<char> source, IVfsFormatOptions? options)
    {
        if (!TryGetFilesystem(name, out IVfsFilesystemType? filesystemType))
        {
            return false;
        }
        if (IsSourceMounted(name, source))
        {
            return false;
        }
        return filesystemType.TryFormat(source, options);
    }

    /// <summary>
    /// Format <paramref name="partition"/> with a registered driver. Refused
    /// while the partition is mounted.
    /// </summary>
    /// <param name="name">Registered filesystem name.</param>
    /// <param name="partition">The partition to format, from <see cref="StorageManager.Partitions"/>.</param>
    /// <param name="options">Driver-specific format options, or null for its defaults.</param>
    /// <returns><c>true</c> on success, <c>false</c> if the driver is missing, the partition is not registered or mounted, or the format fails.</returns>
    public static bool TryFormat(string name, Partition partition, IVfsFormatOptions? options)
    {
        int index = IndexOfPartition(partition);
        return index >= 0 && TryFormat(name, index.ToString(), options);
    }

    /// <summary>
    /// Wipe the filesystem signature on the backing store for a registered
    /// driver so it no longer mounts.
    /// </summary>
    /// <returns><see langword="false"/> when no driver is registered under
    /// <paramref name="name"/>, when that driver already has
    /// <paramref name="source"/> mounted, or when the driver itself refuses
    /// to wipe the signature.</returns>
    public static bool TryDestroy(string name, ReadOnlySpan<char> source)
    {
        if (!TryGetFilesystem(name, out IVfsFilesystemType? filesystemType))
        {
            return false;
        }
        if (IsSourceMounted(name, source))
        {
            return false;
        }
        return filesystemType.TryDestroy(source);
    }

    /// <summary>
    /// Unmount the filesystem at <paramref name="mountPoint"/>: drops the
    /// superblock (which flushes per the driver's Drop semantics) and
    /// removes the mount from the table.
    /// </summary>
    /// <returns><see langword="false"/> when no mount sits at
    /// <paramref name="mountPoint"/> once it is normalized. A driver whose
    /// Drop fails does not report it here.</returns>
    public static bool TryUnmount(string mountPoint)
    {
        string normalizedMountPoint = NormalizeMountPoint(mountPoint);
        for (int i = 0; i < s_mounts.Count; i++)
        {
            VfsMount current = s_mounts[i];
            if (string.Equals(current.MountPoint, normalizedMountPoint, StringComparison.Ordinal))
            {
                current.Superblock.SuperOperations.Drop(current.Superblock);
                s_mounts.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// True when a live mount matches the driver name and source —
    /// formatting or destroying it would rewrite the volume underneath a
    /// superblock that still holds the old geometry and caches.
    /// </summary>
    private static bool IsSourceMounted(string name, ReadOnlySpan<char> source)
    {
        for (int i = 0; i < s_mounts.Count; i++)
        {
            VfsMount current = s_mounts[i];
            if (string.Equals(current.Name, name, StringComparison.Ordinal)
                && source.SequenceEqual(current.Source))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Retrieve a mount by its mount point.
    /// </summary>
    /// <returns><see langword="false"/> when no mount sits at
    /// <paramref name="mountPoint"/> once it is normalized.</returns>
    public static bool TryGetMount(string mountPoint, [NotNullWhen(true)] out VfsMount? mount)
    {
        string normalizedMountPoint = NormalizeMountPoint(mountPoint);

        for (int i = 0; i < s_mounts.Count; i++)
        {
            VfsMount current = s_mounts[i];
            if (string.Equals(current.MountPoint, normalizedMountPoint, StringComparison.Ordinal))
            {
                mount = current;
                return true;
            }
        }

        mount = null;
        return false;
    }

    /// <summary>
    /// Read the filesystem-level statistics of the mount at
    /// <paramref name="mountPoint"/>: total, free and available blocks, the
    /// block size they are counted in, and the inode counts when the driver
    /// tracks them. Multiply <see cref="VfsStatFs.Bavail"/> by
    /// <see cref="VfsStatFs.BlockSize"/> for the free space a kernel can use.
    /// </summary>
    /// <param name="mountPoint">The mount point to query.</param>
    /// <param name="stats">The statistics when the call succeeds.</param>
    /// <returns><see langword="true"/> when the mount exists and its driver produced the statistics.</returns>
    public static bool TryStatFs(string mountPoint, out VfsStatFs stats)
    {
        if (!TryGetMount(mountPoint, out VfsMount? mount))
        {
            stats = default;
            return false;
        }

        return mount.Superblock.SuperOperations.StatFs(mount.Superblock, out stats);
    }

    /// <summary>
    /// Open a file at the given path and return a managed handle wrapper.
    /// </summary>
    /// <returns><see langword="false"/> when <paramref name="path"/> does not
    /// resolve, or resolves to an inode whose driver offers no file
    /// operations, which is what a directory looks like from here.</returns>
    public static bool TryOpenFile(string path, [NotNullWhen(true)] out IVfsFileHandle? file)
    {
        file = null;

        if (!TryResolve(path, out IVfsInode? inode, out string? leafName))
        {
            return false;
        }

        IFileOperations? fileOperations = inode.FileOperations;
        if (fileOperations == null)
        {
            return false;
        }

        IVfsOpenFile openFile = new VfsOpenFile(leafName, inode, fileOperations);
        VfsFileHandle handle = new VfsFileHandle(leafName, inode, openFile)
        {
            OpenedPath = path,
            Tracked = true,
        };
        RegisterOpenFile(handle);
        file = handle;
        return true;
    }

    /// <summary>
    /// Open a directory at the given path and return a managed handle wrapper.
    /// Fails when the path names something other than a directory.
    /// </summary>
    /// <returns><see langword="false"/> when <paramref name="path"/> does not
    /// resolve, when its attributes cannot be read, or when it names something
    /// other than a directory.</returns>
    public static bool TryOpenDirectory(string path, [NotNullWhen(true)] out IVfsDirectoryHandle? directory)
    {
        directory = null;

        if (!TryResolve(path, out IVfsInode? inode, out string? leafName))
        {
            return false;
        }

        // A regular file resolves just as well as a directory. Without the type
        // check the caller gets a directory handle whose every operation then
        // fails inside the driver with a misleading error.
        if (!inode.InodeOperations.GetAttr(inode, out VfsStat stat) || !stat.IsDirectory)
        {
            return false;
        }

        directory = new VfsDirectoryHandle(leafName, inode);
        return true;
    }

    /// <summary>
    /// Wrap an inode into a file or directory handle based on metadata and available operations.
    /// </summary>
    internal static IVfsNodeHandle? WrapNode(string name, IVfsInode inode)
    {
        VfsStat stat;
        if (inode.InodeOperations.GetAttr(inode, out stat))
        {
            if (stat.IsDirectory)
            {
                return new VfsDirectoryHandle(name, inode);
            }
        }

        IFileOperations? fileOperations = inode.FileOperations;
        if (fileOperations != null)
        {
            IVfsOpenFile openFile = new VfsOpenFile(name, inode, fileOperations);
            return new VfsFileHandle(name, inode, openFile);
        }

        return null;
    }

    /// <summary>
    /// Opens any node, file or directory, as a handle. For callers that need
    /// the inode and must not care about its type.
    /// </summary>
    internal static bool TryOpenNode(string path, [NotNullWhen(true)] out IVfsNodeHandle? node)
    {
        node = null;

        if (!TryResolve(path, out IVfsInode? inode, out string? leafName))
        {
            return false;
        }

        node = WrapNode(leafName, inode);
        return node != null;
    }

    private static bool TryResolve(string path, [NotNullWhen(true)] out IVfsInode? inode, [NotNullWhen(true)] out string? leafName)
    {
        inode = null;
        leafName = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        VfsMount? mount = FindMount(path);
        if (mount == null)
        {
            return false;
        }

        string relativePath = TrimMountPrefix(mount.MountPoint, path);
        IVfsInode current = mount.Superblock.Root;

        leafName = mount.MountPoint;
        if (relativePath.Length == 0)
        {
            inode = current;
            return true;
        }

        string[] parts = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            string segment = parts[i];
            if (segment.Length == 0 || segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                return false;
            }

            IInodeOperations operations = current.InodeOperations;
            if (!operations.Lookup(current, segment, out IVfsInode? child))
            {
                return false;
            }

            current = child;
            leafName = segment;
        }

        inode = current;
        return true;
    }

    /// <summary>
    /// True when <paramref name="mountPoint"/> (normalized: leading /, no
    /// trailing /) covers <paramref name="path"/> on a path-segment boundary
    /// — "/mnt" covers "/mnt" and "/mnt/x" but not "/mntx".
    /// </summary>
    internal static bool MountCovers(string mountPoint, string path)
    {
        if (mountPoint == s_directorySeparatorString)
        {
            return true;
        }

        if (!path.StartsWith(mountPoint, StringComparison.Ordinal))
        {
            return false;
        }

        return path.Length == mountPoint.Length || path[mountPoint.Length] == Path.DirectorySeparatorChar;
    }

    private static VfsMount? FindMount(string path)
    {
        VfsMount? bestMatch = null;

        for (int i = 0; i < s_mounts.Count; i++)
        {
            VfsMount candidate = s_mounts[i];
            if (!MountCovers(candidate.MountPoint, path))
            {
                continue;
            }

            if (bestMatch == null || candidate.MountPoint.Length > bestMatch.MountPoint.Length)
            {
                bestMatch = candidate;
            }
        }

        return bestMatch;
    }

    private static string NormalizeMountPoint(string mountPoint)
    {
        if (string.IsNullOrWhiteSpace(mountPoint))
        {
            return s_directorySeparatorString;
        }

        string normalized = mountPoint;
        if (!normalized.StartsWith(s_directorySeparatorString, StringComparison.Ordinal))
        {
            normalized = Path.Combine(s_directorySeparatorString, normalized);
        }

        if (normalized.Length > 1 && normalized.EndsWith(s_directorySeparatorString, StringComparison.Ordinal))
        {
            normalized = normalized.TrimEnd(Path.DirectorySeparatorChar);
        }

        return normalized;
    }

    private static string TrimMountPrefix(string mountPoint, string path)
    {
        if (mountPoint == s_directorySeparatorString)
        {
            return path.TrimStart(Path.DirectorySeparatorChar);
        }

        string trimmed = path.StartsWith(mountPoint, StringComparison.Ordinal)
            ? path.Substring(mountPoint.Length)
            : path;

        return trimmed.TrimStart(Path.DirectorySeparatorChar);
    }
}
