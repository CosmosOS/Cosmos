using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{
    [Plug("System.Globalization.CalendarData, System.Private.CoreLib")]
    class CalendarDataImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_Globalization_CalendarData_GetCalendarInfo__System_String_System_Globalization_CalendarId_System_Globalization_CalendarDataType_System_String__")]
        public unsafe static bool GetCalendarInfo(string localeName, object calendarId, object dataType, out string calendarString)
        {
            throw new NotImplementedException();
        }
    }
}
