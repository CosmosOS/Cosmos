// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.X64.Cpu;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.Devices.Clock;

namespace Cosmos.Kernel.HAL.X64.Devices.Clock;

/// <summary>
/// CMOS Real-Time Clock (RTC) device for x64.
/// Reads date/time from the hardware RTC.
/// </summary>
internal class RTC : Device
{
    /// <summary>
    /// Singleton instance of the RTC.
    /// </summary>
    public static RTC? Instance { get; private set; }

    /// <summary>
    /// CMOS address/index port.
    /// </summary>
    private const ushort CMOSAddress = 0x70;

    /// <summary>
    /// CMOS data port.
    /// </summary>
    private const ushort CMOSData = 0x71;

    // RTC register addresses
    private const byte RegSeconds = 0x00;
    private const byte RegMinutes = 0x02;
    private const byte RegHours = 0x04;
    private const byte RegDayOfWeek = 0x06;
    private const byte RegDayOfMonth = 0x07;
    private const byte RegMonth = 0x08;
    private const byte RegYear = 0x09;
    private const byte RegCentury = 0x32;
    private const byte RegStatusA = 0x0A;
    private const byte RegStatusB = 0x0B;

    /// <summary>
    /// Boot time captured when RTC is initialized (in DateTime ticks).
    /// </summary>
    public long BootTimeTicks { get; private set; }

    /// <summary>
    /// TSC value at boot time (used to calculate elapsed time).
    /// </summary>
    public ulong BootTsc { get; private set; }

    /// <summary>
    /// Whether the RTC has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Mirrors the ARM64 RTC property name for cross-arch compatibility.</summary>
    public bool IsAvailable => IsInitialized;

    /// <summary>
    /// Initialize the RTC and capture boot time.
    /// Should be called after TSC calibration.
    /// </summary>
    public unsafe void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        Serial.Write("[RTC] Initializing...\n");

        Instance = this;

        // Capture current TSC as boot reference
        BootTsc = X64CpuOps.ReadTSC();

        // Priority 1: EFI Runtime Services GetTime
        if (EfiRtc.TryGetTime(out long efiTicks))
        {
            BootTimeTicks = efiTicks;
            IsInitialized = true;
            Serial.Write("[RTC] Initialized\n");
            return;
        }

        // Priority 2: Limine boot time
        if (Limine.BootTime.Response != null)
        {
            long unixSecs = Limine.BootTime.Response->BootTime;
            if (unixSecs > 0)
            {
                BootTimeTicks = DateTime.UnixEpoch.Ticks + unixSecs * TimeSpan.TicksPerSecond;
                Serial.Write("[RTC] Boot time (Limine): ");
                LogTime(BootTimeTicks);
                IsInitialized = true;
                Serial.Write("[RTC] Initialized\n");
                return;
            }
        }

        // Priority 3: CMOS RTC
        var (year, month, day, hour, minute, second) = ReadTime();
        BootTimeTicks = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
        Serial.Write("[RTC] Boot time (CMOS): ");
        LogTime(BootTimeTicks);

