using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class Serial: Hardware {
		private const ushort COM1 = 0x3F8;

		private static ushort GetSerialAddr(byte aSerialIdx) {
			return COM1;
		}

		public static void InitSerial(byte aSerialIdx) {
			ushort xComAddr = GetSerialAddr(aSerialIdx);
			IOWrite((ushort)(xComAddr + 1), 0x00);    // Disable all interrupts
			IOWrite((ushort)(xComAddr + 3), 0x80);    // Enable DLAB (set baud rate divisor)
			IOWrite((ushort)(xComAddr + 0), 0x03);    // Set divisor to 3 (lo byte) 38400 baud
			IOWrite((ushort)(xComAddr + 1), 0x00);    //                  (hi byte)
			IOWrite((ushort)(xComAddr + 3), 0x03);    // 8 bits, no parity, one stop bit
			IOWrite((ushort)(xComAddr + 2), 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
			IOWrite((ushort)(xComAddr + 4), 0x0B);    // IRQs enabled, RTS/DSR set
		}

		private static int IsSerialTransmitEmpty(ushort aSerialAddr) {
			return (IORead((ushort)(aSerialAddr + 5)) & 0x20);
		}

		public static void WriteSerial(byte aSerialIdx, byte aData) {
			ushort xSerialAddr = GetSerialAddr(aSerialIdx);
			while (IsSerialTransmitEmpty(xSerialAddr) == 0) {
				;
			}
			IOWrite(xSerialAddr, aData);
		}		
	}
}
