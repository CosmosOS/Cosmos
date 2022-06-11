using System.IO;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO;

[Plug(typeof(StreamReader))]
public static class StreamReaderImpl
{
    public static void CheckAsyncTaskInProgress(StreamReader aThis)
    {
    }
}
