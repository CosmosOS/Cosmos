using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public class PS2Keyboard : Keyboard
    {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        public PS2Keyboard()
        {           
        }

        protected override void Initialize()
        {
            INTs.SetIrqHandler(0x01, HandleIRQ);
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

        public override void UpdateLeds()
        {
            // for now, lets not do this..
            //IO.Port60.Byte = 0xED;
            //while ((new IOPort(0x64).Byte & 2) != 0)
            //{
            //}
            //var led_status = (Global.ScrollLock ? 1 : 0) | ((Global.NumLock ? 1 : 0) << 1) | ((Global.CapsLock ? 1 : 0) << 2);
            //IO.Port60.Byte = (byte)led_status;
            //while ((new IOPort(0x64).Byte & 2) != 0)
            //{
            //}
        }

        protected override void HandleScancode(byte aScancode, bool aReleased)
        {
            byte key = aScancode;
            if (key == 0x3A && !aReleased)
            {
                // caps lock
                Global.CapsLock = !Global.CapsLock;
                UpdateLeds();
            }
            else if (key == 0x45 && !aReleased)
            {
                // num lock
                Global.NumLock = !Global.NumLock;
                UpdateLeds();
            }
            else if (key == 0x46 && !aReleased)
            {
                // scroll lock
                Global.ScrollLock = !Global.ScrollLock;
                UpdateLeds();
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
            if (KeyLayout == null)
            {
                Debugger.DoSend("No KeyLayout");
            }
            keyInfo = KeyLayout.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, Global.NumLock, Global.CapsLock, Global.ScrollLock);
            return keyInfo != null;
        }
    }
}
