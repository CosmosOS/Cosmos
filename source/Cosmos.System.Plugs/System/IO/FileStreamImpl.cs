using global::System;
using global::System.IO;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(FileStream))]
    [PlugField(FieldId = "$$InnerStream$$", FieldType = typeof(Stream))]
    public class FileStreamImpl
    {
        // This plug basically forwards all calls to the $$InnerStream$$ stream, which is supplied by the file system.

        //  public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,

        public static void Ctor(FileStream aThis, string aPathname, FileMode aMode,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            FileSystemHelpers.Debug("In FileStream.Ctor");
            innerStream = InitializeStream(aPathname, aMode);
        }

        public static void CCtor()
        {
            // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
        }

        public static int Read(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            return innerStream.Read(aBuffer, aOffset, aCount);
        }

        public static void Write(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            innerStream.Write(aBuffer, aOffset, aCount);
        }

        public static long get_Length(FileStream aThis,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            return innerStream.Length;
        }

        public static void SetLength(FileStream aThis, long aLength,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            innerStream.SetLength(aLength);
        }

        public static void Dispose(FileStream aThis, bool disposing,
            [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }
        }

        public static long Seek(FileStream aThis,
                                [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream, long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public static void Flush(FileStream aThis,
           [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            innerStream.Flush();
        }

        public static long get_Position(FileStream aThis,
                                        [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream)
        {
            return innerStream.Position;
        }

        public static void set_Position(FileStream aThis,
                                        [FieldAccess(Name = "$$InnerStream$$")] ref Stream innerStream, long value)
        {
            innerStream.Position = value;
        }

        //static void Init(FileStream aThis, string path, FileMode mode, FileAccess access, int rights, bool useRights, FileShare share, int bufferSize
        //  , FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

        private static Stream InitializeStream(string aPath, FileMode aMode)
        {
            FileSystemHelpers.Debug("In FileStream.InitializeStream");
            if (aPath == null)
            {
                FileSystemHelpers.Debug("In FileStream.Ctor: Path == null is true");
                throw new Exception("The file path cannot be null.");
            }
            if (aPath.Length == 0)
            {
                FileSystemHelpers.Debug("In FileStream.Ctor: Path.Length == 0 is true");
                throw new Exception("The file path cannot be empty.");
            }
            return VFSManager.GetFileStream(aPath);
        }
    }
}
