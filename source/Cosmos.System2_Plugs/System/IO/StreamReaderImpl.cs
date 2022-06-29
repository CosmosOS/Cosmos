using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(typeof(StreamReader))]
    public static class StreamReaderImpl
    {
        public static void CheckAsyncTaskInProgress(StreamReader aThis)
        {
        }
    }
}
