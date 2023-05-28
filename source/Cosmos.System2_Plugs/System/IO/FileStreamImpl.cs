//#define COSMOSDEBUG
using global::System;
using global::System.IO;
using Cosmos.System;
using IL2CPU.API.Attribs;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(FileStream))]
    [PlugField(FieldId = InnerStreamFieldId, FieldType = typeof(Stream))]
    public class FileStreamImpl
    {
        private const string InnerStreamFieldId = "$$InnerStream$$";

        // This plug basically forwards all calls to the $$InnerStream$$ stream, which is supplied by the file system.

        private static void Init(string aPathname, FileMode aMode, ref Stream innerStream)
        {
            Global.Debugger.SendInternal("-- FileStream.Init --");
            Global.Debugger.SendInternal("aPathname = " + aPathname);

            innerStream = InitializeStream(aPathname, aMode);
        }

        public static void Ctor(FileStream aThis, string aPathname, FileMode aMode,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Init(aPathname, aMode, ref innerStream);
        }

        public static void CCtor()
        {
            // plug cctor as it (indirectly) uses Thread.MemoryBarrier()
        }

        public static void Ctor(FileStream aThis, string aPathname, FileMode aMode, FileAccess access,
                                FileShare share, int bufferSize, FileOptions options,
                                [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Init(aPathname, aMode, ref innerStream);
        }

        public static int Read(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream) =>
            aThis.Read(aBuffer.AsSpan().Slice(aOffset, aCount));

        public static int Read(FileStream aThis, Span<byte> aBuffer,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Global.Debugger.SendInternal("FileStream.Read:");

            return innerStream.Read(aBuffer);
        }

        public static int ReadByte(FileStream aThis, [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.ReadByte();
        }

        public static void Write(FileStream aThis, byte[] aBuffer, int aOffset, int aCount,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream) =>
            aThis.Write(aBuffer.AsSpan().Slice(aOffset, aCount));

        public static void Write(FileStream aThis, ReadOnlySpan<byte> aBuffer,
            [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            Global.Debugger.SendInternal($"FileStream.Write:");

            innerStream.Write(aBuffer);
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
            /*
             * It gives NRE and kills the OS, commented it for now... we will "de-plug" FileStream soon
             */
            Global.Debugger.SendInternal($"In FileStream.InitializeStream Flush()");
            //innerStream.Flush();
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

        private static Stream CreateNewFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"-- FileStream.CreateNewFile -- aPath = {aPath} existing = {aPathExists}");

            if (aPathExists)
            {
                Global.Debugger.SendInternal("CreateNew Mode with aPath already existing");
                throw new IOException("File already existing but CreateNew Requested");
            }

            var xEntry = VFSManager.CreateFile(aPath);
            if (xEntry == null)
            {
                return null;
            }

            return VFSManager.GetFileStream(aPath);
        }

        private static Stream TruncateFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"-- FileStream.TruncateFile -- aPath = {aPath} existing = {aPathExists}");

            if (!aPathExists)
            {
                Global.Debugger.SendInternal("Truncate Mode with aPath not existing");
                throw new IOException("File not existing but Truncate Requested");
            }

            Global.Debugger.SendInternal("Truncate Mode: change file lenght to 0 bytes");

            var aStream = VFSManager.GetFileStream(aPath);
            aStream.SetLength(0);

            return aStream;
        }

        private static Stream CreateFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"-- FileStream.CreateFile -- aPath = {aPath} existing = {aPathExists}");

            if (aPathExists == false)
            {
                Global.Debugger.SendInternal($"File does not exist let's call CreateNew() to create it");
                return CreateNewFile(aPath, aPathExists);
            }
            else
            {
                Global.Debugger.SendInternal($"File does exist let's call TruncateFile() to truncate it");
                return TruncateFile(aPath, aPathExists);
            }
        }

        private static Stream AppendToFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"In FileStream.AppendToFile aPath {aPath} existing? {aPathExists}");

            if (aPathExists)
            {
                Global.Debugger.SendInternal("Append mode with aPath already existing let's seek to end of the file");
                var aStream = VFSManager.GetFileStream(aPath);
                Global.Debugger.SendInternal("Actual aStream Lenght: " + aStream.Length);

                aStream.Seek(0, SeekOrigin.End);
                return aStream;
            }
            else
            {
                Global.Debugger.SendInternal("Append mode with aPath not existing let's create a new the file");
                return CreateNewFile(aPath, aPathExists);
            }
        }

        private static Stream OpenFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"In FileStream.OpenFile aPath {aPath} existing? {aPathExists}");

            if (!aPathExists)
            {
                throw new IOException("File not existing but Open Requested");
            }

            Global.Debugger.SendInternal("Open Mode with aPath already existing opening file");
            var aStream = VFSManager.GetFileStream(aPath);

            aStream.Position = 0;
            return aStream;
        }

        private static Stream OpenOrCreateFile(string aPath, bool aPathExists)
        {
            Global.Debugger.SendInternal($"In FileStream.OpenOrCreateFile aPath {aPath} existing? {aPathExists}");

            if (aPathExists)
            {
                Global.Debugger.SendInternal("OpenOrCreateFile Mode with aPath already existing, let's Open it!");
                return OpenFile(aPath, aPathExists);
            }
            else
            {
                Global.Debugger.SendInternal("OpenOrCreateFile Mode with aPath not existing, let's Create it!");
                return CreateNewFile(aPath, aPathExists);
            }
        }

        private static Stream InitializeStream(string aPath, FileMode aMode)
        {
            Global.Debugger.SendInternal($"-- FileStream.InitializeStream --");
            Global.Debugger.SendInternal($"aPath = {aPath}");
            if (aPath == null)
            {
                Global.Debugger.SendInternal("In FileStream.Ctor: Path == null is true");
                throw new ArgumentNullException("The file path cannot be null.");
            }
            if (aPath.Length == 0)
            {
                Global.Debugger.SendInternal("In FileStream.Ctor: Path.Length == 0 is true");
                throw new ArgumentException("The file path cannot be empty.");
            }

            // Before let's see if aPath already exists
            bool aPathExists = VFSManager.FileExists(aPath);

            switch (aMode)
            {
                case FileMode.Append:
                    return AppendToFile(aPath, aPathExists);

                case FileMode.Create:
                    return CreateFile(aPath, aPathExists);

                case FileMode.CreateNew:
                    return CreateNewFile(aPath, aPathExists);

                case FileMode.Open:
                    return OpenFile(aPath, aPathExists);

                case FileMode.OpenOrCreate:
                    return OpenOrCreateFile(aPath, aPathExists);

                case FileMode.Truncate:
                    return TruncateFile(aPath, aPathExists);

                default:
                    Global.Debugger.SendInternal("The mode " + aMode + "is out of range");
                    throw new ArgumentOutOfRangeException("The file mode is invalid");
            }
        }

        public static bool get_CanWrite(FileStream aThis, [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.CanWrite;
        }

        public static bool get_CanRead(FileStream aThis, [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.CanRead;
        }

        public static bool get_CanSeek(FileStream aThis, [FieldAccess(Name = InnerStreamFieldId)] ref Stream innerStream)
        {
            return innerStream.CanSeek;
        }

        public static void WriteByte(FileStream aThis, byte aByte)
        {
            throw new NotImplementedException();
        }

    }
}

