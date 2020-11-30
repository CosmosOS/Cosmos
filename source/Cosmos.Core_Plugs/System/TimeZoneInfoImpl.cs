using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(typeof(global::System.TimeZoneInfo))]
    class TimeZoneInfoImpl
    {
        public static TimeZoneInfo get_Local()
        {
            throw new NotImplementedException();
        }

        public static TimeSpan GetUtcOffset(TimeZoneInfo aThis, DateTime aDateTime)
        {
            throw new NotImplementedException();
        }
    }
}
