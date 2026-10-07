// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem;

/// <summary>
/// Mount option bits.
/// </summary>
[Flags]
public enum MountFlags : uint
{
    /// <summary>No mount options.</summary>
    None = 0,
    /// <summary>Read-only mount.</summary>
    ReadOnly = 1 << 0,
    /// <summary>Ignore suid/sgid.</summary>
    NoSuid = 1 << 1,
    /// <summary>Disallow device nodes.</summary>
    NoDev = 1 << 2,
    /// <summary>No execution.</summary>
    NoExec = 1 << 3,
}
