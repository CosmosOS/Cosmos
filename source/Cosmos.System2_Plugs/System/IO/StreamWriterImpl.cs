#define COSMOSDEBUG
using System;
using System.IO;
using Cosmos.System;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System2.Encoding;

namespace Cosmos.System_Plugs.System.IO
{

    [Plug(Target = typeof(StreamWriter))]
    public static class StreamWriterImpl
    {

#if false
        public static void Ctor(StreamWriter aThis, string path)
        {
            throw new NotImplementedException("StreamWriter Ctor(String path)");
        }
#endif
        private static CosmosEncoding FileEncoding;
        //private static Stream InnerStream;

#if false
        private static void Init(Stream stream, CosmosEncoding encoding)
        {
            //InnerStream = stream;
            FileEncoding = encoding;
        }
#endif
        private static void Init(CosmosEncoding encoding, ref char[] charBuffer, int bufferSize, ref byte[] byteBuffer, bool shouldLeaveOpen)
        {
            FileEncoding = encoding;
            charBuffer = new char[bufferSize];
            byteBuffer = new byte[bufferSize * FileEncoding.GetMaxByteCount(bufferSize)];
        }

        public static void Ctor(StreamWriter aThis, string path,
                         [FieldAccess(Name = "System.IO.Stream System.IO.StreamWriter._stream")] ref Stream _stream,
                         [FieldAccess(Name = "System.Int32 System.IO.StreamWriter._charPos")] ref int _charPos,
                         [FieldAccess(Name = "System.Char[] System.IO.StreamWriter._charBuffer")] ref char[] _charBuffer,
                         [FieldAccess(Name = "System.Byte[] System.IO.StreamWriter._byteBuffer")] ref byte[] _byteBuffer
                         )
        {
            Global.mFileSystemDebugger.SendInternal($"StreamWriter.Ctor() with path {path}");
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException("Empty path");
            }

            _stream = new FileStream(path, FileMode.Create);
            Init(new CosmosUTF8Encoding(), ref _charBuffer, 128, ref _byteBuffer, false);
        }

        public static void Flush(StreamWriter aThis, bool flushStream, bool flushEncoder,
            [FieldAccess(Name = "System.IO.Stream System.IO.StreamWriter._stream")] ref Stream _stream,
            [FieldAccess(Name = "System.Int32 System.IO.StreamWriter._charPos")] ref int _charPos,
            [FieldAccess(Name = "System.Char[] System.IO.StreamWriter._charBuffer")] ref char[] _charBuffer,
            [FieldAccess(Name = "System.Byte[] System.IO.StreamWriter._byteBuffer")] ref byte[] _byteBuffer
            )
        {
            Global.mFileSystemDebugger.SendInternal($"_charPos is {_charPos}");
            // Debug code why is not working?
            if (_charBuffer == null)
                Global.mFileSystemDebugger.SendInternal("_charBuffer is NULL!");
            else if (_charBuffer.Length == 0)
                Global.mFileSystemDebugger.SendInternal("_charBuffer is Empty!");
            /* First 4 chars should be '0', '1', '2' and '3' */
            else
            {
                Global.mFileSystemDebugger.SendInternal("Printing first 4 chars of _charBuffer: ");
                Global.mFileSystemDebugger.SendInternal(_charBuffer[0]);
                Global.mFileSystemDebugger.SendInternal(_charBuffer[1]);
                Global.mFileSystemDebugger.SendInternal(_charBuffer[2]);
                Global.mFileSystemDebugger.SendInternal(_charBuffer[3]);
            }


#if false
            Global.mFileSystemDebugger.SendInternal($"StreamWriter.Flush() with _charPos {_charPos} and _charBuffer{new String(_charBuffer)}");

            FileEncoding.GetBytes(new string(_charBuffer));
            _stream.Write(_byteBuffer, 0, _byteBuffer.Length);
            _stream.Flush();
#endif
                throw new NotImplementedException("Flush()");
        }

        public static void Cctor()
        {
        }
    }

}
