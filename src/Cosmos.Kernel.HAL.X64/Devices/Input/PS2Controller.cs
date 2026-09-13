// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.HAL2/PS2Controller.cs

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.Devices.Input;

namespace Cosmos.Kernel.HAL.X64.Devices.Input;

/// <summary>
/// PS/2 Controller driver for managing keyboard and mouse devices.
/// </summary>
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

    public const byte Ack = 0xFA;
    public const uint WAIT_TIMEOUT = 100000;

    public bool IsDualChannel;
    public bool SelfTestPassed;
    public bool FirstPortTestPassed;
    public bool SecondPortTestPassed;

    public Device? FirstDevice;
    public Device? SecondDevice;

    /// <summary>
    /// Global PS/2 controller instance.
    /// </summary>
    public static PS2Controller? Instance { get; private set; }

    /// <summary>
    /// Initializes the PS/2 controller.
    /// </summary>
    public void Initialize(bool initScrollWheel = false)
    {
        Serial.WriteString("[PS2Controller] Initializing...\n");
        Instance = this;

        // http://wiki.osdev.org/%228042%22_PS/2_Controller#Initialising_the_PS.2F2_Controller

        // Disable Devices
        SendCommand(Command.DisableFirstPS2Port);
        SendCommand(Command.DisableSecondPS2Port);

        // Flush The Output Buffer
        while (WaitToReadData())
        {
            ReadData();
        }

        // Set the Controller Configuration Byte
        SendCommand(Command.GetConfigurationByte);

        byte configByte = ReadData();
        // Check if the controller is dual channel
        IsDualChannel = (configByte & (1 << 5)) != 0;
        // Clear bits 0 and 1 (disable interrupts during init)
        configByte = (byte)(configByte & ~0b0000_0011);

        SendCommand(Command.SetConfigurationByte, configByte);

        // Perform Controller Self Test
        SendCommand(Command.TestPS2Controller);
        SelfTestPassed = ReadData() == 0x55;

        if (!SelfTestPassed)
        {
            Serial.WriteString("[PS2Controller] Self test FAILED\n");
            return;
        }

        // Determine If There Are 2 Channels
        if (IsDualChannel)
        {
            SendCommand(Command.EnableSecondPS2Port);
            SendCommand(Command.GetConfigurationByte);
            configByte = ReadData();

            IsDualChannel = (configByte & (1 << 5)) == 0;

            if (IsDualChannel)
            {
                SendCommand(Command.DisableSecondPS2Port);
            }
        }

        // Perform Interface Tests
        FirstPortTestPassed = TestPort(1);

        if (IsDualChannel)
        {
            SecondPortTestPassed = TestPort(2);
        }

        // Enable Devices (interrupts are enabled later via EnablePortInterrupt)
        if (FirstPortTestPassed)
        {
            SendCommand(Command.EnableFirstPS2Port);
        }

        if (SecondPortTestPassed)
        {
            SendCommand(Command.EnableSecondPS2Port);
        }

        SendCommand(Command.SetConfigurationByte, configByte);

        if (FirstPortTestPassed)
        {
            IdentifyDevice(1, out FirstDevice, initScrollWheel);
        }

        if (SecondPortTestPassed)
        {
            IdentifyDevice(2, out SecondDevice, initScrollWheel);
        }

        Serial.WriteString("[PS2Controller] Initialization complete\n");
    }

    /// <summary>
    /// Identifies a PS/2 device.
    /// </summary>
    private void IdentifyDevice(byte port, out Device? device, bool initScrollWheel)
    {
        device = null;

        if (port != 1 && port != 2)
        {
            Serial.WriteString("[PS2Controller] Invalid port number\n");
            return;
        }

        bool secondPort = port == 2;

        WaitToWrite();
        SendDeviceCommand(DeviceCommand.DisableScanning, secondPort);

        WaitToWrite();
        SendDeviceCommand(DeviceCommand.IdentifyDevice, secondPort);

        byte firstByte = 0;
        byte secondByte = 0;

        if (ReadByteAfterAckWithTimeout(ref firstByte))
        {
            // Mouse device types: 0x00 (standard), 0x03 (scroll wheel), 0x04 (5-button)
            bool isMouse = firstByte == 0x00 || firstByte == 0x03 || firstByte == 0x04;

            if (isMouse)
            {
                Serial.WriteString("[PS2Controller] Mouse detected on port ");
                Serial.WriteNumber(port);
                Serial.WriteString(" (ID: 0x");
                Serial.WriteHex(firstByte);
                Serial.WriteString(")\n");

                var mouse = new PS2Mouse(port, this);
                mouse.Initialize();
                device = mouse;
            }
            // Keyboard identification: 0xAB followed by 0x41, 0xC1, or 0x83
            else if (firstByte == 0xAB && ReadDataWithTimeout(ref secondByte))
            {
                bool isKeyboard = secondByte == 0x41 || secondByte == 0xC1 || secondByte == 0x83;

                if (isKeyboard)
                {
                    Serial.WriteString("[PS2Controller] Keyboard detected on port ");
                    Serial.WriteNumber(port);
                    Serial.WriteString("\n");

                    var keyboard = new PS2Keyboard(port, this);
                    keyboard.Initialize();
                    device = keyboard;
                }
            }
        }
        // Ancient AT keyboard (no response)
        else if (port == 1)
        {
            Serial.WriteString("[PS2Controller] AT keyboard assumed on port 1\n");
            var keyboard = new PS2Keyboard(port, this);
            keyboard.Initialize();
            device = keyboard;
        }

        if (device == null)
        {
            Serial.WriteString("[PS2Controller] No device detected on port ");
            Serial.WriteNumber(port);
            Serial.WriteString(" (FirstByte: 0x");
            Serial.WriteHex(firstByte);
            Serial.WriteString(", SecondByte: 0x");
            Serial.WriteHex(secondByte);
            Serial.WriteString(")\n");
        }
    }

    /// <summary>
    /// Tests a PS/2 device port.
    /// </summary>
    private bool TestPort(byte port)
    {
        if (port == 1)
        {
            SendCommand(Command.TestFirstPS2Port);
        }
        else if (port == 2)
        {
            SendCommand(Command.TestSecondPS2Port);
        }
        else
        {
            return false;
        }

        byte testByte = ReadData();

        if (testByte == 0x00)
        {
            Serial.WriteString("[PS2Controller] Port ");
            Serial.WriteNumber(port);
            Serial.WriteString(" test passed\n");
            return true;
        }

        Serial.WriteString("[PS2Controller] Port ");
        Serial.WriteNumber(port);
        Serial.WriteString(" test failed: 0x");
        Serial.WriteHex(testByte);
        Serial.WriteString("\n");
        return false;
    }

    /// <summary>
    /// Enables interrupts for a specific PS/2 port.
    /// This should be called AFTER the IRQ handler is registered.
    /// </summary>
    /// <param name="port">The PS/2 port (1 or 2).</param>
    public void EnablePortInterrupt(byte port)
    {
        if (port != 1 && port != 2)
        {
            return;
        }

        // Read current configuration byte
        SendCommand(Command.GetConfigurationByte);
        byte configByte = ReadData();

        // Set the interrupt enable bit for the requested port
        if (port == 1)
        {
            configByte |= 0b01;  // Bit 0: First port interrupt enable
        }
        else
        {
            configByte |= 0b10;  // Bit 1: Second port interrupt enable
        }

        // Write updated configuration byte
        SendCommand(Command.SetConfigurationByte, configByte);

        // Drain any spurious data that might have arrived
        while ((Native.IO.Read8(PS2Ports.Status) & 0x01) != 0)
        {
            Native.IO.Read8(PS2Ports.Data);
        }
    }

    /// <summary>
    /// Unmasks the interrupt for a specific PS/2 port at the I/O APIC level.
    /// Call this AFTER all PS/2 configuration and scanning is enabled.
    /// </summary>
    /// <param name="port">The PS/2 port (1 or 2).</param>
    public void UnmaskPortInterrupt(byte port)
    {
        if (port != 1 && port != 2)
        {
            return;
        }

        // Final drain right before unmask
        while ((Native.IO.Read8(PS2Ports.Status) & 0x01) != 0)
        {
            Native.IO.Read8(PS2Ports.Data);
        }

        byte irq = (byte)(port == 1 ? 1 : 12);

        // Unmask at I/O APIC
        Cosmos.Kernel.Core.X64.Cpu.ApicManager.UnmaskIrq(irq);
    }

    /// <summary>
    /// Waits for the acknowledgement byte (0xFA).
    /// </summary>
    public bool WaitForAck()
    {
        int i = 0;

        while (Native.IO.Read8(PS2Ports.Data) != Ack)
        {
            i++;
            if (i >= WAIT_TIMEOUT)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads the byte after acknowledgement.
    /// </summary>
    public byte ReadByteAfterAck()
    {
        byte data;
        int i = 0;

        do
        {
            data = Native.IO.Read8(PS2Ports.Data);
            i++;
            if (i >= WAIT_TIMEOUT)
            {
                break;
            }
        }
        while (data == Ack);

        return data;
    }

    /// <summary>
    /// Reads the byte after acknowledgement with timeout.
    /// </summary>
    public bool ReadByteAfterAckWithTimeout(ref byte data)
    {
        int i = 0;

        do
        {
            data = Native.IO.Read8(PS2Ports.Data);
            i++;
            if (i >= WAIT_TIMEOUT)
            {
                return false;
            }
        }
        while (data == Ack);

        return true;
    }

    /// <summary>
    /// Prepares the controller to write to the second PS/2 port.
    /// </summary>
    public void PrepareSecondPortWrite()
    {
        SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
    }

    /// <summary>
    /// Waits for the PS/2 device reset.
    /// </summary>
    public bool WaitForDeviceReset()
    {
        WaitToReadData();

        byte data = Native.IO.Read8(PS2Ports.Data);

        if (data == 0xAA)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Waits to read data (output buffer full).
    /// </summary>
    public bool WaitToReadData()
    {
        int i = 0;

        while ((Native.IO.Read8(PS2Ports.Status) & 1) == 0)
        {
            i++;
            if (i >= WAIT_TIMEOUT)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Waits to write data (input buffer empty).
    /// </summary>
    public bool WaitToWrite()
    {
        int i = 0;

        while ((Native.IO.Read8(PS2Ports.Status) & (1 << 1)) != 0)
        {
            i++;
            if (i >= WAIT_TIMEOUT)
            {
                return false;
            }
        }

        return true;
    }

    #region IO

    private byte ReadData()
    {
        WaitToReadData();
        return Native.IO.Read8(PS2Ports.Data);
    }

    private bool ReadDataWithTimeout(ref byte data)
    {
        if (WaitToReadData())
        {
            data = Native.IO.Read8(PS2Ports.Data);
            return true;
        }

        return false;
    }

    private void SendCommand(Command command, byte? data = null)
    {
        WaitToWrite();
        Native.IO.Write8(PS2Ports.Command, (byte)command);

        if (data.HasValue)
        {
            WaitToWrite();
            Native.IO.Write8(PS2Ports.Data, data.Value);
        }
    }

    private void SendDeviceCommand(DeviceCommand command, bool secondPort, byte? data = null)
    {
        if (secondPort)
        {
            SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
        }

        WaitToWrite();
        Native.IO.Write8(PS2Ports.Data, (byte)command);

        WaitForAck();

        if (data.HasValue)
        {
            if (secondPort)
            {
                SendCommand(Command.WriteNextByteToSecondPS2PortInputBuffer);
            }

            WaitToWrite();
            Native.IO.Write8(PS2Ports.Data, data.Value);

            WaitForAck();
        }
    }

    #endregion

    /// <summary>
    /// Gets all keyboard devices connected to this controller.
    /// </summary>
    public static KeyboardDevice[] GetKeyboardDevices()
    {
        if (Instance == null)
        {
            return Array.Empty<KeyboardDevice>();
        }

        bool hasFirst = Instance.FirstDevice is KeyboardDevice;
        bool hasSecond = Instance.SecondDevice is KeyboardDevice;

        if (!hasFirst && !hasSecond)
        {
            return Array.Empty<KeyboardDevice>();
        }

        int count = (hasFirst ? 1 : 0) + (hasSecond ? 1 : 0);
        var keyboards = new KeyboardDevice[count];
        int index = 0;

        if (hasFirst)
        {
            keyboards[index++] = (KeyboardDevice)Instance.FirstDevice!;
        }

        if (hasSecond)
        {
            keyboards[index++] = (KeyboardDevice)Instance.SecondDevice!;
        }

        return keyboards;
    }

    /// <summary>
    /// Gets all mouse devices connected to this controller.
    /// </summary>
    public static MouseDevice[] GetMouseDevices()
    {
        if (Instance == null)
        {
            return Array.Empty<MouseDevice>();
        }

        bool hasFirst = Instance.FirstDevice is MouseDevice;
        bool hasSecond = Instance.SecondDevice is MouseDevice;

        if (!hasFirst && !hasSecond)
        {
            return Array.Empty<MouseDevice>();
        }

        int count = (hasFirst ? 1 : 0) + (hasSecond ? 1 : 0);
        var mice = new MouseDevice[count];
        int index = 0;

        if (hasFirst)
        {
            mice[index++] = (MouseDevice)Instance.FirstDevice!;
        }

        if (hasSecond)
        {
            mice[index++] = (MouseDevice)Instance.SecondDevice!;
        }

        return mice;
    }
}
