//#define COSMOSDEBUG

using System;

using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    internal class PS2Controller : Device
    {
        private enum Command : byte
        {
            GetConfigurationByte = 0x20,
            SetConfigurationByte = 0x60,
            DisableSecondPS2Port = 0xA7,
            EnableSecondPS2Port = 0xA8,
            TestSecondPS2Port = 0xA9,
            TestPS2Controller = 0xAA,
            TestFirstPS2Port = 0xAB,
            DisableFirstPS2Port = 0xAD,
            EnableFirstPS2Port = 0xAE,
            WriteNextByteToSecondPS2PortInputBuffer = 0xD4,
            PulseOutputLineBase = 0xF0
        }

        private enum DeviceCommand : byte
        {
            IdentifyDevice = 0xF2,
            DisableScanning = 0xF5
        }

        [Flags]
        private enum Configuration : byte
        {
            FirstPS2PortInterruptEnabled = 0b0000_0001,
            SecondPS2PortInterruptEnabled = 0b0000_0010,
            /// <summary>
            /// System passed POST.
            /// </summary>
            SystemFlag = 0b0000_0100,
            /// <summary>
            /// Must be zero.
            /// </summary>
            Reserved1 = 0b0000_1000,
            FirstPS2PortClockDisabled = 0b0001_0000,
            SecondPS2PortClockDisabled = 0b0010_0000,
            FirstPS2PortTranslationEnabled = 0b0100_0000,
            /// <summary>
            /// Must be zero.
            /// </summary>
            Reserved2 = 0b1000_0000
        }
        
        [Flags]
        private enum Status : byte
        {
            OutputBufferFull = 0b0000_0001,
            InputBufferFull = 0b0000_0010,
            SystemFlag = 0b0000_0100,
            /// <summary>
            /// 1 for controller, 0 for device.
            /// </summary>
            DataIsForPS2Controller = 0b0000_1000,
            Unknown1 = 0b0001_0000,
            Unknown2 = 0b0010_0000,
            TimeOutError = 0b0100_0000,
            ParityError = 0b1000_0000
        }

        [Flags]
        private enum OutputLines : byte
        {
            None = 0x00,
            Reset = 0x01,
            Second = 0x02,
            Third = 0x04,
            Fourth = 0x08,
            All = Reset | Second | Third | Fourth
        }

        private const byte Ack = 0xFA;
        private const byte Nak = 0xFE;

        private const uint WaitTimeout = 1000;

        private Status CurrentStatus => (Status)IO.Status.Byte;

        private bool CanRead => (CurrentStatus & Status.OutputBufferFull) != 0;
        private bool CanWrite => (CurrentStatus & Status.InputBufferFull) == 0;

        private bool mIsDualChannel;
        private bool mSelfTestPassed;
        private bool mFirstPortTestPassed;
        private bool mSecondPortTestPassed;

        public Device FirstDevice { get; private set; }
        public Device SecondDevice { get; private set; }

        private Core.IOGroup.PS2Controller IO = Core.Global.BaseIOGroups.PS2Controller;
        private Debugger mDebugger = new Debugger(nameof(HAL), nameof(PS2Controller));

        /// <summary>
        /// Initializes the PS/2 controller.
        /// </summary>
        public void Initialize()
        {
            // http://wiki.osdev.org/%228042%22_PS/2_Controller#Initialising_the_PS.2F2_Controller

            // Disable Devices
            SendCommand(Command.DisableFirstPS2Port);
            SendCommand(Command.DisableSecondPS2Port);

            // Flush The Output Buffer
            while (CanRead)
            {
                _ = IO.Data.Byte;
            }

            // Set the Controller Configuration Byte

            var xConfiguration = GetConfiguration();

            // check if the controller is dual channel
            mIsDualChannel = (xConfiguration & Configuration.SecondPS2PortClockDisabled) != 0;

            // TODO: when we support the scan code set 2, clear bit 6 too, to disable translation
            xConfiguration = xConfiguration &
                ~(Configuration.FirstPS2PortInterruptEnabled
                | Configuration.SecondPS2PortInterruptEnabled);

            SetConfiguration(xConfiguration);

            // Perform Controller Self Test
            SendCommand(Command.TestPS2Controller);
            mSelfTestPassed = Read() == 0x55;

            if (!mSelfTestPassed)
            {
                return;
            }

            // Determine If There Are 2 Channels
            // note: at this point, mIsDualChannel may be true and the controller may not be dual channel,
            //       but false means that it's surely not dual channel
            if (mIsDualChannel)
            {
                SendCommand(Command.EnableSecondPS2Port);

                xConfiguration = GetConfiguration();
                mIsDualChannel = (xConfiguration & Configuration.SecondPS2PortClockDisabled) == 0;

                if (mIsDualChannel)
                {
                    SendCommand(Command.DisableSecondPS2Port);
                }
            }

            // Perform Interface Tests
            mFirstPortTestPassed = TestPort(1);

            if (mIsDualChannel)
            {
                mSecondPortTestPassed = TestPort(2);
            }

            // Enable Devices
            if (mFirstPortTestPassed)
            {
                SendCommand(Command.EnableFirstPS2Port);
                // enable interrupt
                xConfiguration |= Configuration.FirstPS2PortInterruptEnabled;
            }

            if (mSecondPortTestPassed)
            {
                SendCommand(Command.EnableSecondPS2Port);
                // enable interrupt
                xConfiguration |= Configuration.SecondPS2PortInterruptEnabled;
            }

            SetConfiguration(xConfiguration);

            if (mFirstPortTestPassed)
            {
                FirstDevice = IdentifyDevice(1);
            }

            if (mSecondPortTestPassed)
            {
                SecondDevice = IdentifyDevice(2);
            }
        }

        /// <summary>
        /// Identifies a PS/2 device.
        /// </summary>
        /// <param name="aPort">The port of the PS/2 device to identify.</param>
        /// <param name="aDevice">An instance of the identified device.</param>
        private Device IdentifyDevice(byte aPort)
        {
            if (aPort == 1 || aPort == 2)
            {
                var xSecondPort = aPort == 2;

                if (!SendDeviceCommand(DeviceCommand.DisableScanning, xSecondPort))
                {
                    return null;
                }

                if (!SendDeviceCommand(DeviceCommand.IdentifyDevice, xSecondPort))
                {
                    return null;
                }

                if (TryRead(out var xFirstByte))
                {
                    /*
                     * |--------|---------------------------|
                     * |  Byte  |  Device Type              |
                     * |--------|---------------------------|
                     * |  0x00  |  Standard PS/2 mouse      |
                     * |--------|---------------------------|
                     * |  0x03  |  Mouse with scroll wheel  |
                     * |--------|---------------------------|
                     * |  0x04  |  5-button mouse           |
                     * |--------|---------------------------|
                     */
                    if (xFirstByte == 0x00 || xFirstByte == 0x03 || xFirstByte == 0x04)
                    {
                        var xDevice = new PS2Mouse(this, aPort, xFirstByte);
                        xDevice.Initialize();

                        return xDevice;
                    }
                    /*
                     * |-----------------|----------------------------------------------------------------|
                     * |  Bytes          |  Device Type                                                   |
                     * |-----------------|----------------------------------------------------------------|
                     * |  0xAB, 0x41     |  MF2 keyboard with translation enabled in the PS/2 Controller  |
                     * |  or 0xAB, 0xC1  |  (not possible for the second PS/2 port)                       |
                     * |-----------------|----------------------------------------------------------------|
                     * |  0xAB, 0x83     |  MF2 keyboard                                                  |
                     * |-----------------|----------------------------------------------------------------|
                     */
                    else if (xFirstByte == 0xAB && TryRead(out var xSecondByte))
                    {
                        // TODO: replace xTest with (xSecondByte == 0x41 || xSecondByte == 0xC1)
                        //       when the stack corruption detection works better for complex conditions.
                        //
                        //       https://github.com/CosmosOS/IL2CPU/issues/8
                        //
                        var xTest = (xSecondByte == 0x41 || xSecondByte == 0xC1);

                        if (xTest && aPort == 1)
                        {
                            var xDevice = new PS2Keyboard(this, aPort);
                            xDevice.Initialize();

                            return xDevice;
                        }
                        else if (xSecondByte == 0x83)
                        {
                            var xDevice = new PS2Keyboard(this, aPort);
                            xDevice.Initialize();

                            return xDevice;
                        }
                        else
                        {
                            mDebugger.SendInternal("(PS/2 Controller) Device detection failed:");
                            mDebugger.SendInternal("First Byte: " + xFirstByte);
                            mDebugger.SendInternal("Second Byte: " + xSecondByte);
                        }
                    }
                    else
                    {
                        mDebugger.SendInternal("(PS/2 Controller) Device detection failed:");
                        mDebugger.SendInternal("Byte: " + xFirstByte);
                    }
                }
                /*
                 * |--------|---------------------------------------------------------------------|
                 * |  Byte  |  Device Type                                                        |
                 * |--------|---------------------------------------------------------------------|
                 * |  None  |  Ancient AT keyboard with translation enabled in the PS/Controller  |
                 * |        |  (not possible for the second PS/2 port)                            |
                 * |--------|---------------------------------------------------------------------|
                 */
                else if (aPort == 1)
                {
                    var xDevice = new PS2Keyboard(this, aPort);
                    xDevice.Initialize();

                    return xDevice;
                }
                else
                {
                    mDebugger.SendInternal("(PS/2 Controller) Device detection failed: no bytes received!");
                }

                return null;
            }
            else
            {
                throw new Exception("(PS/2 Controller) Port " + aPort + " doesn't exist");
            }
        }

        /// <summary>
        /// Tests a PS/2 device port.
        /// </summary>
        /// <param name="aPort">The PS/2 device port to test.</param>
        /// <returns></returns>
        private bool TestPort(byte aPort)
        {
            if (aPort == 1 || aPort == 2)
            {
                if (aPort == 1)
                {
                    SendCommand(Command.TestFirstPS2Port);
                }
                else
                {
                    SendCommand(Command.TestSecondPS2Port);
                }

                if (!TryRead(out var xTestByte))
                {
                    mDebugger.SendInternal("(PS/2 Controller) (Port " + aPort + ") Device test failed: no response received!");
                    return false;
                }

                if (xTestByte == 0x00)
                {
                    return true;
                }
                else
                {
                    string xErrorMessage;

                    /*
                     * |--------|-------------------------|
                     * |  Byte  |  Error                  |
                     * |--------|-------------------------|
                     * |  0x01  |  clock line stuck low   |
                     * |--------|-------------------------|
                     * |  0x02  |  clock line stuck high  |
                     * |--------|-------------------------|
                     * |  0x03  |  data line stuck low    |
                     * |--------|-------------------------|
                     * |  0x04  |  data line stuck high   |
                     * |--------|-------------------------|
                     */
                    switch (xTestByte)
                    {
                        case 0x01:
                            xErrorMessage = "Clock Line Stuck Low";
                            break;
                        case 0x02:
                            xErrorMessage = "Clock Line Stuck High";
                            break;
                        case 0x03:
                            xErrorMessage = "Data Line Stuck Low";
                            break;
                        case 0x04:
                            xErrorMessage = "Data Line Stuck High";
                            break;
                        default:
                            throw new Exception("(PS/2 Controller) (Port " + aPort + ") Device test status unknown: " + xTestByte.ToHex());
                    }

                    mDebugger.Send("(PS/2 Controller) (Port " + aPort + ") Device test error: '" + xErrorMessage + "'");
                    return false;
                }
            }
            else
            {
                throw new Exception("(PS/2 Controller) Port " + aPort + " doesn't exist");
            }
        }

        /// <summary>
        /// Pulses the PS/2 controller's output line.
        /// </summary>
        /// <param name="aOutputLines">The flags which indicate the output lines to pulse.</param>
        private void PulseOutputLine(OutputLines aOutputLines)
        {
            byte xMask = (byte)aOutputLines;

            mDebugger.SendInternal("(PS/2 Controller) Pulsing output line:");
            mDebugger.SendInternal("Mask:");
            mDebugger.SendInternal(xMask);

            WaitToWrite();
            IO.Command.Byte = (byte)((byte)Command.PulseOutputLineBase | xMask);

            mDebugger.SendInternal("Output line pulsed.");
        }

        private Configuration GetConfiguration()
        {
            SendCommand(Command.GetConfigurationByte);
            return (Configuration)Read();
        }

        private void SetConfiguration(Configuration aConfiguration) =>
            SendCommand(Command.SetConfigurationByte, (byte)aConfiguration);

        /// <summary>
        /// Waits for the acknowledgment byte (0xFA). Returns false if the timeout expires, true otherwise.
        /// </summary>
        /// <returns>Returns false if the timeout expires, true otherwise.</returns>
        public bool WaitForAck()
        {
            for (int i = 0; i <= WaitTimeout; i++)
            {
                if (IO.Data.Byte == Ack)
                {
                    return true;
                }
            }

            mDebugger.SendInternal("(PS/2 Controller) Timeout expired in PS2Controller.WaitForAck()");
            return false;
        }

        /// <summary>
        /// Waits for a response, which can be one of the following:
        /// Returns false if the timeout expires, true otherwise.
        /// </summary>
        /// <returns>Returns false if the timeout expires, true otherwise.</returns>
        public byte WaitForResponse()
        {
            if (TryRead(out var xByte))
            {
                return xByte;
            }

            return Nak;
        }

        /// <summary>
        /// Prepares the controller to write a command in the second PS/2 device port.
        /// </summary>
        public void PrepareSecondPortWrite()
        {
            SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
        }

        /// <summary>
        /// Waits for the PS/2 device reset.
        /// </summary>
        /// <returns>Returns true if the device resets successfully, false otherwise.</returns>
        public bool WaitForDeviceReset()
        {
            mDebugger.SendInternal("(PS/2 Controller) Waiting for device reset:");

            if (TryRead(out var xByte))
            {
                mDebugger.SendInternal("(PS/2 Controller) Device reset response byte: " + xByte);

                if (xByte == 0xAA || xByte == 0xFA)
                {
                    mDebugger.SendInternal("(PS/2 Controller) Device reset successful!");
                    return true;
                }
                else if (xByte == 0xFC)
                {
                    mDebugger.SendInternal("(PS/2 Controller) Device reset failed!");
                    return false;
                }
                else
                {
                    mDebugger.SendInternal("(PS/2 Controller) Device reset failed: unexpected byte received!");
                    return false;
                }
            }
            else
            {
                mDebugger.SendInternal("(PS/2 Controller) Device reset failed: device disconnected?");
                return false;
            }
        }

        public byte Read()
        {
            WaitToRead();
            return IO.Data.Byte;
        }

        public bool TryRead(out byte aByte)
        {
            for (int i = 0; i < WaitTimeout; i++)
            {
                if (CanRead)
                {
                    aByte = IO.Data.Byte;
                    return true;
                }
            }

            aByte = 0;
            return false;
        }

        public void WaitToRead()
        {
            var i = 0;

            while (!CanRead)
            {
                i++;

                if (i > WaitTimeout)
                {
                    mDebugger.SendInternal("(PS/2 Controller) Timeout expired while waiting to read!");
                    i = 0;
                }
            }
        }

        public void WaitToWrite()
        {
            var i = 0;

            while (!CanWrite)
            {
                i++;

                if (i > WaitTimeout)
                {
                    mDebugger.SendInternal("(PS/2 Controller) Timeout expired while waiting to write!");
                    i++;
                }
            }
        }

        public bool SendDeviceCommand(byte aCommand, bool aSecondPS2Port, byte? aByte = null, int retries = 3)
        {
            mDebugger.SendInternal("(PS/2 Controller) Sending device command:");
            mDebugger.SendInternal("Device command:");
            mDebugger.SendInternal(aCommand);

            if (aSecondPS2Port)
            {
                SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
            }

            WaitToWrite();
            IO.Data.Byte = aCommand;

            if (WaitForResponse() == Nak)
            {
                if (retries > 0)
                {
                    return SendDeviceCommand(aCommand, aSecondPS2Port, aByte, retries - 1);
                }

                return false;
            }

            mDebugger.SendInternal("Device command sent.");

            if (aByte.HasValue)
            {
                mDebugger.SendInternal("(PS/2 Controller) Sending byte after device command:");
                mDebugger.SendInternal("Byte value:");
                mDebugger.SendInternal(aByte.Value);

                if (aSecondPS2Port)
                {
                    SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
                }

                WaitToWrite();
                IO.Data.Byte = aByte.Value;

                if (WaitForResponse() == Nak)
                {
                    if (retries > 0)
                    {
                        return SendDeviceCommand(aCommand, aSecondPS2Port, aByte, retries - 1);
                    }

                    return false;
                }
            }

            return true;
        }

        private void Write(byte aByte)
        {
            WaitToWrite();
            IO.Data.Byte = aByte;
        }

        private void SendCommand(Command aCommand, byte? aByte = null, int retries = 3)
        {
            mDebugger.SendInternal("(PS/2 Controller) Sending command:");
            mDebugger.SendInternal("Command:");
            mDebugger.SendInternal((byte)aCommand);

            WaitToWrite();
            IO.Command.Byte = (byte)aCommand;

            mDebugger.SendInternal("Command sent.");

            if (aByte.HasValue)
            {
                mDebugger.SendInternal("(PS/2 Controller) Sending byte after command:");
                mDebugger.SendInternal("Byte value:");
                mDebugger.SendInternal(aByte.Value);

                WaitToWrite();
                IO.Data.Byte = aByte.Value;
            }
        }

        private bool SendDeviceCommand(DeviceCommand aCommand,bool aSecondPS2Port, byte? aByte = null) =>
            SendDeviceCommand((byte)aCommand, aSecondPS2Port, aByte);
    }
}
