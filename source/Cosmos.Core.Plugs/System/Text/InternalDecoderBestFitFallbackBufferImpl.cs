using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Text
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
