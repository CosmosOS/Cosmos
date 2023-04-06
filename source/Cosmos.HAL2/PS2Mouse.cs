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
                return _MouseID == 3 || _MouseID == 4;
            }
        }

        private readonly PS2Controller _PS2Controller = Global.PS2Controller;
        private readonly Debugger _Debugger = new("HAL", "PS2Mouse");

        private byte _MouseID = 0;

        internal PS2Mouse(byte aPort, byte aMouseID)
        {
            PS2Port = aPort;
            _MouseID = aMouseID;
        }

        /// <summary>
        /// This is the required call to start
        /// the mouse receiving interrupts.
        /// </summary>
        public override void Initialize()
        {
            SendCommand(Command.Reset);
            _PS2Controller.WaitForDeviceReset();

            if (_MouseID == 0)
            {
                _MouseID = TryToEnableScrollWheel();

                _Debugger.SendInternal("(PS/2 Mouse) Mouse ID: " + _MouseID);

                if (_MouseID == 3)
                {
                    _MouseID = TryToEnableAdditionalButtons();
                }

                _Debugger.SendInternal("(PS/2 Mouse) Mouse ID: " + _MouseID);
            }

            //SendCommand(Command.SetDefaults);
            //mPS2Controller.WaitForAck();

            INTs.SetIrqHandler(12, HandleMouse);

            SendCommand(Command.EnablePacketStreaming);
            _PS2Controller.WaitForAck();
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

            return _PS2Controller.ReadByteAfterAck();
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

            return _PS2Controller.ReadByteAfterAck();
        }

        private readonly byte[] _MouseByte = new byte[4];
        private static byte _MouseCycle = 0;

        public void HandleMouse(ref INTs.IRQContext context)
        {
            if (_MouseCycle == 0)
            {
                _MouseByte[0] = IOPort.Read8(Cosmos.Core.IOGroup.PS2Controller.Data);

                //Bit 3 of byte 0 is 1, then we have a good package
                if ((_MouseByte[0] & (1 << 3)) == 1 << 3)
                {
                    _MouseCycle++;
                }
            }
            else if (_MouseCycle == 1)
            {
                _MouseByte[1] = IOPort.Read8(Cosmos.Core.IOGroup.PS2Controller.Data);
                _MouseCycle++;
            }
            else if (_MouseCycle == 2)
            {
                _MouseByte[2] = IOPort.Read8(Cosmos.Core.IOGroup.PS2Controller.Data);

                if (HasScrollWheel)
                {
                    _MouseCycle++;
                }
            }

            // TODO: move conditions to the if statement when stack corruption detection
            //       works better for complex conditions
            var xTest1 = _MouseCycle == 2 && !HasScrollWheel;
            var xTest2 = _MouseCycle == 3 && HasScrollWheel;

            if (xTest1 || xTest2)
            {
                int xDeltaX = 0;
                int xDeltaY = 0;
                int xScrollWheel = 0;

                if ((_MouseByte[0] & (1 << 4)) == 1 << 4)
                {
                    xDeltaX = _MouseByte[1] | ~0xFF;
                }
                else
                {
                    xDeltaX = _MouseByte[1];
                }

                if ((_MouseByte[0] & (1 << 5)) == 1 << 5)
                {
                    xDeltaY = -(_MouseByte[2] | ~0xFF);
                }
                else
                {
                    xDeltaY = -_MouseByte[2];
                }

                var xMouseState = _MouseByte[0] & 0b0000_0111;

                if (HasScrollWheel)
                {
                    var xScrollWheelByte = _MouseByte[3] & 0x0F;
                    xScrollWheel = (xScrollWheelByte & 0b1000) == 0 ? xScrollWheelByte : xScrollWheelByte | ~0x0F;

                    if (_MouseID == 4)
                    {
                        var xAdditionalButtonsByte = _MouseByte[3] & 0b0011_0000;
                        xMouseState |= xAdditionalButtonsByte >> 1;
                    }
                }

                OnMouseChanged?.Invoke(xDeltaX, xDeltaY, xMouseState, xScrollWheel);

                _MouseCycle = 0;
            }
        }

        private void SendCommand(Command aCommand, byte? aByte = null)
        {
            _Debugger.SendInternal("(PS/2 Mouse) Sending command:");
            _Debugger.SendInternal("Command:");
            _Debugger.SendInternal((byte)aCommand);

            if (PS2Port == 2)
            {
                _PS2Controller.PrepareSecondPortWrite();
            }

            _PS2Controller.WaitToWrite();
            IOPort.Write8(Cosmos.Core.IOGroup.PS2Controller.Data, (byte)aCommand);

            _PS2Controller.WaitForAck();

            _Debugger.SendInternal("Command sent.");

            if (aByte.HasValue)
            {
                _Debugger.SendInternal("(PS/2 Mouse) Sending byte after command:");
                _Debugger.SendInternal("Byte value:");
                _Debugger.SendInternal(aByte.Value);

                if (PS2Port == 2)
                {
                    _PS2Controller.PrepareSecondPortWrite();
                }

                _PS2Controller.WaitToWrite();
                IOPort.Write8(Cosmos.Core.IOGroup.PS2Controller.Data, aByte.Value);

                _PS2Controller.WaitForAck();
            }
        }
    }
}
