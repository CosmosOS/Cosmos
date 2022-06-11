using System;
using System.Globalization;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization;

[Plug("System.Globalization.CultureData, System.Private.CoreLib")]
internal class CultureDataImpl
{
    public static string get_TimeSeparator(object aThis) => ":";

    public static string get_AMDesignator(object aThis) => "AM";

    public static string get_PMDesignator(object aThis) => "PM";

    public static string[] get_LongTimes(object aThis) => new[] { "h:mm:ss tt" };

    [PlugMethod(Signature =
        "System_String____System_Globalization_CultureData_LongDates_System_Globalization_CalendarId_")]
    public static string[] LongDates(object aThis, ushort aCalendarId) => new[] { "MM/dd/yyyy" };

    [PlugMethod(Signature =
        "System_String____System_Globalization_CultureData_ShortDates_System_Globalization_CalendarId_")]
    public static string[] ShortDates(object aThis, ushort aCalendarId) => new[] { "MM/dd/yyyy" };

    [PlugMethod(Signature =
        "System_String____System_Globalization_CultureData_YearMonths_System_Globalization_CalendarId_")]
    public static string[] YearMonths(object aThis, ushort aCalendarId) => new[] { "MMMM dd" };

    public static string[] get_ShortTimes(object aThis) => new[] { "h:mm tt" };


    public static int get_FirstDayOfWeek(object aThis) => 0; // Sunday is 0 and standard value

    public static int get_CalendarWeekRule(object aThis) => 0; // FirstDay is 0 and standard value

    [PlugMethod(Signature =
        "System_String____System_Globalization_CultureData_MonthNames_System_Globalization_CalendarId_")]
    public static string[] MonthNames(object aThis, ushort aCalendarId) => new[]
    {
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November",
        "December", ""
    };

    [PlugMethod(Signature =
        "System_String__System_Globalization_CultureData_GetLocaleInfoCoreUserOverride__System_Globalization_CultureData_LocaleStringData")]
    public static string GetLocaleInfoCoreUserOverride(object aThis, uint aLocaleStringData) =>
        throw new NotImplementedException("GetLocaleInfoCoreUserOverride");

    public static int GetLocaleInfoExInt(string aString, uint aUint) =>
        throw new NotImplementedException("GetLocaleInfoExInt");

    public static string get_PercentSymbol(object aThis) => "%";

    public static string get_PerMilleSymbol(object aThis) => "‰";

    public static string get_NegativeInfinitySymbol(object aThis) => "-Infinity";

    public static string get_PositiveInfinitySymbol(object aThis) => "Infinity";

    public static string get_NaNSymbol(object aThis) => "NaN";

    public static int[] get_NumberGroupSizes(object aThis) => new[] { 3 };

    public static int[] get_CurrencyGroupSizes(object aThis) => new[] { 3 };

    public static Calendar get_DefaultCalendar(object aThis) => new GregorianCalendar();
}
