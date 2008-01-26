using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class RTC: Hardware {
		private const ushort AddressPort = 0x70;
		private const ushort DataPort = 0x71;

		private static void WaitForReady() {
			do {
				IOWriteByte(AddressPort, 10);
			}
			while ((IOReadByte(DataPort) & 0x80) != 0);
		}

		public static byte GetSeconds() {
			WaitForReady();
			IOWriteByte(AddressPort, 0);
			return IOReadByte(DataPort);
		}

		public static byte GetMinutes() {
			WaitForReady();
			IOWriteByte(AddressPort, 2);
			return IOReadByte(DataPort);
		}

		public static byte GetHours() {
			WaitForReady();
			IOWriteByte(AddressPort, 4);
			byte xResult = IOReadByte(DataPort);
			return xResult;
		}
	}
}