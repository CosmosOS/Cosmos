using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;

namespace Cosmos.System
{
    public class Keyboard
    {
        public bool NumLock
        {
            get;
            set;
        }

        public bool CapsLock
        {
            get;
            set;
        }

        public bool ScrollLock
        {
            get;
            set;
        }

        public bool ControlPressed
        {
            get;
            set;
        }

        public bool ShiftPressed
        {
            get;
            set;
        }

        public bool AltPressed
        {
            get;
            set;
        }

        protected KeyboardBase _keyboard;
        protected ScanMapBase _scanMap;

        protected Queue<KeyEvent> mQueuedKeys;

        public Keyboard(KeyboardBase Keyboard, ScanMapBase ScanMap)
        {
            _keyboard = Keyboard;
            _keyboard.OnKeyPressed += new KeyboardBase.KeyPressedEventHandler(HandleScanCode);

            if(ScanMap == null)
            {
                ScanMap = new US_Standard();
            }
            _scanMap = ScanMap;

            mQueuedKeys = new Queue<KeyEvent>();
        }

        protected void Enqueue(KeyEvent keyEvent)
        {
            mQueuedKeys.Enqueue(keyEvent);
        }

        /// <summary>
        /// Allow faking scancodes. Used for test kernels
        /// </summary>
        internal void HandleFakeScanCode(byte aScancode, bool aReleased)
        {
            HandleScanCode(aScancode, aReleased);
        }

        protected void HandleScanCode(byte aScanCode, bool aReleased)
        {
            byte key = aScanCode;
            if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.CapsLock) && !aReleased)
            {
                // caps lock
                CapsLock = !CapsLock;
                _keyboard.UpdateLeds();
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.NumLock) && !aReleased)
            {
                // num lock
                NumLock = !NumLock;
                _keyboard.UpdateLeds();
            }
            else if (_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.ScrollLock) && !aReleased)
            {
                // scroll lock
                ScrollLock = !ScrollLock;
                _keyboard.UpdateLeds();
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
                    Global.Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
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

        public bool GetKey(byte aScancode, out KeyEvent keyInfo)
        {
            if (_scanMap == null)
            {
                Debugger.DoSend("No KeyLayout");
            }
            keyInfo = _scanMap.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);
            return keyInfo != null;
        }

        public bool TryReadKey(out KeyEvent oKey)
        {
            if (mQueuedKeys.Count > 0)
            {
                oKey = mQueuedKeys.Dequeue();
                return true;
            }
            oKey = default(KeyEvent);
            return false;
        }

        public KeyEvent ReadKey()
        {
            while (mQueuedKeys.Count == 0)
            {
                _keyboard.WaitForKey();
            }
            return mQueuedKeys.Dequeue();
        }

        public ScanMapBase GetKeyLayout()
        {
            return _scanMap;
        }

        public void SetKeyLayout(ScanMapBase ScanMap)
        {
            _scanMap = ScanMap;
        }
    }
}
