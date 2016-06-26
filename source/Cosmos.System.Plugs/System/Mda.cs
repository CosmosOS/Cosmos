using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(TargetName = "System.Mda, mscorlib")]
    public static class Mda
    {
        public static bool IsStreamWriterBufferedDataLostEnabled()
        {
            return false;
        }

        public static bool IsStreamWriterBufferedDataLostCaptureAllocatedCallStack()
        {
            return false;
        }
    }
}
