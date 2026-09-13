// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Path-level VFS operations: the kernel current directory, the virtual root
/// (the synthesized "/" that exists while nothing is mounted over it), and
/// whole-path create/unlink/rename/remove built on the handle layer —
/// including the semantics open handles depend on (delete-pending unlink,
/// destination-preserving rename).
/// </summary>
public static partial class VfsManager
{
    /// <summary>Leaf suffix used to move a rename destination aside until the real
    /// rename has succeeded — a replacing rename must not lose the
    /// destination when the rename itself fails.</summary>
    private const string ReplaceBackupSuffix = ".~replace";

    /// <summary>Handles produced by <see cref="TryOpenFile"/>; consulted so unlinking
    /// an open file can defer to the last close.</summary>
    private static readonly List<VfsFileHandle> s_openFileHandles = new();

    private static string s_currentDirectory = "/";

    /// <summary>Kernel-wide current directory; always a normalized absolute path.</summary>
    /// <remarks>
    /// Internal with the path helpers below it: their only callers are the PAL
    /// adapter in <c>Cosmos.Kernel.Plugs</c>, and a kernel reaches the current
    /// directory through <see cref="global::System.IO.Directory"/>, which that
    /// adapter is plugged under. A second public anchor would just disagree
    /// with whatever cwd the kernel's own shell keeps.
    /// </remarks>
    internal static string CurrentDirectory => s_currentDirectory;

    /// <summary>Sets <see cref="CurrentDirectory"/>; the target must exist and be a directory.</summary>
    internal static bool TrySetCurrentDirectory(string path)
    {
        string? fullPath = MakeAbsolute(path);
        if (fullPath == null || !TryStat(fullPath, out VfsStat stat))
        {
            return false;
        }

        if (!stat.IsDirectory)
        {
            return false;
        }

        s_currentDirectory = fullPath;
        return true;
    }

    /// <summary>Anchors a relative path at <see cref="CurrentDirectory"/> and strips
    /// trailing separators. Returns null for a null/empty input. Does not
    /// collapse <c>.</c>/<c>..</c> segments.</summary>
    internal static string? MakeAbsolute(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string result = path[0] == Path.DirectorySeparatorChar
            ? path
            : (s_currentDirectory == s_directorySeparatorString
                ? Path.Combine(s_directorySeparatorString, path)
                : Path.Combine(s_currentDirectory, path));

        int end = result.Length;
        while (end > 1 && result[end - 1] == Path.DirectorySeparatorChar)
        {
            end--;
        }

        return end == result.Length ? result : result.Substring(0, end);
    }

    /// <summary>Splits an absolute path into its parent path and leaf name
    /// ("/mnt/dir/file" → "/mnt/dir" + "file"; "/file" → "/" + "file").</summary>
    internal static void SplitParentLeaf(string fullPath, out string parentPath, out string leaf)
    {
        int lastSeparator = fullPath.LastIndexOf('/');
        if (lastSeparator <= 0)
        {
            parentPath = s_directorySeparatorString;
            leaf = fullPath.Length > 1 ? fullPath.Substring(1) : string.Empty;
            return;
        }

        parentPath = fullPath.Substring(0, lastSeparator);
        leaf = fullPath.Substring(lastSeparator + 1);
    }

