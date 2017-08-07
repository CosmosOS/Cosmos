using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2 {
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
			return FromBCD(IOReadByte(DataPort));
		}

		public static byte GetMinutes() {
			WaitForReady();
			IOWriteByte(AddressPort, 2);
			return FromBCD(IOReadByte(DataPort));
		}

		public static byte GetHours() {
			WaitForReady();
			IOWriteByte(AddressPort, 4);
            return FromBCD(IOReadByte(DataPort));
		}

        //ToDo convert this to an extension method for FromBCD in Cosmos.Kernel
        /// <summary>
        /// Converts a BCD coded value to hex coded 
        /// </summary>
        /// <param name="value">BCD coded</param>
        /// <returns>Hex coded</returns>
        private static byte FromBCD(byte value)
        {
            return (byte)(((value >> 4) & 0x0F) * 10 + (value & 0x0F));
        }
	}
}