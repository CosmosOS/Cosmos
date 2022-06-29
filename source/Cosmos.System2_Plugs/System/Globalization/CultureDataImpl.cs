using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{
    [Plug("System.Globalization.CultureData, System.Private.CoreLib")]
    class CultureDataImpl
    {
        public static string get_TimeSeparator(object aThis)
        {
            return ":";
        }

        public static string get_AMDesignator(object aThis)
        {
            return "AM";
        }

        public static string get_PMDesignator(object aThis)
        {
            return "PM";
        }

        public static string[] get_LongTimes(object aThis)
        {
            return new string[] { "h:mm:ss tt" };
        }

        [PlugMethod(Signature = "System_String____System_Globalization_CultureData_LongDates_System_Globalization_CalendarId_")]
        public static string[] LongDates(object aThis, ushort aCalendarId)
        {
            return new string[] { "MM/dd/yyyy" };
        }

        [PlugMethod(Signature = "System_String____System_Globalization_CultureData_ShortDates_System_Globalization_CalendarId_")]
        public static string[] ShortDates(object aThis, ushort aCalendarId)
        {
            return new string[] { "MM/dd/yyyy" };
        }

        [PlugMethod(Signature = "System_String____System_Globalization_CultureData_YearMonths_System_Globalization_CalendarId_")]
        public static string[] YearMonths(object aThis, ushort aCalendarId)
        {
            return new string[] { "MMMM dd" };
        }

        public static string[] get_ShortTimes(object aThis)
        {
            return new string[] { "h:mm tt" };
        }


        public static int get_FirstDayOfWeek(object aThis)
        {
            return 0; // Sunday is 0 and standard value
        }

        public static int get_CalendarWeekRule(object aThis)
        {
            return 0; // FirstDay is 0 and standard value
        }

        [PlugMethod(Signature = "System_String____System_Globalization_CultureData_MonthNames_System_Globalization_CalendarId_")]
        public static string[] MonthNames(object aThis, ushort aCalendarId)
        {
            return new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", "" };
        }

        [PlugMethod(Signature = "System_String__System_Globalization_CultureData_GetLocaleInfoCoreUserOverride__System_Globalization_CultureData_LocaleStringData")]
        public static string GetLocaleInfoCoreUserOverride(object aThis, uint aLocaleStringData)
        {
            throw new NotImplementedException("GetLocaleInfoCoreUserOverride");
        }

        public static int GetLocaleInfoExInt(string aString, uint aUint)
        {
            throw new NotImplementedException("GetLocaleInfoExInt");
        }

        public static string get_PercentSymbol(object aThis)
        {
            return "%";
        }

        public static string get_PerMilleSymbol(object aThis)
        {
            return "‰";
        }

        public static string get_NegativeInfinitySymbol(object aThis)
        {
            return "-Infinity";
        }

        public static string get_PositiveInfinitySymbol(object aThis)
        {
            return "Infinity";
        }

        public static string get_NaNSymbol(object aThis)
        {
            return "NaN";
        }

        public static int[] get_NumberGroupSizes(object aThis)
        {
            return new int[] { 3 };
        }

        public static int[] get_CurrencyGroupSizes(object aThis)
        {
            return new int[] { 3 };
        }

        public static Calendar get_DefaultCalendar(object aThis)
        {
            return new GregorianCalendar();
        }
    }
}
