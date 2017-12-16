using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug(TargetName = "System.Text.InternalDecoderBestFitFallbackBuffer")]
    public static class InternalDecoderBestFitFallbackBufferImpl
    {
        // See note in EncoderFallbackImpl
        public static object get_InternalSyncObject()
        {
            return new object();
        }
    }
}
