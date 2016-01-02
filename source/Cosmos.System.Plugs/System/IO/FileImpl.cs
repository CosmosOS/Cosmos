using global::System;
using global::System.IO;

using Cosmos.Common.Extensions;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(File))]
    public static class FileImpl
    {
        public static bool Exists(string aFile)
        {
            return VFSManager.FileExists(aFile);
        }

        public static string ReadAllText(string aFile)
        {
            FileSystemHelpers.Debug("In FileImpl.ReadAllText");
            using (var xFS = new FileStream(aFile, FileMode.Open))
            {
                var xBuff = new byte[(int)xFS.Length];
                var xResult = xFS.Read(xBuff, 0, xBuff.Length);
                if (xResult != xBuff.Length)
                {
                    throw new Exception("Couldn't read complete file!");
                }
                FileSystemHelpers.Debug("Bytes read");
                var xResultStr = xBuff.GetUtf8String(0, (uint)xBuff.Length);
                FileSystemHelpers.Debug("ResultString retrieved");
                return xResultStr;
            }
        }

        public static void WriteAllText(string aFile, string aText)
        {
            FileSystemHelpers.Debug("Creating stream with file " + aFile);
            using (var xFS = new FileStream(aFile, FileMode.Create))
            {
                FileSystemHelpers.Debug("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint)aText.Length);
                FileSystemHelpers.Debug("Writing bytes");
                xFS.Write(xBuff, 0, xBuff.Length);
                FileSystemHelpers.Debug("Bytes written");
            }
        }

        public static void AppendAllText(string aFile, string aText)
        {
            FileSystemHelpers.Debug("Creating stream in Append Mode with file  " + aFile);
            using (var xFS = new FileStream(aFile, FileMode.Append))
            {
                FileSystemHelpers.Debug("Converting " + aText + " to UFT8");
                var xBuff = aText.GetUtf8Bytes(0, (uint)aText.Length);
                FileSystemHelpers.Debug("Writing bytes");
                xFS.Write(xBuff, 0, xBuff.Length);
                FileSystemHelpers.Debug("Bytes written");
            }
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            String text = String.Empty;

            for (int i = 0; i < contents.Length; i++)
            {
                text += contents[i];
                text += '\n';
            }

            FileSystemHelpers.Debug("Writing contents\n" + text);
            WriteAllText(path, text);
        }

        public static byte[] ReadAllBytes(string aFile)
        {
            FileSystemHelpers.Debug("In FileImpl.ReadAllText");
            using (var xFS = new FileStream(aFile, FileMode.Open))
            {
                var xBuff = new byte[(int)xFS.Length];
                var xResult = xFS.Read(xBuff, 0, xBuff.Length);
                if (xResult != xBuff.Length)
                {
                    throw new Exception("Couldn't read complete file!");
                }
                FileSystemHelpers.Debug("Bytes read");
                
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

            FileSystemHelpers.Debug("File.Create", "aFile =", aFile);
            var xEntry = VFSManager.CreateFile(aFile);
            if (xEntry == null)
            {
                return null;
            }

            return new FileStream(aFile, FileMode.Open);
        }
    }
}
