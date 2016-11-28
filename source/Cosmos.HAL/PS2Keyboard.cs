using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public class PS2Keyboard : KeyboardBase
    {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        public PS2Keyboard() : base()
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
            OnKeyPressed?.Invoke(xScanCode, xReleased);
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
    }
}
