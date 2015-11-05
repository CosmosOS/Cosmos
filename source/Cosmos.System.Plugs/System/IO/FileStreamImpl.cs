using System;
using System.Collections.Generic;
using System.IO;
using IO = System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    using Cosmos.System.FileSystem;

    [Plug(Target = typeof(IO::FileStream))]
    [PlugField(FieldId = "$$InnerStream$$", FieldType = typeof(IO::Stream))]
    public class FileStreamImpl
    {
        // This plug basically forwards all calls to the $$InnerStream$$ stream, which is supplied by the file system.

        //  public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,

        public static void Ctor(IO::FileStream aThis, string aPathname, IO::FileMode aMode,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            FatHelpers.Debug("In FileStream.Ctor");
            innerStream = InitializeStream(aPathname, aMode);
        }

        public static void CCtor()
        {
            // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
        }

        public static int Read(IO::FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            return innerStream.Read(aBuffer, aOffset, aCount);
        }

        public static void Write(IO::FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            innerStream.Write(aBuffer, aOffset, aCount);
        }

        public static long get_Length(IO::FileStream aThis,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            return innerStream.Length;
        }

        public static void SetLength(IO::FileStream aThis, long aLength,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            innerStream.SetLength(aLength);
        }

        public static void Dispose(IO::FileStream aThis, bool disposing,
            [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }
        }

        public static long Seek(IO::FileStream aThis,
                                [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream, long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public static void Flush(IO::FileStream aThis,
           [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            innerStream.Flush();
        }

        public static long get_Position(IO::FileStream aThis,
                                        [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            return innerStream.Position;
        }

        public static void set_Position(IO::FileStream aThis,
                                        [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream, long value)
        {
            innerStream.Position = value;
        }

        //static void Init(IO::FileStream aThis, string path, IO::FileMode mode, IO::FileAccess access, int rights, bool useRights, IO::FileShare share, int bufferSize
        //  , IO::FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

        private static Stream InitializeStream(string aPath, FileMode aMode)
        {
            FatHelpers.Debug("In FileStream.InitializeStream");
            if (aPath == null)
            {
                FatHelpers.Debug("In FileStream.Ctor: Path == null is true");
                throw new Exception("The file path cannot be null.");
            }
            if (aPath.Length == 0)
            {
                FatHelpers.Debug("In FileStream.Ctor: Path.Length == 0 is true");
                throw new Exception("The file path cannot be empty.");
            }
            return VFSManager.GetFileStream(aPath);
        }
    }
}
