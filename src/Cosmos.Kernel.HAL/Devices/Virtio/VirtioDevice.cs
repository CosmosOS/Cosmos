// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Registry and driver binding for virtio devices, independent of transport.
/// The MMIO bus scan is driven by the platform initializer that knows the
/// window layout (QEMU virt on ARM64); the PCI scan runs on every architecture
/// from the shared HAL initialization after PCI enumeration.
/// </summary>
// Note: This class is eagerly constructed at startup because accessing s_devices causes issues otherwise.
[EagerStaticClassConstruction]
internal static class VirtioDevice
{
    private const int MaxDevices = 32;

    private static readonly object?[] s_devices = new object?[MaxDevices];
    private static int s_count;

    /// <summary>
    /// Retrieves a registered virtio device by its type.
    /// </summary>
    /// <typeparam name="T">The type of device to retrieve.</typeparam>
    /// <returns>The first device instance of the specified type, or null if not found.</returns>
    public static T? GetDevice<T>() where T : class
    {
        for (int i = 0; i < s_count; i++)
        {
            if (s_devices[i] is T device)
            {
                return device;
            }
        }
        return null;
    }

    public static IKeyboardDevice[] GetKeyboards()
    {
        // Count registered keyboards
        int count = 0;
        for (int i = 0; i < s_count; i++)
        {
            if (s_devices[i] is VirtioKeyboard)
            {
                count++;
            }
        }

        // Collect keyboards into an array
        IKeyboardDevice[] keyboards = new IKeyboardDevice[count];
        int index = 0;
        for (int i = 0; i < s_count; i++)
        {
            if (s_devices[i] is VirtioKeyboard keyboard)
            {
                keyboards[index++] = keyboard;
            }
        }

        return keyboards;
    }

    public static IMouseDevice[] GetMice()
    {
        // Count registered mice
        int count = 0;
        for (int i = 0; i < s_count; i++)
        {
            if (s_devices[i] is VirtioMouse)
            {
                count++;
            }
        }

        // Collect mice into an array
        IMouseDevice[] mice = new IMouseDevice[count];
        int index = 0;
        for (int i = 0; i < s_count; i++)
        {
            if (s_devices[i] is VirtioMouse mouse)
            {
                mice[index++] = mouse;
            }
        }

        return mice;
    }

    /// <summary>
    /// Scans a virtio MMIO window (a run of fixed-size slots with consecutive
    /// interrupt lines) and binds drivers to the devices found. The caller
    /// owns the platform knowledge: window address, slot layout, interrupt
    /// base, and how a line interrupt is wired (<paramref name="irqEnable"/>).
    /// </summary>
    public static void InitializeMmioBus(ulong busBase, ulong slotStride, uint slotCount, uint irqBase,
        VirtioIrqEnable irqEnable)
    {
        Serial.Write("[VirtioDevice] Scanning virtio MMIO bus at 0x");
        Serial.WriteHex(busBase);
        Serial.Write("\n");

        for (uint slot = 0; slot < slotCount; slot++)
        {
            VirtioMmioTransport? transport =
                VirtioMmioTransport.TryProbe(busBase + slot * slotStride, irqBase + slot, irqEnable);
            if (transport == null)
            {
                continue;
            }

            Serial.Write("[VirtioDevice] MMIO slot ");
            Serial.WriteNumber(slot);
            Serial.Write(": device type ");
            Serial.WriteNumber(transport.DeviceType);
            Serial.Write(", INTID ");
            Serial.WriteNumber(irqBase + slot);
            Serial.Write("\n");

            RegisterFromTransport(transport);
        }
    }

