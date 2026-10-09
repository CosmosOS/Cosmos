namespace Cosmos.Kernel.Core.Memory.VAS;

/// <summary>
/// Architecture-independent page permissions and attributes.
/// Each architecture mapper translates these to its own descriptor bits.
/// </summary>
[Flags]
internal enum PageFlags : byte
{
    /// <summary>No flags set.</summary>
    None = 0,

    /// <summary>Page is readable.</summary>
    Read = 1 << 0,

    /// <summary>Page is writable.</summary>
    Write = 1 << 1,

    /// <summary>Page is executable.</summary>
    Execute = 1 << 2,

    /// <summary>Page is accessible from user mode (ring 3 / EL0).</summary>
    User = 1 << 3,

    /// <summary>Page is global (not flushed on TLB context switch).</summary>
    Global = 1 << 4,

    /// <summary>Disable caching for this page.</summary>
    CacheDisable = 1 << 5,

    /// <summary>Use write-through caching policy.</summary>
    WriteThrough = 1 << 6,
}
