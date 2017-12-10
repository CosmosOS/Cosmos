using IL2CPU.API.Attribs;
using System.Text;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug(TargetName = "System.Text.InternalEncoderBestFitFallback")]
    public static class InternalEncoderBestFitFallbackImpl
    {
        public static EncoderFallbackBuffer CreateFallbackBuffer(object aThis)
        {
            return null;
        }
    }
}
