// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Globalization;
using Cosmos.TestRunner.Protocol;

namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>What a guest's host request asks the engine to do.</summary>
internal enum HostRequestKind
{
    /// <summary>Pull a USB stick out.</summary>
    StickUnplug,

    /// <summary>Plug a USB stick back in, on the same image.</summary>
    StickPlug,

    /// <summary>Pull one of the profile's <c>"usb"</c> devices out.</summary>
    DeviceUnplug,

    /// <summary>Plug one of the profile's <c>"usb"</c> devices back in.</summary>
    DevicePlug,

    /// <summary>Move a USB mouse and set the buttons it holds.</summary>
    PointerMove
}

/// <summary>
/// One request of the guest (<see cref="Ds2Vs.HostRequest"/>), parsed. The
/// guest sends words separated by spaces, all numbers in decimal:
/// <list type="bullet">
/// <item><c>usb-unplug [n]</c>, <c>usb-plug [n]</c>: USB stick n, 0 when omitted.</item>
/// <item><c>usb-device-unplug n</c>, <c>usb-device-plug n</c>: entry n of the profile's <c>"usb"</c> list.</item>
/// <item><c>usb-pointer-move n dx dy [buttons]</c>: entry n, a <c>usb-mouse</c>, moves by
/// (dx, dy) and then holds <c>buttons</c>, 0 when omitted.</item>
/// </list>
/// </summary>
/// <param name="Kind">What the request asks for.</param>
/// <param name="Index">The stick, or the entry of the profile's <c>"usb"</c> list, it acts on.</param>
/// <param name="DeltaX">Pointer movement to the right, for <see cref="HostRequestKind.PointerMove"/>.</param>
/// <param name="DeltaY">Pointer movement down, for <see cref="HostRequestKind.PointerMove"/>.</param>
/// <param name="Buttons">
/// Buttons held once the pointer moved, for <see cref="HostRequestKind.PointerMove"/>:
/// <see cref="LeftButton"/>, <see cref="RightButton"/> and <see cref="MiddleButton"/>,
/// the bits of a HID boot mouse report's first byte.
/// </param>
internal sealed record HostRequest(HostRequestKind Kind, int Index, int DeltaX = 0, int DeltaY = 0, int Buttons = 0)
{
    /// <summary>Pulls stick n (0 when omitted) off the xHCI controller.</summary>
    public const string UsbUnplugWord = "usb-unplug";

    /// <summary>Plugs stick n (0 when omitted) back in, on the same image.</summary>
    public const string UsbPlugWord = "usb-plug";

    /// <summary>Pulls entry n of the profile's <c>"usb"</c> list off the xHCI controller.</summary>
    public const string UsbDeviceUnplugWord = "usb-device-unplug";

    /// <summary>Plugs entry n of the profile's <c>"usb"</c> list back in, as the same model.</summary>
    public const string UsbDevicePlugWord = "usb-device-plug";

    /// <summary>Moves entry n of the profile's <c>"usb"</c> list, a <c>usb-mouse</c>, and sets its buttons.</summary>
    public const string UsbPointerMoveWord = "usb-pointer-move";

    /// <summary>The left button's bit in <see cref="Buttons"/>.</summary>
    public const int LeftButton = 1;

    /// <summary>The right button's bit in <see cref="Buttons"/>.</summary>
    public const int RightButton = 2;

    /// <summary>The middle button's bit in <see cref="Buttons"/>.</summary>
    public const int MiddleButton = 4;

    /// <summary>Every button bit a request may set.</summary>
    private const int AllButtons = LeftButton | RightButton | MiddleButton;

    /// <summary>
    /// Parses one request as the guest sent it.
    /// </summary>
    /// <exception cref="FormatException">The request is unknown or its arguments are malformed.</exception>
    public static HostRequest Parse(string text)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            throw new FormatException("empty request");
        }

        switch (words[0])
        {
            case UsbUnplugWord:
            case UsbPlugWord:
                RequireArgumentCount(words, 0, 1, "[n]");
                return new HostRequest(
                    words[0] == UsbUnplugWord ? HostRequestKind.StickUnplug : HostRequestKind.StickPlug,
                    words.Length == 2 ? ParseIndex(words[1]) : 0);

            case UsbDeviceUnplugWord:
            case UsbDevicePlugWord:
                RequireArgumentCount(words, 1, 1, "n");
                return new HostRequest(
                    words[0] == UsbDeviceUnplugWord ? HostRequestKind.DeviceUnplug : HostRequestKind.DevicePlug,
                    ParseIndex(words[1]));

            case UsbPointerMoveWord:
                RequireArgumentCount(words, 3, 4, "n dx dy [buttons]");
                return new HostRequest(
                    HostRequestKind.PointerMove,
                    ParseIndex(words[1]),
                    ParseDelta(words[2]),
                    ParseDelta(words[3]),
                    words.Length == 5 ? ParseButtons(words[4]) : 0);

            default:
                throw new FormatException($"unknown request '{words[0]}'");
        }
    }

    private static void RequireArgumentCount(string[] words, int minimum, int maximum, string usage)
    {
        int count = words.Length - 1;
        if (count < minimum || count > maximum)
        {
            throw new FormatException($"expected '{words[0]} {usage}'");
        }
    }

    // Digits only: a sign, a space or a thousands separator is a malformed
    // request, not an index.
    private static int ParseIndex(string word) =>
        int.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out int index)
            ? index
            : throw new FormatException($"'{word}' is not a device index");

    private static int ParseDelta(string word) =>
        int.TryParse(word, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int delta)
            ? delta
            : throw new FormatException($"'{word}' is not a pointer movement");

    private static int ParseButtons(string word) =>
        int.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out int buttons) && (buttons & ~AllButtons) == 0
            ? buttons
            : throw new FormatException($"'{word}' is not a button mask (1 left, 2 right, 4 middle)");
}
