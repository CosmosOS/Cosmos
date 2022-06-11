using System.Globalization;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization;

[Plug(typeof(DateTimeFormatInfo))]
internal class DateTimeFormatInfoImpl
{
    public static string get_DecimalSeparator(DateTimeFormatInfo aThis) => ".";

    public static string get_TimeSeparator(DateTimeFormatInfo aThis) => ":";

    public static string get_ShortDatePattern(DateTimeFormatInfo aThis) => "h:mm tt";

    public static string get_LongTimePattern(DateTimeFormatInfo aThis) => "h:mm:ss tt";

    public static string get_AMDesignator(DateTimeFormatInfo aThis) => "AM";

    public static string get_PMDesignator(DateTimeFormatInfo aThis) => "PM";

    public static string[] get_AbbreviatedDayNames(DateTimeFormatInfo aThis) =>
        new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

    public static string[] get_AbbreviatedMonthNames(DateTimeFormatInfo aThis) => new[]
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""
    }; // its 13 for some reason

    [PlugMethod(Signature =
        "System_Array_System_Globalization_CalendarId_System_Globalization_DateTimeFormatInfo__get_OptionalCalendars")]
    public static ushort[] get_OptionalCalendars(DateTimeFormatInfo aThis) => new ushort[] { 1 }; // 1 is Gregorian

    public static string get_MonthDayPattern(DateTimeFormatInfo aThis) => "MMMM dd";

    public static string GetEraName(DateTimeFormatInfo aThis, int aInt) => "C.E.";

    public static string[] InternalGetGenitiveMonthNames(DateTimeFormatInfo aThis, bool aBool) => new[]
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""
    }; // its 13 for some reason

    public static string[] InternalGetLeapYearMonthNames(DateTimeFormatInfo aThis) => new string[0];

    public static string[] InternalGetAbbreviatedMonthNames(DateTimeFormatInfo aThis) => new[]
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""
    }; // its 13 for some reason

    public static string[] InternalGetAbbreviatedDayOfWeekNames(DateTimeFormatInfo aThis) =>
        new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

    public static string[] InternalGetDayOfWeekNames(DateTimeFormatInfo aThis) => new[]
        { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
}
