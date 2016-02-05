using global::System;
using global::System.IO;

using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    // TODO A lot of these methods should be implemented using StreamReader / StreamWriter
    [Plug(Target = typeof(File))]
    public static class FileImpl
    {
        public static bool Exists(string aFile)
        {
            return VFSManager.FileExists(aFile);
        }

        public static string ReadAllText(string aFile)
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
                var xResultStr = xBuff.GetUtf8String(0, (uint)xBuff.Length);
                Global.mFileSystemDebugger.SendInternal("ResultString retrieved");
                return xResultStr;
            }
        }

        public static void WriteAllText(string aFile, string aText)
        {
            Global.mFileSystemDebugger.SendInternal("Creating stream with file " + aFile);
            using (var xFS = new FileStream(aFile, FileMode.Create))
            {
                Global.mFileSystemDebugger.SendInternal("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint)aText.Length);
                Global.mFileSystemDebugger.SendInternal("Writing bytes");
                xFS.Write(xBuff, 0, xBuff.Length);
                Global.mFileSystemDebugger.SendInternal("Bytes written");
            }
        }

        public static void AppendAllText(string aFile, string aText)
        {
            Global.mFileSystemDebugger.SendInternal("Creating stream in Append Mode with file  " + aFile);
            using (var xFS = new FileStream(aFile, FileMode.Append))
            {
                Global.mFileSystemDebugger.SendInternal("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint) aText.Length);
                Global.mFileSystemDebugger.SendInternal("Writing bytes");
                xFS.Write(xBuff, 0, xBuff.Length);
                Global.mFileSystemDebugger.SendInternal("Bytes written");
            }
        }

        public static string[] ReadAllLines(string aFile)
        {
            String text = ReadAllText(aFile);

            Global.mFileSystemDebugger.SendInternal("Read contents");
            Global.mFileSystemDebugger.SendInternal(text);

            String []result = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            Global.mFileSystemDebugger.SendInternal("content as array of lines:");
#if COSMOSDEBUG
            for (int i = 0; i < result.Length; i++)
                Global.mFileSystemDebugger.SendInternal(result[i]);
#endif

            return result;
        }

        public static void WriteAllLines(string aFile, string[] contents)
        {
            String text = null;

            for (int i = 0; i < contents.Length; i++)
            {
                text = String.Concat(text, contents[i], Environment.NewLine);
            }

            Global.mFileSystemDebugger.SendInternal("Writing contents");
            Global.mFileSystemDebugger.SendInternal(text);

            WriteAllText(aFile, text);
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
                // This variable is not needed 'aBytes' is already a Byte[]
                //var xBuff = aBytes;
                
                xFS.Write(aBytes, 0, aBytes.Length);
            }
        }

        public static void Copy(string srcFile, string destFile)
        {
            byte[] xBuff;
            using (var xFS = new FileStream(srcFile, FileMode.Open))
            {
                xBuff = new byte[(int)xFS.Length];
                var s1 = xFS.Read(xBuff, 0, xBuff.Length);
                var yFS = new FileStream(destFile, FileMode.Create);
                yFS.Write(xBuff, 0, xBuff.Length);

            }
        }

        public static FileStream Create(string aFile)
        {
            if (aFile == null)
            {
                throw new ArgumentNullException("aFile");
            }

            if (aFile.Length == 0)
            {
                throw new ArgumentException("File path must not be empty.", "aFile");
            }

            Global.mFileSystemDebugger.SendInternal($"File.Create : aFile = {aFile}");
            var xEntry = VFSManager.CreateFile(aFile);
            if (xEntry == null)
            {
                return null;
            }

            return new FileStream(aFile, FileMode.Open);
        }
    }
}
