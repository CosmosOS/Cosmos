using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;

namespace Cosmos.HAL
{
    public class PS2Keyboard : Keyboard
    {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        public PS2Keyboard()
        {
            INTs.SetIrqHandler(0x01, HandleIRQ);
        }

        protected override void Initialize()
        {
        }

        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            byte xScanCode = IO.Port60.Byte;
            bool xReleased = (xScanCode & 0x80) == 0x80;
            if (xReleased)
            {
                xScanCode = (byte)(xScanCode ^ 0x80);
            }
            HandleScancode(xScanCode, xReleased);
        }

        private void updateLed()
        {
            IO.Port60.Byte = 0xED;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
            var led_status = (ScrollLock ? 1 : 0) | ((NumLock ? 1 : 0) << 1) | ((CapsLock ? 1 : 0) << 2);
            IO.Port60.Byte = (byte)led_status;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
        }

        protected override void HandleScancode(byte aScancode, bool aReleased)
        {
            byte key = aScancode;
            if (key == 0x3A && !aReleased)
            {
                // caps lock
                CapsLock = !CapsLock;
                updateLed();
            }
            else if (key == 0x45 && !aReleased)
            {
                // num lock
                NumLock = !NumLock;
                updateLed();
            }
            else if (key == 0x46 && !aReleased)
            {
                // scroll lock
                ScrollLock = !ScrollLock;
                updateLed();
            }
            else
                switch (key)
                {
                    case 0x1D:
                        {
                            ControlPressed = !aReleased;
                            break;
                        }
                    case 0x2A:
                    case 0x36:
                        {
                            ShiftPressed = !aReleased;
                            break;
                        }
                    case 0x38:
                        {
                            AltPressed = !aReleased;
                            break;
                        }
                    default:
                        {
                            if (ControlPressed && AltPressed && (key == 0x53))
                            {
                                Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                                Core.Global.CPU.Reboot();
                            }

                            if (!aReleased)
                            {
                                KeyEvent keyInfo;
                                if (GetKey(key, aReleased, out keyInfo))
                                {
                                    Enqueue(keyInfo);
                                }
                            }


                            break;
                        }
                }
        }

        public bool GetKey(byte aScancode, bool released, out KeyEvent keyInfo)
        {
            keyInfo = KeyLayout.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);
            return keyInfo != null;
        }
    }
}
