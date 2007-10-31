using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class IO {
		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IO_WriteByte)]
		public static void WriteToPort(ushort aPort, byte aData) {
			// implemented by OpCodeMap
		}
		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IO_ReadByte)]
		public static byte ReadFromPort(ushort aPort) {
			// implemented by OpCodeMap
			return 0;
		}

		public static void InitIO() {
			COM1 = 0x3F8;
		}

		private static ushort COM1;

		private static ushort GetSerialAddr(byte aSerialIdx) {
			return COM1;
		}

		/// <summary>
		/// Initializes a serial port.
		/// </summary>
		/// <param name="aSerialIdx">The zero-based index of the serial port to use</param>
		public static void InitSerial(byte aSerialIdx) {
			// todo: implement other com ports
			ushort xComAddr = GetSerialAddr(aSerialIdx);
			System.Diagnostics.Debugger.Break();
			WriteToPort((ushort)(xComAddr + 1), 0x00);    // Disable all interrupts
			WriteToPort((ushort)(xComAddr + 3), 0x80);    // Enable DLAB (set baud rate divisor)
			WriteToPort((ushort)(xComAddr + 0), 0x03);    // Set divisor to 3 (lo byte) 38400 baud
			WriteToPort((ushort)(xComAddr + 1), 0x00);    //                  (hi byte)
			WriteToPort((ushort)(xComAddr + 3), 0x03);    // 8 bits, no parity, one stop bit
			WriteToPort((ushort)(xComAddr + 2), 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
			WriteToPort((ushort)(xComAddr + 4), 0x0B);    // IRQs enabled, RTS/DSR set
		}

		private static int IsSerialTransmitEmpty(ushort aSerialAddr) {
			return (ReadFromPort((ushort)(aSerialAddr + 5)) & 0x20);
		}

		public static void WriteSerial(byte aSerialIdx, byte aData) {
			ushort xSerialAddr = GetSerialAddr(aSerialIdx);
			while (IsSerialTransmitEmpty(xSerialAddr) == 0) {
				;
			}
			WriteToPort(xSerialAddr, aData);
		}

		public static void WriteSerialString(byte aSerialIdx, string aData) {
			for (int i = 0; i < aData.Length; i++) {
				WriteSerial(aSerialIdx, (byte)aData[i]);
			}
		}
	}
}
