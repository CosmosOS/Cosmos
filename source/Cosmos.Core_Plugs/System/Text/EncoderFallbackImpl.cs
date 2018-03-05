using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Text
{
    [Plug(Target = typeof(EncoderFallback))]
    public static class EncoderFallbackImpl
    {
        // Encoders use this, but we plug their methods anwyays so we just fill empty for now.
        public static object get_InternalSyncObject()
        {
            return new object();
        }

        public static EncoderFallbackBuffer CreateFallbackBuffer(EncoderFallback aThis)
        {
            return null;
        }
    }
}
