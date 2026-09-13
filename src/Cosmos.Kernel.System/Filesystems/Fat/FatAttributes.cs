// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>
/// FAT attribute byte / date / time conversions for the VFS layer.
/// </summary>
internal static class FatAttributes
{
    // FAT predates POSIX permissions; we synthesize 0o755 / 0o644 and clear
    // write bits when ATTR_READ_ONLY is set so callers see plausible mode.
    private const VfsMode DirPermissions =
        VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.OwnerExecute |
        VfsMode.GroupRead | VfsMode.GroupExecute |
        VfsMode.OtherRead | VfsMode.OtherExecute;

    private const VfsMode FilePermissions =
        VfsMode.OwnerRead | VfsMode.OwnerWrite |
        VfsMode.GroupRead |
        VfsMode.OtherRead;

    private const VfsMode WriteMask =
        VfsMode.OwnerWrite | VfsMode.GroupWrite | VfsMode.OtherWrite;

    public static VfsMode ToMode(FatAttr attributes)
    {
        bool isDir = (attributes & FatAttr.Directory) != 0;
        VfsMode mode = isDir
            ? VfsMode.Directory | DirPermissions
            : VfsMode.RegularFile | FilePermissions;

        if ((attributes & FatAttr.ReadOnly) != 0)
        {
            mode &= ~WriteMask;
        }

        return mode;
    }

    public static FatAttr ToFatAttr(VfsMode mode)
    {
        FatAttr attr = (mode & VfsMode.FileTypeMask) == VfsMode.Directory
            ? FatAttr.Directory
            : FatAttr.None;

        if ((mode & VfsMode.OwnerWrite) == 0 && (mode & VfsMode.FileTypeMask) != VfsMode.Directory)
        {
            attr |= FatAttr.ReadOnly;
        }

        return attr;
    }

    /// <summary>FAT dates count years from 1980 (bits 15-9 of the date word).</summary>
    private const int FatEpochYear = 1980;

    /// <summary>Bit shift of the year field within the FAT date word (bits 15-9, fatgen103 "Date and Time Formats").</summary>
    private const int DateYearShift = 9;

    /// <summary>Year field mask after shifting (7 bits, 1980..2107).</summary>
    private const int DateYearMask = 0x7F;

    /// <summary>Bit shift of the month field within the FAT date word (bits 8-5, fatgen103 "Date and Time Formats").</summary>
    private const int DateMonthShift = 5;

    /// <summary>Month field mask after shifting (bits 8-5).</summary>
    private const int DateMonthMask = 0x0F;

    /// <summary>Day field mask (bits 4-0).</summary>
    private const int DateDayMask = 0x1F;

    /// <summary>Bit shift of the hour field within the FAT time word (bits 15-11, fatgen103 "Date and Time Formats").</summary>
    private const int TimeHourShift = 11;

    /// <summary>Hour field mask after shifting (bits 15-11).</summary>
    private const int TimeHourMask = 0x1F;

    /// <summary>Bit shift of the minute field within the FAT time word (bits 10-5, fatgen103 "Date and Time Formats").</summary>
    private const int TimeMinuteShift = 5;

    /// <summary>Minute field mask after shifting (bits 10-5).</summary>
    private const int TimeMinuteMask = 0x3F;

    /// <summary>Seconds field mask (bits 4-0), stored at 2-second granularity.</summary>
    private const int TimeTwoSecondMask = 0x1F;

    /// <summary>The stored seconds field counts 2-second units.</summary>
    private const int TwoSecondGranularity = 2;

    /// <summary>The tenths byte counts hundredths 0..199; 100+ rolls into the next second.</summary>
    private const int TenthsPerSecond = 100;

    /// <summary>Nanoseconds per hundredth of a second.</summary>
    private const long NanosecondsPerHundredth = 10_000_000L;

    /// <summary>Seconds per day / hour / minute for the epoch math.</summary>
    private const long SecondsPerDay = 86_400L;
    private const long SecondsPerHour = 3600L;
    private const long SecondsPerMinute = 60L;

    /// <summary>Unix epoch year the VFS timespec counts seconds from.</summary>
    private const int UnixEpochYear = 1970;

    /// <summary>Months per year; upper bound of the FAT month field.</summary>
    private const int MonthsPerYear = 12;

    /// <summary>Longest month length in days; upper bound of the FAT day field.</summary>
    private const int MaxDaysInMonth = 31;

    /// <summary>Days in a Gregorian leap year.</summary>
    private const int DaysPerLeapYear = 366;

    /// <summary>Days in a Gregorian common (non-leap) year.</summary>
    private const int DaysPerCommonYear = 365;

    /// <summary>Month number of February, the month that gains the leap day.</summary>
    private const int February = 2;

    /// <summary>Gregorian rule: every 4th year is a leap year.</summary>
    private const int LeapYearInterval = 4;

    /// <summary>Gregorian rule: century years are not leap years.</summary>
    private const int LeapCenturyException = 100;

    /// <summary>Gregorian rule: years divisible by 400 are leap years after all.</summary>
    private const int LeapQuadCenturyInterval = 400;

    /// <summary>Days per month (non-leap); compiler-emitted static data, no per-call allocation.</summary>
    private static ReadOnlySpan<byte> DaysInMonth => new byte[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    public static VfsTimespec UnpackDateTime(ushort fatDate, ushort fatTime, byte tenths)
    {
        if (fatDate == 0)
        {
            return new VfsTimespec(0, 0);
        }

        int year = ((fatDate >> DateYearShift) & DateYearMask) + FatEpochYear;
        int month = (fatDate >> DateMonthShift) & DateMonthMask;
        int day = fatDate & DateDayMask;

        int hour = (fatTime >> TimeHourShift) & TimeHourMask;
        int minute = (fatTime >> TimeMinuteShift) & TimeMinuteMask;
        int second = (fatTime & TimeTwoSecondMask) * TwoSecondGranularity + (tenths >= TenthsPerSecond ? 1 : 0);

        long epochSeconds = ToUnixSeconds(year, month, day, hour, minute, second);
        long nanoseconds = (long)(tenths % TenthsPerSecond) * NanosecondsPerHundredth;
        return new VfsTimespec(epochSeconds, nanoseconds);
    }

    private static long ToUnixSeconds(int year, int month, int day, int hour, int minute, int second)
    {
        if (month < 1 || month > MonthsPerYear || day < 1 || day > MaxDaysInMonth)
        {
            return 0;
        }

        long days = 0;
        for (int y = UnixEpochYear; y < year; y++)
        {
            days += IsLeap(y) ? DaysPerLeapYear : DaysPerCommonYear;
        }
        for (int m = 1; m < month; m++)
        {
            days += DaysInMonth[m - 1];
            if (m == February && IsLeap(year))
            {
                days++;
            }
        }
        days += day - 1;
        return days * SecondsPerDay + hour * SecondsPerHour + minute * SecondsPerMinute + second;
    }

    private static bool IsLeap(int year)
    {
        return (year % LeapYearInterval == 0 && year % LeapCenturyException != 0) || year % LeapQuadCenturyInterval == 0;
    }
}
