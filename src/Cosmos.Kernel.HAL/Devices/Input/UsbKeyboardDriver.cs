// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// USB class driver for keyboards: binds every HID interface that declares
/// the boot keyboard protocol (HID 1.11 §4.2-§4.3), which every PC keyboard
/// does so firmware can use it. The keyboards come and go through
/// <see cref="KeyboardAttached"/> and <see cref="KeyboardDetached"/>: the
/// USB core first enumerates during the driver pass, after the platform
/// initializer listed its own keyboards, so none is there to list.
/// </summary>
internal sealed class UsbKeyboardDriver : UsbClassDriver
{
    private const byte BootInterfaceSubclass = 0x01;
    private const byte KeyboardProtocol = 0x01;

    /// <summary>
    /// The keyboards present, which <see cref="Disconnect"/> looks the
    /// unplugged one up in. Replaced on every change, never changed in place.
    /// Null rather than <c>[]</c> until the first one binds: an initializer
    /// would give this type a class constructor.
    /// </summary>
    private static UsbKeyboard[]? s_keyboards;

    /// <summary>
    /// The driver's <see cref="Name"/>, which the driver kit also refuses as
    /// a registration name: the device list names an interface's owner by it.
    /// </summary>
    internal const string DriverName = "HID boot keyboard";

    public override string Name => DriverName;

    /// <summary>
    /// Called with every keyboard that becomes usable: on the boot thread
    /// during the driver pass, then on the hot-plug thread. System's library
    /// initializer hooks the keyboard manager here, before either runs.
    /// </summary>
    public static Action<UsbKeyboard>? KeyboardAttached { get; set; }

    /// <summary>Called on the hot-plug thread with every keyboard that was unplugged.</summary>
    public static Action<UsbKeyboard>? KeyboardDetached { get; set; }

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
