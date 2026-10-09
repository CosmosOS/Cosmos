namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// PageType enum. Used to define the type of the page.
/// Data Types from 1, special meanings from 255 down.
/// </summary>
internal enum PageType : byte
{
    /// <summary>
    /// Empty page.
    /// Can also indicate invalid page.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// GC page.
    /// A page managed by the GC, here is where managed objects are allocated.
    /// </summary>
    GCHeap = 1,

    /// <summary>
    /// Small heap page.
    /// </summary>
    HeapSmall = 3,

    /// <summary>
    /// Medium heap page.
    /// </summary>
    HeapMedium = 5,

    /// <summary>
    /// Large heap page.
    /// </summary>
    HeapLarge = 7,

    /// <summary>
    /// User Managed page
    /// </summary>
    Unmanaged = 9,

    /// <summary>
    /// Page Directory page
    /// </summary>
    PageDirectory = 11,

    /// <summary>
    /// PageAllocator type page.
    /// </summary>
    PageAllocator = 32,

    /// <summary>
    /// Page which is part of the SMT
    /// </summary>
    SMT = 64,

    // Extension of previous page.
    /// <summary>
    /// Extension of pre-existing page.
    /// </summary>
    Extension = 128
}
