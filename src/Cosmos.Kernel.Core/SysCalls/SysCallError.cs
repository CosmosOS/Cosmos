// This code is licensed under MIT license (see LICENSE for details)

namespace Cosmos.Kernel.Core.SysCalls;

/// <summary>
/// Negative errno-style result codes returned by syscall handlers. On
/// failure a handler returns <see cref="SysCallResult"/> with a non-<see cref="None"/>
/// error code and an unspecified <see cref="SysCallResult.Value"/>. The
/// values mirror the POSIX errno constants where there is a direct
/// equivalent so userland ABI code can reuse the same defines.
/// </summary>
internal enum SysCallError : uint
{
    /// <summary>No error — the call succeeded, see <see cref="SysCallResult.Value"/>.</summary>
    None = 0,

    /// <summary>Function not implemented (handler not registered).</summary>
    Enosys = 1,

    /// <summary>Invalid argument.</summary>
    Einval = 2,

    /// <summary>Bad file descriptor / handle.</summary>
    Ebadf = 3,

    /// <summary>Bad address — user pointer failed validation.</summary>
    Efault = 4,

    /// <summary>Operation not permitted.</summary>
    Eperm = 5,

    /// <summary>Out of memory.</summary>
    Enomem = 6,

    /// <summary>Resource temporarily unavailable; retry.</summary>
    Eagain = 7,

    /// <summary>No such file or directory.</summary>
    Enoent = 8,

    /// <summary>I/O error.</summary>
    Eio = 9,
}
