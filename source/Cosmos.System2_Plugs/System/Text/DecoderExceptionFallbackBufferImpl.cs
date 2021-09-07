using System;
using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Text
{
    [Plug(Target = typeof(DecoderExceptionFallbackBuffer))]
    public static class DecoderExceptionFallbackBufferImpl
    { 
        public static bool Fallback(DecoderExceptionFallbackBuffer aThis, byte[] bytesUnknown, int index)
        {
            throw new NotImplementedException("DecoderExceptionFallbackBuffer Fallback()");
        }
    }
}
