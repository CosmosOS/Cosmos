// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem;

/// <summary>
/// Linux <c>timespec64</c>-style instant for inode timestamps.
/// </summary>
public struct VfsTimespec
{
    /// <summary>Whole seconds since the Unix epoch (<c>tv_sec</c>).</summary>
    public long Seconds;
    /// <summary>Nanoseconds within the second, in [0, 999999999] (<c>tv_nsec</c>).</summary>
    public long Nanoseconds;

    /// <summary>
    /// Creates a timespec from seconds and nanoseconds.
    /// </summary>
    /// <param name="seconds">Whole seconds since the Unix epoch.</param>
    /// <param name="nanoseconds">Nanoseconds within the second.</param>
    public VfsTimespec(long seconds, long nanoseconds)
    {
        Seconds = seconds;
        Nanoseconds = nanoseconds;
    }
}
