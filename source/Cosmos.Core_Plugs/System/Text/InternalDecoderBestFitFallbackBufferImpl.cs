using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug("System.Text.InternalDecoderBestFitFallbackBuffer, System.Private.CoreLib")]
    public static class InternalDecoderBestFitFallbackBufferImpl
    {
        // See note in EncoderFallbackImpl
        public static object get_InternalSyncObject()
        {
            return new object();
        }
    }
}
