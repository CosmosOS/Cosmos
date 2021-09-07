//#define COSMOSDEBUG
using System;

using Cosmos.Core;
using Cosmos.Core_Asm.MemoryOperations;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.MemoryOperations
{
    [Plug(Target = typeof(Cosmos.Core.MemoryOperations))]
    public static unsafe class MemoryOperationsImpl
    {
        [PlugMethod(Assembler = typeof(MemoryOperationsFill16BlocksAsm))]
        private static unsafe void Fill16Blocks(byte* dest, int value, int BlocksNum)
        {
            throw new NotImplementedException();
        }

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
            //Cosmos.Core.MemoryOperations.Fill16Blocks(dest + xByteRemaining, value, xBlocksNum);
            Fill16Blocks(dest + xByteRemaining, value, xBlocksNum);

            /*
             * If needed there is yet space of optimization here for example:
             * - you can check if size is a multiple of 64 and if yes use an yet more faster Fill64Blocks
             * - or if it is not so try to see if it is a multiple of 32
             * at that point probably it would be better to move a lot of the logic to assembler
             */
        }

        [PlugMethod(Assembler = typeof(MemoryOperationsCopy16BytesAsm))]
        private static unsafe void Copy16Bytes(byte* dest, byte* src)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(MemoryOperationsCopy32BytesAsm))]
        private static unsafe void Copy32Bytes(byte* dest, byte* src)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(MemoryOperationsCopy64BytesAsm))]
        private static unsafe void Copy64Bytes(byte* dest, byte* src)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(MemoryOperationsCopy128BytesAsm))]
        private static unsafe void Copy128Bytes(byte* dest, byte* src)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Assembler = typeof(MemoryOperationsCopy128BlocksAsm))]
        private static unsafe void Copy128Blocks(byte* dest, byte* src, int blockNums)
        {
            throw new NotImplementedException();
        }

        /*
         * Tiny memory copy with jump table optimized
         */
        unsafe private static void CopyTiny(byte* dest, byte* src, int size)
        {
            /* We do copy in reverse */
            byte* dd = dest + size;
            byte* ss = src  + size;

            switch (size)
            {
                case 64:
                    Copy64Bytes(dd - 64, ss - 64);
                    goto case 0;
                case 0:
                    break;

                case 65:
                    Copy64Bytes(dd - 65, ss - 65);
                    goto case 1;
                case 1:
                    dd[-1] = ss[-1];
                    break;

                case 66:
                    Copy64Bytes(dd - 66, ss - 66);
                    goto case 2;
                case 2:
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 67:
                    Copy64Bytes(dd - 67, ss - 67);
                    goto case 3;
                case 3:
                    *((ushort*)(dd - 3)) = *((ushort*)(ss - 3));
                    dd[-1] = ss[-1];
                    break;

                case 68:
                    Copy64Bytes(dd - 68, ss - 68);
                    goto case 4;
                case 4:
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 69:
                    Copy64Bytes(dd - 68, ss - 68);
                    goto case 5;
                case 5:
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 70:
                    Copy64Bytes(dd - 70, ss - 70);
                    goto case 6;
                case 6:
                    *((uint*)(dd - 6)) = *((uint*)(ss - 6));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 71:
                    Copy64Bytes(dd - 71, ss - 71);
                    goto case 7;
                case 7:
                    *((uint*)(dd - 7)) = *((uint*)(ss - 7));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 72:
                    Copy64Bytes(dd - 72, ss - 72);
                    goto case 8;
                case 8:
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 73:
                    Copy64Bytes(dd - 73, ss - 73);
                    goto case 9;
                case 9:
                    *((ulong*)(dd - 9)) = *((ulong*)(ss - 9));
                    dd[-1] = ss[-1];
                    break;

                case 74:
                    Copy64Bytes(dd - 74, ss - 74);
                    goto case 10;
                case 10:
                    *((ulong*)(dd - 10)) = *((ulong*)(ss - 10));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 75:
                    Copy64Bytes(dd - 75, ss - 75);
                    goto case 11;
                case 11:
                    *((ulong*)(dd - 11)) = *((ulong*)(ss - 11));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 76:
                    Copy64Bytes(dd - 76, ss - 76);
                    goto case 12;
                case 12:
                    *((ulong*)(dd - 12)) = *((ulong*)(ss - 12));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 77:
                    Copy64Bytes(dd - 77, ss - 77);
                    goto case 13;
                case 13:
                    *((ulong*)(dd - 13)) = *((ulong*)(ss - 13));
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 78:
                    Copy64Bytes(dd - 78, ss - 78);
                    goto case 14;
                case 14:
                    *((ulong*)(dd - 14)) = *((ulong*)(ss - 14));
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 79:
                    Copy64Bytes(dd - 79, ss - 79);
                    goto case 15;
                case 15:
                    *((ulong*)(dd - 15)) = *((ulong*)(ss - 15));
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 80:
                    Copy64Bytes(dd - 80, ss - 80);
                    goto case 16;
                case 16:
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 81:
                    Copy64Bytes(dd - 81, ss - 81);
                    goto case 17;
                case 17:
                    Copy16Bytes(dd - 17, ss - 17);
                    dd[-1] = ss[-1];
                    break;

                case 82:
                    Copy64Bytes(dd - 82, ss - 82);
                    goto case 18;
                case 18:
                    Copy16Bytes(dd - 18, ss - 18);
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 83:
                    Copy64Bytes(dd - 83, ss - 83);
                    goto case 19;
                case 19:
                    Copy16Bytes(dd - 19, ss - 19);
                    *((ushort*)(dd - 3)) = *((ushort*)(ss - 3));
                    dd[-1] = ss[-1];
                    break;

                case 84:
                    Copy64Bytes(dd - 84, ss - 84);
                    goto case 20;
                case 20:
                    Copy16Bytes(dd - 20, ss - 20);
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 85:
                    Copy64Bytes(dd - 85, ss - 85);
                    goto case 21;
                case 21:
                    Copy16Bytes(dd - 21, ss - 21);
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 86:
                    Copy64Bytes(dd - 86, ss - 86);
                    goto case 22;
                case 22:
                    Copy16Bytes(dd - 22, ss - 22);
                    *((uint*)(dd - 6)) = *((uint*)(ss - 6));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 87:
                    Copy64Bytes(dd - 87, ss - 87);
                    goto case 23;
                case 23:
                    Copy16Bytes(dd - 23, ss - 23);
                    *((uint*)(dd - 7)) = *((uint*)(ss - 7));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 88:
                    Copy64Bytes(dd - 88, ss - 88);
                    goto case 24;
                case 24:
                    Copy16Bytes(dd - 24, ss - 24);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 89:
                    Copy64Bytes(dd - 89, ss - 89);
                    goto case 25;
                case 25:
                    Copy16Bytes(dd - 25, ss - 25);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 90:
                    Copy64Bytes(dd - 90, ss - 90);
                    goto case 26;
                case 26:
                    Copy16Bytes(dd - 26, ss - 26);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 91:
                    Copy64Bytes(dd - 91, ss - 91);
                    goto case 27;
                case 27:
                    Copy16Bytes(dd - 27, ss - 27);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 92:
                    Copy64Bytes(dd - 92, ss - 92);
                    goto case 28;
                case 28:
                    Copy16Bytes(dd - 28, ss - 28);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 93:
                    Copy64Bytes(dd - 93, ss - 93);
                    goto case 29;
                case 29:
                    Copy16Bytes(dd - 29, ss - 29);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 94:
                    Copy64Bytes(dd - 94, ss - 94);
                    goto case 30;
                case 30:
                    Copy16Bytes(dd - 30, ss - 30);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 95:
                    Copy64Bytes(dd - 95, ss - 95);
                    goto case 31;
                case 31:
                    Copy16Bytes(dd - 31, ss - 31);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 96:
                    Copy64Bytes(dd - 96, ss - 96);
                    goto case 32;
                case 32:
                    Copy32Bytes(dd - 32, ss - 32);
                    break;

                case 97:
                    Copy64Bytes(dd - 97, ss - 97);
                    goto case 33;
                case 33:
                    Copy32Bytes(dd - 33, ss - 33);
                    dd[-1] = ss[-1];
                    break;

                case 98:
                    Copy64Bytes(dd - 98, ss - 98);
                    goto case 34;
                case 34:
                    Copy32Bytes(dd - 34, ss - 34);
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 99:
                    Copy64Bytes(dd - 99, ss - 99);
                    goto case 35;
                case 35:
                    Copy32Bytes(dd - 35, ss - 35);
                    *((ushort*)(dd - 3)) = *((ushort*)(ss - 3));
                    dd[-1] = ss[-1];
                    break;

                case 100:
                    Copy64Bytes(dd - 100, ss - 100);
                    goto case 36;
                case 36:
                    Copy32Bytes(dd - 36, ss - 36);
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 101:
                    Copy64Bytes(dd - 101, ss - 101);
                    goto case 37;
                case 37:
                    Copy32Bytes(dd - 37, ss - 37);
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 102:
                    Copy64Bytes(dd - 102, ss - 102);
                    goto case 38;
                case 38:
                    Copy32Bytes(dd - 38, ss - 38);
                    *((uint*)(dd - 6)) = *((uint*)(ss - 6));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 103:
                    Copy64Bytes(dd - 103, ss - 103);
                    goto case 39;
                case 39:
                    Copy32Bytes(dd - 39, ss - 39);
                    *((uint*)(dd - 7)) = *((uint*)(ss - 7));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 104:
                    Copy64Bytes(dd - 104, ss - 104);
                    goto case 40;
                case 40:
                    Copy32Bytes(dd - 40, ss - 40);
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 105:
                    Copy64Bytes(dd - 105, ss - 105);
                    goto case 41;
                case 41:
                    Copy32Bytes(dd - 41, ss - 41);
                    *((ulong*)(dd - 9)) = *((ulong*)(ss - 9));
                    dd[-1] = ss[-1];
                    break;

                case 106:
                    Copy64Bytes(dd - 106, ss - 106);
                    goto case 42;
                case 42:
                    Copy32Bytes(dd - 42, ss - 42);
                    *((ulong*)(dd - 10)) = *((ulong*)(ss - 10));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 107:
                    Copy64Bytes(dd - 107, ss - 107);
                    goto case 43;
                case 43:
                    Copy32Bytes(dd - 43, ss - 43);
                    *((ulong*)(dd - 11)) = *((ulong*)(ss - 11));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 108:
                    //Cosmos.Core.MemoryOperations.Copy64Bytes(dd - 108, ss - 108);
                    Copy64Bytes(dd - 108, ss - 108);
                    goto case 44;
                case 44:
                    Copy32Bytes(dd - 44, ss - 44);
                    *((ulong*)(dd - 12)) = *((ulong*)(ss - 12));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 109:
                    Copy64Bytes(dd - 109, ss - 109);
                    goto case 45;
                case 45:
                    Copy32Bytes(dd - 45, ss - 45);
                    *((ulong*)(dd - 13)) = *((ulong*)(ss - 13));
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 110:
                    Copy64Bytes(dd - 110, ss - 110);
                    goto case 46;
                case 46:
                    Copy32Bytes(dd - 46, ss - 46);
                    *((ulong*)(dd - 14)) = *((ulong*)(ss - 14));
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 111:
                    Copy64Bytes(dd - 111, ss - 111);
                    goto case 47;
                case 47:
                    Copy32Bytes(dd - 47, ss - 47);
                    *((ulong*)(dd - 15)) = *((ulong*)(ss - 15));
                    *((ulong*)(dd - 8)) = *((ulong*)(ss - 8));
                    break;

                case 112:
 
                    Copy64Bytes(dd - 112, ss - 112);
                    goto case 48;
                case 48:
                    Copy32Bytes(dd - 48, ss - 48);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 113:
                    Copy64Bytes(dd - 113, ss - 113);
                    goto case 49;
                case 49:
                    Copy32Bytes(dd - 49, ss - 49);
                    Copy16Bytes(dd - 17, ss - 17);
                    dd[-1] = ss[-1];
                    break;

                case 114:
                    Copy64Bytes(dd - 114, ss - 114);
                    goto case 50;
                case 50:
                    Copy32Bytes(dd - 50, ss - 50);
                    Copy16Bytes(dd - 18, ss - 18);
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 115:
                    Copy64Bytes(dd - 115, ss - 115);
                    goto case 51;
                case 51:
                    Copy32Bytes(dd - 51, ss - 51);
                    Copy16Bytes(dd - 19, ss - 19);
                    *((ushort*)(dd - 3)) = *((ushort*)(ss - 3));
                    dd[-1] = ss[-1];
                    break;

                case 116:
                    Copy64Bytes(dd - 116, ss - 116);
                    goto case 52;
                case 52:
                    Copy32Bytes(dd - 52, ss - 52);
                    Copy16Bytes(dd - 20, ss - 20);
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 117:
                    Copy64Bytes(dd - 117, ss - 117);
                    goto case 53;
                case 53:
                    Copy32Bytes(dd - 53, ss - 53);
                    Copy16Bytes(dd - 21, ss - 21);
                    *((uint*)(dd - 5)) = *((uint*)(ss - 5));
                    dd[-1] = ss[-1];
                    break;

                case 118:
                    Copy64Bytes(dd - 118, ss - 118);
                    goto case 54;
                case 54:
                    Copy32Bytes(dd - 54, ss - 54);
                    Copy16Bytes(dd - 22, ss - 22);
                    *((uint*)(dd - 6)) = *((uint*)(ss - 6));
                    *((ushort*)(dd - 2)) = *((ushort*)(ss - 2));
                    break;

                case 119:
                    Copy64Bytes(dd - 119, ss - 119);
                    goto case 55;
                case 55:
                    Copy32Bytes(dd - 55, ss - 55);
                    Copy16Bytes(dd - 23, ss - 23);
                    *((uint*)(dd - 7)) = *((uint*)(ss - 7));
                    *((uint*)(dd - 4)) = *((uint*)(ss - 4));
                    break;

                case 120:
                    Copy64Bytes(dd - 120, ss - 120);
                    goto case 56;
                case 56:
                    Copy32Bytes(dd - 56, ss - 56);
                    Copy16Bytes(dd - 24, ss - 24);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 121:
                    Copy64Bytes(dd - 121, ss - 121);
                    goto case 57;
                case 57:
                    Copy32Bytes(dd - 57, ss - 57);
                    Copy16Bytes(dd - 25, ss - 25);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 122:
                    Copy64Bytes(dd - 122, ss - 122);
                    goto case 58;
                case 58:
                    Copy32Bytes(dd - 58, ss - 58);
                    Copy16Bytes(dd - 26, ss - 26);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 123:
                    Copy64Bytes(dd - 123, ss - 123);
                    goto case 59;
                case 59:
                    Copy32Bytes(dd - 59, ss - 59);
                    Copy16Bytes(dd - 27, ss - 27);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 124:
                    Copy64Bytes(dd - 124, ss - 124);
                    goto case 60;
                case 60:
                    Copy32Bytes(dd - 60, ss - 60);
                    Copy16Bytes(dd - 28, ss - 28);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 125:
                    Copy64Bytes(dd - 125, ss - 125);
                    goto case 61;
                case 61:
                    Copy32Bytes(dd - 61, ss - 61);
                    Copy16Bytes(dd - 29, ss - 29);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 126:
                    Copy64Bytes(dd - 126, ss - 126);
                    goto case 62;
                case 62:
                    Copy32Bytes(dd - 62, ss - 62);
                    Copy16Bytes(dd - 30, ss - 30);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 127:
                    Copy64Bytes(dd - 127, ss - 127);
                    goto case 63;
                case 63:
                    Copy32Bytes(dd - 63, ss - 63);
                    Copy16Bytes(dd - 31, ss - 31);
                    Copy16Bytes(dd - 16, ss - 16);
                    break;

                case 128:
                    Copy128Bytes(dd - 128, ss - 128);
                    break;
            }
        }

        unsafe public static void Copy(byte* dest, byte* src, int size)
        {
            Global.mDebugger.SendInternal("Copying array of size " + size + " ...");

            if (size < 129)
            {
                Global.mDebugger.SendInternal("Size less than 129 bytes Calling CopyTiny...");
                CopyTiny(dest, src, size);
                Global.mDebugger.SendInternal("CopyTiny returned");
                return;
            }

            int xBlocksNum;
            int xByteRemaining;
            const int xBlockSize = 128;

#if NETSTANDARD1_5
            xBlocksNum = size / xBlockSize;
            xByteRemaining = size % xBlockSize;
#else
            xBlocksNum = Math.DivRem(size, xBlockSize, out xByteRemaining);
#endif
            Global.mDebugger.SendInternal($"size {size} is composed of {xBlocksNum} blocks of {xBlockSize} bytes with {xByteRemaining} remainder");

            // TODO call Copy128Blocks()
            for (int i = 0; i < xByteRemaining; i++)
            {
                *(dest + i) = *(src + i);
            }

            Global.mDebugger.SendInternal("Calling Copy128Blocks...");
            /* Let's call the assembler version now to do the 128 byte block copies */
            Copy128Blocks(dest + xByteRemaining, src + xByteRemaining, xBlocksNum);
            Global.mDebugger.SendInternal("Copy128Blocks returned");
        }
    }
}
