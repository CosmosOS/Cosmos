// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Converts the two date forms ISO 9660 records into Unix seconds: the 7-byte
/// recording date of a directory record and a Rock Ridge <c>TF</c> stamp
/// (years since 1900, month, day, hour, minute, second, zone in quarter hours
/// east of GMT), and the 17-byte digit form of a volume descriptor and a
/// long-form <c>TF</c> stamp. Integer day arithmetic, so that the stat path
/// allocates nothing.
/// </summary>
internal static class Iso9660Date
{
    /// <summary>Days from 1970-01-01 to 0000-03-01 in the proleptic Gregorian calendar, the epoch the day count below is taken from.</summary>
    private const long DaysFromCivilEpochToUnixEpoch = 719_468;

    /// <summary>Days in a 400-year Gregorian cycle.</summary>
    private const long DaysPerEra = 146_097;

    /// <summary>Years in a 400-year Gregorian cycle.</summary>
    private const long YearsPerEra = 400;

    private const long SecondsPerMinute = 60;
    private const long SecondsPerHour = 3_600;
    private const long SecondsPerDay = 86_400;
    private const long SecondsPerQuarterHour = 900;

    /// <summary>The zone offset field is a signed count of quarter hours, bounded to a day each way.</summary>
    private const int MaxZoneQuarterHours = 52;

    /// <summary>
    /// Reads a 7-byte recording date. A zeroed date (what a writer records
    /// when it has none) and an out-of-range field both read as 0, the Unix
    /// epoch.
    /// </summary>
    /// <param name="date">The 7 bytes.</param>
    /// <returns>Unix seconds, UTC.</returns>
    public static long FromRecordingDate(ReadOnlySpan<byte> date)
    {
        if (date.Length < Iso9660Layout.ShortDateLength)
        {
            return 0;
        }

        int year = Iso9660Layout.RecordDateBaseYear + date[0];
        int month = date[1];
        int day = date[2];
        int hour = date[3];
        int minute = date[4];
        int second = date[5];
        int zone = (sbyte)date[6];
        return ToUnixSeconds(year, month, day, hour, minute, second, zone);
    }

    /// <summary>
    /// Reads a 17-byte descriptor date: four digits of year, then two each of
    /// month, day, hour, minute, second and hundredths, then the zone byte.
    /// </summary>
    /// <param name="date">The 17 bytes.</param>
    /// <returns>Unix seconds, UTC; 0 for a blank or malformed date.</returns>
    public static long FromDescriptorDate(ReadOnlySpan<byte> date)
    {
        if (date.Length < Iso9660Layout.LongDateLength)
        {
            return 0;
        }

        if (!TryReadDigits(date, 0, 4, out int year)
            || !TryReadDigits(date, 4, 2, out int month)
            || !TryReadDigits(date, 6, 2, out int day)
            || !TryReadDigits(date, 8, 2, out int hour)
            || !TryReadDigits(date, 10, 2, out int minute)
            || !TryReadDigits(date, 12, 2, out int second))
        {
            return 0;
        }

        int zone = (sbyte)date[16];
        return ToUnixSeconds(year, month, day, hour, minute, second, zone);
    }

    private static bool TryReadDigits(ReadOnlySpan<byte> text, int offset, int count, out int value)
    {
        value = 0;
        for (int i = 0; i < count; i++)
        {
            int digit = text[offset + i] - '0';
            if (digit < 0 || digit > 9)
            {
                return false;
            }

            value = (value * 10) + digit;
        }

        return true;
    }

    private static long ToUnixSeconds(int year, int month, int day, int hour, int minute, int second, int zoneQuarterHours)
    {
        if (month < 1 || month > 12 || day < 1 || day > 31
            || hour > 23 || minute > 59 || second > 59
            || zoneQuarterHours < -MaxZoneQuarterHours || zoneQuarterHours > MaxZoneQuarterHours)
        {
            return 0;
        }

        long days = DaysFromCivil(year, month, day);
        long seconds = (days * SecondsPerDay) + (hour * SecondsPerHour) + (minute * SecondsPerMinute) + second;

        // The zone is the local time's offset east of GMT: subtracting it
        // brings the local stamp back to UTC.
        return seconds - (zoneQuarterHours * SecondsPerQuarterHour);
    }

    /// <summary>
    /// Days since 1970-01-01 of a proleptic Gregorian date (Howard Hinnant's
    /// days_from_civil): the year is shifted to start in March so that the
    /// leap day falls last.
    /// </summary>
    private static long DaysFromCivil(long year, long month, long day)
    {
        if (month <= 2)
        {
            year--;
        }

        long era = (year >= 0 ? year : year - (YearsPerEra - 1)) / YearsPerEra;
        long yearOfEra = year - (era * YearsPerEra);
        long shiftedMonth = month > 2 ? month - 3 : month + 9;
        long dayOfYear = (((153 * shiftedMonth) + 2) / 5) + day - 1;
        long dayOfEra = (yearOfEra * 365) + (yearOfEra / 4) - (yearOfEra / 100) + dayOfYear;
        return (era * DaysPerEra) + dayOfEra - DaysFromCivilEpochToUnixEpoch;
    }
}
