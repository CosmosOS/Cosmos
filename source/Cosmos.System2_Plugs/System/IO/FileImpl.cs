//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.System;
using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System.Text;

namespace Cosmos.System_Plugs.System.IO
{
    // TODO A lot of these methods should be implemented using StreamReader / StreamWriter
    [Plug(Target = typeof(File))]
    public static class FileImpl
    {
        /*
         * Plug needed for the usual issue that Array can not be converted in IEnumerable... it is starting
         * to become annoying :-(
         */
        public static void WriteAllLines(string aFile, string[] contents)
        {
            if (aFile == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (aFile.Length == 0)
            {
                throw new ArgumentException("Empty", "aFile");
            }

            Global.mFileSystemDebugger.SendInternal("Writing contents");

            using (var xSW = new StreamWriter(aFile))
            {
                foreach (var current in contents)
                    xSW.WriteLine(current);
            }
        }

        public static byte[] ReadAllBytes(string aFile)
        {
            Global.mFileSystemDebugger.SendInternal("In FileImpl.ReadAllText");
            using (var xFS = new FileStream(aFile, FileMode.Open))
            {
                var xBuff = new byte[(int)xFS.Length];
                var xResult = xFS.Read(xBuff, 0, xBuff.Length);
                if (xResult != xBuff.Length)
                {
                    throw new Exception("Couldn't read complete file!");
                }
                Global.mFileSystemDebugger.SendInternal("Bytes read");

                return xBuff;
            }
        }

        public static void WriteAllBytes(string aFile, byte[] aBytes)
        {
            using (var xFS = new FileStream(aFile, FileMode.Create))
            {
                xFS.Write(aBytes, 0, aBytes.Length);
            }
        }
    }
}
