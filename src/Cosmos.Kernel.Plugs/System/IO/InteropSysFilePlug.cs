// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;
using Cosmos.Build.API.Attributes;
using Microsoft.Win32.SafeHandles;
using PalError = global::Interop.Error;
using PalSys = global::Interop.Sys;

namespace Cosmos.Kernel.Plugs.System.IO;

/// <summary>
/// Plugs the file-I/O surface of CoreLib's <c>Interop.Sys</c> onto the Cosmos
/// VFS (via <see cref="FileDescriptorTable"/>), so the BCL's own System.IO code —
/// File, Directory, FileStream, FileSystemInfo, directory enumeration — runs
/// unmodified on top of <c>VfsManager</c>.
/// </summary>
/// <remarks>
/// These replace the LibraryImport-generated wrappers (and the few raw
/// externs), so none of the generated marshalling runs. The BCL reads
/// failures from <c>Marshal.GetLastPInvokeError()</c>, so every failing plug
/// stores a PAL <c>Interop.Error</c> value there — with
/// <c>ConvertErrorPlatformToPal</c> plugged as the identity, that value flows
/// unchanged into the BCL's exception mapping. Complements the existing
/// <see cref="InteropSysPlug"/> (random/monitors/errno); members are disjoint.
/// </remarks>
[Plug("Interop/Sys")]
internal static unsafe class InteropSysFilePlug
{
    /// <summary>statfs magic reported for every descriptor — any value outside the
    /// BCL's network-filesystem set (nfs/smb/cifs) keeps file locking enabled.</summary>
    private const uint MsdosSuperMagic = 0x4d44;

    private static int Fail(PalError error)
    {
        Marshal.SetLastPInvokeError((int)error);
        return -1;
    }

    private static int DescriptorOf(SafeHandle fd) => (int)fd.DangerousGetHandle();

    // ---------------- open/close ----------------

    [PlugMember]
    public static SafeFileHandle Open(string filename, PalSys.OpenFlags flags, int mode)
    {
        PalError error = FileDescriptorTable.Open(filename, flags, mode, out int fd);
        if (error != PalError.SUCCESS)
        {
            Marshal.SetLastPInvokeError((int)error);
            return new SafeFileHandle(new IntPtr(-1), ownsHandle: false);
        }

        return new SafeFileHandle(new IntPtr(fd), ownsHandle: true);
    }

