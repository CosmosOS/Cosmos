//#define COSMOSDEBUG
using System;
using System.Runtime.CompilerServices;

namespace Cosmos.Core
{
    /// <summary>
    /// MemoryOperations class. Used to do operations on the memory.
    /// </summary>
    public unsafe class MemoryOperations
    {
        #region Fill

        /// <summary>
        /// Fill memory block. Plugged.
        /// </summary>
        /// <param name="aDest">A destination.</param>
        /// <param name="aValue">A data value.</param>
        /// <param name="aSize">A data size.</param>
        public static unsafe void Fill(byte* aDest, int aValue, uint aSize)
        {
            // Plugged but we use this for unit tests
            var bytes = BitConverter.GetBytes(aValue);
            for (int i = 0; i < aSize; i++)
            {
                aDest[i] = bytes[i % 4];
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(uint* dest, uint value, uint size)
        {
            Fill((byte*)dest, (int)value, size * 4);
        }

        /// <summary>
        /// Fill destination region with value.
        /// </summary>
        /// <param name="dest">Location</param>
        /// <param name="value">Value</param>
        /// <param name="size">Number of integers to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(int* dest, int value, uint size)
        {
            Fill((byte*)dest, value, size * 4);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(uint[] dest, uint value)
        {
            fixed (uint* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(int[] dest, int value)
        {
            fixed (int* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(ushort* dest, ushort value, uint size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x10001;
            Fill((byte*)dest, valueFiller, size * 2);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(short* dest, short value, uint size)
        {
            /* Broadcast 'value' to fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (ushort)value * 0x10001;
            Fill((byte*)dest, valueFiller, size * 2);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(ushort[] dest, ushort value)
        {
            fixed (ushort* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(byte* dest, byte value, uint size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = value * 0x1010101;
            Fill(dest, valueFiller, size);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte* dest, sbyte value, uint size)
        {
            /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
            int valueFiller = (byte)value * 0x1010101;
            Fill((byte*)dest, valueFiller, size);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(byte[] dest, byte value)
        {
            fixed (byte* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte[] dest, sbyte value)
        {
            fixed (sbyte* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Fill source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(sbyte[] dest, sbyte[] src)
        {
            fixed (sbyte* destPtr = dest)
            fixed (sbyte* srcPtr = src)
            {
                Copy(destPtr, srcPtr, (uint)dest.Length);
            }
        }
        #endregion Fill

        #region Copy
        /// <summary>
        /// Copy source to destination.
        /// plugged.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        public static unsafe void Copy(byte *dest, byte *src, uint size)
        {
            // Plugged
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(uint* dest, uint *src, uint size)
        {
            Copy((byte*)dest, (byte *)src, size * 4);
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(int* dest, int *src, uint size)
        {
            Copy((byte*)dest, (byte*)src, size * 4);
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(uint[] dest, uint[] src)
        {
            fixed (uint* destPtr = dest)
            fixed (uint *srcPtr = src)
            {
                Copy(destPtr, srcPtr, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(int[] dest, int[] src)
        {
            fixed (int* destPtr = dest)
            fixed (int* srcPtr = src)
            {
                Copy(destPtr, srcPtr, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ushort* dest, ushort* src, uint size)
        {
            Copy((byte*)dest, (byte*)src, size * 2);
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(short* dest, short* src, uint size)
        {
            Copy((byte*)dest, (byte*)src, size * 2);
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ushort[] dest, ushort[] src)
        {
            fixed (ushort* destPtr = dest)
            fixed (ushort* srcPtr = src)
            {
                Copy(destPtr, srcPtr, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                Fill(destPtr, value, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(sbyte* dest, sbyte *src, uint size)
        {
            Copy((byte*)dest, (byte*)src, size);
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(byte[] dest, byte[] src)
        {
            fixed (byte* destPtr = dest)
            fixed (byte* srcPtr = src)
            {
                Copy(destPtr, srcPtr, (uint)dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(byte[] aDest, int aDestOffset, byte[] aSrc, int aSrcOffset, int aCount)
        {
            fixed (byte* destPtr = &aDest[aDestOffset])
            fixed (byte* srcPtr = &aSrc[aSrcOffset])
            {
                Copy(destPtr, srcPtr, (uint)aCount);
            }
        }

        #endregion Copy
    }
}
