namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>
/// Shared tuning knobs of the QEMU test hosts (<see cref="QemuX64Host"/> and
/// <see cref="QemuARM64Host"/>): run timeout, guest sizing, the UART
/// monitor loop's delays and the audio backend. Implementation policy,
/// deliberately kept off the <see cref="IQemuHost"/> contract.
/// </summary>
internal static class QemuHostDefaults
{
    /// <summary>Default maximum time in seconds to let a kernel run in QEMU before timing out.</summary>
    internal const int DefaultTimeoutSeconds = 30;

    /// <summary>Default QEMU guest memory size in megabytes.</summary>
    internal const int DefaultMemoryMb = 512;

    /// <summary>Grace delay before killing QEMU once the UART monitor finished, in milliseconds — lets trailing UART bytes land.</summary>
    internal const int KillGraceDelayMs = 200;

    /// <summary>Delay after QEMU exits to let the UART log flush to disk, in milliseconds.</summary>
    internal const int UartFlushDelayMs = 100;

    /// <summary>Polling interval of the UART log monitor loop, in milliseconds.</summary>
    internal const int UartPollIntervalMs = 100;

    /// <summary>
    /// Seconds of UART quiet (no protocol-frame magic) after a TestPass before
    /// the UART monitor declares the kernel stalled — handles destructive ops
    /// (e.g. Power.Shutdown) that hang instead of cleanly exiting QEMU.
    /// </summary>
    internal const int StallSecondsAfterTestPass = 10;

    /// <summary>
    /// The <c>-audiodev</c> driver a profile's audio controller plays into.
    /// A runner has no audio server, and QEMU's default backend then opens no
    /// voice for the codec; <c>none</c> takes the frames at the stream's rate
    /// and discards them, so the guest's stream engine still advances.
    /// </summary>
    internal const string AudioBackend = "none";
}
