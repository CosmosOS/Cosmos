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
			ushort xComAddr = GetSerialAddr(aSerialIdx);
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

		public static void WriteSerialHexNumber(byte aSerialIdx, uint aNumber) {
			WriteNumber(aSerialIdx, aNumber, 16);
			WriteSerialString(aSerialIdx, "x0 (Reverse hex)");
		}

		private static void WriteNumber(byte aSerialIdx, uint aValue, byte aBase) {
			uint theValue = aValue;
			int xReturnedChars = 0;
			while (theValue > 0) {
				switch (theValue % aBase) {
					case 0: {
							WriteSerialString(aSerialIdx, "0");
							xReturnedChars++;
							break;
						}
					case 1: {
							WriteSerialString(aSerialIdx, "1");
							xReturnedChars++;
							break;
						}
					case 2: {
							WriteSerialString(aSerialIdx, "2");
							xReturnedChars++;
							break;
						}
					case 3: {
							WriteSerialString(aSerialIdx, "3");
							xReturnedChars++;
							break;
						}
					case 4: {
							WriteSerialString(aSerialIdx, "4");
							xReturnedChars++;
							break;
						}
					case 5: {
							WriteSerialString(aSerialIdx, "5");
							xReturnedChars++;
							break;
						}
					case 6: {
							WriteSerialString(aSerialIdx, "6");
							xReturnedChars++;
							break;
						}
					case 7: {
							WriteSerialString(aSerialIdx, "7");
							xReturnedChars++;
							break;
						}
					case 8: {
							WriteSerialString(aSerialIdx, "8");
							xReturnedChars++;
							break;
						}
					case 9: {
							WriteSerialString(aSerialIdx, "9");
							xReturnedChars++;
							break;
						}
					case 10: {
							WriteSerialString(aSerialIdx, "A");
							xReturnedChars++;
							break;
						}
					case 11: {
							xReturnedChars++;
							WriteSerialString(aSerialIdx, "B");
							break;
						}
					case 12: {
							WriteSerialString(aSerialIdx, "C");
							xReturnedChars++;
							break;
						}
					case 13: {
							WriteSerialString(aSerialIdx, "D");
							xReturnedChars++;
							break;
						}
					case 14: {
							WriteSerialString(aSerialIdx, "E");
							xReturnedChars++;
							break;
						}
					case 15: {
							WriteSerialString(aSerialIdx, "F");
							xReturnedChars++;
							break;
						}
				}
				theValue = theValue / aBase;
			}
			while (xReturnedChars < 8) {
				WriteSerialString(aSerialIdx, "0");
				xReturnedChars++;
			}
		}
	}
}