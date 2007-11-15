using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class DebugUtil {
		private static bool mInitialized;
		private static void CheckInitialized() {
			if (!mInitialized) {
				mInitialized = true;
				Console.Write("Initializing COM1...");
				IO.InitIO();
				IO.InitSerial(0);
				SendMessage("Debug", "Comport initialized!");
			}
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.Debug_Write)]
		[GlueMethod(MethodType = GlueMethodTypeEnum.Debug_WriteLine)]
		public static void SendMessage(string aModule, string aData) {
			CheckInitialized();
			IO.WriteSerialString(0, "<Message Type=\"Info\" Module=\"");
			IO.WriteSerialString(0, aModule);
			IO.WriteSerialString(0, "\" String=\"");
			IO.WriteSerialString(0, aData);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendHandleIrq(byte aIRQ) {
			CheckInitialized();
			IO.WriteSerialString(0, "<HandleIRQ IRQ=\"");
			WriteNumber(aIRQ, 4);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendKeyboardScanCodeReceived(uint aChar) {
			CheckInitialized();
			IO.WriteSerialString(0, "<Keyboard_ScanCodeReceived Char=\"");
			WriteNumber(aChar, 32);
			IO.WriteSerialString(0, "\"/>");
		}

		public static void SendKeyboardCharReceived(KeyboardKeys aChar, bool aReleased) {
			CheckInitialized();
			IO.WriteSerialString(0, "<Keyboard_CharReceived Char=\"");
			WriteNumber((uint)aChar, 32);
			IO.WriteSerialString(0, "\" Released=\"");
			if (aReleased) {
				IO.WriteSerialString(0, "true");
			} else {
				IO.WriteSerialString(0, "false");
			}
			IO.WriteSerialString(0, "\"/>");
		}

		public static void SendError(string aModule, string aData) {
			CheckInitialized();
			IO.WriteSerialString(0, "<Message Type=\"Error\" Module=\"");
			IO.WriteSerialString(0, aModule);
			IO.WriteSerialString(0, "\" String=\"");
			IO.WriteSerialString(0, aData);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendWarning(string aModule, string aData) {
			CheckInitialized();
			IO.WriteSerialString(0, "<Message Type=\"Warning\" Module=\"");
			IO.WriteSerialString(0, aModule);
			IO.WriteSerialString(0, "\" String=\"");
			IO.WriteSerialString(0, aData);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendIDT_RegisterInterrupt(ushort aInterrupt) {
			CheckInitialized();
			IO.WriteSerialString(0, "<IDT_RegisterInterrupt Interrupt=\"");
			WriteNumber(aInterrupt);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendIDT_InterruptOccurred(ushort aInterrupt, uint aParam) {
			CheckInitialized();
			IO.WriteSerialString(0, "<IDT_InterruptOccurred Interrupt=\"");
			WriteNumber(aInterrupt);
			IO.WriteSerialString(0, "\" Param=\"");
			WriteNumber(aParam);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendMM_Init(uint aStart, uint aLength) {
			CheckInitialized();
			IO.WriteSerialString(0, "<MM_Init Start=\"");
			WriteNumber(aStart);
			IO.WriteSerialString(0, "\" Length=\"");
			WriteNumber(aLength);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendMM_Alloc(uint aStartAddr, uint aLength) {
			CheckInitialized();
			IO.WriteSerialString(0, "<MM_Alloc StartAddr=\"");
			WriteNumber(aStartAddr);
			IO.WriteSerialString(0, "\" Length=\"");
			WriteNumber(aLength);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		private static void WriteNumber(uint aValue) {
			WriteNumber(aValue, 32);
		}

		private static void WriteNumber(uint aValue, byte aBitCount) {
			uint xValue = aValue;
			byte xCurrentBits = aBitCount;
			IO.WriteSerialString(0, "0x");
			while (xCurrentBits >= 4) {
				xCurrentBits -= 4;
				byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
				string xDigitString = null;
				switch (xCurrentDigit) {
					case 0:
						xDigitString = "0";
						goto default;
					case 1:
						xDigitString = "1";
						goto default;
					case 2:
						xDigitString = "2";
						goto default;
					case 3:
						xDigitString = "3";
						goto default;
					case 4:
						xDigitString = "4";
						goto default;
					case 5:
						xDigitString = "5";
						goto default;
					case 6:
						xDigitString = "6";
						goto default;
					case 7:
						xDigitString = "7";
						goto default;
					case 8:
						xDigitString = "8";
						goto default;
					case 9:
						xDigitString = "9";
						goto default;
					case 10:
						xDigitString = "A";
						goto default;
					case 11:
						xDigitString = "B";
						goto default;
					case 12:
						xDigitString = "C";
						goto default;
					case 13:
						xDigitString = "D";
						goto default;
					case 14:
						xDigitString = "E";
						goto default;
					case 15:
						xDigitString = "F";
						goto default;
					default:
						IO.WriteSerialString(0, xDigitString);
						break;
				}
			}
		}

		public static void SendMultiBoot_MMap(BootInformationStruct.MMapEntry aEntry) {
			IO.WriteSerialString(0, "<BootInfo_MMap AddrLow=\"");
			WriteNumber(aEntry.AddrLow);
			IO.WriteSerialString(0, "\" AddrHigh=\"");
			WriteNumber(aEntry.AddrHigh);
			IO.WriteSerialString(0, "\" LengthLow=\"");
			WriteNumber(aEntry.LengthLow);
			IO.WriteSerialString(0, "\" LengthHigh=\"");
			WriteNumber(aEntry.LengthHigh);
			IO.WriteSerialString(0, "\" Type=\"");
			WriteNumber(aEntry.Type);
			IO.WriteSerialString(0, "\"/>\r\n");
		}

		public static void SendMM_MemChunkFound(uint aStart, uint aLength) {
			IO.WriteSerialString(0, "<MM_MemChunkFound Start=\"");
			WriteNumber(aStart);
			IO.WriteSerialString(0, "\" Length=\"");
			WriteNumber(aLength);
			IO.WriteSerialString(0, "\"/>\r\n");
		}
	}
}