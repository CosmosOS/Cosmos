using System;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// ManagedMemoryBlock class. Used to read and write a managed memory block.
/// </summary>
internal unsafe class ManagedMemoryBlock
{
    private readonly byte[] _array;
    private readonly Memory<byte> _memory;
    private readonly int _alignedOffset;

    /// <summary>
    /// Offset (pointer address of aligned start).
    /// </summary>
    public ulong Offset { get; private set; }

    /// <summary>
    /// Size of the usable memory block.
    /// </summary>
    public uint Size { get; }

    /// <summary>
    /// Gets a Span view of the memory block.
    /// </summary>
    public Span<byte> Span => _memory.Span;

    /// <summary>
    /// Gets a Memory view of the memory block.
    /// </summary>
    public Memory<byte> Memory => _memory;

    /// <summary>
    /// Create a new buffer with the given size, not aligned
    /// </summary>
    /// <param name="aByteCount">Size of buffer</param>
    public ManagedMemoryBlock(uint aByteCount) : this(aByteCount, 1, false)
    {
    }

    /// <summary>
    /// Create a new buffer with the given size, aligned on the byte boundary specified
    /// </summary>
    /// <param name="aByteCount">Size of buffer</param>
    /// <param name="alignment">Byte Boundary alignment</param>
    public ManagedMemoryBlock(uint aByteCount, int alignment) : this(aByteCount, alignment, true)
    {
    }

    /// <summary>
    /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
    /// </summary>
    /// <param name="aByteCount">Size of buffer</param>
    /// <param name="aAlignment">Byte Boundary alignment</param>
    /// <param name="aAlign">true if buffer should be aligned, false otherwise</param>
    public ManagedMemoryBlock(uint aByteCount, int aAlignment, bool aAlign)
    {
        _array = new byte[aByteCount + aAlignment - 1];
        Size = aByteCount;

        fixed (byte* bodystart = _array)
        {
            ulong baseAddress = (ulong)bodystart;
            Offset = baseAddress;

            if (aAlign)
            {
                // Calculate aligned offset
                ulong remainder = baseAddress % (ulong)aAlignment;
                if (remainder != 0)
                {
                    Offset = baseAddress + (ulong)aAlignment - remainder;
                }
            }

            _alignedOffset = (int)(Offset - baseAddress);
        }

        _memory = new Memory<byte>(_array, _alignedOffset, (int)aByteCount);
    }

    /// <summary>
    /// Get or set the byte at the given offset
    /// </summary>
    /// <param name="offset">Address Offset</param>
    /// <returns>Byte value at given offset</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid offset.</exception>
    public byte this[uint offset]
    {
        get
        {
            if (offset >= Size)
            {
                return 0;
            }

            return Span[(int)offset];
        }
        set
        {
            if (offset >= Size)
            {
                return;
            }

            Span[(int)offset] = value;
        }
    }

    /// <summary>
    /// Fill memory block.
    /// </summary>
    /// <param name="aByteOffset">A start.</param>
    /// <param name="aCount">A count.</param>
    /// <param name="aData">A data to fill (as uint, fills aCount uint values).</param>
    public void Fill(uint aByteOffset, uint aCount, uint aData)
    {
        if (aByteOffset >= Size)
        {
            return;
        }

        var remaining = Span.Slice((int)aByteOffset);
        var availableUints = remaining.Length / 4;
        if (availableUints == 0)
        {
            return;
        }

        var count = Math.Min((int)aCount, availableUints);
        var span = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(remaining.Slice(0, count * 4));
        span.Fill(aData);
    }

    /// <summary>
    /// Fill memory block with integer value
    /// </summary>
    /// <param name="aByteOffset">A starting position in the memory block (byte offset)</param>
    /// <param name="aCount">Number of uint values to fill.</param>
    /// <param name="aData">A data to fill memory block with.</param>
    public void Fill(int aByteOffset, int aCount, int aData)
    {
        if (aByteOffset < 0 || aByteOffset >= Size)
        {
            return;
        }

        var remaining = Span.Slice(aByteOffset);
        var availableUints = remaining.Length / 4;
        if (availableUints == 0)
        {
            return;
        }

        var count = Math.Min(aCount, availableUints);
        var span = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(remaining.Slice(0, count * 4));
        span.Fill((uint)aData);
    }

    /// <summary>
    /// Fill entire memory block with a uint value.
    /// </summary>
    /// <param name="aData">A data to fill.</param>
    public void Fill(uint aData)
    {
        var alignedLength = (Span.Length / 4) * 4;
        if (alignedLength == 0)
        {
            return;
        }

        var span = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(Span.Slice(0, alignedLength));
        span.Fill(aData);
    }

    /// <summary>
    /// Copy data from a byte array into this memory block.
    /// </summary>
    /// <param name="aStart">Starting offset in this memory block.</param>
    /// <param name="aData">Source byte array.</param>
    /// <param name="aIndex">Starting index in source array.</param>
    /// <param name="aCount">Number of bytes to copy.</param>
    public void Copy(int aStart, byte[] aData, int aIndex, int aCount)
    {
        aData.AsSpan(aIndex, aCount).CopyTo(Span.Slice(aStart));
    }

    /// <summary>
    /// Copy data from a byte span into this memory block.
    /// </summary>
    /// <param name="aStart">Starting offset in this memory block.</param>
    /// <param name="aData">Source span.</param>
    public void Copy(int aStart, ReadOnlySpan<byte> aData)
    {
        aData.CopyTo(Span.Slice(aStart));
    }

