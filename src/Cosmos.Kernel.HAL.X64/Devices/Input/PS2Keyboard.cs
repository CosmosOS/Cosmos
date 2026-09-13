// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.HAL2/PS2Keyboard.cs

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Input;

namespace Cosmos.Kernel.HAL.X64.Devices.Input;

/// <summary>
/// PS/2 Keyboard driver.
/// </summary>
internal class PS2Keyboard : KeyboardDevice
{
    private enum Command : byte
    {
        SetLEDs = 0xED,
        GetOrSetScanCodeSet = 0xF0,
        EnableScanning = 0xF4,
        DisableScanning = 0xF5,
        Reset = 0xFF
    }

    // Static reference to the first keyboard instance (for IRQ handler)
    private static PS2Keyboard? s_instance;

    // Flag to prevent multiple IRQ registrations
    private static bool s_irqRegistered;

    /// <summary>
    /// Registers IRQ handler for keyboard interrupts.
    /// Called explicitly from Kernel.Initialize to ensure proper initialization order.
    /// </summary>
    public static void RegisterIRQHandler()
    {
        if (PS2Controller.Instance == null)
        {
            Serial.WriteString("[PS2Keyboard] WARNING: PS2Controller instance is null!\n");
            return;
        }

        // Drain any pending data from buffer
        DrainBuffer();

        // Register the IRQ handler and route through APIC (start masked to prevent spurious interrupts)
        InterruptManager.SetIrqHandler(1, StaticHandleIRQ, startMasked: true);

        // Enable keyboard scanning
        PS2Controller.Instance.WaitToWrite();
        Native.IO.Write8(PS2Ports.Data, 0xF4);  // Enable scanning command
        PS2Controller.Instance.WaitForAck();

        // Drain any remaining data after ACK
        while ((Native.IO.Read8(PS2Ports.Status) & 0x01) != 0)
        {
            Native.IO.Read8(PS2Ports.Data);
        }

        // Enable PS/2 controller IRQ generation (config bit 0)
        PS2Controller.Instance.EnablePortInterrupt(1);

        // Unmask IRQ1 at I/O APIC - ready for keypresses
        PS2Controller.Instance.UnmaskPortInterrupt(1);

        Serial.WriteString("[PS2Keyboard] Keyboard ready for input\n");
    }

    /// <summary>
    /// The PS/2 port this keyboard is connected to (1 or 2).
    /// </summary>
    public byte PS2Port { get; }

    private readonly PS2Controller _controller;

    /// <summary>
    /// Creates a new PS/2 keyboard instance.
    /// </summary>
    /// <param name="port">The PS/2 port (1 or 2).</param>
    /// <param name="controller">The PS/2 controller managing this keyboard.</param>
    internal PS2Keyboard(byte port, PS2Controller controller)
    {
        PS2Port = port;
        _controller = controller;
    }

    /// <summary>
    /// Initializes the keyboard.
    /// </summary>
    public override void Initialize()
    {
        Serial.WriteString("[PS2Keyboard] Initializing on port ");
        Serial.WriteNumber(PS2Port);
        Serial.WriteString("...\n");


        Serial.WriteString("[PS2Keyboard] Resetting keyboard...\n");
        // Reset keyboard
        SendCommand(Command.Reset);
        _controller.WaitForDeviceReset();

        // NOTE: Don't enable scanning here - it will be done in RegisterIRQHandler()
        // after the IOAPIC is properly configured. Enabling scanning now would
        // cause spurious edges at the IOAPIC before we're ready to handle them.

        Serial.WriteString("[PS2Keyboard] Initialized (scanning will be enabled later)\n");

        // Store first keyboard instance for IRQ handler
        if (s_instance == null)
        {
            s_instance = this;
        }

        UpdateLeds();
    }

