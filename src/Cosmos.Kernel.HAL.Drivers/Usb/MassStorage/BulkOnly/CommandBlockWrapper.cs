// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.BulkOnly;

/// <summary>
/// The Command Block Wrapper a SCSI command goes out in, on the bulk OUT
/// endpoint (BOT 1.0 s5.1): 31 bytes, little-endian. Creating one clears
/// the buffer and fills every field, so the reserved bits and the unused
/// tail of the command block are zero.
/// </summary>
internal readonly ref struct CommandBlockWrapper
{
    /// <summary>Size of a wrapper in bytes.</summary>
    internal const int Size = 31;

    /// <summary>Longest command block a wrapper carries.</summary>
    internal const int MaxCommandLength = 16;

    /// <summary>dCBWSignature: "USBC" in little-endian order.</summary>
    private const uint Signature = 0x43425355;

    private const int TagOffset = 4;
    private const int DataTransferLengthOffset = 8;
    private const int FlagsOffset = 12;
    private const int LunOffset = 13;
    private const int CommandLengthOffset = 14;
    private const int CommandOffset = 15;

    /// <summary>bmCBWFlags bit 7: the data stage moves from the device to the host.</summary>
    private const byte DataInFlag = 0x80;

    private readonly Span<byte> _wrapper;

    /// <summary>The wrapper's bytes, to send.</summary>
    internal ReadOnlySpan<byte> Bytes => _wrapper;

    /// <summary>Clears the first <see cref="Size"/> bytes of <paramref name="buffer"/> and writes the wrapper of one command there.</summary>
    /// <param name="buffer">At least <see cref="Size"/> bytes.</param>
    /// <param name="tag">dCBWTag, which the device echoes in the status wrapper that answers it.</param>
    /// <param name="lun">The logical unit, 0 to 15.</param>
    /// <param name="command">The command block, at most <see cref="MaxCommandLength"/> bytes.</param>
    /// <param name="dataLength">Bytes the data stage moves; 0 for a command without one.</param>
    /// <param name="dataIn">True when the data stage moves from the device to the host.</param>
    internal CommandBlockWrapper(Span<byte> buffer, uint tag, byte lun, ReadOnlySpan<byte> command, int dataLength, bool dataIn)
    {
        _wrapper = buffer[..Size];
        _wrapper.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(_wrapper, Signature);
        BinaryPrimitives.WriteUInt32LittleEndian(_wrapper[TagOffset..], tag);
        BinaryPrimitives.WriteUInt32LittleEndian(_wrapper[DataTransferLengthOffset..], (uint)dataLength);
        _wrapper[FlagsOffset] = dataIn ? DataInFlag : (byte)0;
        _wrapper[LunOffset] = lun;
        _wrapper[CommandLengthOffset] = (byte)command.Length;
        command.CopyTo(_wrapper[CommandOffset..]);
    }
}
