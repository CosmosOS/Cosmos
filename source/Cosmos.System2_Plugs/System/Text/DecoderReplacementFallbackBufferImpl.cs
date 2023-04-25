using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System.Text;

namespace Cosmos.System_Plugs.System.Text
{
    [Plug(Target = typeof(DecoderReplacementFallbackBuffer))]
    public static class DecoderReplacementFallbackBufferImpl
    {
        public static bool Fallback(DecoderReplacementFallbackBuffer aThis, byte[] bytesUnknown, int index)
        {
            Debugger debugger = new("Decoder");
            debugger.Send("Fallback NotImplemented!!!");
            Debugger.DoBochsBreak();
            throw new NotImplementedException("DecoderExceptionFallbackBuffer Fallback()");
        }
    }
}