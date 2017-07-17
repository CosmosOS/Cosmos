using Cosmos.IL2CPU.API;

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
