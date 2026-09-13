// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Cpu;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.ARM64.Devices.Timer;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.Devices.Clock;

namespace Cosmos.Kernel.HAL.ARM64.Devices.Clock;

/// <summary>
/// ARM64 Real-Time Clock implementation.
/// Reads boot time from the PL031 RTC (ARM Primecell RTC, present on QEMU virt machine).
/// Uses the ARM64 Generic Timer counter for sub-second elapsed time tracking.
///
/// PL031 RTCDR register provides Unix time (seconds since 1970-01-01 UTC).
/// QEMU virt machine maps PL031 at physical address 0x09010000.
/// </summary>
internal class RTC : Device
{
    /// <summary>Singleton instance of the RTC.</summary>
    public static RTC? Instance { get; private set; }

    // PL031 RTC base address on QEMU virt machine (physical)
    private const ulong PL031_BASE_PHYS = 0x09010000;

    // PL031 register offsets
    private const ulong RTCDR = 0x000; // Data register: seconds since Unix epoch (read-only)
    private const ulong RTCLR = 0x008; // Load register
    private const ulong RTCCR = 0x00C; // Control register (bit 0 = enable)

    // PL031 PrimeCell ID bytes (at offsets 0xFF0-0xFFC) for detection
    private const ulong RTCPCELLID0 = 0xFF0;
    private const ulong RTCPCELLID1 = 0xFF4;
    private const ulong RTCPCELLID2 = 0xFF8;
    private const ulong RTCPCELLID3 = 0xFFC;

    // Fallback boot time: 2024-01-01 00:00:00 UTC (if no RTC)
    private const long FallbackBootTicks = 638_388_288_000_000_000L;

    /// <summary>Whether the PL031 RTC was successfully detected and read.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Boot time in DateTime ticks (UTC).</summary>
    public long BootTimeTicks { get; private set; }

    /// <summary>GenericTimer counter value captured at boot.</summary>
    private ulong _bootCounter;

    /// <summary>GenericTimer frequency (Hz) captured at boot.</summary>
    private ulong _timerFrequency;

