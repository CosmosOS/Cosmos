#define COSMOSDEBUG
using System;
using System.Runtime.CompilerServices;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.MemoryOperations
{
    [Plug(Target = typeof(Cosmos.Core.MemoryOperations))]
    public unsafe class MemoryOperationsImpl
    {
        unsafe public static void Fill(byte* dest, int value, int size)
        {
            //Console.WriteLine("Filling array of size " + size + " with value 0x" + value.ToString("X"));
            //Global.mDebugger.SendInternal("Filling array of size " + size + " with value " + value);

            /* For very little sizes (until 15 bytes) we hand unroll the loop */
            switch (size)
            {
                case 0:
                    return;

                case 1:
                    *dest = (byte)value;
                    return;

                case 2:
                    *(short*)dest = (short)value;
                    return;

                case 3:
                    *(short*)dest = (short)value;
                    *(dest + 2) = (byte)value;
                    return;

                case 4:
                    *(int*)dest = value;
                    return;

                case 5:
                    *(int*)dest = value;
                    *(dest + 4) = (byte)value;
                    return;

                case 6:
                    *(int*)dest = value;
                    *(short*)(dest + 4) = (short)value;
                    return;

                case 7:
                    *(int*)dest = value;
                    *(short*)(dest + 4) = (short)value;
                    *(dest + 6) = (byte)value;
                    return;

                case 8:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    return;

                case 9:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(dest + 8) = (byte)value;
                    return;

                case 10:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(short*)(dest + 8) = (short)value;
                    return;

                case 11:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(short*)(dest + 8) = (short)value;
                    *(dest + 10) = (byte)value;
                    return;

                case 12:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(int*)(dest + 8) = value;
                    return;

                case 13:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(int*)(dest + 8) = value;
                    *(dest + 12) = (byte)value;
                    return;

                case 14:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(int*)(dest + 8) = value;
                    *(short*)(dest + 12) = (byte)value;
                    return;

                case 15:
                    *(int*)dest = value;
                    *(int*)(dest + 4) = value;
                    *(int*)(dest + 8) = value;
                    *(short*)(dest + 12) = (short)value;
                    *(dest + 14) = (byte)value;
                    return;
            }

            /*
			 * OK size is >= 16 it does not make any sense to do it with primitive types as C#
			 * has not a Int128 type, the Int128 operations will be done in assembler but we can
			 * do yet in the Managed world the two things:
			 * 1. Check of how many blocks of 16 bytes size is composed
			 * 2. If there are reaming bytes (that is size is not a perfect multiple of size)
			 *    we do the Fill() using a simple managed for() loop of bytes
			 */
            int xBlocksNum;
            int xByteRemaining;

#if NETSTANDARD1_5
            xBlocksNum = size / 16;
            xByteRemaining = size % 16;
#else
            xBlocksNum = Math.DivRem(size, 16, out xByteRemaining);
#endif

            //Global.mDebugger.SendInternal("size " + size + " is composed of " + BlocksNum + " block of 16 bytes with " + ByteRemaining + " remainder");

            for (int i = 0; i < xByteRemaining; i++)
            {
                *(dest + i) = (byte)value;
            }

            /* Let's call the assembler version now to do the 16 byte block copies */
            Cosmos.Core.MemoryOperations.Fill16Blocks(dest + xByteRemaining, value, xBlocksNum);

            /*
             * If needed there is yet space of optimization here for example:
             * - you can check if size is a multiple of 64 and if yes use an yet more faster Fill64Blocks
             * - or if it is not so try to see if it is a multiple of 32
             * at point probably it would be better to move a lot of the logic to assembler
             */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(int[] dest, int value)
        {
            fixed (int* destPtr = dest)
            {
                Fill((byte*)destPtr, value, dest.Length * 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(ushort[] dest, ushort value)
        {
            fixed (ushort* destPtr = dest)
            {
                /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
                int valueFiller = value * 0x10001;
                Fill((byte*)destPtr, valueFiller, dest.Length * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(short[] dest, short value)
        {
            fixed (short* destPtr = dest)
            {
                /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
                int valueFiller = (ushort)value * 0x10001;
                Fill((byte*)destPtr, valueFiller, dest.Length * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void Fill(byte[] dest, byte value)
        {
            fixed (byte* destPtr = dest)
            {
                /* Broadcast 'value' fill all the integer register (0x42 --> 0x42424242) */
                int valueFiller = value * 0x1010101;
                Fill(destPtr, valueFiller, dest.Length);
            }
        }
    }
}
