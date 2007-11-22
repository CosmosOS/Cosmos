using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public static class Interrupts {
		public static void HandleInterrupt_Default(uint aParam, uint aInterrupt) {
			Console.Write("Interrupt occurred. Interrupt = ");
			WriteNumber(aInterrupt, 32);
			Console.Write(", Param = ");
			WriteNumber(aParam, 32);
			Console.WriteLine("");
			if (aInterrupt >= 0x20 && aInterrupt <= 0x2F) {
				if (aInterrupt >= 0x28) {
					CPU.WriteByteToPort(0xA0, 0x20);
				}
				CPU.WriteByteToPort(0x20, 0x20);
			}
		}

		public static void HandleInterrupt_20(uint aParam) {
			Console.WriteLine("PIT IRQ occurred");
			CPU.WriteByteToPort(0x20, 0x20);
		}

		public static void IncludeAllHandlers() {
			bool xTest = false;
			if(xTest) {
				HandleInterrupt_Default(0, 0);
				HandleInterrupt_20(0);
			}
		}

		private static void WriteNumber(uint aValue, byte aBitCount) {
			uint xValue = aValue;
			byte xCurrentBits = aBitCount;
			Console.Write("0x");
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
						Console.Write(xDigitString);
						break;
				}
			}
		}

	}
}