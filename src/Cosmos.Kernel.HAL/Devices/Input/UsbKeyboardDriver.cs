// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// USB class driver for keyboards: binds every HID interface that declares
/// the boot keyboard protocol (HID 1.11 §4.2-§4.3), which every PC keyboard
/// does so firmware can use it, and exposes them to the platform
/// initializer the way <see cref="Virtio.VirtioDevice.GetKeyboards"/> does.
/// The keyboards plugged in or pulled out after boot are reported through
/// <see cref="KeyboardAttached"/> and <see cref="KeyboardDetached"/>.
/// </summary>
internal sealed class UsbKeyboardDriver : UsbDriver
{
    private const byte BootInterfaceSubclass = 0x01;
    private const byte KeyboardProtocol = 0x01;

    /// <summary>
    /// The keyboards present. Replaced on every change, never changed in
    /// place, so a reader on another thread still sees a whole list.
    /// </summary>
    private static UsbKeyboard[]? s_keyboards;

    public override string Name => "HID boot keyboard";

    /// <summary>
    /// Called with every keyboard that becomes usable: on the boot path
    /// before anyone listens, then on the hot-plug thread.
    /// </summary>
    public static Action<UsbKeyboard>? KeyboardAttached { get; set; }

    /// <summary>Called on the hot-plug thread with every keyboard that was unplugged.</summary>
    public static Action<UsbKeyboard>? KeyboardDetached { get; set; }

    /// <summary>Keyboards present (empty when none, or before USB enumeration).</summary>
    public static IKeyboardDevice[] GetKeyboards()
    {
        UsbKeyboard[] present = s_keyboards ?? [];
        IKeyboardDevice[] keyboards = new IKeyboardDevice[present.Length];
        for (int i = 0; i < keyboards.Length; i++)
        {
            keyboards[i] = present[i];
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

        UsbKeyboard[] present = s_keyboards ?? [];
        UsbKeyboard[] keyboards = new UsbKeyboard[present.Length + 1];
        present.CopyTo(keyboards, 0);
        keyboards[present.Length] = keyboard;
        s_keyboards = keyboards;

        KeyboardAttached?.Invoke(keyboard);
        return true;
    }

    public override void Disconnect(UsbDevice device, UsbInterface usbInterface)
    {
        UsbKeyboard[] present = s_keyboards ?? [];
        List<UsbKeyboard> kept = new(present.Length);
        UsbKeyboard? removed = null;
        foreach (UsbKeyboard keyboard in present)
        {
            if (keyboard.Device == device && keyboard.InterfaceNumber == usbInterface.Number)
            {
                removed = keyboard;
            }
            else
            {
                kept.Add(keyboard);
            }
        }

        if (removed is null)
        {
            return;
        }

        s_keyboards = kept.ToArray();
        removed.Disable();
        KeyboardDetached?.Invoke(removed);
    }
}
