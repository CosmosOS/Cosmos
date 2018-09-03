//#define COSMOSDEBUG

using System;

using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    /// <summary>
    /// This class describes the PS/2 keyboard.
    /// </summary>
    internal class PS2Keyboard : KeyboardBase
    {
        private enum Command : byte
        {
            SetLEDs = 0xED,
            Echo = 0xEE,
            GetOrSetScanCodeSet = 0xF0,
            EnableScanning = 0xF4,
            DisableScanning = 0xF5,
            Reset = 0xFF
        }

        private readonly PS2Controller mPS2Controller;
        private readonly byte mPS2Port;

        private Core.IOGroup.PS2Controller IO = Core.Global.BaseIOGroups.PS2Controller;
        private Debugger mDebugger = new Debugger(nameof(HAL), nameof(PS2Keyboard));

        public PS2Keyboard(PS2Controller aPS2Controller, byte aPort)
        {
            mPS2Controller = aPS2Controller;
            mPS2Port = aPort;
        }

        public override void Initialize()
        {
            SendCommand(Command.Reset);
            mPS2Controller.WaitForDeviceReset();

            //VMware doesn't support the Get/SetScanCode command
            //mDebugger.SendInternal("(PS/2 Keyboard) Current scan code set: " + GetScanCodeSet());
            //SetScanCodeSet(1);
            //mDebugger.SendInternal("(PS/2 Keyboard) Current scan code set: " + GetScanCodeSet());

            INTs.SetIrqHandler(1, HandleIRQ);

            SendCommand(Command.EnableScanning);

            Global.mDebugger.SendInternal("(PS/2 Keyboard) Initialized");
        }

        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            var xScanCode = IO.Data.Byte;
            var xReleased = (xScanCode & 0x80) == 0x80;

            if (xReleased)
            {
                xScanCode = (byte)(xScanCode ^ 0x80);
            }

            OnKeyPressed?.Invoke(xScanCode, xReleased);
        }

        public override void UpdateLeds(
            bool aScrollLock,
            bool aNumLock,
            bool aCapsLock)
        {
            byte xByte = 0;

            if (aScrollLock)
            {
                xByte |= 0b0001;
            }

            if (aNumLock)
            {
                xByte |= 0b0010;
            }

            if (aCapsLock)
            {
                xByte |= 0b0100;
            }

            // needs testing
            //SendCommand(Command.SetLEDs, xByte);

            Global.mDebugger.SendInternal("(PS/2 Keyboard) Leds updated");
        }

        /// <summary>
        /// Gets the current keyboard scan code set.
        /// </summary>
        /// <returns>Returns the current scan code set.</returns>
        private byte GetScanCodeSet()
        {
            SendCommand(Command.GetOrSetScanCodeSet, 0);
            return mPS2Controller.Read();
        }

        /// <summary>
        /// Sets the scan code set.
        /// </summary>
        /// <param name="aScanCodeSet">The scan code set to set. Can be 1, 2 or 3.</param>
        private void SetScanCodeSet(byte aScanCodeSet)
        {
            switch (aScanCodeSet)
            {
                case 1:
                case 2:
                case 3:
                    SendCommand(Command.GetOrSetScanCodeSet, aScanCodeSet);
                    return;
                default:
                    throw new Exception("(PS/2 Keyboard) Scan code set '" + aScanCodeSet + "' doesn't exist");
            }
        }

        private void SendCommand(Command aCommand, byte? aByte = null) =>
            mPS2Controller.SendDeviceCommand((byte)aCommand, mPS2Port == 2, aByte);
    }
}
