using Cosmos.IL2CPU.Plugs;
using System.Text;

namespace Cosmos.System.Plugs.System.Text
{
    [Plug(Target = typeof(DecoderFallback))]
    public static class DecoderFallbackImpl
    {
        // See note in EncoderFallbackImpl
        public static object get_InternalSyncObject()
        {
            return new object();
        }
    }
}