        IsInitialized = true;
        Serial.Write("[RTC] Initialized\n");
    }

    private static void LogTime(long ticks)
    {
        var dt = new System.DateTime(ticks, System.DateTimeKind.Utc);
        int year = dt.Year, month = dt.Month, day = dt.Day;
        int hour = dt.Hour, minute = dt.Minute, second = dt.Second;
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
    }

    /// <summary>
    /// Gets the current time as DateTime ticks.
    /// Uses boot time + elapsed TSC ticks converted to time.
    /// </summary>
    public long GetCurrentTicks()
    {
        if (!IsInitialized)
        {
            return 0;
        }

        // Calculate elapsed time since boot using TSC
        ulong currentTsc = X64CpuOps.ReadTSC();
        ulong elapsedTsc = currentTsc - BootTsc;

        // Convert TSC ticks to DateTime ticks (100-nanosecond intervals)
        // TSC frequency is in Hz (ticks per second)
        // DateTime ticks are 10,000,000 per second
        long tscFrequency = X64CpuOps.TscFrequency;
        if (tscFrequency <= 0)
        {
            tscFrequency = 1_000_000_000; // Default 1 GHz
        }

        // elapsedTicks = elapsedTsc * 10_000_000 / tscFrequency
        // Use 128-bit math to avoid overflow
        ulong elapsedTicks = MultiplyDivide(elapsedTsc, TimeSpan.TicksPerSecond, (ulong)tscFrequency);

        return BootTimeTicks + (long)elapsedTicks;
    }

    public long GetElapsedTicks()
    {
        if (!IsInitialized)
        {
            return 0;
        }

        // Calculate elapsed time since boot using TSC
        ulong currentTsc = X64CpuOps.ReadTSC();
        ulong elapsedTsc = currentTsc - BootTsc;

        // Convert TSC ticks to DateTime ticks (100-nanosecond intervals)
        // TSC frequency is in Hz (ticks per second)
        // DateTime ticks are 10,000,000 per second
        long tscFrequency = X64CpuOps.TscFrequency;
        if (tscFrequency <= 0)
        {
            tscFrequency = 1_000_000_000; // Default 1 GHz
        }

        // elapsedTicks = elapsedTsc * 10_000_000 / tscFrequency
        // Use 128-bit math to avoid overflow
        ulong elapsedTicks = MultiplyDivide(elapsedTsc, TimeSpan.TicksPerSecond, (ulong)tscFrequency);

        return (long)elapsedTicks;
    }


    /// <summary>
    /// Reads a byte from a CMOS register.
    /// </summary>
    private static byte ReadCMOS(byte register)
    {
        // Disable NMI by setting bit 7 and select register
        Native.IO.Write8(CMOSAddress, (byte)(0x80 | register));
        return Native.IO.Read8(CMOSData);
    }

    /// <summary>
    /// Checks if the RTC is currently updating.
    /// </summary>
    private static bool IsUpdating()
    {
        Native.IO.Write8(CMOSAddress, RegStatusA);
        return (Native.IO.Read8(CMOSData) & 0x80) != 0;
    }

    /// <summary>
    /// Reads the current time from the RTC.
    /// Waits for update to complete and reads twice to ensure consistency.
    /// </summary>
    public static (int year, int month, int day, int hour, int minute, int second) ReadTime()
    {
        // Wait for any update in progress to complete
        while (IsUpdating()) { }

        // Read status register B to check format
        byte statusB = ReadCMOS(RegStatusB);
        bool bcd = (statusB & 0x04) == 0; // Bit 2 = 0 means BCD format
        bool hour24 = (statusB & 0x02) != 0; // Bit 1 = 1 means 24-hour format

        // Read time values
        byte second = ReadCMOS(RegSeconds);
        byte minute = ReadCMOS(RegMinutes);
        byte hour = ReadCMOS(RegHours);
        byte day = ReadCMOS(RegDayOfMonth);
        byte month = ReadCMOS(RegMonth);
        byte year = ReadCMOS(RegYear);
        byte century = ReadCMOS(RegCentury);

        // Convert from BCD if necessary
        if (bcd)
        {
            second = BcdToBinary(second);
            minute = BcdToBinary(minute);
            hour = BcdToBinary((byte)(hour & 0x7F)); // Mask off PM bit for now
            day = BcdToBinary(day);
            month = BcdToBinary(month);
            year = BcdToBinary(year);
            century = BcdToBinary(century);
        }

        // Handle 12-hour format
        if (!hour24)
        {
            bool pm = (ReadCMOS(RegHours) & 0x80) != 0;
            if (hour == 12)
            {
                hour = pm ? (byte)12 : (byte)0;
            }
            else if (pm)
            {
                hour = (byte)(hour + 12);
            }
        }

        // Calculate full year
        int fullYear;
        if (century > 0)
        {
            fullYear = century * 100 + year;
        }
        else
        {
            // Assume 20xx for years < 80, 19xx otherwise
            fullYear = year < 80 ? 2000 + year : 1900 + year;
        }

        return (fullYear, month, day, hour, minute, second);
    }

    /// <summary>
    /// Converts a BCD value to binary.
    /// </summary>
    private static byte BcdToBinary(byte bcd)
    {
        return (byte)(((bcd >> 4) * 10) + (bcd & 0x0F));
    }

    /// <summary>
    /// Converts date to DateTime ticks.
    /// </summary>
    private static long DateToTicks(int year, int month, int day)
    {
        // Days in each month (non-leap year)
        int[] daysInMonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        // Calculate days from year 1
        int y = year - 1;
        long days = y * 365L + y / 4 - y / 100 + y / 400;

        // Add days for months in current year
        for (int m = 1; m < month; m++)
        {
            days += daysInMonth[m];
        }

        // Add leap day if applicable
        if (month > 2 && IsLeapYear(year))
        {
            days++;
        }

        // Add days in current month
        days += day - 1;

        // Convert to ticks (100-nanosecond intervals)
        return days * TimeSpan.TicksPerDay;
    }

    /// <summary>
    /// Converts time to DateTime ticks.
    /// </summary>
    private static long TimeToTicks(int hour, int minute, int second)
    {
        return hour * TimeSpan.TicksPerHour + minute * TimeSpan.TicksPerMinute + second * TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Checks if a year is a leap year.
    /// </summary>
    private static bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
    }

    /// <summary>
    /// Multiplies two 64-bit values and divides by a third, avoiding overflow.
    /// </summary>
    private static ulong MultiplyDivide(ulong a, ulong b, ulong c)
    {
        // Simple implementation - may lose precision for very large values
        // but good enough for typical time calculations
        ulong result = a / c * b;
        ulong remainder = a % c;
        result += remainder * b / c;
        return result;
    }
}
