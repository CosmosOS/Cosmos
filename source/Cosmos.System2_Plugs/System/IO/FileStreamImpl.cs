//#define COSMOSDEBUG

using global::System;
using global::System.IO;

using Cosmos.System;
using Cosmos.IL2CPU.API;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(FileStream))]
    [PlugField(FieldId = InnerStreamFieldId, FieldType = typeof(Stream))]
    public class FileStreamImpl
    {
        private const string InnerStreamFieldId = "$$InnerStream$$";

        // This plug basically forwards all calls to the $$InnerStream$$ stream, which is supplied by the file system.

        //  public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,

        public static void Ctor(FileStream aThis, string aPathname, FileMode aMode,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Global.mFileSystemDebugger.SendInternal("FileStream.Ctor:");

            innerStream = InitializeStream(aPathname, aMode);
        }

        public static void CCtor()
        {
            // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
        }

        public static int Read(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Global.mFileSystemDebugger.SendInternal("FileStream.Read:");

            return innerStream.Read(aBuffer, aOffset, aCount);
        }

        public static int ReadByte(FileStream aThis, [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.ReadByte();
        }

        public static void Write(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Global.mFileSystemDebugger.SendInternal("FileStream.Write:");

            innerStream.Write(aBuffer, aOffset, aCount);
        }

        public static long get_Length(FileStream aThis,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.Length;
        }

        public static void SetLength(FileStream aThis, long aLength,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            innerStream.SetLength(aLength);
        }

        public static void Dispose(FileStream aThis, bool disposing,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }
        }

        public static long Seek(FileStream aThis,
                                [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream, long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public static void Flush(FileStream aThis,
           [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            innerStream.Flush();
        }

        public static long get_Position(FileStream aThis,
                                        [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.Position;
        }

        public static void set_Position(FileStream aThis,
                                        [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream, long value)
        {
            innerStream.Position = value;
        }

        //static void Init(FileStream aThis, string path, FileMode mode, FileAccess access, int rights, bool useRights, FileShare share, int bufferSize
        //  , FileOptions options, Microsoft.Win32.Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy) { }

        private static Stream InitializeStream(string aPath, FileMode aMode)
        {
            Global.mFileSystemDebugger.SendInternal("In FileStream.InitializeStream");
            if (aPath == null)
            {
                Global.mFileSystemDebugger.SendInternal("In FileStream.Ctor: Path == null is true");
                throw new ArgumentNullException("The file path cannot be null.");
            }
            if (aPath.Length == 0)
            {
                Global.mFileSystemDebugger.SendInternal("In FileStream.Ctor: Path.Length == 0 is true");
                throw new ArgumentException("The file path cannot be empty.");
            }

            //Global.mFileSystemDebugger.SendInternal("Calling VFSManager.GetFileStream...");
            return VFSManager.GetFileStream(aPath);

            // Naive and not working implementation of FileMode. Probably is better to do this at lower level...
#if false
            // Before let's see if aPath already exists
            bool aPathExists = File.Exists(aPath);

            Stream aStream = null;

            switch (aMode)
            {
                case FileMode.Append:
                    // TODO it seems that GetFileStream effectively Creates the file if not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    if (aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Append mode with aPath already existing let's seek to end of the file");
                        Global.mFileSystemDebugger.SendInternal("Actual aStream Lenght: ", aStream.Length);
                        aStream.Position = aStream.Length;
                        //aStream.Seek(0, SeekOrigin.End);
                    }
                    else
                    {
                        Global.mFileSystemDebugger.SendInternal("Append mode with aPath not existing let's create a new the file");
                    }
                    break;

                case FileMode.Create:
                    Global.mFileSystemDebugger.SendInternal("Create Mode aPath will be overwritten if existing");
                    // TODO it seems that GetFileStream effectively Creates the file if not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    break;

                case FileMode.CreateNew:
                    if (aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath already existing");
                        throw new IOException("File already existing but CreateNew Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath not existing new file created");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    break;

                case FileMode.Open:
                    if (!aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Open Mode with aPath not existing");
                        throw new IOException("File not existing but Open Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("Open Mode with aPath existing opening file");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    aStream.Position = 0;
                    break;

                case FileMode.OpenOrCreate:
                    Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath not existing new file created");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    break;

                case FileMode.Truncate:
                    if (!aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Truncate Mode with aPath not existing");
                        throw new IOException("File not existing but Truncate Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("Truncate Mode with aPath existing change its lenght to 0 bytes");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(aPath);
                    aStream.SetLength(0);
                    break;

                default:
                    Global.mFileSystemDebugger.SendInternal("The mode " + aMode + "is out of range");
                    throw new ArgumentOutOfRangeException("The file mode is invalid");
            }

            return aStream;
#endif
        }
    }
}
