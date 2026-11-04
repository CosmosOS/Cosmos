// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Firmware;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.RISCV64;

/// <summary>
/// RISC-V Real-Time Clock implementation.
/// Reads the boot time from the EFI runtime services, Limine or the Goldfish
/// RTC of the QEMU virt machine, and tracks elapsed time with the time CSR.
///
/// The Goldfish RTC counts nanoseconds since the Unix epoch in a 64-bit
/// register read as two halves: reading TIME_LOW latches TIME_HIGH, so the
/// low half is read first. QEMU virt maps it at physical address 0x101000.
/// </summary>
internal class GoldfishRtc
{
    /// <summary>Singleton instance of the RTC.</summary>
    public static GoldfishRtc? Instance { get; private set; }

    // Goldfish RTC base address on QEMU virt machine (physical)
    private const ulong GoldfishBasePhys = 0x00101000;

    // Goldfish RTC register offsets
    private const ulong TimeLow = 0x00;  // Low 32 bits of the time; latches TimeHigh
    private const ulong TimeHigh = 0x04; // High 32 bits, as latched

    private const ulong NanosecondsPerTick = 100;

    // Sanity bounds: 2000-01-01 and 2100-01-01, in Unix seconds
    private const ulong SaneMinimumSeconds = 946_684_800UL;
    private const ulong SaneMaximumSeconds = 4_102_444_800UL;

    // Fallback boot time: 2024-01-01 00:00:00 UTC (if no RTC)
    private const long FallbackBootTicks = 638_388_288_000_000_000L;

    /// <summary>Whether a time source was found and read.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Boot time in DateTime ticks (UTC).</summary>
    public long BootTimeTicks { get; private set; }

    /// <summary>Time CSR value captured at boot.</summary>
    private ulong _bootCounter;

    /// <summary>Time CSR frequency (Hz) captured at boot.</summary>
    private ulong _timerFrequency;

    /// <summary>
    /// Initializes the RTC: captures the time CSR for elapsed-time tracking,
    /// then takes the boot wall-clock time from the first source that
    /// answers, or a fixed fallback epoch.
    /// </summary>
    public void Initialize()
    {
        Serial.Write("[RTC] Initializing...\n");

        Instance = this;

        if (SbiTimer.Instance is not null)
        {
            _bootCounter = SbiTimer.Instance.GetCurrentCounter();
            _timerFrequency = SbiTimer.Instance.TimerFrequency;
        }

        Serial.Write("[RTC] Timer frequency: ");
        Serial.WriteNumber(_timerFrequency);
        Serial.Write(" Hz\n");

        // Priority 1: EFI Runtime Services GetTime (works on real UEFI hardware)
        if (EfiRtc.TryGetTime(out long efiTicks))
        {
            BootTimeTicks = efiTicks;
            IsAvailable = true;
            Serial.Write("[RTC] Initialized\n");
            return;
        }

        // Priority 2: Limine boot time
        if (BootFirmware.TryGetBootTime(out long unixSecs) && unixSecs > 0)
        {
            BootTimeTicks = DateTime.UnixEpoch.Ticks + unixSecs * TimeSpan.TicksPerSecond;
            IsAvailable = true;
            WriteBootTime("Limine", BootTimeTicks);
            Serial.Write("[RTC] Initialized\n");
            return;
        }

        // Priority 3: the Goldfish RTC (QEMU virt), in the low 4 GiB Limine maps
        if (TryReadGoldfish(GoldfishBasePhys + AddressSpace.HhdmOffset, out ulong nanoseconds))
        {
            BootTimeTicks = DateTime.UnixEpoch.Ticks + (long)(nanoseconds / NanosecondsPerTick);
            IsAvailable = true;
            WriteBootTime("Goldfish", BootTimeTicks);
            Serial.Write("[RTC] Initialized\n");
            return;
        }

        // Priority 4: Fallback epoch
        BootTimeTicks = FallbackBootTicks;
        IsAvailable = false;
        Serial.Write("[RTC] No time source found, using fallback epoch (2024-01-01)\n");
        Serial.Write("[RTC] Initialized\n");
    }

    /// <summary>
    /// Gets the current time as DateTime ticks (UTC): the boot time plus the
    /// time CSR ticks elapsed since, in 100-ns intervals.
    /// </summary>
    public long GetCurrentTicks() => BootTimeTicks + GetElapsedTicks();

    /// <summary>
    /// Gets the time elapsed since <see cref="Initialize"/> captured the time
    /// CSR, in DateTime ticks (100-ns intervals); 0 when the timer was not up.
    /// </summary>
    public long GetElapsedTicks()
    {
        if (_timerFrequency == 0 || SbiTimer.Instance is null)
        {
            return 0;
        }

        ulong counterElapsed = SbiTimer.Instance.GetCurrentCounter() - _bootCounter;
        return (long)MultiplyDivide(counterElapsed, TimeSpan.TicksPerSecond, _timerFrequency);
    }

    /// <summary>
    /// Reads the Goldfish RTC at <paramref name="baseVirt"/>: false when the
    /// value is outside 2000-2100, which is what an absent device reads.
    /// </summary>
    private static bool TryReadGoldfish(ulong baseVirt, out ulong nanoseconds)
    {
        uint low = Native.MMIO.Read32(baseVirt + TimeLow);
        uint high = Native.MMIO.Read32(baseVirt + TimeHigh);
        nanoseconds = ((ulong)high << 32) | low;

        ulong seconds = nanoseconds / 1_000_000_000UL;
        if (seconds < SaneMinimumSeconds || seconds > SaneMaximumSeconds)
        {
            Serial.Write("[RTC] Goldfish value out of sane range\n");
            return false;
        }

        return true;
    }

    /// <summary>Logs the boot time a source gave.</summary>
    private static void WriteBootTime(string source, long ticks)
    {
        DateTime time = new(ticks, DateTimeKind.Utc);
        Serial.Write("[RTC] Boot time (");
        Serial.Write(source);
        Serial.Write("): ");
        Serial.WriteNumber((ulong)time.Year);
        Serial.Write("-");
        WriteTwoDigits(time.Month);
        Serial.Write("-");
        WriteTwoDigits(time.Day);
        Serial.Write(" ");
        WriteTwoDigits(time.Hour);
        Serial.Write(":");
        WriteTwoDigits(time.Minute);
        Serial.Write(":");
        WriteTwoDigits(time.Second);
        Serial.Write(" UTC\n");
    }

    private static void WriteTwoDigits(int value)
    {
        if (value < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber((ulong)value);
    }

    /// <summary>Multiplies two 64-bit values and divides by a third without overflow.</summary>
    private static ulong MultiplyDivide(ulong a, ulong b, ulong c)
    {
        ulong result = a / c * b;
        ulong remainder = a % c;
        result += remainder * b / c;
        return result;
    }
}
