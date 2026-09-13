// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Vfs;
using PalError = global::Interop.Error;
using PalSys = global::Interop.Sys;

namespace Cosmos.Kernel.Plugs.System.IO;

/// <summary>
/// Backing tables for the <c>Interop.Sys</c> file-I/O plugs: adapts the BCL's
/// PAL contract (integer descriptors, <c>DirectoryEntry</c> streams, PAL error
/// codes) onto <see cref="VfsManager"/>. Only PAL-shaped adaptation lives
/// here — filesystem semantics (delete-pending unlink, replacing rename, the
/// current directory, the virtual root) are <see cref="VfsManager"/>'s.
/// </summary>
/// <remarks>
/// Descriptors are small non-negative integers because
/// <c>SafeFileHandle.IsInvalid</c> rejects anything outside [0, int.MaxValue].
/// Descriptors 0-2 are reserved for the stdio convention (the console is
/// plugged at the ConsolePal level and never allocates them). Like
/// <see cref="VfsManager"/> itself, this layer assumes single-threaded use and
/// takes no locks. Paths are the VFS's own unix-style absolute paths; the BCL
/// pre-collapses <c>.</c>/<c>..</c> segments before calling down.
/// </remarks>
internal static unsafe class FileDescriptorTable
{
    private const int MaxOpenFiles = 64;
    private const int FirstFileDescriptor = 3;
    private const int MaxDirectoryStreams = 32;

    /// <summary>Longest VFS name (255 UTF-16 chars) at worst 3 UTF-8 bytes each, plus the NUL.</summary>
    private const int DirectoryNameBufferBytes = (3 * 255) + 1;

    private const int CopyChunkBytes = 32 * 1024;

    /// <summary>Access-mode bits inside <see cref="PalSys.OpenFlags"/> (O_RDONLY/O_WRONLY/O_RDWR).</summary>
    private const PalSys.OpenFlags AccessModeMask = (PalSys.OpenFlags)0xF;

    private sealed class OpenFileState
    {
        public OpenFileState(IVfsFileHandle handle, string path, bool readable, bool writable)
        {
            Handle = handle;
            Path = path;
            Readable = readable;
            Writable = writable;
        }

        public IVfsFileHandle Handle { get; }
        public string Path { get; }
        public bool Readable { get; }
        public bool Writable { get; }
    }

    private sealed class DirectoryStreamState
    {
        public DirectoryStreamState(string[] names, bool[] isDirectory, byte* nameBuffer)
        {
            Names = names;
            IsDirectory = isDirectory;
            NameBuffer = nameBuffer;
            Index = 0;
        }

        public string[] Names { get; }
        public bool[] IsDirectory { get; }
        public byte* NameBuffer { get; }
        public int Index { get; set; }
    }

    private static readonly OpenFileState?[] s_openFiles = new OpenFileState?[MaxOpenFiles];
    private static readonly DirectoryStreamState?[] s_directoryStreams = new DirectoryStreamState?[MaxDirectoryStreams];

    internal static string CurrentDirectory => VfsManager.CurrentDirectory;

    // ---------------- descriptor operations ----------------

    internal static PalError Open(string path, PalSys.OpenFlags flags, int mode, out int fd)
    {
        fd = -1;

        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        PalSys.OpenFlags access = flags & AccessModeMask;
        bool writable = access == PalSys.OpenFlags.O_WRONLY || access == PalSys.OpenFlags.O_RDWR;
        bool readable = access == PalSys.OpenFlags.O_RDONLY || access == PalSys.OpenFlags.O_RDWR;

        IVfsFileHandle? handle;
        if (VfsManager.TryStat(fullPath, out VfsStat stat))
        {
            // POSIX order: O_CREAT|O_EXCL reports EEXIST for ANY existing
            // name, directories included — File.Copy relies on EEXIST (not
            // EISDIR→EACCES) to produce its "is a directory" IOException.
            if ((flags & PalSys.OpenFlags.O_CREAT) != 0 && (flags & PalSys.OpenFlags.O_EXCL) != 0)
            {
                return PalError.EEXIST;
            }

            if (stat.IsDirectory)
            {
                // SafeFileHandle.Open remaps EISDIR to EACCES, which is the
                // BCL's documented behavior for opening a directory path.
                return PalError.EISDIR;
            }

            if (!VfsManager.TryOpenFile(fullPath, out handle) || handle == null)
            {
                return PalError.EIO;
            }

            if ((flags & PalSys.OpenFlags.O_TRUNC) != 0 && writable && stat.Size > 0
                && !SetSize(handle.Inode, 0))
            {
                handle.Dispose();
                return PalError.EIO;
            }
        }
        else
        {
            if ((flags & PalSys.OpenFlags.O_CREAT) == 0)
            {
                return PalError.ENOENT;
            }

            PalError createError = CreateRegularFile(fullPath, mode);
            if (createError != PalError.SUCCESS)
            {
                return createError;
            }

            if (!VfsManager.TryOpenFile(fullPath, out handle) || handle == null)
            {
                return PalError.EIO;
            }
        }

        int slot = FindFreeSlot(s_openFiles);
        if (slot < 0)
        {
            handle.Dispose();
            return PalError.EMFILE;
        }

        s_openFiles[slot] = new OpenFileState(handle, fullPath, readable, writable);
        fd = slot + FirstFileDescriptor;
        return PalError.SUCCESS;
    }

