#define COSMOSDEBUG
using System.Runtime.CompilerServices;

namespace Cosmos.Core
{
    public unsafe class MemoryOperations
    {
        public static unsafe void Fill(byte* dest, int value, int size)
        {
            // Plugged
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(uint* dest, uint value, int size)
        {
            Fill((byte*)dest, (int)value, size * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(int* dest, int value, int size)
        {
            Fill((byte*)dest, value, size * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(uint[] dest, uint value)
        {
            fixed (uint* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(int[] dest, int value)
        {
            fixed (int* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(ushort* dest, ushort value, int size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x10001;
            Fill((byte*)dest, valueFiller, size * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(short* dest, short value, int size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (ushort)value * 0x10001;
            Fill((byte*)dest, valueFiller, size * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(ushort[] dest, ushort value)
        {
            fixed (ushort* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(byte* dest, byte value, int size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x1010101;
            Fill(dest, valueFiller, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte* dest, sbyte value, int size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (byte)value * 0x1010101;
            Fill((byte*)dest, valueFiller, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(byte[] dest, byte value)
        {
            fixed (byte* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte[] dest, sbyte value)
        {
            fixed (sbyte* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        public static unsafe void Copy(byte *dest, byte *src, int size)
        {
            // Plugged
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(uint* dest, uint *src, int size)
        {
            Copy((byte*)dest, (byte *)src, size * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(int* dest, int *src, int size)
        {
            Copy((byte*)dest, (byte*)src, size * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(uint[] dest, uint[] src)
        {
            fixed (uint* destPtr = dest)
            fixed (uint *srcPtr = src)
            {
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(int[] dest, int[] src)
        {
            fixed (int* destPtr = dest)
            fixed (int* srcPtr = src)
            {
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ushort* dest, ushort* src, int size)
        {
            Copy((byte*)dest, (byte*)src, size * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(short* dest, short* src, int size)
        {
            Copy((byte*)dest, (byte*)src, size * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ushort[] dest, ushort[] src)
        {
            fixed (ushort* destPtr = dest)
            fixed (ushort* srcPtr = src)
            {
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                Fill(destPtr, value, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(sbyte* dest, sbyte *src, int size)
        {
            Copy((byte*)dest, (byte*)src, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(byte[] dest, byte[] src)
        {
            fixed (byte* destPtr = dest)
            fixed (byte* srcPtr = src)
            {
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte[] dest, sbyte[] src)
        {
            fixed (sbyte* destPtr = dest)
            fixed (sbyte* srcPtr = src)
            {
                Copy(destPtr, srcPtr, dest.Length);
            }
        }
    }
}
