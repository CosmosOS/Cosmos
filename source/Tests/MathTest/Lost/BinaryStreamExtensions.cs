using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost
{
	public static class BinaryStreamExtensions
	{
		public static void WriteShort(this Stream destStream, short value)
		{
			ushort uval = (ushort)value;
			destStream.WriteByte((byte)(uval & 0xFFu));
			destStream.WriteByte((byte)(uval >> 8));
		}
		public static void WriteShort(this Stream destStream, long value)
		{
			short svalue = checked((short)value);
			destStream.WriteShort(svalue);
		}
		public static void WriteInt(this Stream destStream, int value)
		{
			uint uval = (uint)value;
			for(int i = 0; i < 4; i++)
			{
				destStream.WriteByte((byte)(uval & 0xFFu));
				uval >>= 8;
			}
		}
		public static void WriteInt(this Stream destStream, long value)
		{
			int ivalue = checked((int)value);
			destStream.WriteInt(ivalue);
		}
		public static void WriteLong(this Stream destStream, long value)
		{
			ulong uval = (ulong)value;
			for (int i = 0; i < 8; i++)
			{
				destStream.WriteByte((byte)(uval & 0xFFu));
				uval >>= 8;
			}
		}
		public static void WriteByte(this Stream destStream, int value)
		{
			byte bvalue = checked((byte)value);
			destStream.WriteByte(bvalue);
		}
		public static void WriteSByte(this Stream destStream, int value)
		{
			sbyte val = checked((sbyte)value);
			destStream.WriteByte((byte)val);
		}
		public static void WriteUShort(this Stream destStream, int value)
		{
			ushort val = checked((ushort)value);
			destStream.WriteShort((short)val);
		}
		public static void WriteSByte(this Stream destStream, long value)
		{
			sbyte val = checked((sbyte)value);
			destStream.WriteByte((byte)val);
		}
		public static void WriteByte(this Stream destStream, long value)
		{
			byte bvalue = checked((byte)value);
			destStream.WriteByte(bvalue);
		}
	}
}
