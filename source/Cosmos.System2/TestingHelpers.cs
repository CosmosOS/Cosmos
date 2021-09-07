using sysIO = System.IO;
using Cosmos.Debug.Kernel;

namespace Cosmos.System
{
    // This class exists purely for testing purposes.
    /// <summary>
    /// Testing helpers class.
    /// </summary>
    internal static class TestingHelpers
    {
        /// <summary>
        /// Add fake scan codes to the keyboard, fake pressing keys.
        /// Used to test kernals.
        /// </summary>
        /// <param name="aScanCode">A key code.</param>
        /// <param name="aReleased">Is key pressed.</param>
        /// <exception cref="sysIO.IOException">An I/O error occurred.</exception>
        internal static void KeyboardAddFakeScanCode(byte aScanCode, bool aReleased)
        {
            Global.mDebugger.Send("Before HandleFakeScanCode");
            KeyboardManager.HandleFakeScanCode(aScanCode, aReleased);
            Global.mDebugger.Send("After HandleFakeScanCode");
        }
    }
}
