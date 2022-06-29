//#define COSMOSDEBUG

using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    /// <summary>
    /// This class describes the PS/2 mouse.
    /// </summary>
    public class PS2Mouse : MouseBase
    {
        enum Command : byte
        {
            SetScaling1_1 = 0xE6,
            SetScaling2_1 = 0xE7,
            SetResolution = 0xE8,
            StatusRequest = 0xE9,
            SetStreamMode = 0xEA,
            RequestSinglePacket = 0xEB,
            ResetWrapMode = 0xEC,
            SetWrapMode = 0xEE,
            SetRemoteMode = 0xF0,
            GetMouseID = 0xF2,
            SetSampleRate = 0xF3,
            EnablePacketStreaming = 0xF4,
            DisablePacketStreaming = 0xF5,
            SetDefaults = 0xF6,
            Resend = 0xFE,
            Reset = 0xFF
        }

        public byte PS2Port { get; }

        private bool HasScrollWheel
        {
            get
            {
                return mMouseID == 3 || mMouseID == 4;
            }
        }

        private Core.IOGroup.PS2Controller IO = Core.Global.BaseIOGroups.PS2Controller;
        private PS2Controller mPS2Controller = Global.PS2Controller;
        private Debugger mDebugger = new Debugger("HAL", "PS2Mouse");

        private byte mMouseID = 0;

        internal PS2Mouse(byte aPort, byte aMouseID)
        {
            PS2Port = aPort;
            mMouseID = aMouseID;
        }

        /// <summary>
        /// This is the required call to start
        /// the mouse receiving interrupts.
        /// </summary>
        public override void Initialize()
        {
            SendCommand(Command.Reset);
            mPS2Controller.WaitForDeviceReset();

            if (mMouseID == 0)
            {
                mMouseID = TryToEnableScrollWheel();

                mDebugger.SendInternal("(PS/2 Mouse) Mouse ID: " + mMouseID);

                if (mMouseID == 3)
                {
                    mMouseID = TryToEnableAdditionalButtons();
                }

                mDebugger.SendInternal("(PS/2 Mouse) Mouse ID: " + mMouseID);
            }

            //SendCommand(Command.SetDefaults);
            //mPS2Controller.WaitForAck();

            INTs.SetIrqHandler(12, HandleMouse);

            SendCommand(Command.EnablePacketStreaming);
            mPS2Controller.WaitForAck();
        }

        /// <summary>
        /// Tries to enable the scroll wheel.
        /// </summary>
        /// <returns>Returns the mouse id.</returns>
        private byte TryToEnableScrollWheel()
        {
            SendCommand(Command.SetSampleRate, 200);
            SendCommand(Command.SetSampleRate, 100);
            SendCommand(Command.SetSampleRate, 80);

            SendCommand(Command.GetMouseID);

            return mPS2Controller.ReadByteAfterAck();
        }

        /// <summary>
        /// Tries to enable additional buttons (buttons 4 and 5).
        /// </summary>
        /// <returns>Returns the mouse id.</returns>
        private byte TryToEnableAdditionalButtons()
        {
            SendCommand(Command.SetSampleRate, 200);
            SendCommand(Command.SetSampleRate, 200);
            SendCommand(Command.SetSampleRate, 80);

            SendCommand(Command.GetMouseID);

            return mPS2Controller.ReadByteAfterAck();
        }

        private byte[] mMouseByte = new byte[4];
        private static byte mMouseCycle = 0;

        public void HandleMouse(ref INTs.IRQContext context)
        {
            if (mMouseCycle == 0)
            {
                mMouseByte[0] = IO.Data.Byte;

                //Bit 3 of byte 0 is 1, then we have a good package
                if ((mMouseByte[0] & (1 << 3)) == (1 << 3))
                {
                    mMouseCycle++;
                }
            }
            else if (mMouseCycle == 1)
            {
                mMouseByte[1] = IO.Data.Byte;
                mMouseCycle++;
            }
            else if (mMouseCycle == 2)
            {
                mMouseByte[2] = IO.Data.Byte;

                if (HasScrollWheel)
                {
                    mMouseCycle++;
                }
            }

            // TODO: move conditions to the if statement when stack corruption detection
            //       works better for complex conditions
            var xTest1 = (mMouseCycle == 2 && !HasScrollWheel);
            var xTest2 = (mMouseCycle == 3 && HasScrollWheel);

            if (xTest1 || xTest2)
            {
                int xDeltaX = 0;
                int xDeltaY = 0;
                int xScrollWheel = 0;
                int xMouseState;

                if ((mMouseByte[0] & (1 << 4)) == (1 << 4))
                {
                    xDeltaX = (mMouseByte[1] | ~0xFF);
                }
                else
                {
                    xDeltaX = mMouseByte[1];
                }

                if ((mMouseByte[0] & (1 << 5)) == 1 << 5)
                {
                    xDeltaY = -(mMouseByte[2] | ~0xFF);
                }
                else
                {
                    xDeltaY = -mMouseByte[2];
                }

                xMouseState = mMouseByte[0] & 0b0000_0111;

                if (HasScrollWheel)
                {
                    var xScrollWheelByte = mMouseByte[3] & 0x0F;
                    xScrollWheel = (xScrollWheelByte & 0b1000) == 0 ? xScrollWheelByte : xScrollWheelByte | ~0x0F;

                    if (mMouseID == 4)
                    {
                        var xAdditionalButtonsByte = mMouseByte[3] & 0b0011_0000;
                        xMouseState |= (xAdditionalButtonsByte >> 1);
                    }
                }


                mDebugger.SendInternal($"(PS/2 Mouse) IRQ 12: Mouse State: ({xDeltaX}, {xDeltaY}, {xMouseState})");

                OnMouseChanged?.Invoke(xDeltaX, xDeltaY, xMouseState, xScrollWheel);
                
                mMouseCycle = 0;
            }
        }
        
        private void SendCommand(Command aCommand, byte? aByte = null)
        {
            mDebugger.SendInternal("(PS/2 Mouse) Sending command:");
            mDebugger.SendInternal("Command:");
            mDebugger.SendInternal((byte)aCommand);

            if (PS2Port == 2)
            {
                mPS2Controller.PrepareSecondPortWrite();
            }

            mPS2Controller.WaitToWrite();
            IO.Data.Byte = (byte)aCommand;

            mPS2Controller.WaitForAck();

            mDebugger.SendInternal("Command sent.");

            if (aByte.HasValue)
            {
                mDebugger.SendInternal("(PS/2 Mouse) Sending byte after command:");
                mDebugger.SendInternal("Byte value:");
                mDebugger.SendInternal(aByte.Value);

                if (PS2Port == 2)
                {
                    mPS2Controller.PrepareSecondPortWrite();
                }

                mPS2Controller.WaitToWrite();
                IO.Data.Byte = aByte.Value;

                mPS2Controller.WaitForAck();
            }
        }
    }
}
