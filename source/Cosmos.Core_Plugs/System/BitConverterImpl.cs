using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(typeof(BitConverter))]
    public class BitConverterImpl
    {
        public static byte[] GetBytes(bool value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 1);

            byte[] r = new byte[1];
            r[0] = (value ? (byte)1 : (byte)0);
            return r;
        }

        public static byte[] GetBytes(char value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 2);

            return GetBytes((short)value);
        }

        public unsafe static byte[] GetBytes(short value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 2);

            byte[] bytes = new byte[2];
            fixed (byte* b = bytes)
                *((short*)b) = value;
            return bytes;
        }

        public unsafe static byte[] GetBytes(int value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            byte[] bytes = new byte[4];
            fixed (byte* b = bytes)
                *((int*)b) = value;
            return bytes;
        }

        public unsafe static byte[] GetBytes(long value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 8);

            byte[] bytes = new byte[8];
            fixed (byte* b = bytes)
                *((long*)b) = value;
            return bytes;
        }

        public static byte[] GetBytes(ushort value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 2);

            return GetBytes((short)value);
        }

        public static byte[] GetBytes(uint value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            return GetBytes((int)value);
        }

        public static byte[] GetBytes(ulong value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 8);

            return GetBytes((long)value);
        }

        public unsafe static byte[] GetBytes(float value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            return GetBytes(*(int*)&value);
        }

        public unsafe static byte[] GetBytes(double value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 8);

            return GetBytes(*(long*)&value);
        }

        public static unsafe short ToInt16(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowValueArgumentNull();
            if ((uint)startIndex >= value.Length)
                ThrowStartIndexArgumentOutOfRange();
            if (startIndex > value.Length - 2)
                ThrowValueArgumentTooSmall();
            Contract.EndContractBlock();

            fixed (byte* pbyte = &value[startIndex])
            {
                if (startIndex % 2 == 0)
                {
                    // data is aligned 
                    return *((short*)pbyte);
                }
                else if (BitConverter.IsLittleEndian)
                {
                    return (short)((*pbyte) | (*(pbyte + 1) << 8));
                }
                else
                {
                    return (short)((*pbyte << 8) | (*(pbyte + 1)));
                }
            }
        }

        public static unsafe int ToInt32(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowValueArgumentNull();
            if ((uint)startIndex >= value.Length)
                ThrowStartIndexArgumentOutOfRange();
            if (startIndex > value.Length - 4)
                ThrowValueArgumentTooSmall();
            Contract.EndContractBlock();

            fixed (byte* pbyte = &value[startIndex])
            {
                if (startIndex % 4 == 0)
                {
                    // data is aligned 
                    return *((int*)pbyte);
                }
                else if (BitConverter.IsLittleEndian)
                {
                    return (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                }
                else
                {
                    return (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
                }
            }
        }

        public static unsafe long ToInt64(byte[] value, int startIndex)
        {
            if (value == null)
                ThrowValueArgumentNull();
            if ((uint)startIndex >= value.Length)
                ThrowStartIndexArgumentOutOfRange();
            if (startIndex > value.Length - 8)
                ThrowValueArgumentTooSmall();
            Contract.EndContractBlock();

            fixed (byte* pbyte = &value[startIndex])
            {
                if (startIndex % 8 == 0)
                {
                    // data is aligned 
                    return *((long*)pbyte);
                }
                else if (BitConverter.IsLittleEndian)
                {
                    int i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                    int i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
                    return (uint)i1 | ((long)i2 << 32);
                }
                else
                {
                    int i1 = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
                    int i2 = (*(pbyte + 4) << 24) | (*(pbyte + 5) << 16) | (*(pbyte + 6) << 8) | (*(pbyte + 7));
                    return (uint)i2 | ((long)i1 << 32);
                }
            }
        }

        private static void ThrowValueArgumentNull()
        {
            throw new ArgumentNullException("value");
        }

        private static void ThrowStartIndexArgumentOutOfRange()
        {
            throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
        }

        private static void ThrowValueArgumentTooSmall()
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall", "value");
        }
    }
}
