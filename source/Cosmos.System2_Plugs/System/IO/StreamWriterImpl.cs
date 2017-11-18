#define COSMOSDEBUG
using System;
using System.IO;
using Cosmos.System;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System2.Encoding;
using System.Text;

namespace Cosmos.System_Plugs.System.IO
{

    [Plug(Target = typeof(StreamWriter))]
    public static class StreamWriterImpl
    {
        private const string StreamFieldId = "System.IO.Stream System.IO.StreamWriter._stream";
        private const string CharPosFieldId = "System.Int32 System.IO.StreamWriter._charPos";
        private const string CharLenFieldId = "System.Int32 System.IO.StreamWriter._charLen";
        private const string CharBufferFieldId = "System.Char[] System.IO.StreamWriter._charBuffer";
        private const string ByteBufferFieldId = "System.Byte[] System.IO.StreamWriter._byteBuffer";

        private static CosmosEncoding FileEncoding;
        //private static Stream InnerStream;

        private static void Init(String path, bool append, ref Stream stream, CosmosEncoding encoding,
                                 ref char[] charBuffer, int bufferSize, ref byte[] byteBuffer, bool shouldLeaveOpen,
                                 ref char[] CoreNewLine)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException("Empty path");
            }

            Global.mFileSystemDebugger.SendInternal($"StreamWriter.Init() with path {path} append {append} bufferSize {bufferSize}");
            stream = new FileStream(path, append ? FileMode.Append : FileMode.Create);
            FileEncoding = encoding;
            charBuffer = new char[bufferSize];
            byteBuffer = new byte[FileEncoding.GetMaxByteCount(bufferSize)];
            CoreNewLine = new char[] { '\n', '\r' };
        }

        /*
         * This constructor is really plugged only to enforce our simplified version of UTF8 encoding using other
         * enconding will be silently ignored (I liked to check for this and throw Exception but == operator
         * does not work with Encoding neither Equals)
         */
        public static void Ctor(StreamWriter aThis, string path, bool append, Encoding encoding, int bufferSize,
                                [FieldAccess(Name = StreamFieldId)] ref Stream _stream,
                                [FieldAccess(Name = CharPosFieldId)] ref int _charPos,
                                [FieldAccess(Name = CharLenFieldId)] ref int _charLen,
                                [FieldAccess(Name = CharBufferFieldId)] ref char[] _charBuffer,
                                [FieldAccess(Name = ByteBufferFieldId)] ref byte[] _byteBuffer,
                                [FieldAccess(Name = "System.Char[] System.IO.TextWriter.CoreNewLine")] ref char[] CoreNewLine
                                )
        {
#if false
            //if (!Equals(encoding, Encoding.UTF8))
            if (!encoding.Equals(Encoding.UTF8))
                throw new NotImplementedException("Only UFT8 Encoding implemented");
#endif

            Global.mFileSystemDebugger.SendInternal($"StreamWriter.Ctor() with path {path} append {append} Encoding and bufferSize {bufferSize}");
            Init(path, append, ref _stream, new CosmosUTF8Encoding(), ref _charBuffer, _charLen = bufferSize, ref _byteBuffer, false, ref CoreNewLine);
        }

        public static void Flush(StreamWriter aThis, bool flushStream, bool flushEncoder,
            [FieldAccess(Name = StreamFieldId)] ref Stream _stream,
            [FieldAccess(Name = CharPosFieldId)] ref int _charPos,
            [FieldAccess(Name = CharBufferFieldId)] ref char[] _charBuffer,
            [FieldAccess(Name = ByteBufferFieldId)] ref byte[] _byteBuffer
            )
        {
            if (_stream == null)
            {
                throw new ObjectDisposedException(null, "Object already disposed");
            }
            if (_charPos == 0 && !flushStream && !flushEncoder)
            {
                return;
            }

            if (_charBuffer == null)
                return;
            if (_charBuffer.Length == 0)
                return;

            int numBytes = FileEncoding.GetBytes(_charBuffer, 0, _charPos, _byteBuffer, 0);
            _charPos = 0;

            if (numBytes > 0)
            {
                Global.mFileSystemDebugger.SendInternal($"numBytes is {numBytes} doing Write...");
               _stream.Write(_byteBuffer, 0, numBytes);
                Global.mFileSystemDebugger.SendInternal("Write done!");
            }

            Global.mFileSystemDebugger.SendInternal("Flush() ended");

            //_stream.Flush();
        }

        public static void Cctor()
        {
        }
    }
}
