using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(typeof(global::System.Globalization.DateTimeFormatInfo))]
    class DateTimeFormatInfoImpl
    {
        public static string get_DecimalSeparator(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return ".";
        }

        public static string get_TimeSeparator(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return ":";
        }
        public static string get_ShortDatePattern(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return "h:mm tt";
        }

        public static string get_LongTimePattern(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return "h:mm:ss tt";
        }

        public static string get_AMDesignator(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return "AM";
        }

        public static string get_PMDesignator(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return "PM";
        }

        public static string[] get_AbbreviatedDayNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        }

        public static string[] get_AbbreviatedMonthNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "" }; // its 13 for some reason
        }

        [PlugMethod(Signature = "System_Array_System_Globalization_CalendarId_System_Globalization_DateTimeFormatInfo__get_OptionalCalendars")]
        public static ushort[] get_OptionalCalendars(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new ushort[] { 1 }; // 1 is Gregorian
        }

        public static string get_MonthDayPattern(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return "MMMM dd";
        }

        public static string GetEraName(global::System.Globalization.DateTimeFormatInfo aThis, int aInt)
        {
            return "C.E.";
        }

        public static string[] InternalGetGenitiveMonthNames(global::System.Globalization.DateTimeFormatInfo aThis, bool aBool)
        {
            return new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "" }; // its 13 for some reason
        }

        public static string[] InternalGetLeapYearMonthNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new string[0];
        }

        public static string[] InternalGetAbbreviatedMonthNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "" }; // its 13 for some reason
        }

        public static string[] InternalGetAbbreviatedDayOfWeekNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        }

        public static string[] InternalGetDayOfWeekNames(global::System.Globalization.DateTimeFormatInfo aThis)
        {
            return new[] { "Sunday", "Monday", "Tuesday","Wednesday", "Thursday", "Friday", "Saturday" };
        }
    }
}
