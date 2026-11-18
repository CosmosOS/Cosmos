// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;

namespace DevKernel.Diagnostics.Smp;

// Times of the SMP monitor on the monotonic clock. The waits of the scenario and of its
// workers end on a deadline taken here, never on a count of turns.
internal static class SmpClock
{
    public const int MsPerSecond = 1000;

    // The timestamp that many milliseconds from now.
    public static long DeadlineIn(int milliseconds)
    {
        return Stopwatch.GetTimestamp() + Stopwatch.Frequency * milliseconds / MsPerSecond;
    }

    public static bool HasPassed(long deadline)
    {
        return Stopwatch.GetTimestamp() >= deadline;
    }

    // Milliseconds gone by since a timestamp.
    public static long MsSince(long timestamp)
    {
        return (Stopwatch.GetTimestamp() - timestamp) * MsPerSecond / Stopwatch.Frequency;
    }
}
