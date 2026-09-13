// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Clock;

/// <summary>
/// Shared helper for reading wall-clock time via EFI Runtime Services GetTime().
/// Works on any UEFI platform (x64 or ARM64) after ExitBootServices because
/// EFI Runtime Services remain valid indefinitely.
/// </summary>
internal static class EfiRtc
{
    /// <summary>
    /// Attempts to read the current wall-clock time via EFI Runtime Services.
    /// Returns true and sets <paramref name="ticks"/> to DateTime ticks (UTC) on success.
    /// </summary>
    public static unsafe bool TryGetTime(out long ticks)
    {
        ticks = 0;

        if (Limine.EfiSystemTable.Response == null)
        {
            Serial.Write("[RTC] EFI system table response is null\n");
            return false;
        }

        EfiSystemTable* st = Limine.EfiSystemTable.Response->Address;
        if (st == null || st->RuntimeServices == null)
        {
            Serial.Write("[RTC] EFI system table or RuntimeServices is null\n");
            return false;
        }

        EfiTime time = default;
        ulong status = st->RuntimeServices->GetTime(&time, null);

        Serial.Write("[RTC] EFI GetTime status: ");
        Serial.WriteNumber(status);
        Serial.Write("\n");

        if (status != 0) // EFI_SUCCESS = 0
        {
            return false;
        }

        Serial.Write("[RTC] Boot time (EFI): ");
        Serial.WriteNumber(time.Year);
        Serial.Write("-");
        if (time.Month < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber(time.Month);
        Serial.Write("-");
        if (time.Day < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber(time.Day);
        Serial.Write(" ");
        if (time.Hour < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber(time.Hour);
        Serial.Write(":");
        if (time.Minute < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber(time.Minute);
        Serial.Write(":");
        if (time.Second < 10)
        {
            Serial.Write("0");
        }

        Serial.WriteNumber(time.Second);
        if (time.TimeZone == unchecked((short)0x07FF))
        {
            Serial.Write(" (TZ unspecified)\n");
        }
        else
        {
            Serial.Write(" UTC\n");
        }

        long dateTicks = DateToTicks(time.Year, time.Month, time.Day);
        long timeTicks = time.Hour * TimeSpan.TicksPerHour
                       + time.Minute * TimeSpan.TicksPerMinute
                       + time.Second * TimeSpan.TicksPerSecond
                       + time.Nanosecond / 100;

        long tzOffset = 0;
        if (time.TimeZone != unchecked((short)0x07FF))
        {
            tzOffset = -(long)time.TimeZone * TimeSpan.TicksPerMinute;
        }

        ticks = dateTicks + timeTicks + tzOffset;
        return ticks > 0;
    }

    private static long DateToTicks(int year, int month, int day)
    {
        int[] daysInMonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        int y = year - 1;
        long days = y * 365L + y / 4 - y / 100 + y / 400;
        for (int m = 1; m < month; m++)
        {
            days += daysInMonth[m];
        }

        if (month > 2 && IsLeapYear(year))
        {
            days++;
        }

        days += day - 1;
        return days * TimeSpan.TicksPerDay;
    }

    private static bool IsLeapYear(int year)
        => (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
}
