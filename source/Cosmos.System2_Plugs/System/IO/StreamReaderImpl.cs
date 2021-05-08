#define COSMOSDEBUG
using System;
using System.IO;
using System.Text;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(typeof(StreamReader))]
    public static class StreamReaderImpl
    {
        public static Debugger debugger = new Debugger("", "");
        public static void CheckAsyncTaskInProgress(StreamReader aThis) { }

        [PlugMethod(IsOptional = false)]
        public static string ReadToEnd(StreamWriter aThis, [FieldAccess(Name = "System.Int32 System.IO.StreamReader._charLen")] ref int _charLen,
            [FieldAccess(Name = "System.Int32 System.IO.StreamReader._charPos")] ref int _charPos,
            [FieldAccess(Name = "System.Char[] System.IO.StreamReader._charBuffer")] ref char[] _charBuffer,
            [FieldAccess(Name = "System.Byte[] System.IO.StreamReader._byteBuffer")] ref byte[] _byteBuffer,
            [FieldAccess(Name = "System.IO.Stream System.IO.StreamReader._stream")] ref Stream _stream,
            [FieldAccess(Name = "System.Int32 System.IO.StreamReader._byteLen")] ref int _byteLen,
            [FieldAccess(Name = "System.Text.Decoder System.IO.StreamReader._decoder")] ref Decoder _decoder)
        {
            debugger.Send("-- StreamWriter.ReadToEnd --");
            debugger.Send($"_charLen = {_charLen}");
            debugger.Send($"_charPos = {_charPos}");
            debugger.Send($"_byteLen = {_byteLen}");
            debugger.Send($"aStringBuilder = {new StringBuilder().Append(new char[] { 'a', 'b', 'c', 'd' }, 0, 2)}");
            debugger.Send($"_charBuffer.Length = {_charBuffer.Length}");
            debugger.Send($"_charBuffer[0] = {_charBuffer[0]}");
            var stringBuilder = new StringBuilder(_charLen - _charPos);
            do
            {
                debugger.Send("-- Trying to append StreamWriter.ReadToEnd --");
                debugger.Send($"_charLen = {_charLen}");
                debugger.Send($"_charPos = {_charPos}");
                debugger.Send($"_byteLen = {_byteLen}");
                debugger.Send($"_charBuffer[0] = {_charBuffer[0]}");
                var charCount = _charLen - _charPos;
                debugger.Send($"charCount = {charCount}");
                stringBuilder.Append(_charBuffer, _charPos, charCount);
                debugger.Send($"_stringBuilder string = {stringBuilder}");
                _charPos = _charLen;
                debugger.Send("-- After append StreamWriter.ReadToEnd --");
                debugger.Send($"_charLen = {_charLen}");
                debugger.Send($"_charPos = {_charPos}");
                debugger.Send($"_byteLen = {_byteLen}");
                ReadBuffer(aThis, ref _charLen, ref _charPos, ref _charBuffer, ref _byteBuffer, ref _stream, ref _byteLen, ref _decoder);
                debugger.Send("-- Back in StreamWriter.ReadToEnd --");
                debugger.Send($"_charLen = {_charLen}");
                debugger.Send($"_charPos = {_charPos}");
                debugger.Send($"_byteLen = {_byteLen}");
                debugger.Send($"_charBuffer.Length = {_charBuffer.Length}");            }
            while (_charLen > 0);
            debugger.Send("-- Returning StreamWriter.ReadToEnd --");
            return stringBuilder.ToString();
        }

        [PlugMethod(IsOptional = false)]
        public static int ReadBuffer(StreamWriter aThis, ref int _charLen, ref int _charPos, ref char[] _charBuffer, ref byte[] _byteBuffer, ref Stream _stream, ref int _byteLen, ref Decoder _decoder)
        {
            debugger.Send("-- StreamWriter.ReadBuffer --");
            debugger.Send($"_byteLen = {_byteLen}");
            _charLen = 0;
            _charPos = 0;
            do
            {
                _byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
                debugger.Send("-- After read StreamWriter.ReadBuffer --");
                debugger.Send($"_byteLen = {_byteLen}");
                debugger.Send($"_byteBuffer = {BitConverter.ToString(_byteBuffer)}");
                if (_byteLen == 0)
                {
                    return _charLen;
                }
                _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                debugger.Send("-- After decoder StreamWriter.ReadBuffer --");
                debugger.Send($"_byteBuffer = {BitConverter.ToString(_byteBuffer)}");
                debugger.Send($"_byteLen = {_byteLen}");
                debugger.Send($"_charLen = {_charLen}");
                debugger.Send($"_charBuffer.Length = {_charBuffer.Length}");
            }
            while (_charLen == 0);
            return _charLen;
        }
    }
}
