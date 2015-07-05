using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;
using Cosmos.IL2CPU.Plugs;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target = typeof(File))]
    public static class FileImpl
    {
        public static bool Exists(string aFile)
        {
            return FileSystem.VFS.VFSManager.FileExists(aFile);
        }



        public static string ReadAllText(string aFile)
        {
            using (var xFS = new FileStream(aFile, FileMode.Open))
            {
                var xBuff = new byte[(int)xFS.Length];
                var xResult = xFS.Read(xBuff, 0, xBuff.Length);
                if (xResult != xBuff.Length)
                {
                    throw new Exception("Couldn't read complete file!");
                }

                return xBuff.GetUtf8String(0, (uint)xBuff.Length);
            }
        }

        //    if (!File.Exists(aFile))
        //        return "Could not find file " + aFile;
        //        //throw new FileNotFoundException("Could not find file " + aFile);

        //    return VFSManager.ReadFileAsString(aFile);
        //}
    }
}