    /// <summary>
    /// Copy data from an int array into this memory block.
    /// </summary>
    /// <param name="aStart">Starting byte offset in this memory block.</param>
    /// <param name="aData">Source int array.</param>
    /// <param name="aIndex">Starting index in source array.</param>
    /// <param name="aCount">Number of bytes to copy.</param>
    public void Copy(int aStart, int[] aData, int aIndex, int aCount)
    {
        var srcBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(aData.AsSpan(aIndex));
        srcBytes.Slice(0, aCount).CopyTo(Span.Slice(aStart));
    }

    /// <summary>
    /// Copy data from a uint span into this memory block.
    /// </summary>
    /// <param name="aStart">Starting byte offset in this memory block.</param>
    /// <param name="aData">Source uint span.</param>
    public void Copy(int aStart, ReadOnlySpan<uint> aData)
    {
        var srcBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(aData);
        srcBytes.CopyTo(Span.Slice(aStart));
    }

    /// <summary>
    /// Copy MemoryBlock into ManagedMemoryBlock
    /// </summary>
    /// <param name="block">MemoryBlock to copy.</param>
    public unsafe void Copy(MemoryBlock block)
    {
        var src = new ReadOnlySpan<byte>((byte*)block.Base, (int)block.Size);
        src.CopyTo(Span);
    }

    /// <summary>
    /// Copies data from the memory block to the specified array.
    /// </summary>
    /// <param name="aStart">The start index in the memory block (as int index, not byte offset).</param>
    /// <param name="aData">The array into which data will be copied.</param>
    /// <param name="aIndex">The starting index in the array where data will be copied.</param>
    /// <param name="aCount">The number of bytes to copy.</param>
    public void Get(int aStart, int[] aData, int aIndex, int aCount)
    {
        var srcSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, int>(Span);
        var srcBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(srcSpan.Slice(aStart));
        var destBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(aData.AsSpan(aIndex));
        srcBytes.Slice(0, aCount).CopyTo(destBytes);
    }

    /// <summary>
    /// Get a span view of a portion of the memory block.
    /// </summary>
    /// <param name="offset">Starting byte offset.</param>
    /// <param name="length">Length in bytes.</param>
    /// <returns>A span view of the specified region.</returns>
    public Span<byte> GetSpan(int offset, int length) => Span.Slice(offset, length);

    /// <summary>
    /// Get a span view of a portion of the memory block as uint values.
    /// </summary>
    /// <param name="byteOffset">Starting byte offset.</param>
    /// <param name="count">Number of uint values.</param>
    /// <returns>A span view of the specified region as uint.</returns>
    public Span<uint> GetUIntSpan(int byteOffset, int count)
    {
        return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(Span.Slice(byteOffset)).Slice(0, count);
    }

    /// <summary>
    /// Read 8-bit from the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <returns>Byte value.</returns>
    public byte Read8(uint aByteOffset) => Span[(int)aByteOffset];

    /// <summary>
    /// Write 8-bit to the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <param name="value">Value to write.</param>
    public void Write8(uint aByteOffset, byte value)
    {
        Span[(int)aByteOffset] = value;
    }

    /// <summary>
    /// Read 16-bit from the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <returns>UInt16 value.</returns>
    public ushort Read16(uint aByteOffset)
    {
        return System.Runtime.InteropServices.MemoryMarshal.Read<ushort>(Span.Slice((int)aByteOffset));
    }

    /// <summary>
    /// Write 16-bit to the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <param name="value">Value to write.</param>
    public void Write16(uint aByteOffset, ushort value)
    {
        System.Runtime.InteropServices.MemoryMarshal.Write(Span.Slice((int)aByteOffset), in value);
    }

    /// <summary>
    /// Read 32-bit from the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <returns>UInt32 value.</returns>
    public uint Read32(uint aByteOffset)
    {
        return System.Runtime.InteropServices.MemoryMarshal.Read<uint>(Span.Slice((int)aByteOffset));
    }

    /// <summary>
    /// Write 32-bit to the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <param name="value">Value to write.</param>
    public void Write32(uint aByteOffset, uint value)
    {
        System.Runtime.InteropServices.MemoryMarshal.Write(Span.Slice((int)aByteOffset), in value);
    }

    /// <summary>
    /// Read 64-bit from the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <returns>UInt64 value.</returns>
    public ulong Read64(uint aByteOffset)
    {
        return System.Runtime.InteropServices.MemoryMarshal.Read<ulong>(Span.Slice((int)aByteOffset));
    }

    /// <summary>
    /// Write 64-bit to the memory block.
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <param name="value">Value to write.</param>
    public void Write64(uint aByteOffset, ulong value)
    {
        System.Runtime.InteropServices.MemoryMarshal.Write(Span.Slice((int)aByteOffset), in value);
    }

    /// <summary>
    /// Write string to the memory block (as ASCII bytes).
    /// </summary>
    /// <param name="aByteOffset">Data offset.</param>
    /// <param name="value">Value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if string exceeds memory block bounds.</exception>
    public void WriteString(uint aByteOffset, string value)
    {
        if (value.Length + aByteOffset > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        var dest = Span.Slice((int)aByteOffset, value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            dest[i] = (byte)value[i];
        }
    }
}
