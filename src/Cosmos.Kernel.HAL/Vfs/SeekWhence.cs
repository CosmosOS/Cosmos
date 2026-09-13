// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Origin for seek.
/// </summary>
public enum SeekWhence
{
    /// <summary>Offset is relative to the start of the file (<c>SEEK_SET</c>).</summary>
    Set = 0,
    /// <summary>Offset is relative to the current position (<c>SEEK_CUR</c>).</summary>
    Cur = 1,
    /// <summary>Offset is relative to the end of the file (<c>SEEK_END</c>).</summary>
    End = 2,
}
