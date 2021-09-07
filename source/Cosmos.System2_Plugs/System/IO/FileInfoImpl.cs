using System.IO;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(FileInfo))]
    public static class FileInfoImpl
    {
        /* Optimize this: CosmosVFS should expose an attribute without the need to open the file for reading... */
        public static long get_Length(FileInfo aThis)
        {
            using (var xFs = aThis.OpenRead())
            {
                return xFs.Length;
            }
        }
    }
}