    /// <summary>
    /// Binds drivers to the virtio PCI functions discovered by
    /// <see cref="PciManager.Setup"/>. Runs on both architectures.
    /// </summary>
    public static void InitializePciBus()
    {
        if (PciManager.Devices == null)
        {
            return;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice pci = PciManager.Devices[i];
            uint deviceType = VirtioPciTransport.GetDeviceType(pci);
            if (deviceType == 0 || pci.Claimed)
            {
                continue;
            }

            // Leave devices we have no driver for untouched (e.g. the
            // virtio-scsi boot CD on ARM64 — still owned by firmware state).
            if (!WantsDeviceType(deviceType))
            {
                Serial.Write("[VirtioDevice] Skipping virtio PCI device type ");
                Serial.WriteNumber(deviceType);
                Serial.Write("\n");
                continue;
            }

            VirtioPciTransport? transport = VirtioPciTransport.TryCreate(pci);
            if (transport == null)
            {
                continue;
            }

            pci.Claimed = true;
            RegisterFromTransport(transport);
        }
    }

    private static bool WantsDeviceType(uint deviceType) => deviceType switch
    {
        VirtioTransport.DeviceTypeNetwork => CosmosFeatures.NetworkEnabled,
        VirtioTransport.DeviceTypeInput => CosmosFeatures.KeyboardEnabled || CosmosFeatures.MouseEnabled,
        _ => false,
    };

    private static void RegisterFromTransport(VirtioTransport transport)
    {
        switch (transport.DeviceType)
        {
            case VirtioTransport.DeviceTypeNetwork:
                if (CosmosFeatures.NetworkEnabled)
                {
                    VirtioNet netDevice = new VirtioNet(transport);
                    netDevice.Initialize();
                    // Only advertise devices that came up: a NIC without an
                    // interrupt path never processes RX, so registering it
                    // would just make DHCP/ARP silently time out.
                    if (netDevice.Ready)
                    {
                        Add(netDevice);
                    }
                }
                break;
            case VirtioTransport.DeviceTypeInput:
                RegisterInputDevice(transport);
                break;
            default:
                Serial.Write("[VirtioDevice] Unsupported virtio device type ");
                Serial.WriteNumber(transport.DeviceType);
                Serial.Write("\n");
                break;
        }
    }

    /// <summary>
    /// Registers an input device based on enabled features. Creates either a
    /// mouse or keyboard device depending on CosmosFeatures and the event
    /// types the device reports.
    /// </summary>
    private static void RegisterInputDevice(VirtioTransport transport)
    {
        // Minimal handshake so the device config space becomes readable.
        transport.BeginInit();

        // Probe supported event types for this device
        bool supportsKey = SupportsEventType(transport, VirtioInput.EV_KEY);
        bool supportsRel = SupportsEventType(transport, VirtioInput.EV_REL);
        // TODO: TouchScreen support with EV_ABS

        // Mouse devices support both EV_KEY and EV_REL.
        if (supportsKey && supportsRel)
        {
            if (CosmosFeatures.MouseEnabled)
            {
                VirtioMouse mouse = new VirtioMouse(transport);
                mouse.Initialize();
                if (mouse.IsInitialized)
                {
                    Add(mouse);
                }
            }
            return;
        }

        // Keyboard devices support EV_KEY but not EV_REL.
        if (supportsKey && CosmosFeatures.KeyboardEnabled)
        {
            VirtioKeyboard keyboard = new VirtioKeyboard(transport);
            keyboard.Initialize();
            if (keyboard.IsInitialized)
            {
                Add(keyboard);
            }
        }
    }

    /// <summary>
    /// Checks if the virtio input device supports a specific event type by
    /// querying virtio_input_config (select/subsel, then the size byte).
    /// </summary>
    private static bool SupportsEventType(VirtioTransport transport, ushort eventType)
    {
        transport.WriteDeviceConfig8(0, VirtioInput.CFG_EV_BITS);
        transport.WriteDeviceConfig8(1, (byte)eventType);
        return transport.ReadDeviceConfig8(2) > 0;
    }

    private static void Add(object device)
    {
        if (s_count >= MaxDevices)
        {
            Serial.Write("[VirtioDevice] Device registry full\n");
            return;
        }

        s_devices[s_count++] = device;
    }
}
