// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Input;

/// <summary>
/// One event as the device writes it into a buffer of the eventq (virtio 1.2
/// §5.8.6): a type, a code within that type and a value. Read in place out
/// of the queue's DMA memory, and little-endian, as every virtio structure
/// is on both architectures.
/// </summary>
/// <remarks>
/// The three fields are the Linux input layer's own (linux/input.h), which
/// virtio-input passes through unchanged, so the codes below are evdev's:
/// the same numbers a Linux driver would see from the same device.
/// </remarks>
internal static class InputEvent
{
    /// <summary>Bytes one event takes, which is also a buffer's length: type, code, value.</summary>
    internal const int Size = 8;

    /// <summary>EV_SYN: the end of one batch of events, and where a pointer's movement is reported.</summary>
    internal const ushort TypeSync = 0x00;

    /// <summary>EV_KEY: a key or a button went down or came up.</summary>
    internal const ushort TypeKey = 0x01;

    /// <summary>EV_REL: an axis moved by the value, as a mouse's does.</summary>
    internal const ushort TypeRelative = 0x02;

    /// <summary>EV_ABS: an axis is at the value, as a tablet's or a touchscreen's is.</summary>
    internal const ushort TypeAbsolute = 0x03;

    /// <summary>REL_X, horizontal movement; positive moves right.</summary>
    internal const ushort RelativeX = 0x00;

    /// <summary>REL_Y, vertical movement; positive moves down, as on screen.</summary>
    internal const ushort RelativeY = 0x01;

    /// <summary>REL_WHEEL, wheel movement; positive scrolls up, which is the opposite of the kit's sign.</summary>
    internal const ushort RelativeWheel = 0x08;

    /// <summary>BTN_LEFT, the primary button.</summary>
    internal const ushort ButtonLeft = 0x110;

    /// <summary>BTN_RIGHT, the secondary button.</summary>
    internal const ushort ButtonRight = 0x111;

    /// <summary>BTN_MIDDLE, often the wheel pressed down.</summary>
    internal const ushort ButtonMiddle = 0x112;

    private const int CodeOffset = 2;
    private const int ValueOffset = 4;

    /// <summary>The event's type, one of the <c>Type</c> constants above.</summary>
    /// <param name="input">The <see cref="Size"/> bytes the device wrote.</param>
    /// <returns>The type.</returns>
    internal static ushort ReadType(ReadOnlySpan<byte> input) => BinaryPrimitives.ReadUInt16LittleEndian(input);

    /// <summary>The code within the type: a key code, an axis or a button.</summary>
    /// <param name="input">The <see cref="Size"/> bytes the device wrote.</param>
    /// <returns>The code.</returns>
    internal static ushort ReadCode(ReadOnlySpan<byte> input) =>
        BinaryPrimitives.ReadUInt16LittleEndian(input[CodeOffset..]);

    /// <summary>
    /// The value: how far an axis moved, or, for a key, 0 for released, 1
    /// for pressed and 2 for the device's own repeat of a key held down.
    /// </summary>
    /// <param name="input">The <see cref="Size"/> bytes the device wrote.</param>
    /// <returns>The value, signed: a relative axis reports movement either way.</returns>
    internal static int ReadValue(ReadOnlySpan<byte> input) =>
        BinaryPrimitives.ReadInt32LittleEndian(input[ValueOffset..]);
}
