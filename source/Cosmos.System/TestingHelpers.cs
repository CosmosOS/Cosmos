using Cosmos.Debug.Kernel;

namespace Cosmos.System
{
    // This class exists purely for testing purposes.
    internal static class TestingHelpers
    {
        internal static void KeyboardAddFakeScanCode(byte aScanCode, bool aReleased)
        {
            Debugger.DoSend("Before HandleFakeScanCode");
            if (HAL.Global.Keyboard == null)
            {
                Debugger.DoSend("No Keyboard set!");
            }
            HAL.Global.Keyboard.HandleFakeScanCode(aScanCode, aReleased);
            Debugger.DoSend("After HandleFakeScanCode");
        }
    }
}
