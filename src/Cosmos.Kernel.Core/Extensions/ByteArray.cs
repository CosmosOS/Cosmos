// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Extensions;

internal static class ByteArray
{
    public static byte Read8(this byte[] memory, uint offset)
    {
        ValidateRange(memory, offset, sizeof(byte));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                return *(ptr + offset);
            }
        }
    }

    public static void Write8(this byte[] memory, uint offset, byte value)
    {
        ValidateRange(memory, offset, sizeof(byte));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                *(ptr + offset) = value;
            }
        }
    }

    public static ushort Read16(this byte[] memory, uint offset)
    {
        ValidateRange(memory, offset, sizeof(ushort));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                return *(ushort*)(ptr + offset);
            }
        }
    }

    public static void Write16(this byte[] memory, uint offset, ushort value)
    {
        ValidateRange(memory, offset, sizeof(ushort));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                *(ushort*)(ptr + offset) = value;
            }
        }
    }

    public static uint Read32(this byte[] memory, uint offset)
    {
        ValidateRange(memory, offset, sizeof(uint));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                return *(uint*)(ptr + offset);
            }
        }
    }

    public static void Write32(this byte[] memory, uint offset, uint value)
    {
        ValidateRange(memory, offset, sizeof(uint));
        unsafe
        {
            fixed (byte* ptr = memory)
            {
                *(uint*)(ptr + offset) = value;
            }
        }
    }

    public static void WriteString(this byte[] memory, uint offset, string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (offset + value.Length > memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        for (int i = 0; i < value.Length; i++)
        {
            memory[offset + i] = (byte)value[i];
        }
    }

    public static void Fill(this byte[] memory, byte value)
    {
        for (int i = 0; i < memory.Length; i++)
        {
            memory[i] = value;
        }
    }

    public static void Fill(this byte[] memory, int start, int count, byte value)
    {
        ValidateRange(memory, (uint)start, (uint)count);
        for (int i = 0; i < count; i++)
        {
            memory[start + i] = value;
        }
    }

    private static void ValidateRange(byte[] memory, uint offset, uint size)
    {
        if (offset + size > memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset outside array bounds.");
        }
    }
}
