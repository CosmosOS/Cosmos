using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2 {
	public class Serial: Hardware {
		private const ushort COM1 = 0x3F8;
        private const ushort COM2 = 0x2F8;
		private static bool[] _serialInited = new []{false, false};

		private static ushort GetSerialAddr(byte aSerialIdx) {
            if (aSerialIdx == 0) {
                return COM1;
            }
//            if(aSerialIdx==1) {
                return COM2;
            //}
            //throw new Exception("Serial port not available");
		}

		public static void InitSerial(byte aSerialIdx) {
            if (_serialInited[aSerialIdx+1]) {
                return;
            }
            ushort xComAddr = GetSerialAddr(aSerialIdx);
            IOWriteByte((ushort)(xComAddr + 1), 0x00);    // Disable all interrupts
            IOWriteByte((ushort)(xComAddr + 3), 0x80);    // Enable DLAB (set baud rate divisor)
            IOWriteByte((ushort)(xComAddr + 0), 0x0C);    // Set divisor to 3 (lo byte) 38400 baud
            IOWriteByte((ushort)(xComAddr + 1), 0x00);    //                  (hi byte)
            IOWriteByte((ushort)(xComAddr + 3), 0x03);    // 8 bits, no parity, one stop bit
            IOWriteByte((ushort)(xComAddr + 2), 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
            IOWriteByte((ushort)(xComAddr + 4), 0x0B);    // IRQs enabled, RTS/DSR set
            _serialInited[aSerialIdx + 1] = true;
		}

		private static int IsSerialTransmitEmpty(ushort aSerialAddr) {
			return (IOReadByte((ushort)(aSerialAddr + 5)) & 0x20);
		}

		public static void WriteSerial(byte aSerialIdx, byte aData) {
			ushort xSerialAddr = GetSerialAddr(aSerialIdx);
            if (!_serialInited[aSerialIdx + 1]) {
				InitSerial(aSerialIdx);
			}
			while (IsSerialTransmitEmpty(xSerialAddr) == 0) {
				;
			}
			IOWriteByte(xSerialAddr, aData);
		}
	}
}