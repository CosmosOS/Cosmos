using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;

using MyConsole = System.Console;

namespace Cosmos.System
{
    public static class KeyboardManager
    {

        public static bool NumLock
        {
            get;
            set;
        }

        public static bool CapsLock
        {
            get;
            set;
        }

        public static bool ScrollLock
        {
            get;
            set;
        }

        public static bool ControlPressed
        {
            get;
            set;
        }

        public static bool ShiftPressed
        {
            get;
            set;
        }

        public static bool AltPressed
        {
            get;
            set;
        }
 
 
        public static bool KeyAvailable
        {
            get
            {
                return mQueuedKeys.Count > 0;
            }
        }

public static List<KeyboardBase> Keyboards = new List<KeyboardBase>();

        private static ScanMapBase _scanMap = new US_Standard();
        private static Queue<KeyEvent> mQueuedKeys = new Queue<KeyEvent>();

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
            byte key = aScanCode;
            if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.CapsLock) && !aReleased)
            {
                // caps lock
                CapsLock = !CapsLock;
                UpdateLeds();
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.NumLock) && !aReleased)
            {
                // num lock
                NumLock = !NumLock;
                UpdateLeds();
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.ScrollLock) && !aReleased)
            {
                // scroll lock
                ScrollLock = !ScrollLock;
                UpdateLeds();
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LCtrl) || _scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RCtrl))
            {
                ControlPressed = !aReleased;
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LShift) || _scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RShift))
            {
                ShiftPressed = !aReleased;
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LAlt) || _scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RAlt))
            {
                AltPressed = !aReleased;
            }
            else
            {
                if (ControlPressed && AltPressed && _scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.Delete))
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
            foreach(KeyboardBase keyboard in Keyboards)
            {
                keyboard.UpdateLeds();
            }
        }

        public static bool GetKey(byte aScancode, out KeyEvent keyInfo)
        {
            if (_scanMap == null)
            {
                Global.mDebugger.Send("No KeyLayout");
            }

            keyInfo = _scanMap.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);

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
            return _scanMap;
        }

        public static void SetKeyLayout(ScanMapBase ScanMap)
        {
            if (ScanMap != null)
            {
                _scanMap = ScanMap;
            }
        }

        public static void AddKeyboard(KeyboardBase Keyboard)
        {
            //if (!KeyboardExists(Keyboard.GetType()))
            //{
                Keyboard.OnKeyPressed = HandleScanCode;
                Keyboards.Add(Keyboard);
            //}
        }

        //public static bool KeyboardExists(Type KeyboardType)
        //{
        //    foreach (KeyboardBase Keyboard in Keyboards)
        //    {
        //        if (Keyboard.GetType() == KeyboardType)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}
