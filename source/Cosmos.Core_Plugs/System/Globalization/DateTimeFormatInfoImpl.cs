using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
