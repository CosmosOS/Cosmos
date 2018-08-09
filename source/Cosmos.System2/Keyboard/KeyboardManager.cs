using System.Collections.Generic;

using Cosmos.HAL;
using Cosmos.System.ScanMaps;

using MyConsole = System.Console;

namespace Cosmos.System
{
    public static class KeyboardManager
    {
        public static bool NumLock { get; set; }
        public static bool CapsLock { get; set; }
        public static bool ScrollLock { get; set; }

        public static bool ControlPressed { get; set; }
        public static bool ShiftPressed { get; set; }
        public static bool AltPressed { get; set; }


        public static bool KeyAvailable => mQueuedKeys.Count > 0;

        private static List<KeyboardBase> mKeyboardList = new List<KeyboardBase>();
        private static ScanMapBase mScanMap = new US_Standard();
        private static Queue<KeyEvent> mQueuedKeys = new Queue<KeyEvent>();

        static KeyboardManager()
        {
            foreach (var keyboard in HAL.Global.GetKeyboardDevices())
            {
                AddKeyboard(keyboard);
            }
        }

        private static void Enqueue(KeyEvent keyEvent)
        {
            mQueuedKeys.Enqueue(keyEvent);
        }

        /// <summary>
        /// Allow faking scancodes. Used for test kernels
        /// </summary>
        internal static void HandleFakeScanCode(byte aScancode, bool aReleased)
        {
            HandleScanCode(aScancode, aReleased);
        }

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
                if (ControlPressed && AltPressed && mScanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.Delete))
                {
                    //Global.Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                    MyConsole.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                    Power.Reboot();
                }

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

        private static void UpdateLeds()
        {
            foreach (KeyboardBase keyboard in mKeyboardList)
            {
                keyboard.UpdateLeds();
            }
        }

        public static bool GetKey(byte aScancode, out KeyEvent keyInfo)
        {
            if (mScanMap == null)
            {
                Global.mDebugger.Send("No KeyLayout");
            }

            keyInfo = mScanMap.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);

            return keyInfo != null;
        }

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

        public static KeyEvent ReadKey()
        {
            while (mQueuedKeys.Count == 0)
            {
                KeyboardBase.WaitForKey();
            }

            return mQueuedKeys.Dequeue();
        }

        public static ScanMapBase GetKeyLayout()
        {
            return mScanMap;
        }

        public static void SetKeyLayout(ScanMapBase aScanMap)
        {
            if (aScanMap != null)
            {
                mScanMap = aScanMap;
            }
        }

        private static void AddKeyboard(KeyboardBase aKeyboard)
        {
            Global.mDebugger.Send("KeyboardManager.AddKeyboard");

            aKeyboard.OnKeyPressed = HandleScanCode;
            mKeyboardList.Add(aKeyboard);
        }
    }
}