    /// <summary>
    /// Static IRQ handler to avoid delegate allocation.
    /// </summary>
    private static void StaticHandleIRQ(ref IRQContext context)
    {
        // Read scancode from port 0x60
        byte scanCode = Native.IO.Read8(0x60);

        // Skip invalid scan codes (0x00 and 0xFF are invalid)
        if (scanCode == 0 || scanCode == 0xFF)
        {
            return;
        }

        bool released = (scanCode & 0x80) == 0x80;

        if (released)
        {
            scanCode = (byte)(scanCode ^ 0x80);
        }

        // Use the instance's OnKeyPressed callback (set by KeyboardManager.RegisterKeyboard)
        if (s_instance?.OnKeyPressed != null)
        {
            s_instance.OnKeyPressed.Invoke(scanCode, released);
        }

        // EOI is sent by InterruptManager.Dispatch after this handler returns
    }

    /// <summary>
    /// Drains any pending data from the keyboard buffer.
    /// Should be called before enabling IRQ to avoid spurious interrupts.
    /// </summary>
    public static void DrainBuffer()
    {
        int count = 0;
        while ((Native.IO.Read8(PS2Ports.Status) & 0x01) != 0 && count < 100)
        {
            Native.IO.Read8(PS2Ports.Data);
            count++;
        }
    }

    /// <summary>
    /// Updates keyboard LEDs (Caps Lock, Num Lock, Scroll Lock).
    /// </summary>
    public override void UpdateLeds()
    {
        // TODO: Implement LED update
        // var ledStatus = (scrollLock ? 1 : 0) | ((numLock ? 1 : 0) << 1) | ((capsLock ? 1 : 0) << 2);
        // SendCommand(Command.SetLEDs, (byte)ledStatus);
    }

    /// <summary>
    /// Check if a key is available in the buffer.
    /// </summary>
    public override bool KeyAvailable => (Native.IO.Read8(PS2Ports.Status) & 0x01) != 0;

    /// <summary>
    /// Enable keyboard scanning and register IRQ handler if not already done.
    /// Called by KeyboardManager after OnKeyPressed callback is set.
    /// </summary>
    public override void Enable()
    {
        // Register IRQ handler on first Enable() call (after callback is set)
        if (!s_irqRegistered)
        {
            RegisterIRQHandler();
            s_irqRegistered = true;
            return;
        }

        _controller.WaitToWrite();
        Native.IO.Write8(PS2Ports.Data, (byte)Command.EnableScanning);
        _controller.WaitForAck();
    }

    /// <summary>
    /// Disable keyboard scanning.
    /// </summary>
    public override void Disable()
    {
        _controller.WaitToWrite();
        Native.IO.Write8(PS2Ports.Data, (byte)Command.DisableScanning);
        _controller.WaitForAck();
    }

    /// <summary>
    /// Gets the current keyboard scan code set.
    /// </summary>
    private byte GetScanCodeSet()
    {
        SendCommand(Command.GetOrSetScanCodeSet, 0);
        return _controller.ReadByteAfterAck();
    }

    /// <summary>
    /// Sets the scan code set.
    /// </summary>
    /// <param name="scanCodeSet">The scan code set (1, 2, or 3).</param>
    private void SetScanCodeSet(byte scanCodeSet)
    {
        if (scanCodeSet == 1 || scanCodeSet == 2 || scanCodeSet == 3)
        {
            SendCommand(Command.GetOrSetScanCodeSet, scanCodeSet);
        }
    }

    private void SendCommand(Command command, byte? data = null)
    {
        if (PS2Port == 2)
        {
            _controller.PrepareSecondPortWrite();
        }

        _controller.WaitToWrite();
        Native.IO.Write8(PS2Ports.Data, (byte)command);

        _controller.WaitForAck();

        if (data.HasValue)
        {
            if (PS2Port == 2)
            {
                _controller.PrepareSecondPortWrite();
            }

            _controller.WaitToWrite();
            Native.IO.Write8(PS2Ports.Data, data.Value);

            _controller.WaitForAck();
        }
    }
}
