using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{
    internal enum CalendarDataType
    {
        Uninitialized,
        NativeName,
        MonthDay,
        ShortDates,
        LongDates,
        YearMonths,
        DayNames,
        AbbrevDayNames,
        MonthNames,
        AbbrevMonthNames,
        SuperShortDayNames,
        MonthGenitiveNames,
        AbbrevMonthGenitiveNames,
        EraNames,
        AbbrevEraNames
    }

    [Plug("System.Globalization.CalendarData, System.Private.CoreLib")]
    class CalendarDataImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_Globalization_CalendarData_GetCalendarInfo__System_String_System_Globalization_CalendarId_System_Globalization_CalendarDataType_System_String__")]
        public unsafe static bool GetCalendarInfo(string localeName, object calendarId, CalendarDataType aDataType, out string calendarString)
        {
            switch (aDataType)
            {
                case CalendarDataType.Uninitialized:
                    break;
                case CalendarDataType.NativeName:
                    break;
                case CalendarDataType.MonthDay:
                    break;
                case CalendarDataType.ShortDates:
                    break;
                case CalendarDataType.LongDates:
                    break;
                case CalendarDataType.YearMonths:
                    break;
                case CalendarDataType.DayNames:
                    break;
                case CalendarDataType.AbbrevDayNames:
                    break;
                case CalendarDataType.MonthNames:
                    break;
                case CalendarDataType.AbbrevMonthNames:
                    break;
                case CalendarDataType.SuperShortDayNames:
                    break;
                case CalendarDataType.MonthGenitiveNames:
                    break;
                case CalendarDataType.AbbrevMonthGenitiveNames:
                    break;
                case CalendarDataType.EraNames:
                    break;
                case CalendarDataType.AbbrevEraNames:
                    break;
                default:
                    break;
            }
            Debugger.DoSendNumber((int)aDataType);
            throw new NotImplementedException();
        }
    }
}