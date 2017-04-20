#define COSMOSDEBUG
using System.Runtime.CompilerServices;

namespace Cosmos.Core
{
    public unsafe class MemoryOperations
    {
        public static unsafe void Fill16Blocks(byte* dest, int value, int BlocksNum)
        {
            // Plugged
        }

        unsafe public static void Fill(byte* dest, int value, int size)
        {
            // Plugged
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(uint* dest, uint value, int size)
        {
            Fill((byte*)dest, (int)value, size);
        }

        unsafe public static void Fill(int* dest, int value, int size)
        {
            Fill((byte*)dest, value, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(uint[] dest, uint value)
        {
            fixed (uint* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length * 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(int[] dest, int value)
        {
            fixed (int* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length * 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(ushort* dest, ushort value, int size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x10001;
            Fill((byte*)dest, valueFiller, size);
        }

        unsafe public static void Fill(short* dest, short value, int size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (ushort)value * 0x10001;
            Fill((byte*)dest, valueFiller, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(ushort[] dest, ushort value)
        {
            fixed (ushort* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(byte* dest, byte value, int size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x1010101;
            Fill(dest, valueFiller, size);
        }

        unsafe public static void Fill(sbyte* dest, sbyte value, int size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (byte)value * 0x1010101;
            Fill((byte*)dest, valueFiller, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(byte[] dest, byte value)
        {
            fixed (byte* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(sbyte[] dest, sbyte value)
        {
            fixed (sbyte* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }
    }
}
