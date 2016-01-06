using global::System;
using global::System.IO;

using Cosmos.Common.Extensions;
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

#if COSMOSDEBUG
                for (int i = 0; i < xBuff.Length; i++)
                {
                    FileSystemHelpers.Debug("xBuff is", xBuff[i].ToString("0:x2"));
                }
#endif

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

        public static string[] ReadAllLines(string aFile)
        {
            String text = ReadAllText(aFile);

            FileSystemHelpers.Debug("Read content");
            FileSystemHelpers.Debug("\n", text);

            String []result = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            FileSystemHelpers.Debug("content as array of lines:");
#if COSMOSDEBUG
            for (int i = 0; i < result.Length; i++)
                FileSystemHelpers.Debug(result[i]);
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

            FileSystemHelpers.Debug("Writing contents");
            FileSystemHelpers.Debug("\n" + text);

            WriteAllText(aFile, text);
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
