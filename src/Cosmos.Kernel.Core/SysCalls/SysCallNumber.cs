// This code is licensed under MIT license (see LICENSE for details)

namespace Cosmos.Kernel.Core.SysCalls;

/// <summary>
/// Well-known syscall numbers for the kernel-side dispatch table. The
/// numbering is stable across releases: userland ABI compatibility relies on
/// these never being renumbered (only appended). Unallocated slots in the
/// dispatcher return <see cref="SysCallError.Enosys"/>.
/// </summary>
internal enum SysCallNumber : uint
{
    /// <summary>Terminate the calling process. Args: exit code.</summary>
    Exit = 0,

    /// <summary>Write to an open file/console handle. Args: fd, buf, count.</summary>
    Write = 1,

    /// <summary>Read from an open file/console handle. Args: fd, buf, count.</summary>
    Read = 2,

    /// <summary>Open a path. Args: path ptr, flags, mode.</summary>
    Open = 3,

    /// <summary>Close an open handle. Args: fd.</summary>
    Close = 4,

    /// <summary>Yield the calling thread to the scheduler. No args.</summary>
    Yield = 5,

    /// <summary>Return the calling process's PID. No args.</summary>
    GetPid = 6,

    /// <summary>Set the program break. Args: new brk (0 returns current).</summary>
    Brk = 7,

    /// <summary>Map anonymous/file-backed memory. Args: addr, length, prot, flags, fd, offset.</summary>
    Mmap = 8,

    /// <summary>Unmap a previously mapped region. Args: addr, length.</summary>
    Munmap = 9,
}
