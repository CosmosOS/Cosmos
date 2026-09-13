// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Devices.Virtio;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// Virtio-input mouse driver. Transport-agnostic: works over virtio MMIO
/// (QEMU virt) and virtio PCI (q35 virtio-mouse-pci) alike.
/// </summary>
internal unsafe class VirtioMouse : MouseDevice
{
    // Linux mouse button codes
    private const ushort BTN_LEFT = 0x110;
    private const ushort BTN_RIGHT = 0x111;
    private const ushort BTN_MIDDLE = 0x112;

    // Linux relative axis codes
    private const ushort REL_X = 0x00;
    private const ushort REL_Y = 0x01;
    private const ushort REL_WHEEL = 0x08;

    // Queue size
    private const uint QueueSize = 64;

    // Event buffers
    private const int EventBufferCount = 32;

    private readonly VirtioTransport _transport;
    private Virtqueue? _eventQueue;
    private VirtioInputEvent* _eventBuffers;

    private bool _initialized;
    private bool _irqRegistered;

    // Temporary state for accumulating events
    private int _tempDeltaX;
    private int _tempDeltaY;
    private int _tempDeltaZ;
    private bool _tempLeftButton;
    private bool _tempRightButton;
    private bool _tempMiddleButton;
    private bool _hasEvents;

    /// <summary>
    /// Returns true if the device was successfully initialized.
    /// </summary>
    public bool IsInitialized => _initialized;

    /// <summary>The transport this device was bound over (MMIO or PCI).</summary>
    public VirtioTransport Transport => _transport;

    public override bool DataAvailable => false; // Events are pushed via interrupt

    internal VirtioMouse(VirtioTransport transport)
    {
        _transport = transport;
        X = 0;
        Y = 0;
    }

    /// <summary>
    /// Initializes the virtio mouse device.
    /// </summary>
    public override void Initialize()
    {
        Serial.Write("[VirtioMouse] Initializing (");
        Serial.Write(_transport.TransportName);
        Serial.Write(" transport)...\n");

        _transport.BeginInit();

        // The mouse needs no device-specific features.
        if (!_transport.NegotiateFeatures(0, out _))
        {
            _transport.Fail();
            return;
        }

        _eventQueue = _transport.CreateQueue(VirtioInput.EVENTQ, QueueSize);
        if (_eventQueue == null)
        {
            Serial.Write("[VirtioMouse] ERROR: Failed to setup event queue\n");
            _transport.Fail();
            return;
        }

        // Allocate event buffers and add to queue
        _eventBuffers = (VirtioInputEvent*)MemoryOp.Alloc((uint)(EventBufferCount * sizeof(VirtioInputEvent)));
        for (int i = 0; i < EventBufferCount; i++)
        {
            AddEventBuffer(i);
        }

        // Notify device
        _transport.NotifyQueue(VirtioInput.EVENTQ);

        _transport.FinishInit();

        _initialized = true;
        Serial.Write("[VirtioMouse] Initialization complete\n");
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
            VirtioInputEvent* evt = &_eventBuffers[id];
            if (evt->Type == VirtioInput.EV_REL)
            {
                // Accumulate relative axis changes to account for multiple REL events
                if (evt->Code == REL_X)
                {
                    _tempDeltaX += (int)evt->Value;
                    _hasEvents = true;
                }
                else if (evt->Code == REL_Y)
                {
                    _tempDeltaY += (int)evt->Value;
                    _hasEvents = true;
                }
                else if (evt->Code == REL_WHEEL)
                {
                    _tempDeltaZ += (int)evt->Value;
                    _hasEvents = true;
                }
            }
            else if (evt->Type == VirtioInput.EV_KEY)
            {
                bool pressed = evt->Value != 0;
                if (evt->Code == BTN_LEFT)
                {
                    _tempLeftButton = pressed;
                    _hasEvents = true;
                }
                else if (evt->Code == BTN_RIGHT)
                {
                    _tempRightButton = pressed;
                    _hasEvents = true;
                }
                else if (evt->Code == BTN_MIDDLE)
                {
                    _tempMiddleButton = pressed;
                    _hasEvents = true;
                }
            }
            else if (evt->Type == VirtioInput.EV_SYN && _hasEvents)
            {
                // Sync event - dispatch accumulated changes
                X += _tempDeltaX;
                Y += _tempDeltaY;
                ScrollDelta = _tempDeltaZ;

                LeftButton = _tempLeftButton;
                RightButton = _tempRightButton;
                MiddleButton = _tempMiddleButton;

                OnMouseEvent?.Invoke(_tempDeltaX, _tempDeltaY, _tempDeltaZ, _tempLeftButton, _tempRightButton,
                    _tempMiddleButton);

                _tempDeltaX = 0;
                _tempDeltaY = 0;
                _tempDeltaZ = 0;
                _hasEvents = false;
            }

            // Re-add buffer
            _eventQueue.FreeDescriptor((int)id);
            AddEventBuffer((int)id);
            processedAny = true;
        }

        if (processedAny)
        {
            _transport.NotifyQueue(VirtioInput.EVENTQ);
        }
    }

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

    public override void Enable()
    {
        if (!_irqRegistered && _initialized)
        {
            _transport.EnableInterrupt(OnDeviceInterrupt);
            _irqRegistered = true;
        }
    }

    public override void Disable()
    {
        // Not implemented
    }
}