    /// <summary>True for "/" and for every active mount point.</summary>
    internal static bool IsMountPoint(string fullPath)
    {
        if (fullPath == s_directorySeparatorString)
        {
            return true;
        }

        for (int i = 0; i < s_mounts.Count; i++)
        {
            if (string.Equals(s_mounts[i].MountPoint, fullPath, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Stats any node by absolute path. "/" always succeeds: when no mount
    /// covers it, a synthesized read-only directory stat is returned so the
    /// root exists even on a system with nothing mounted.</summary>
    public static bool TryStat(string fullPath, out VfsStat stat)
    {
        if (TryResolve(fullPath, out IVfsInode? inode, out _))
        {
            return inode.InodeOperations.GetAttr(inode, out stat);
        }

        if (fullPath == s_directorySeparatorString)
        {
            stat = VirtualRootStat();
            return true;
        }

        stat = default;
        return false;
    }

    /// <summary>First path segments of all mount points — the directory names a
    /// listing of the virtual root shows while no filesystem is mounted at "/".</summary>
    internal static string[] GetVirtualRootEntries()
    {
        var collected = new List<string>(s_mounts.Count);
        for (int i = 0; i < s_mounts.Count; i++)
        {
            string mountPoint = s_mounts[i].MountPoint;
            if (mountPoint == s_directorySeparatorString)
            {
                continue;
            }

            int nextSeparator = mountPoint.IndexOf(Path.DirectorySeparatorChar, 1);
            string firstSegment = nextSeparator < 0
                ? mountPoint.Substring(1)
                : mountPoint.Substring(1, nextSeparator - 1);

            if (!collected.Contains(firstSegment))
            {
                collected.Add(firstSegment);
            }
        }

        return [.. collected];
    }

    /// <summary>Creates a regular file at <paramref name="fullPath"/>; the parent
    /// directory must already exist on a mounted filesystem.</summary>
    public static bool TryCreateFile(string fullPath, VfsMode permissions)
    {
        SplitParentLeaf(fullPath, out string parentPath, out string leaf);
        if (leaf.Length == 0)
        {
            return false;
        }

        if (!TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent))
        {
            return false;
        }

        VfsMode mode = (permissions & VfsMode.PermissionMask) | VfsMode.RegularFile;
        return parent.TryCreateFile(leaf, mode, out _);
    }

    /// <summary>Creates a directory at <paramref name="fullPath"/>; the parent
    /// directory must already exist on a mounted filesystem.</summary>
    public static bool TryCreateDirectory(string fullPath, VfsMode permissions)
    {
        SplitParentLeaf(fullPath, out string parentPath, out string leaf);
        if (leaf.Length == 0)
        {
            return false;
        }

        if (!TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent))
        {
            return false;
        }

        VfsMode mode = (permissions & VfsMode.PermissionMask) | VfsMode.Directory;
        return parent.TryCreateDirectory(leaf, mode, out _);
    }

    /// <summary>Removes the file at <paramref name="fullPath"/>. When open handles
    /// still reference the node the removal is deferred to the last close
    /// (delete-pending) instead — FAT-style drivers free the data clusters
    /// immediately on unlink, out from under any live handle.</summary>
    public static bool TryUnlink(string fullPath)
    {
        if (MarkPendingIfOpen(fullPath))
        {
            return true;
        }

        SplitParentLeaf(fullPath, out string parentPath, out string leaf);
        if (!TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent))
        {
            return false;
        }

        return parent.TryUnlink(leaf);
    }

    /// <summary>Removes the directory at <paramref name="fullPath"/> (driver enforces emptiness).</summary>
    public static bool TryRemoveDirectory(string fullPath)
    {
        SplitParentLeaf(fullPath, out string parentPath, out string leaf);
        if (!TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent))
        {
            return false;
        }

        return parent.TryRemoveDirectory(leaf);
    }

    /// <summary>
    /// Renames <paramref name="oldFullPath"/> to <paramref name="newFullPath"/>,
    /// replacing an existing destination of the same kind (an existing
    /// destination directory must be empty). Refuses mount points, renames
    /// into the source's own subtree, and cross-mount renames. The
    /// destination survives a failed rename, and a destination that is still
    /// open goes delete-pending instead of being dropped under its handles.
    /// </summary>
    /// <param name="oldFullPath">Absolute path of the entry to move.</param>
    /// <param name="newFullPath">Absolute path it should have afterwards.</param>
    /// <returns><see langword="false"/> when either path is a mount point, the
    /// source does not exist, the destination is inside the source's own
    /// subtree, the two sit on different mounts, the two are of different
    /// kinds, the destination is a directory that is not empty, or the driver
    /// refuses the move. Renaming a path onto itself succeeds.</returns>
    public static bool TryRename(string oldFullPath, string newFullPath)
    {
        if (IsMountPoint(oldFullPath) || IsMountPoint(newFullPath))
        {
            return false;
        }

        if (!TryStat(oldFullPath, out VfsStat oldStat))
        {
            return false;
        }

        // Renaming a path onto itself is a no-op, but only once the source is
        // known to exist: taking the shortcut first reported success for a
        // path that resolves to nothing, where rename(2) reports ENOENT.
        if (string.Equals(oldFullPath, newFullPath, StringComparison.Ordinal))
        {
            return true;
        }

        bool oldIsDirectory = oldStat.IsDirectory;
        if (oldIsDirectory && newFullPath.StartsWith($"{oldFullPath}{s_directorySeparatorString}", StringComparison.Ordinal))
        {
            return false;
        }

        VfsMount? oldMount = FindMount(oldFullPath);
        VfsMount? newMount = FindMount(newFullPath);
        if (newMount == null || !ReferenceEquals(oldMount, newMount))
        {
            return false;
        }

        SplitParentLeaf(oldFullPath, out string oldParentPath, out string oldLeaf);
        SplitParentLeaf(newFullPath, out string newParentPath, out string newLeaf);

        if (!TryOpenDirectory(oldParentPath, out IVfsDirectoryHandle? oldParent)
            || !TryOpenDirectory(newParentPath, out IVfsDirectoryHandle? newParent))
        {
            return false;
        }

        bool destinationExists = TryStat(newFullPath, out VfsStat newStat);

        // FAT lookups are case-insensitive, so the destination stat can
        // resolve to the SOURCE entry itself (case-only rename). Dropping it
        // would destroy the file — detect the case and hand the pair
        // straight to the driver. Raw driver Ino (first cluster) identifies
        // every non-empty node; the path comparison covers empty files.
        bool sameEntry = destinationExists
            && ((oldStat.Ino != 0 && newStat.Ino == oldStat.Ino)
                || string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase));

