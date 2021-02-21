using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(typeof(StreamWriter))]
    public static class StreamWriterImpl
    {
        public static void CheckAsyncTaskInProgress(StreamWriter aThis) { }

    }
}
