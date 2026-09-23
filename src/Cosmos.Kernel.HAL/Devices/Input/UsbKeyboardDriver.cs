// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// USB class driver for keyboards: binds every HID interface that declares
/// the boot keyboard protocol (HID 1.11 §4.2-§4.3), which every PC keyboard
/// does so firmware can use it, and exposes them to the platform
/// initializer the way <see cref="Virtio.VirtioDevice.GetKeyboards"/> does.
/// </summary>
internal sealed class UsbKeyboardDriver : UsbDriver
{
    private const byte BootInterfaceSubclass = 0x01;
    private const byte KeyboardProtocol = 0x01;

    private static List<UsbKeyboard>? s_keyboards;

    public override string Name => "HID boot keyboard";

    /// <summary>Keyboards bound so far (empty when none, or before USB enumeration).</summary>
    public static IKeyboardDevice[] GetKeyboards()
    {
        if (s_keyboards is null)
        {
            return [];
        }

        IKeyboardDevice[] keyboards = new IKeyboardDevice[s_keyboards.Count];
        for (int i = 0; i < keyboards.Length; i++)
        {
            keyboards[i] = s_keyboards[i];
        }

        return keyboards;
    }

    public override bool TryBind(UsbDevice device, UsbInterface usbInterface)
    {
        if (usbInterface.Class != UsbClassCode.Hid
            || usbInterface.Subclass != BootInterfaceSubclass
            || usbInterface.Protocol != KeyboardProtocol)
        {
            return false;
        }

        UsbEndpoint? endpoint = usbInterface.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        if (endpoint is null)
        {
            return false;
        }

        UsbKeyboard keyboard = new(device, usbInterface.Number, endpoint);
        keyboard.Initialize();
        if (!keyboard.IsInitialized)
        {
            return false;
        }

        (s_keyboards ??= []).Add(keyboard);
        return true;
    }
}
