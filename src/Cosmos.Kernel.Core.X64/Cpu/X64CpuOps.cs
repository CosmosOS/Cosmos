using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64.Cpu;

internal class X64CpuOps : ICpuOps
{
    public void Halt() => InternalCpu.Halt();

    public void DisableInterrupts() => Cosmos.Kernel.Core.CPU.InternalCpu.DisableInterrupts();

    public void EnableInterrupts() => Cosmos.Kernel.Core.CPU.InternalCpu.EnableInterrupts();

    /// <summary>
    /// TSC (Time Stamp Counter) frequency in Hz.
    /// Default is 1 GHz as a reasonable estimate for modern CPUs.
    /// Calibrated during kernel initialization using PIT as reference.
    /// </summary>
    public static long TscFrequency { get; private set; } = 1_000_000_000;

    /// <summary>
    /// Gets whether the TSC frequency has been calibrated.
    /// </summary>
    public static bool IsTscCalibrated { get; private set; }

    /// <summary>
    /// Reads the Time Stamp Counter (TSC).
    /// Returns a 64-bit value representing CPU cycles since reset.
    /// </summary>
    public static ulong ReadTSC() => X64CpuNative.ReadTsc();

    // Native import lives in Cosmos.Kernel.Core.X64/Bridge/Import/X64CpuNative.cs.

    /// <summary>
    /// Calibrates the TSC frequency using LAPIC timer as a reference.
    /// Must be called after LAPIC timer is calibrated.
    /// Must be called before any code accesses Stopwatch.Frequency.
    /// </summary>
    public static void CalibrateTsc()
    {
        if (!Cpu.LocalApic.IsTimerCalibrated)
        {
            Serial.Write("[TSC] ERROR: LAPIC timer not calibrated\n");
            return;
        }

        const uint calibrationMs = 10;

        // Read TSC before
        ulong tscStart = X64CpuNative.ReadTsc();

        // Wait using calibrated LAPIC timer
        Cpu.LocalApic.Wait(calibrationMs);

        // Read TSC after
        ulong tscEnd = X64CpuNative.ReadTsc();

        ulong tscElapsed = tscEnd - tscStart;
        ulong ticksPerMs = tscElapsed / calibrationMs;

        TscFrequency = (long)(ticksPerMs * 1000);
        IsTscCalibrated = true;
    }
}
