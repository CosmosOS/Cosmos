using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class Serial {
		public static void Write(byte aSerialIdx, string aData) {
			for (int i = 0; i < aData.Length; i++) {
				Hardware.Serial.WriteSerial(aSerialIdx, (byte)aData[i]);
			}
		}

		public static void WriteLine(byte aSerialIndex, string aText) {
			Write(aSerialIndex, aText);
			Write(aSerialIndex, "\r\n");
		}
	}
}