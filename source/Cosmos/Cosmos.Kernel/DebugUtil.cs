using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public static class DebugUtil {
		public static void Initialize() {
			Hardware.DebugUtil.Initialize();
		}

		private static void StartLogging() {
			Hardware.DebugUtil.StartLogging();
		}

		private static void EndLogging() {
			Hardware.DebugUtil.EndLogging();
		}

		public static void SendMessage(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Info\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendError(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Error\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendWarning(string aModule, string aData) {
			StartLogging();
			Serial.Write(0, "<Message Type=\"Warning\" Module=\"");
			Serial.Write(0, aModule);
			Serial.Write(0, "\" String=\"");
			Serial.Write(0, aData);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

		public static void SendMM_Alloc(uint aStartAddr, uint aLength) {
			StartLogging();
			Serial.Write(0, "<MM_Alloc StartAddr=\"");
			Hardware.DebugUtil.WriteNumber(aStartAddr, 32);
			Serial.Write(0, "\" Length=\"");
			Hardware.DebugUtil.WriteNumber(aLength, 32);
			Serial.Write(0, "\"/>\r\n");
			EndLogging();
		}

 	}
}