namespace Cosmos.System
{
    // This class exists purely for testing purposes.
    internal static class TestingHelpers
    {
        internal static void KeyboardAddFakeScanCode(byte aScanCode, bool aReleased)
        {
            HAL.Global.Keyboard.HandleFakeScanCode(aScanCode, aReleased);
        }
    }
}
