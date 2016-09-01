//#define COSMOSDEBUG

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
            Global.mFileSystemDebugger.SendInternal("File.Exists:");

            if (string.IsNullOrEmpty(aFile))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aFile));
            }
            Global.mFileSystemDebugger.SendInternal("aFile =");
            Global.mFileSystemDebugger.SendInternal(aFile);

            return VFSManager.FileExists(aFile);
        }

        public static string ReadAllText(string aFile)
        {
            Global.mFileSystemDebugger.SendInternal("File.ReadAllText:");

            if (string.IsNullOrEmpty(aFile))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aFile));
            }
            Global.mFileSystemDebugger.SendInternal("aFile =");
            Global.mFileSystemDebugger.SendInternal(aFile);

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

            if (string.IsNullOrEmpty(aFile))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aFile));
            }
            Global.mFileSystemDebugger.SendInternal("aFile =");
            Global.mFileSystemDebugger.SendInternal(aFile);

            if (string.IsNullOrEmpty(aText))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aText));
            }
            Global.mFileSystemDebugger.SendInternal("aText =");
            Global.mFileSystemDebugger.SendInternal(aText);

            using (var xFS = new FileStream(aFile, FileMode.Create))
            {
                Global.mFileSystemDebugger.SendInternal("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint)aText.Length);
                if ((xBuff != null) && (xBuff.Length > 0))
                {
                    Global.mFileSystemDebugger.SendInternal("xBuff.Length =");
                    Global.mFileSystemDebugger.SendInternal((uint)xBuff.Length);
                    Global.mFileSystemDebugger.SendInternal("Writing bytes");
                    xFS.Write(xBuff, 0, xBuff.Length);
                    Global.mFileSystemDebugger.SendInternal("Bytes written");
                }
                else
                {
                    throw new Exception("No text data to write.");
                }
            }
        }

        public static void AppendAllText(string aFile, string aText)
        {
            Global.mFileSystemDebugger.SendInternal("Creating stream in Append Mode with file  " + aFile);
            using (var xFS = new FileStream(aFile, FileMode.Append))
            {
                Global.mFileSystemDebugger.SendInternal("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint)aText.Length);
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

            string[] result = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            Global.mFileSystemDebugger.SendInternal("content as array of lines:");

            return result;
        }

        public static void WriteAllLines(string aFile, string[] contents)
        {
            String text = null;

            for (int i = 0; i < contents.Length; i++)
            {
                text = string.Concat(text, contents[i], Environment.NewLine);
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
                xFS.Write(aBytes, 0, aBytes.Length);
            }
        }

        public static void Copy(string srcFile, string destFile)
        {
            using (var xFS = new FileStream(srcFile, FileMode.Open))
            {
                var xBuff = new byte[(int)xFS.Length];
                var yFS = new FileStream(destFile, FileMode.Create);
                yFS.Write(xBuff, 0, xBuff.Length);
            }
        }

        public static void InternalDelete(string aPath, bool checkHost)
        {
            String xFullPath = Path.GetFullPath(aPath);

            VFSManager.DeleteFile(xFullPath);
        }

        public static FileStream Create(string aFile)
        {
            Global.mFileSystemDebugger.SendInternal("File.Create:");

            if (string.IsNullOrEmpty(aFile))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aFile));
            }

            var xEntry = VFSManager.CreateFile(aFile);
            if (xEntry == null)
            {
                return null;
            }

            return new FileStream(aFile, FileMode.Open);
        }
    }
}
