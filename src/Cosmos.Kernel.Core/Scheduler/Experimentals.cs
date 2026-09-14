namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.Core. An experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The scheduler policy seam: <see cref="IScheduler"/>,
    /// <see cref="SchedulerManager"/>, <see cref="SchedulerThread"/>,
    /// <see cref="PerCpuState"/>, <see cref="SchedulerExtensible"/>,
    /// <see cref="InterruptMaskScope"/>, and the
    /// <see cref="SchedulerThreadState"/>/<see cref="SchedulerThreadFlags"/> enums.
    /// </summary>
    internal const string SchedulerSeamDiagId = "COSMOS0001";
}
