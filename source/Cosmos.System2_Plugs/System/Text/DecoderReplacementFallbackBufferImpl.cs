using System;
using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Text
{
    [Plug(Target = typeof(DecoderReplacementFallbackBuffer))]
    public static class DecoderReplacementFallbackBufferImpl
    { 
        public static bool Fallback(DecoderReplacementFallbackBuffer aThis, byte[] bytesUnknown, int index)
        {
            throw new NotImplementedException("DecoderExceptionFallbackBuffer Fallback()");
        }
    }
}
