using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug("System.IO.Strategies.OSFileStreamStrategy, System.Private.CoreLib")]
    public class OSFileStreamStrategyImpl
    {
        public static void CCtor()
        {
        }

        public static void SetLength(object aThis, long len)
        {
            throw new NotImplementedException();
        }

        public static long get_Length(object aThis)
        {
            throw new NotImplementedException();
        }

        public static bool get_CanSeek(object aThis)
        {
            throw new NotImplementedException();
        }

        public static long Seek(object aThis, long a, object origin)
        {
            throw new NotImplementedException();
        }
    }
}
