using System;
using System.Collections.Generic;
using IO = System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target = typeof(IO::FileStream))]
                [PlugField(FieldId = "$$InnerStream$$", FieldType = typeof(IO::Stream))]
    public class FileStreamImpl
    {
        // This plug basically forwards all calls to the $$InnerStream$$ stream, which is supplied by the file system.




                    //  public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,

        public static void Ctor(IO::FileStream aThis, string aPathname, IO::FileMode aMode, [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            innerStream = VFSManager.GetFileStream(aPathname);
        }

        public static void CCtor()
        {
            // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
        }

        public static int Read(IO::FileStream aThis, byte[] aBuffer, int aOffset, int aCount, [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            return innerStream.Read(aBuffer, aOffset, aCount);
        }

        public static long get_Length(IO::FileStream aThis, [FieldAccess(Name = "$$InnerStream$$")] ref IO::Stream innerStream)
        {
            return innerStream.Length;
        }

        public static void SetLength(IO::FileStream aThis, long value)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(IO::FileStream aThis, bool disposing)
        {
            throw new NotImplementedException();
        }

        //static void Init(IO::FileStream aThis, string path, IO::FileMode mode, IO::FileAccess access, int rights, bool useRights, IO::FileShare share, int bufferSize
        //  , IO::FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }


    }
}
