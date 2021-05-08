using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;

namespace Cosmos.System
{
    /// <summary>
    /// Keyboard manager class. Used to manage keyboard.
    /// </summary>
    public static class KeyboardManager
    {
        /// <summary>
        /// Get and set NumLock.
        /// </summary>
        public static bool NumLock { get; set; }

        /// <summary>
        /// Get and set CapsLock.
        /// </summary>
        public static bool CapsLock { get; set; }

        /// <summary>
        /// Get and set ScrollLock.
        /// </summary>
        public static bool ScrollLock { get; set; }

        /// <summary>
        /// Get and set Ctrl pressed.
        /// </summary>
        public static bool ControlPressed { get; set; }

        /// <summary>
        /// Get and set Shift pressed.
        /// </summary>
        public static bool ShiftPressed { get; set; }

        /// <summary>
        /// Get and set Alt pressed.
        /// </summary>
        public static bool AltPressed { get; set; }

        /// <summary>
        /// Get if queued keys exists.
        /// </summary>
        public static bool KeyAvailable => mQueuedKeys.Count > 0;

        private static List<KeyboardBase> mKeyboardList = new List<KeyboardBase>();
        private static ScanMapBase mScanMap = new US_Standard();
        private static Queue<KeyEvent> mQueuedKeys = new Queue<KeyEvent>();

        /// <summary>
        /// Create new instance of the <see cref="KeyboardManager"/> class.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        static KeyboardManager()
        {
            foreach (var keyboard in HAL.Global.GetKeyboardDevices())
            {
                AddKeyboard(keyboard);
            }
        }

        /// <summary>
        /// Enqueue keyEvent.
        /// </summary>
        /// <param name="keyEvent">KeyEvent to enqueue.</param>
        private static void Enqueue(KeyEvent keyEvent)
        {
            mQueuedKeys.Enqueue(keyEvent);
        }

        /// <summary>
        /// Allow faking scancodes. Used for test kernels
        /// </summary>
        /// <param name="aScancode">A scan code.</param>
        /// <param name="aReleased">Key released.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        internal static void HandleFakeScanCode(byte aScancode, bool aReleased)
        {
            HandleScanCode(aScancode, aReleased);
        }

        /// <summary>
        /// Handle scan code. Used to update LEDs, 
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="aReleased">Key released.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        private static void HandleScanCode(byte aScanCode, bool aReleased)
        {
            Global.mDebugger.Send("KeyboardManager.HandleScanCode");

            byte key = aScanCode;
            if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.CapsLock) && !aReleased)
            {
                // caps lock
                CapsLock = !CapsLock;
                UpdateLeds();
            }
            else if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.NumLock) && !aReleased)
            {
                // num lock
                NumLock = !NumLock;
                UpdateLeds();
            }
            else if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.ScrollLock) && !aReleased)
            {
                // scroll lock
                ScrollLock = !ScrollLock;
                UpdateLeds();
            }
            else if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LCtrl) || mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RCtrl))
            {
                ControlPressed = !aReleased;
            }
            else if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LShift) || mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RShift))
            {
                ShiftPressed = !aReleased;
            }
            else if (mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LAlt) || mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RAlt))
            {
                AltPressed = !aReleased;
            }
            else
            {
                if (!aReleased)
                {
                    KeyEvent keyInfo;

                    if (GetKey(key, out keyInfo))
                    {
                        Enqueue(keyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Update keyboard LEDs.
        /// </summary>
        private static void UpdateLeds()
        {
            foreach (KeyboardBase keyboard in mKeyboardList)
            {
                keyboard.UpdateLeds();
            }
        }

        /// <summary>
        /// Get key pressed.
        /// </summary>
        /// <param name="aScancode">A scan code.</param>
        /// <param name="keyInfo">KeyEvent output.</param>
        /// <returns>bool value.</returns>
        public static bool GetKey(byte aScancode, out KeyEvent keyInfo)
        {
            if (mScanMap == null)
            {
                Global.mDebugger.Send("No KeyLayout");
            }

            keyInfo = mScanMap.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);

            return keyInfo != null;
        }

        /// <summary>
        /// Try read key.
        /// </summary>
        /// <param name="oKey">Output KeyEvent.</param>
        /// <returns>bool value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when queue is empty.</exception>
        public static bool TryReadKey(out KeyEvent oKey)
        {
            if (mQueuedKeys.Count > 0)
            {
                oKey = mQueuedKeys.Dequeue();
                return true;
            }

            oKey = default(KeyEvent);

            return false;
        }

        /// <summary>
        /// Read key.
        /// </summary>
        /// <returns>KeyEvent value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when queue is empty.</exception>
        public static KeyEvent ReadKey()
        {
            while (mQueuedKeys.Count == 0)
            {
                KeyboardBase.WaitForKey();
            }

            return mQueuedKeys.Dequeue();
        }

        /// <summary>
        /// Get key layout.
        /// </summary>
        /// <returns>ScanMapBase value.</returns>
        public static ScanMapBase GetKeyLayout()
        {
            return mScanMap;
        }

        /// <summary>
        /// Set key layout.
        /// </summary>
        /// <param name="aScanMap">A scan map</param>
        public static void SetKeyLayout(ScanMapBase aScanMap)
        {
            if (aScanMap != null)
            {
                mScanMap = aScanMap;
            }
        }

        /// <summary>
        /// Add keyboard
        /// </summary>
        /// <param name="aKeyboard">A keyboard to add.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        private static void AddKeyboard(KeyboardBase aKeyboard)
        {
            Global.mDebugger.Send("KeyboardManager.AddKeyboard");

            aKeyboard.OnKeyPressed = HandleScanCode;
            mKeyboardList.Add(aKeyboard);
        }
    }
}
