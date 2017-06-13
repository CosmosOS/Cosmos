using Cosmos.Debug.Kernel;

namespace Cosmos.System
{
    // This class exists purely for testing purposes.
    internal static class TestingHelpers
    {
        internal static void KeyboardAddFakeScanCode(byte aScanCode, bool aReleased)
        {
            Global.mDebugger.Send("Before HandleFakeScanCode");
            KeyboardManager.HandleFakeScanCode(aScanCode, aReleased);
            Global.mDebugger.Send("After HandleFakeScanCode");
        }
    }
}
