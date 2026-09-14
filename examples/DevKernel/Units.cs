namespace DevKernel;

/// <summary>
/// Unit conversions shared by the shell commands and the graphics overlays.
/// </summary>
internal static class Units
{
    /// <summary>Bytes per kibibyte.</summary>
    public const ulong BytesPerKiB = 1024;

    /// <summary>Bytes per mebibyte.</summary>
    public const ulong BytesPerMiB = BytesPerKiB * BytesPerKiB;

    /// <summary>Nanoseconds per millisecond, for converting scheduler times to ms (short alias of the kernel-wide constant).</summary>
    public const ulong NsPerMs = Cosmos.Kernel.System.Diagnostics.SchedulerInfo.NanosecondsPerMillisecond;

    /// <summary>Scale factor for expressing a ratio as a percentage.</summary>
    public const ulong PercentScale = 100;

    /// <summary>Converts a byte count to whole kibibytes, truncating the remainder.</summary>
    public static ulong ToKiB(ulong bytes) => bytes / BytesPerKiB;

    /// <summary>Converts a byte count to whole mebibytes, truncating the remainder.</summary>
    public static ulong ToMiB(ulong bytes) => bytes / BytesPerMiB;
}