        if (destinationExists && !sameEntry)
        {
            bool newIsDirectory = newStat.IsDirectory;
            if (oldIsDirectory != newIsDirectory)
            {
                return false;
            }

            if (newIsDirectory
                && TryOpenDirectory(newFullPath, out IVfsDirectoryHandle? target)
                && target.TryReadDir(out IReadOnlyList<IVfsInode> entries) && entries.Count > 0)
            {
                return false;
            }

            // The FAT driver refuses existing destinations, so move the
            // destination aside first and only discard it once the real
            // rename has succeeded.
            string backupLeaf = $"{newLeaf}{ReplaceBackupSuffix}";
            if (!newParent.TryRename(newLeaf, newParent, backupLeaf))
            {
                return false;
            }

            if (!oldParent.TryRename(oldLeaf, newParent, newLeaf))
            {
                // Best-effort restore; the destination survives the failure.
                newParent.TryRename(backupLeaf, newParent, newLeaf);
                return false;
            }

            DropDisplacedEntry(newParent, newParentPath, backupLeaf, newIsDirectory, newFullPath);
            return true;
        }

        return oldParent.TryRename(oldLeaf, newParent, newLeaf);
    }

    // ---------------- open-handle tracking ----------------

    internal static void RegisterOpenFile(VfsFileHandle handle) => s_openFileHandles.Add(handle);

    /// <summary>Called from <see cref="VfsFileHandle.Dispose"/> on tracked handles;
    /// executes a deferred unlink once the last handle on the node closes.</summary>
    internal static void OnOpenFileClosed(VfsFileHandle handle)
    {
        s_openFileHandles.Remove(handle);

        string? pending = handle.PendingUnlinkPath;
        if (pending != null && !AnyOpenHandlePendingOn(pending))
        {
            RemoveEntryDirect(pending);
        }
    }

    /// <summary>When any open handle references the node at <paramref name="fullPath"/>,
    /// marks those handles delete-pending and returns true (the caller
    /// reports success without touching the filesystem yet).</summary>
    private static bool MarkPendingIfOpen(string fullPath)
    {
        IVfsInode? inode = null;
        if (TryResolve(fullPath, out IVfsInode? resolved, out _))
        {
            inode = resolved;
        }

        bool any = false;
        for (int i = 0; i < s_openFileHandles.Count; i++)
        {
            VfsFileHandle handle = s_openFileHandles[i];

            // Empty FAT files are not in the driver's inode cache, so the
            // resolved inode can be a fresh object — the path comparison is
            // the reliable fallback there (OrdinalIgnoreCase for FAT-style
            // case-insensitive namespaces).
            if (string.Equals(handle.OpenedPath, fullPath, StringComparison.OrdinalIgnoreCase)
                || (inode != null && ReferenceEquals(handle.Inode, inode)))
            {
                handle.PendingUnlinkPath = fullPath;
                any = true;
            }
        }

        return any;
    }

    private static bool AnyOpenHandlePendingOn(string fullPath)
    {
        for (int i = 0; i < s_openFileHandles.Count; i++)
        {
            if (string.Equals(s_openFileHandles[i].PendingUnlinkPath, fullPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Removes a directory entry without the public-path guards; used for
    /// deferred (delete-pending) removals where all checks already ran.</summary>
    private static void RemoveEntryDirect(string fullPath)
    {
        SplitParentLeaf(fullPath, out string parentPath, out string leaf);
        if (TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent))
        {
            parent.TryUnlink(leaf);
        }
    }

    /// <summary>Discards the destination entry a successful replacing rename moved
    /// aside; handles still open on it go delete-pending instead.</summary>
    private static void DropDisplacedEntry(
        IVfsDirectoryHandle parent, string parentPath, string backupLeaf, bool isDirectory, string originalFullPath)
    {
        if (isDirectory)
        {
            // Verified empty before the rename; a failure here only leaks a
            // stray entry, the rename itself already succeeded.
            parent.TryRemoveDirectory(backupLeaf);
            return;
        }

        string backupPath = parentPath == s_directorySeparatorString
            ? Path.Combine(s_directorySeparatorString, backupLeaf)
            : Path.Combine(parentPath, backupLeaf);

        IVfsInode? backupInode = null;
        if (parent.TryLookup(backupLeaf, out IVfsNodeHandle? backupNode))
        {
            using (backupNode)
            {
                backupInode = backupNode.Inode;
            }
        }

        bool pending = false;
        for (int i = 0; i < s_openFileHandles.Count; i++)
        {
            VfsFileHandle handle = s_openFileHandles[i];
            if (string.Equals(handle.OpenedPath, originalFullPath, StringComparison.OrdinalIgnoreCase)
                || (backupInode != null && ReferenceEquals(handle.Inode, backupInode)))
            {
                handle.PendingUnlinkPath = backupPath;
                pending = true;
            }
        }

        if (!pending)
        {
            parent.TryUnlink(backupLeaf);
        }
    }

    private static VfsStat VirtualRootStat()
    {
        VfsStat stat = default;
        stat.Mode = VfsMode.Directory
            | VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.OwnerExecute
            | VfsMode.GroupRead | VfsMode.GroupExecute
            | VfsMode.OtherRead | VfsMode.OtherExecute;
        stat.NLink = 1;
        return stat;
    }
}
