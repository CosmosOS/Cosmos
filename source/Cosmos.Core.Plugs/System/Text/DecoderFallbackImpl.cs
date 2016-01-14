using System.Text;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Text
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