    [PlugMember]
    public static int Close(IntPtr fd)
    {
        PalError error = FileDescriptorTable.Close((int)fd);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // ---------------- byte I/O ----------------

    [PlugMember]
    public static int Read(SafeHandle fd, byte* buffer, int count)
    {
        PalError error = FileDescriptorTable.Read(DescriptorOf(fd), buffer, count, out int bytesRead);
        return error == PalError.SUCCESS ? bytesRead : Fail(error);
    }

    [PlugMember]
    public static int Write(SafeHandle fd, byte* buffer, int bufferSize)
    {
        PalError error = FileDescriptorTable.Write(DescriptorOf(fd), buffer, bufferSize, out int bytesWritten);
        return error == PalError.SUCCESS ? bytesWritten : Fail(error);
    }

    [PlugMember]
    public static int Write(IntPtr fd, byte* buffer, int bufferSize)
    {
        int descriptor = (int)fd;
        if (descriptor == 1 || descriptor == 2)
        {
            // stdout/stderr writes (Debug output) go to the serial console.
            FileDescriptorTable.WriteConsole(buffer, bufferSize);
            return bufferSize;
        }

        PalError error = FileDescriptorTable.Write(descriptor, buffer, bufferSize, out int bytesWritten);
        return error == PalError.SUCCESS ? bytesWritten : Fail(error);
    }

    [PlugMember]
    public static int PRead(SafeHandle fd, byte* buffer, int bufferSize, long fileOffset)
    {
        PalError error = FileDescriptorTable.PRead(DescriptorOf(fd), buffer, bufferSize, fileOffset, out int bytesRead);
        return error == PalError.SUCCESS ? bytesRead : Fail(error);
    }

    [PlugMember]
    public static int PWrite(SafeHandle fd, byte* buffer, int bufferSize, long fileOffset)
    {
        PalError error = FileDescriptorTable.PWrite(DescriptorOf(fd), buffer, bufferSize, fileOffset, out int bytesWritten);
        return error == PalError.SUCCESS ? bytesWritten : Fail(error);
    }

    [PlugMember]
    public static long LSeek(SafeFileHandle fd, long offset, PalSys.SeekWhence whence)
    {
        PalError error = FileDescriptorTable.Seek(DescriptorOf(fd), offset, (int)whence, out long newPosition);
        return error == PalError.SUCCESS ? newPosition : Fail(error);
    }

    // ---------------- descriptor metadata ----------------

    [PlugMember]
    public static int FStat(SafeHandle fd, out PalSys.FileStatus output)
    {
        PalError error = FileDescriptorTable.StatDescriptor(DescriptorOf(fd), out output);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int FSync(SafeFileHandle fd)
    {
        PalError error = FileDescriptorTable.Fsync(DescriptorOf(fd));
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int FTruncate(SafeFileHandle fd, long length)
    {
        PalError error = FileDescriptorTable.Truncate(DescriptorOf(fd), length);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int FChMod(SafeFileHandle fd, int mode)
    {
        PalError error = FileDescriptorTable.SetDescriptorMode(DescriptorOf(fd), mode);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int CopyFile(SafeFileHandle source, SafeFileHandle destination, long sourceLength)
    {
        PalError error = FileDescriptorTable.CopyDescriptor(DescriptorOf(source), DescriptorOf(destination));
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // Advisory locking and access hints have no VFS backing; succeeding keeps
    // SafeFileHandle.Init and FileStream.Lock/Unlock on their happy paths.

    [PlugMember]
    public static int FLock(SafeFileHandle fd, PalSys.LockOperations operation) => 0;

    [PlugMember]
    public static int FLock(IntPtr fd, PalSys.LockOperations operation) => 0;

    [PlugMember]
    public static int LockFileRegion(SafeHandle fd, long offset, long length, PalSys.LockType lockType) => 0;

    [PlugMember]
    public static int FAllocate(SafeFileHandle fd, long offset, long length) => 0;

    [PlugMember]
    public static int PosixFAdvise(SafeFileHandle fd, long offset, long length, PalSys.FileAdvice advice) => 0;

    [PlugMember]
    public static uint GetFileSystemType(SafeFileHandle fd) => MsdosSuperMagic;

    // ---------------- path metadata ----------------

    [PlugMember]
    public static int Stat(string path, out PalSys.FileStatus output)
    {
        PalError error = FileDescriptorTable.StatPath(path, out output);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int Stat(ReadOnlySpan<char> path, out PalSys.FileStatus output)
    {
        PalError error = FileDescriptorTable.StatPath(path.ToString(), out output);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // The VFS has no symlinks, so LStat and Stat are the same operation.

    [PlugMember]
    public static int LStat(string path, out PalSys.FileStatus output)
    {
        PalError error = FileDescriptorTable.StatPath(path, out output);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int LStat(ReadOnlySpan<char> path, out PalSys.FileStatus output)
    {
        PalError error = FileDescriptorTable.StatPath(path.ToString(), out output);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int ChMod(string path, int mode)
    {
        PalError error = FileDescriptorTable.SetPathMode(path, mode);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int UTimensat(string path, PalSys.TimeSpec* times) => 0;

    [PlugMember]
    public static int FUTimens(SafeHandle fd, PalSys.TimeSpec* times) => 0;

    // ---------------- namespace operations ----------------

    [PlugMember]
    public static int MkDir(ReadOnlySpan<char> path, int mode)
    {
        PalError error = FileDescriptorTable.CreateDirectory(path.ToString(), mode);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int RmDir(string path)
    {
        PalError error = FileDescriptorTable.RemoveDirectory(path);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int Unlink(string pathname)
    {
        PalError error = FileDescriptorTable.UnlinkFile(pathname);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int Rename(string oldPath, string newPath)
    {
        PalError error = FileDescriptorTable.Rename(oldPath, newPath);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    [PlugMember]
    public static int Rename(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath)
    {
        PalError error = FileDescriptorTable.Rename(oldPath.ToString(), newPath.ToString());
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // Hard links and symlinks do not exist on the VFS: File.Move's
    // link-then-unlink fast path falls back to copy on ENOTSUP.

    [PlugMember]
    public static int Link(string source, string link) => Fail(PalError.ENOTSUP);

    [PlugMember]
    public static int SymLink(string target, string linkPath) => Fail(PalError.EPERM);

    [PlugMember]
    public static string? ReadLink(ReadOnlySpan<char> path)
    {
        Marshal.SetLastPInvokeError((int)PalError.EINVAL);
        return null;
    }

    // ---------------- directory enumeration ----------------

    [PlugMember]
    public static IntPtr OpenDir(string path)
    {
        PalError error = FileDescriptorTable.OpenDirectoryStream(path, out IntPtr handle);
        if (error != PalError.SUCCESS)
        {
            Marshal.SetLastPInvokeError((int)error);
            return IntPtr.Zero;
        }

        return handle;
    }

    [PlugMember]
    public static int ReadDir(IntPtr dir, PalSys.DirectoryEntry* outputEntry)
        => FileDescriptorTable.ReadDirectoryStream(dir, outputEntry);

    [PlugMember]
    public static int CloseDir(IntPtr dir)
    {
        PalError error = FileDescriptorTable.CloseDirectoryStream(dir);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // ---------------- current directory ----------------

    [PlugMember]
    public static string GetCwd() => FileDescriptorTable.CurrentDirectory;

    [PlugMember]
    public static int ChDir(string path)
    {
        PalError error = FileDescriptorTable.SetCurrentDirectory(path);
        return error == PalError.SUCCESS ? 0 : Fail(error);
    }

    // ---------------- error machinery ----------------

    // The plugs above store PAL Interop.Error values directly as the "raw
    // errno", so both conversions are the identity.

    [PlugMember]
    public static PalError ConvertErrorPlatformToPal(int platformErrno) => (PalError)platformErrno;

    [PlugMember]
    public static int ConvertErrorPalToPlatform(PalError error) => (int)error;

    [PlugMember]
    public static byte* StrErrorR(int platformErrno, byte* buffer, int bufferSize)
    {
        if (buffer == null || bufferSize <= 0)
        {
            return null;
        }

        string message = ErrorMessage((PalError)platformErrno);
        int length = message.Length < bufferSize - 1 ? message.Length : bufferSize - 1;
        for (int i = 0; i < length; i++)
        {
            buffer[i] = (byte)message[i];
        }

        buffer[length] = 0;
        return buffer;
    }

    // ---------------- identity/capability probes ----------------

    // Both run from Interop.Sys's static constructor; returning 0 disables
    // the BSD hidden-flag machinery (FAT has no such flag).

    [PlugMember]
    public static int LChflagsCanSetHiddenFlag() => 0;

    [PlugMember]
    public static int CanGetHiddenFlag() => 0;

    /// <summary>Effective uid 0 matches the Uid the VFS reports on every node, keeping
    /// FileAttributes.ReadOnly evaluation on the owner branch.</summary>
    [PlugMember]
    public static uint GetEUid() => 0;

    /// <summary>Effective gid 0 matches the Gid the VFS reports on every node. With
    /// <see cref="GetEUid"/> this keeps mode checks off the group database, which
    /// the kernel does not have.</summary>
    [PlugMember]
    public static bool IsMemberOfGroup(uint gid) => gid == 0;

    private static string ErrorMessage(PalError error)
    {
        switch (error)
        {
            case PalError.ENOENT:
                return "No such file or directory";
            case PalError.EEXIST:
                return "File exists";
            case PalError.EISDIR:
                return "Is a directory";
            case PalError.ENOTDIR:
                return "Not a directory";
            case PalError.ENOTEMPTY:
                return "Directory not empty";
            case PalError.EINVAL:
                return "Invalid argument";
            case PalError.EBADF:
                return "Bad file descriptor";
            case PalError.EMFILE:
                return "Too many open files";
            case PalError.ENOSPC:
                return "No space left on device";
            case PalError.EFBIG:
                return "File too large";
            case PalError.EXDEV:
                return "Cross-device link";
            case PalError.EBUSY:
                return "Device or resource busy";
            case PalError.EROFS:
                return "Read-only file system";
            case PalError.EPERM:
                return "Operation not permitted";
            case PalError.EACCES:
                return "Permission denied";
            case PalError.ENOTSUP:
                return "Operation not supported";
            case PalError.EAFNOSUPPORT:
                return "Address family not supported by protocol";
            case PalError.ENETUNREACH:
                return "Network is unreachable";
            case PalError.EHOSTNOTFOUND:
                return "Name or service not known";
            default:
                return "I/O error";
        }
    }
}
