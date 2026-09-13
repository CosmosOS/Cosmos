// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Devices.Virtio;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// Virtio-input keyboard driver. Transport-agnostic: works over virtio MMIO
/// (QEMU virt) and virtio PCI (q35 virtio-keyboard-pci) alike.
/// </summary>
internal unsafe class VirtioKeyboard : KeyboardDevice
{
    // Queue size
    private const uint QueueSize = 64;

    // Event buffers
    private const int EventBufferCount = 32;

    private readonly VirtioTransport _transport;
    private Virtqueue? _eventQueue;
    private VirtioInputEvent* _eventBuffers;

    private bool _initialized;
    private bool _irqRegistered;

    /// <summary>
    /// Returns true if the device was successfully initialized.
    /// </summary>
    public bool IsInitialized => _initialized;

    /// <summary>The transport this device was bound over (MMIO or PCI).</summary>
    public VirtioTransport Transport => _transport;

    public override bool KeyAvailable => false;  // Events are pushed via interrupt

    internal VirtioKeyboard(VirtioTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Initializes the virtio keyboard device.
    /// </summary>
    public override void Initialize()
    {
        Serial.Write("[VirtioKeyboard] Initializing (");
        Serial.Write(_transport.TransportName);
        Serial.Write(" transport)...\n");

        _transport.BeginInit();

        // The keyboard needs no device-specific features.
        if (!_transport.NegotiateFeatures(0, out _))
        {
            _transport.Fail();
            return;
        }

        _eventQueue = _transport.CreateQueue(VirtioInput.EVENTQ, QueueSize);
        if (_eventQueue == null)
        {
            Serial.Write("[VirtioKeyboard] ERROR: Failed to setup event queue\n");
            _transport.Fail();
            return;
        }

        // Allocate event buffers and add to queue
        _eventBuffers = (VirtioInputEvent*)MemoryOp.Alloc((uint)(EventBufferCount * sizeof(VirtioInputEvent)));
        for (int i = 0; i < EventBufferCount; i++)
        {
            AddEventBuffer(i);
        }

        // Notify device that buffers are available
        _transport.NotifyQueue(VirtioInput.EVENTQ);

        _transport.FinishInit();

        _initialized = true;
        Serial.Write("[VirtioKeyboard] Initialization complete\n");
    }

    private void AddEventBuffer(int bufferIndex)
    {
        if (_eventQueue == null)
        {
            return;
        }

        int descIdx = _eventQueue.AllocDescriptor();
        if (descIdx < 0)
        {
            return;
        }

        ulong bufferAddr = VirtioDma.VirtToPhys((ulong)(&_eventBuffers[bufferIndex]));
        _eventQueue.SetupDescriptor(descIdx, bufferAddr, (uint)sizeof(VirtioInputEvent),
            Virtqueue.VRING_DESC_F_WRITE, 0);
        _eventQueue.AddAvailable((ushort)descIdx);
    }

    private void OnDeviceInterrupt(uint isrStatus)
    {
        if (!_initialized)
        {
            return;
        }

        ProcessEvents();
    }

    private void ProcessEvents()
    {
        if (_eventQueue == null)
        {
            return;
        }

        bool processedAny = false;
        while (_eventQueue.GetUsedBuffer(out uint id, out uint len))
        {
            // Process the event
            VirtioInputEvent* evt = &_eventBuffers[id];

            if (evt->Type == VirtioInput.EV_KEY)
            {
                // Convert Linux keycode to PS2 scan code
                byte scanCode = LinuxToPS2ScanCode(evt->Code);
                bool released = evt->Value == 0;

                // Invoke instance callback (set by KeyboardManager.RegisterKeyboard)
                OnKeyPressed?.Invoke(scanCode, released);
            }

            // Re-add buffer to queue
            _eventQueue.FreeDescriptor((int)id);
            AddEventBuffer((int)id);
            processedAny = true;
        }

        if (processedAny)
        {
            _transport.NotifyQueue(VirtioInput.EVENTQ);
        }
    }

    /// <summary>
    /// Polls for keyboard events (for non-interrupt mode).
    /// </summary>
    public override void Poll()
    {
        if (!_initialized || _eventQueue == null)
        {
            return;
        }

        uint isrStatus = _transport.ReadAndAckIsr();
        if (isrStatus != 0 || _eventQueue.HasUsedBuffers())
        {
            ProcessEvents();
        }
    }

    public override void UpdateLeds()
    {
        // LED updates via status queue not implemented yet
    }

    /// <summary>
    /// Enable keyboard and register the interrupt handler if not already done.
    /// Called by KeyboardManager after OnKeyPressed callback is set.
    /// </summary>
    public override void Enable()
    {
        // Register interrupt handler on first Enable() call (after callback is set)
        if (!_irqRegistered && _initialized)
        {
            _transport.EnableInterrupt(OnDeviceInterrupt);
            _irqRegistered = true;
        }
    }

    public override void Disable()
    {
        // Not implemented - would need to disable IRQ
    }

    /// <summary>
    /// Converts Linux keycode to PS/2 scan code set 1.
    /// </summary>
    private static byte LinuxToPS2ScanCode(ushort linuxCode)
    {
        // Linux keycodes are mostly similar to PS/2 scan codes for basic keys
        // This is a simplified mapping for common keys
        return linuxCode switch
        {
            // Function row
            1 => 0x01,   // KEY_ESC -> Esc
            59 => 0x3B,  // KEY_F1 -> F1
            60 => 0x3C,  // KEY_F2 -> F2
            61 => 0x3D,  // KEY_F3 -> F3
            62 => 0x3E,  // KEY_F4 -> F4
            63 => 0x3F,  // KEY_F5 -> F5
            64 => 0x40,  // KEY_F6 -> F6
            65 => 0x41,  // KEY_F7 -> F7
            66 => 0x42,  // KEY_F8 -> F8
            67 => 0x43,  // KEY_F9 -> F9
            68 => 0x44,  // KEY_F10 -> F10

            // Number row
            2 => 0x02,   // KEY_1 -> 1
            3 => 0x03,   // KEY_2 -> 2
            4 => 0x04,   // KEY_3 -> 3
            5 => 0x05,   // KEY_4 -> 4
            6 => 0x06,   // KEY_5 -> 5
            7 => 0x07,   // KEY_6 -> 6
            8 => 0x08,   // KEY_7 -> 7
            9 => 0x09,   // KEY_8 -> 8
            10 => 0x0A,  // KEY_9 -> 9
            11 => 0x0B,  // KEY_0 -> 0
            12 => 0x0C,  // KEY_MINUS -> -
            13 => 0x0D,  // KEY_EQUAL -> =
            14 => 0x0E,  // KEY_BACKSPACE -> Backspace

            // Top letter row
            15 => 0x0F,  // KEY_TAB -> Tab
            16 => 0x10,  // KEY_Q -> Q
            17 => 0x11,  // KEY_W -> W
            18 => 0x12,  // KEY_E -> E
            19 => 0x13,  // KEY_R -> R
            20 => 0x14,  // KEY_T -> T
            21 => 0x15,  // KEY_Y -> Y
            22 => 0x16,  // KEY_U -> U
            23 => 0x17,  // KEY_I -> I
            24 => 0x18,  // KEY_O -> O
            25 => 0x19,  // KEY_P -> P
            26 => 0x1A,  // KEY_LEFTBRACE -> [
            27 => 0x1B,  // KEY_RIGHTBRACE -> ]
            28 => 0x1C,  // KEY_ENTER -> Enter

            // Middle letter row
            29 => 0x1D,  // KEY_LEFTCTRL -> Left Ctrl
            30 => 0x1E,  // KEY_A -> A
            31 => 0x1F,  // KEY_S -> S
            32 => 0x20,  // KEY_D -> D
            33 => 0x21,  // KEY_F -> F
            34 => 0x22,  // KEY_G -> G
            35 => 0x23,  // KEY_H -> H
            36 => 0x24,  // KEY_J -> J
            37 => 0x25,  // KEY_K -> K
            38 => 0x26,  // KEY_L -> L
            39 => 0x27,  // KEY_SEMICOLON -> ;
            40 => 0x28,  // KEY_APOSTROPHE -> '
            41 => 0x29,  // KEY_GRAVE -> `

            // Bottom letter row
            42 => 0x2A,  // KEY_LEFTSHIFT -> Left Shift
            43 => 0x2B,  // KEY_BACKSLASH -> \
            44 => 0x2C,  // KEY_Z -> Z
            45 => 0x2D,  // KEY_X -> X
            46 => 0x2E,  // KEY_C -> C
            47 => 0x2F,  // KEY_V -> V
            48 => 0x30,  // KEY_B -> B
            49 => 0x31,  // KEY_N -> N
            50 => 0x32,  // KEY_M -> M
            51 => 0x33,  // KEY_COMMA -> ,
            52 => 0x34,  // KEY_DOT -> .
            53 => 0x35,  // KEY_SLASH -> /
            54 => 0x36,  // KEY_RIGHTSHIFT -> Right Shift

            // Bottom row
            56 => 0x38,  // KEY_LEFTALT -> Left Alt
            57 => 0x39,  // KEY_SPACE -> Space
            58 => 0x3A,  // KEY_CAPSLOCK -> Caps Lock

            // Arrow keys (extended)
            103 => 0x48, // KEY_UP -> Up
            105 => 0x4B, // KEY_LEFT -> Left
            106 => 0x4D, // KEY_RIGHT -> Right
            108 => 0x50, // KEY_DOWN -> Down

            _ => 0x00    // Unknown key
        };
    }
}