    internal static PalError Close(int fd)
    {
        if (fd >= 0 && fd < FirstFileDescriptor)
        {
            // stdio descriptors are not table-backed; closing them is a no-op.
            return PalError.SUCCESS;
        }

        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        // A pending unlink on the node executes inside Dispose once this was
        // the last open handle — VfsManager tracks its own handles.
        file.Handle.Dispose();
        s_openFiles[fd - FirstFileDescriptor] = null;
        return PalError.SUCCESS;
    }

    internal static PalError Read(int fd, byte* buffer, int count, out int bytesRead)
    {
        bytesRead = 0;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null || !file.Readable)
        {
            return PalError.EBADF;
        }

        if (count < 0)
        {
            return PalError.EINVAL;
        }

        bytesRead = (int)file.Handle.Read(new Span<byte>(buffer, count));
        return PalError.SUCCESS;
    }

    internal static PalError Write(int fd, byte* buffer, int count, out int bytesWritten)
    {
        bytesWritten = 0;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null || !file.Writable)
        {
            return PalError.EBADF;
        }

        if (count < 0)
        {
            return PalError.EINVAL;
        }

        ReadOnlySpan<byte> source = new ReadOnlySpan<byte>(buffer, count);
        int total = 0;
        while (total < count)
        {
            long written = file.Handle.Write(source.Slice(total));
            if (written <= 0)
            {
                break;
            }

            total += (int)written;
        }

        bytesWritten = total;
        if (total == 0 && count > 0)
        {
            // The FAT driver reports failure as a zero-length write; the two
            // causes it cannot distinguish for us are the 4 GiB size cap and
            // cluster exhaustion.
            return file.Handle.Position + count > uint.MaxValue ? PalError.EFBIG : PalError.ENOSPC;
        }

        return PalError.SUCCESS;
    }

    internal static PalError PRead(int fd, byte* buffer, int count, long offset, out int bytesRead)
    {
        bytesRead = 0;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null || !file.Readable)
        {
            return PalError.EBADF;
        }

        long saved = file.Handle.Position;
        if (!file.Handle.TrySeek(offset, SeekWhence.Set))
        {
            return PalError.EINVAL;
        }

        PalError error = Read(fd, buffer, count, out bytesRead);
        file.Handle.TrySeek(saved, SeekWhence.Set);
        return error;
    }

    internal static PalError PWrite(int fd, byte* buffer, int count, long offset, out int bytesWritten)
    {
        bytesWritten = 0;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null || !file.Writable)
        {
            return PalError.EBADF;
        }

        long saved = file.Handle.Position;
        if (!file.Handle.TrySeek(offset, SeekWhence.Set))
        {
            return PalError.EINVAL;
        }

        PalError error = Write(fd, buffer, count, out bytesWritten);
        file.Handle.TrySeek(saved, SeekWhence.Set);
        return error;
    }

    internal static PalError Seek(int fd, long offset, int whence, out long newPosition)
    {
        newPosition = -1;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        if (whence < (int)SeekWhence.Set || whence > (int)SeekWhence.End)
        {
            return PalError.EINVAL;
        }

        if (!file.Handle.TrySeek(offset, (SeekWhence)whence))
        {
            return PalError.EINVAL;
        }

        newPosition = file.Handle.Position;
        return PalError.SUCCESS;
    }

    internal static PalError StatDescriptor(int fd, out PalSys.FileStatus status)
    {
        status = default;

        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        if (!file.Handle.TryStat(out VfsStat stat))
        {
            return PalError.EIO;
        }

        FillStatus(file.Path, in stat, out status);
        return PalError.SUCCESS;
    }

    internal static PalError Fsync(int fd)
    {
        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        return file.Handle.TryFlush() ? PalError.SUCCESS : PalError.EIO;
    }

    internal static PalError Truncate(int fd, long length)
    {
        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        if (length < 0 || !file.Writable)
        {
            return PalError.EINVAL;
        }

        if (!SetSize(file.Handle.Inode, (ulong)length))
        {
            return length > uint.MaxValue ? PalError.EFBIG : PalError.EIO;
        }

        return PalError.SUCCESS;
    }

    internal static PalError SetDescriptorMode(int fd, int mode)
    {
        OpenFileState? file = GetOpenFile(fd);
        if (file == null)
        {
            return PalError.EBADF;
        }

        return ApplyMode(file.Handle.Inode, mode);
    }

    internal static PalError CopyDescriptor(int sourceFd, int destinationFd)
    {
        OpenFileState? source = GetOpenFile(sourceFd);
        OpenFileState? destination = GetOpenFile(destinationFd);
        if (source == null || destination == null || !source.Readable || !destination.Writable)
        {
            return PalError.EBADF;
        }

        byte[] chunk = new byte[CopyChunkBytes];
        while (true)
        {
            long read = source.Handle.Read(chunk);
            if (read <= 0)
            {
                return PalError.SUCCESS;
            }

            int total = 0;
            while (total < (int)read)
            {
                long written = destination.Handle.Write(chunk.AsSpan(total, (int)read - total));
                if (written <= 0)
                {
                    return PalError.ENOSPC;
                }

                total += (int)written;
            }
        }
    }

    /// <summary>Routes fd 1/2 writes (Debug/stderr output from the BCL) to the serial console.</summary>
    internal static void WriteConsole(byte* buffer, int count)
    {
        if (count <= 0)
        {
            return;
        }

        Span<char> chars = count <= 512 ? stackalloc char[count] : new char[count];
        for (int i = 0; i < count; i++)
        {
            chars[i] = (char)buffer[i];
        }

        Log.Write(new string(chars.Slice(0, count)));
    }

    // ---------------- path operations ----------------

    internal static PalError StatPath(string path, out PalSys.FileStatus status)
    {
        status = default;

        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        if (!VfsManager.TryStat(fullPath, out VfsStat stat))
        {
            return PalError.ENOENT;
        }

        FillStatus(fullPath, in stat, out status);
        return PalError.SUCCESS;
    }

    internal static PalError UnlinkFile(string path)
    {
        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        if (VfsManager.IsMountPoint(fullPath))
        {
            return PalError.EISDIR;
        }

        if (!VfsManager.TryStat(fullPath, out VfsStat stat))
        {
            return PalError.ENOENT;
        }

        if (stat.IsDirectory)
        {
            return PalError.EISDIR;
        }

        return VfsManager.TryUnlink(fullPath) ? PalError.SUCCESS : PalError.EIO;
    }

    internal static PalError CreateDirectory(string path, int mode)
    {
        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.EINVAL;
        }

        if (VfsManager.TryStat(fullPath, out _))
        {
            return PalError.EEXIST;
        }

        VfsManager.SplitParentLeaf(fullPath, out _, out string leaf);
        if (leaf.Length == 0)
        {
            return PalError.EINVAL;
        }

        PalError parentError = RequireMountedParent(fullPath);
        if (parentError != PalError.SUCCESS)
        {
            return parentError;
        }

        return VfsManager.TryCreateDirectory(fullPath, PermissionBits(mode))
            ? PalError.SUCCESS
            : PalError.EIO;
    }

    internal static PalError RemoveDirectory(string path)
    {
        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        if (VfsManager.IsMountPoint(fullPath))
        {
            return PalError.EBUSY;
        }

        if (!VfsManager.TryStat(fullPath, out VfsStat stat))
        {
            return PalError.ENOENT;
        }

        if (!stat.IsDirectory)
        {
            return PalError.ENOTDIR;
        }

        if (VfsManager.TryOpenDirectory(fullPath, out IVfsDirectoryHandle? target) && target != null
            && target.TryReadDir(out IReadOnlyList<IVfsInode> entries) && entries.Count > 0)
        {
            return PalError.ENOTEMPTY;
        }

        return VfsManager.TryRemoveDirectory(fullPath) ? PalError.SUCCESS : PalError.EIO;
    }

    internal static PalError Rename(string oldPath, string newPath)
    {
        string? oldFull = VfsManager.MakeAbsolute(oldPath);
        string? newFull = VfsManager.MakeAbsolute(newPath);
        if (oldFull == null || newFull == null)
        {
            return PalError.ENOENT;
        }

        if (string.Equals(oldFull, newFull, StringComparison.Ordinal))
        {
            return PalError.SUCCESS;
        }

        if (VfsManager.IsMountPoint(oldFull) || VfsManager.IsMountPoint(newFull))
        {
            return PalError.EBUSY;
        }

        if (!VfsManager.TryStat(oldFull, out VfsStat oldStat))
        {
            return PalError.ENOENT;
        }

        bool oldIsDirectory = oldStat.IsDirectory;
        if (oldIsDirectory && newFull.StartsWith(oldFull + "/", StringComparison.Ordinal))
        {
            return PalError.EINVAL;
        }

        VfsManager.VfsMount? oldMount = FindMount(oldFull, out _);
        VfsManager.VfsMount? newMount = FindMount(newFull, out _);
        if (newMount == null)
        {
            return PalError.ENOENT;
        }

        if (!ReferenceEquals(oldMount, newMount))
        {
            return PalError.EXDEV;
        }

        // Errno discrimination for an existing destination; VfsManager.TryRename
        // re-verifies the same conditions before mutating (and owns the
        // same-entry guard for FAT's case-insensitive lookups).
        if (VfsManager.TryStat(newFull, out VfsStat newStat))
        {
            bool sameEntry = (oldStat.Ino != 0 && newStat.Ino == oldStat.Ino)
                || string.Equals(oldFull, newFull, StringComparison.OrdinalIgnoreCase);

            if (!sameEntry)
            {
                bool newIsDirectory = newStat.IsDirectory;
                if (oldIsDirectory && !newIsDirectory)
                {
                    return PalError.ENOTDIR;
                }

                if (!oldIsDirectory && newIsDirectory)
                {
                    return PalError.EISDIR;
                }

                if (newIsDirectory
                    && VfsManager.TryOpenDirectory(newFull, out IVfsDirectoryHandle? target) && target != null
                    && target.TryReadDir(out IReadOnlyList<IVfsInode> entries) && entries.Count > 0)
                {
                    return PalError.ENOTEMPTY;
                }
            }
        }

        return VfsManager.TryRename(oldFull, newFull) ? PalError.SUCCESS : PalError.EIO;
    }

    internal static PalError SetPathMode(string path, int mode)
    {
        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        if (fullPath == "/")
        {
            return PalError.EPERM;
        }

        // Any node, not just a directory: chmod applies to files too.
        if (!VfsManager.TryOpenNode(fullPath, out IVfsNodeHandle? node))
        {
            return PalError.ENOENT;
        }

        using (node)
        {
            return ApplyMode(node.Inode, mode);
        }
    }

    internal static PalError SetCurrentDirectory(string path)
    {
        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        if (!VfsManager.TryStat(fullPath, out VfsStat stat))
        {
            return PalError.ENOENT;
        }

        if (!stat.IsDirectory)
        {
            return PalError.ENOTDIR;
        }

        return VfsManager.TrySetCurrentDirectory(fullPath) ? PalError.SUCCESS : PalError.EIO;
    }

    // ---------------- directory streams ----------------

    internal static PalError OpenDirectoryStream(string path, out IntPtr handle)
    {
        handle = IntPtr.Zero;

        string? fullPath = VfsManager.MakeAbsolute(path);
        if (fullPath == null)
        {
            return PalError.ENOENT;
        }

        string[] names;
        bool[] isDirectory;
        if (fullPath == "/" && !VfsManager.TryOpenDirectory("/", out _))
        {
            // Nothing is mounted at "/": list the virtual root (the first
            // segments of the mount points; empty with no mounts at all).
            names = VfsManager.GetVirtualRootEntries();
            isDirectory = new bool[names.Length];
            for (int i = 0; i < isDirectory.Length; i++)
            {
                isDirectory[i] = true;
            }
        }
        else
        {
            if (!VfsManager.TryStat(fullPath, out VfsStat stat))
            {
                return PalError.ENOENT;
            }

            if (!stat.IsDirectory)
            {
                return PalError.ENOTDIR;
            }

            if (!VfsManager.TryOpenDirectory(fullPath, out IVfsDirectoryHandle? directory) || directory == null
                || !directory.TryReadDir(out IReadOnlyList<IVfsInode> entries))
            {
                return PalError.EIO;
            }

            names = new string[entries.Count];
            isDirectory = new bool[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                IVfsInode entry = entries[i];
                names[i] = entry.Name;
                bool directoryEntry = entry.FileOperations == null;
                if (entry.InodeOperations != null && entry.InodeOperations.GetAttr(entry, out VfsStat entryStat))
                {
                    directoryEntry = entryStat.IsDirectory;
                }

                isDirectory[i] = directoryEntry;
            }
        }

        int slot = FindFreeSlot(s_directoryStreams);
        if (slot < 0)
        {
            return PalError.EMFILE;
        }

        byte* nameBuffer = (byte*)NativeMemory.Alloc(DirectoryNameBufferBytes);
        s_directoryStreams[slot] = new DirectoryStreamState(names, isDirectory, nameBuffer);
        handle = new IntPtr(slot + 1);
        return PalError.SUCCESS;
    }

    /// <summary>Implements the raw <c>SystemNative_ReadDir</c> protocol:
    /// 0 = entry produced, -1 = end of stream, positive = error code.</summary>
    internal static int ReadDirectoryStream(IntPtr handle, PalSys.DirectoryEntry* entry)
    {
        DirectoryStreamState? stream = GetDirectoryStream(handle);
        if (stream == null)
        {
            return (int)PalError.EBADF;
        }

        if (stream.Index >= stream.Names.Length)
        {
            *entry = default;
            return -1;
        }

        int index = stream.Index;
        stream.Index = index + 1;

        int length = EncodeUtf8(stream.Names[index], stream.NameBuffer, DirectoryNameBufferBytes - 1);
        stream.NameBuffer[length] = 0;

        entry->Name = stream.NameBuffer;
        entry->NameLength = length;
        entry->InodeType = stream.IsDirectory[index] ? PalSys.NodeType.DT_DIR : PalSys.NodeType.DT_REG;
        return 0;
    }

    internal static PalError CloseDirectoryStream(IntPtr handle)
    {
        DirectoryStreamState? stream = GetDirectoryStream(handle);
        if (stream == null)
        {
            return PalError.EBADF;
        }

        NativeMemory.Free(stream.NameBuffer);
        s_directoryStreams[(int)handle - 1] = null;
        return PalError.SUCCESS;
    }

    // ---------------- internals ----------------

    private static OpenFileState? GetOpenFile(int fd)
    {
        int slot = fd - FirstFileDescriptor;
        if (slot < 0 || slot >= MaxOpenFiles)
        {
            return null;
        }

        return s_openFiles[slot];
    }

    private static DirectoryStreamState? GetDirectoryStream(IntPtr handle)
    {
        int slot = (int)handle - 1;
        if (slot < 0 || slot >= MaxDirectoryStreams)
        {
            return null;
        }

        return s_directoryStreams[slot];
    }

    private static int FindFreeSlot<T>(T?[] table) where T : class
    {
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    private static PalError CreateRegularFile(string fullPath, int mode)
    {
        VfsManager.SplitParentLeaf(fullPath, out _, out string leaf);
        if (leaf.Length == 0)
        {
            return PalError.EINVAL;
        }

        PalError parentError = RequireMountedParent(fullPath);
        if (parentError != PalError.SUCCESS)
        {
            return parentError;
        }

        return VfsManager.TryCreateFile(fullPath, PermissionBits(mode))
            ? PalError.SUCCESS
            : PalError.EIO;
    }

    /// <summary>Errno discrimination for creating an entry at <paramref name="fullPath"/>:
    /// missing parent → ENOENT, parent is a file → ENOTDIR, parent stats as a
    /// directory but is not backed by a mount (the virtual root) → EROFS.</summary>
    private static PalError RequireMountedParent(string fullPath)
    {
        VfsManager.SplitParentLeaf(fullPath, out string parentPath, out _);

        if (!VfsManager.TryStat(parentPath, out VfsStat parentStat))
        {
            return PalError.ENOENT;
        }

        if (!parentStat.IsDirectory)
        {
            return PalError.ENOTDIR;
        }

        if (!VfsManager.TryOpenDirectory(parentPath, out IVfsDirectoryHandle? parent) || parent == null)
        {
            return PalError.EROFS;
        }

        return PalError.SUCCESS;
    }

    private static bool SetSize(IVfsInode inode, ulong size)
    {
        VfsStat attributes = default;
        attributes.Size = size;
        return inode.InodeOperations != null
            && inode.InodeOperations.SetAttr(inode, SetAttrFlags.Size, in attributes);
    }

    private static PalError ApplyMode(IVfsInode inode, int mode)
    {
        if (inode.InodeOperations == null || !inode.InodeOperations.GetAttr(inode, out VfsStat current))
        {
            return PalError.EIO;
        }

        VfsStat attributes = default;
        attributes.Mode = PermissionBits(mode) | (current.Mode & VfsMode.FileTypeMask);
        return inode.InodeOperations.SetAttr(inode, SetAttrFlags.Mode, in attributes)
            ? PalError.SUCCESS
            : PalError.EPERM;
    }

    private static VfsMode PermissionBits(int mode) => (VfsMode)mode & VfsMode.PermissionMask;

    private static void FillStatus(string fullPath, in VfsStat stat, out PalSys.FileStatus status)
    {
        status = default;
        status.Flags = PalSys.FileStatusFlags.None;
        status.Mode = (int)stat.Mode;
        status.Uid = stat.Uid;
        status.Gid = stat.Gid;
        status.Size = (long)stat.Size;
        status.ATime = stat.Atime.TvSec;
        status.ATimeNsec = stat.Atime.TvNsec;
        status.MTime = stat.Mtime.TvSec;
        status.MTimeNsec = stat.Mtime.TvNsec;
        status.CTime = stat.Ctime.TvSec;
        status.CTimeNsec = stat.Ctime.TvNsec;
        status.BirthTime = 0;
        status.BirthTimeNsec = 0;
        FindMount(fullPath, out int mountOrdinal);
        status.Dev = mountOrdinal;
        status.RDev = 0;
        // FAT reports Ino 0 for empty files and the fixed FAT12/16 root; the
        // BCL compares Dev+Ino to detect "same file" (File.Move), so synthesize
        // distinct, stable inode numbers from the path for those.
        status.Ino = stat.Ino != 0 ? (long)stat.Ino : SyntheticInode(fullPath);
        status.UserFlags = 0;
    }

    private static long SyntheticInode(string fullPath)
    {
        // FNV-1a; bit 62 keeps synthetic numbers clear of real cluster numbers.
        ulong hash = 14695981039346656037UL;
        for (int i = 0; i < fullPath.Length; i++)
        {
            hash ^= fullPath[i];
            hash *= 1099511628211UL;
        }

        return (long)((hash & 0x3FFFFFFFFFFFFFFFUL) | 0x4000000000000000UL);
    }

    /// <summary>Longest-prefix mount lookup that also reports the mount's 1-based
    /// ordinal — the stable device number <c>FileStatus.Dev</c> carries.</summary>
    private static VfsManager.VfsMount? FindMount(string fullPath, out int ordinal)
    {
        VfsManager.VfsMount? best = null;
        ordinal = 0;

        IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
        for (int i = 0; i < mounts.Count; i++)
        {
            VfsManager.VfsMount candidate = mounts[i];
            if (VfsManager.MountCovers(candidate.MountPoint, fullPath)
                && (best == null || candidate.MountPoint.Length > best.MountPoint.Length))
            {
                best = candidate;
                ordinal = i + 1;
            }
        }

        return best;
    }

    private static int EncodeUtf8(string value, byte* destination, int capacity)
    {
        int offset = 0;
        for (int i = 0; i < value.Length; i++)
        {
            int codePoint = value[i];
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                codePoint = char.ConvertToUtf32(value[i], value[i + 1]);
                i++;
            }

            if (codePoint < 0x80)
            {
                if (offset + 1 > capacity)
                {
                    break;
                }

                destination[offset++] = (byte)codePoint;
            }
            else if (codePoint < 0x800)
            {
                if (offset + 2 > capacity)
                {
                    break;
                }

                destination[offset++] = (byte)(0xC0 | (codePoint >> 6));
                destination[offset++] = (byte)(0x80 | (codePoint & 0x3F));
            }
            else if (codePoint < 0x10000)
            {
                if (offset + 3 > capacity)
                {
                    break;
                }

                destination[offset++] = (byte)(0xE0 | (codePoint >> 12));
                destination[offset++] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
                destination[offset++] = (byte)(0x80 | (codePoint & 0x3F));
            }
            else
            {
                if (offset + 4 > capacity)
                {
                    break;
                }

                destination[offset++] = (byte)(0xF0 | (codePoint >> 18));
                destination[offset++] = (byte)(0x80 | ((codePoint >> 12) & 0x3F));
                destination[offset++] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
                destination[offset++] = (byte)(0x80 | (codePoint & 0x3F));
            }
        }

        return offset;
    }
}
