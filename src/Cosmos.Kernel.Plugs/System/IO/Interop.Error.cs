// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

// Mirror of CoreLib's Interop.Error — see Interop.Sys.cs for how these
// global-namespace mirror declarations work and why they must match CoreLib
// exactly. Source of truth: dotnet/runtime
// src/libraries/Common/src/Interop/Unix/Interop.Errors.cs.
internal static partial class Interop
{
    /// <summary>Platform-agnostic PAL error codes (CoreLib's <c>Interop.Error</c>) —
    /// the PAL's own portable constants, not any OS's native errno numbers.</summary>
    internal enum Error
    {
        SUCCESS = 0,

        E2BIG = 0x10001,
        EACCES = 0x10002,
        EAFNOSUPPORT = 0x10005,
        EAGAIN = 0x10006,
        EBADF = 0x10008,
        EBUSY = 0x1000A,
        ECANCELED = 0x1000B,
        EEXIST = 0x10014,
        EFBIG = 0x10016,
        EINTR = 0x1001B,
        EINVAL = 0x1001C,
        EIO = 0x1001D,
        EISDIR = 0x1001F,
        ELOOP = 0x10020,
        EMFILE = 0x10021,
        EMLINK = 0x10022,
        ENAMETOOLONG = 0x10025,
        ENETUNREACH = 0x10028,
        ENFILE = 0x10029,
        ENODEV = 0x1002C,
        ENOENT = 0x1002D,
        ENOMEM = 0x10031,
        ENOSPC = 0x10034,
        ENOSYS = 0x10037,
        ENOTDIR = 0x10039,
        ENOTEMPTY = 0x1003A,
        ENOTSUP = 0x1003D,
        ENXIO = 0x1003F,
        EOVERFLOW = 0x10040,
        EPERM = 0x10042,
        ERANGE = 0x10047,
        EROFS = 0x10048,
        ESPIPE = 0x10049,
        EXDEV = 0x1004F,
        EHOSTNOTFOUND = 0x20001,
    }
}
