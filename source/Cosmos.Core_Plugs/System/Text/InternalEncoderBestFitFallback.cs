using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug("System.Text.InternalEncoderBestFitFallback, System.Private.CoreLib")]
    public static class InternalEncoderBestFitFallbackImpl
    {
        public static EncoderFallbackBuffer CreateFallbackBuffer(object aThis)
        {
            return null;
        }
    }
}
