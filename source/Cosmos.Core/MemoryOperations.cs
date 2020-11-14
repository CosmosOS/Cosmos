//#define COSMOSDEBUG
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
        /// <param name="dest">A destination.</param>
        /// <param name="value">A data value.</param>
        /// <param name="size">A data size.</param>
        public static unsafe void Fill(byte* dest, int value, int size)
        {
            // Plugged
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(uint* dest, uint value, int size)
        {
            Fill((byte*)dest, (int)value, size * 4);
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(int* dest, int value, int size)
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
                Fill(destPtr, value, dest.Length);
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
                Fill(destPtr, value, dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(ushort* dest, ushort value, int size)
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
        public static unsafe void Fill(short* dest, short value, int size)
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
                Fill(destPtr, value, dest.Length);
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
                Fill(destPtr, value, dest.Length);
            }
        }

        /// <summary>
        /// Fill data to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="value">Data value.</param>
        /// <param name="size">Data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill(byte* dest, byte value, int size)
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
        public static unsafe void Fill(sbyte* dest, sbyte value, int size)
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
                Fill(destPtr, value, dest.Length);
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
                Fill(destPtr, value, dest.Length);
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
                Copy(destPtr, srcPtr, dest.Length);
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
        public static unsafe void Copy(byte *dest, byte *src, int size)
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
        public static unsafe void Copy(uint* dest, uint *src, int size)
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
        public static unsafe void Copy(int* dest, int *src, int size)
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
                Copy(destPtr, srcPtr, dest.Length);
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
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(ushort* dest, ushort* src, int size)
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
        public static unsafe void Copy(short* dest, short* src, int size)
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
                Copy(destPtr, srcPtr, dest.Length);
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
                Fill(destPtr, value, dest.Length);
            }
        }

        /// <summary>
        /// Copy source to destination.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="size">Size of data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(sbyte* dest, sbyte *src, int size)
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
                Copy(destPtr, srcPtr, dest.Length);
            }
        }

        #endregion Copy
    }
}