    /// <summary>
    /// Initializes the ARM64 RTC. Reads boot wall-clock time from PL031 if available,
    /// otherwise uses a fixed fallback epoch. Captures the GenericTimer counter for
    /// elapsed-time tracking.
    /// </summary>
    public unsafe void Initialize()
    {
        Serial.Write("[RTC] Initializing...\n");

        Instance = this;

        // Capture GenericTimer reference point for elapsed time
        if (GenericTimer.Instance != null)
        {
            _bootCounter = GenericTimer.Instance.GetCurrentCounter();
            _timerFrequency = GenericTimer.Instance.TimerFrequency;
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
        if (Limine.BootTime.Response != null)
        {
            long unixSecs = Limine.BootTime.Response->BootTime;
            Serial.Write("[RTC] Limine BootTime response: ");
            Serial.WriteNumber((ulong)unixSecs);
            Serial.Write("\n");
            if (unixSecs > 0)
            {
                BootTimeTicks = DateTime.UnixEpoch.Ticks + unixSecs * TimeSpan.TicksPerSecond;
                IsAvailable = true;

                UnixSecondsToDate((uint)unixSecs, out int year, out int month, out int day,
                                  out int hour, out int minute, out int second);
                Serial.Write("[RTC] Boot time (Limine): ");
                Serial.WriteNumber((ulong)year);
                Serial.Write("-");
                if (month < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)month);
                Serial.Write("-");
                if (day < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)day);
                Serial.Write(" ");
                if (hour < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)hour);
                Serial.Write(":");
                if (minute < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)minute);
                Serial.Write(":");
                if (second < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)second);
                Serial.Write(" UTC\n");

                Serial.Write("[RTC] Initialized\n");
                return;
            }
            else
            {
                Serial.Write("[RTC] Limine BootTime response present but BootTime <= 0, skipping\n");
            }
        }
        else
        {
            Serial.Write("[RTC] Limine BootTime response is null (feature unsupported by bootloader)\n");
        }

        // Priority 3: PL031 MMIO RTC (QEMU virt only — skip on real hardware)
        if (GICv3.IsMmioAvailable)
        {
            ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
            ulong pl031Virt = PL031_BASE_PHYS + hhdmOffset;

            if (TryReadPL031(pl031Virt, out uint unixSeconds))
            {
                BootTimeTicks = DateTime.UnixEpoch.Ticks + (long)unixSeconds * TimeSpan.TicksPerSecond;
                IsAvailable = true;

                UnixSecondsToDate(unixSeconds, out int year, out int month, out int day,
                                  out int hour, out int minute, out int second);
                Serial.Write("[RTC] Boot time (PL031): ");
                Serial.WriteNumber((ulong)year);
                Serial.Write("-");
                if (month < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)month);
                Serial.Write("-");
                if (day < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)day);
                Serial.Write(" ");
                if (hour < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)hour);
                Serial.Write(":");
                if (minute < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)minute);
                Serial.Write(":");
                if (second < 10)
                {
                    Serial.Write("0");
                }

                Serial.WriteNumber((ulong)second);
                Serial.Write(" UTC\n");

                Serial.Write("[RTC] Initialized\n");
                return;
            }
        }

        // Priority 4: Fallback epoch
        BootTimeTicks = FallbackBootTicks;
        IsAvailable = false;
        Serial.Write("[RTC] No time source found, using fallback epoch (2024-01-01)\n");
        Serial.Write("[RTC] Initialized\n");
    }

    /// <summary>
    /// Gets the current time as DateTime ticks (UTC).
    /// Uses boot time + elapsed Generic Timer ticks converted to 100-ns intervals.
    /// </summary>
    public long GetCurrentTicks()
    {
        ulong elapsed = 0;

        if (_timerFrequency > 0 && GenericTimer.Instance != null)
        {
            ulong current = GenericTimer.Instance.GetCurrentCounter();
            ulong counterElapsed = current - _bootCounter;
            // Convert counter ticks → 100-ns DateTime ticks
            // = counterElapsed * 10_000_000 / frequency
            elapsed = MultiplyDivide(counterElapsed, TimeSpan.TicksPerSecond, _timerFrequency);
        }

        return BootTimeTicks + (long)elapsed;
    }

    public long GetElapsedTicks()
    {
        ulong elapsed = 0;

        if (_timerFrequency > 0 && GenericTimer.Instance != null)
        {
            ulong current = GenericTimer.Instance.GetCurrentCounter();
            ulong counterElapsed = current - _bootCounter;
            // Convert counter ticks → 100-ns DateTime ticks
            // = counterElapsed * 10_000_000 / frequency
            elapsed = MultiplyDivide(counterElapsed, TimeSpan.TicksPerSecond, _timerFrequency);
        }

        return (long)elapsed;
    }

    /// <summary>
    /// Attempts to read the current Unix timestamp from a PL031 RTC at the given virtual address.
    /// Returns false if no valid PL031 is detected at that address.
    /// </summary>
    private static unsafe bool TryReadPL031(ulong baseVirt, out uint unixSeconds)
    {
        unixSeconds = 0;

        // Verify PrimeCell IDs: expected 0x0D, 0xF0, 0x05, 0xB1
        uint id0 = Native.MMIO.Read32(baseVirt + RTCPCELLID0) & 0xFF;
        uint id1 = Native.MMIO.Read32(baseVirt + RTCPCELLID1) & 0xFF;
        uint id2 = Native.MMIO.Read32(baseVirt + RTCPCELLID2) & 0xFF;
        uint id3 = Native.MMIO.Read32(baseVirt + RTCPCELLID3) & 0xFF;

        Serial.Write("[RTC] PL031 cell IDs: 0x");
        Serial.WriteHex(id0);
        Serial.Write(" 0x");
        Serial.WriteHex(id1);
        Serial.Write(" 0x");
        Serial.WriteHex(id2);
        Serial.Write(" 0x");
        Serial.WriteHex(id3);
        Serial.Write("\n");

        if (id0 != 0x0D || id1 != 0xF0 || id2 != 0x05 || id3 != 0xB1)
        {
            Serial.Write("[RTC] PL031 PrimeCell IDs not found\n");
            return false;
        }

        // Ensure RTC is enabled (RTCCR bit 0)
        uint ctrl = Native.MMIO.Read32(baseVirt + RTCCR);
        if ((ctrl & 1) == 0)
        {
            Native.MMIO.Write32(baseVirt + RTCCR, ctrl | 1);
        }

        // Read Unix seconds from RTCDR
        unixSeconds = Native.MMIO.Read32(baseVirt + RTCDR);

        Serial.Write("[RTC] PL031 RTCDR=0x");
        Serial.WriteHex(unixSeconds);
        Serial.Write(" (Unix seconds)\n");

        // Sanity check: must be > 2000-01-01 (946684800) and < year 2100 (4102444800)
        if (unixSeconds < 946_684_800u || unixSeconds > 4_102_444_800u)
        {
            Serial.Write("[RTC] RTCDR value out of sane range\n");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts a Unix timestamp (seconds since 1970-01-01) to calendar date/time.
    /// </summary>
    private static void UnixSecondsToDate(uint unixSeconds,
        out int year, out int month, out int day,
        out int hour, out int minute, out int second)
    {
        second = (int)(unixSeconds % 60); unixSeconds /= 60;
        minute = (int)(unixSeconds % 60); unixSeconds /= 60;
        hour = (int)(unixSeconds % 24); unixSeconds /= 24;

        // Days since 1970-01-01
        uint days = unixSeconds;

        // Compute year
        year = 1970;
        while (true)
        {
            uint daysInYear = IsLeapYear(year) ? 366u : 365u;
            if (days < daysInYear)
            {
                break;
            }

            days -= daysInYear;
            year++;
        }

        // Compute month
        int[] daysPerMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        if (IsLeapYear(year))
        {
            daysPerMonth[1] = 29;
        }

        month = 1;
        foreach (int d in daysPerMonth)
        {
            if (days < (uint)d)
            {
                break;
            }

            days -= (uint)d;
            month++;
        }

        day = (int)days + 1;
    }

    private static bool IsLeapYear(int year)
        => (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);

    /// <summary>Multiplies two 64-bit values and divides by a third without overflow.</summary>
    private static ulong MultiplyDivide(ulong a, ulong b, ulong c)
    {
        ulong result = a / c * b;
        ulong remainder = a % c;
        result += remainder * b / c;
        return result;
    }
}
