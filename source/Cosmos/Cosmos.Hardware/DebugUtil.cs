//#define WRITE_TO_DEBUG
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public static class DebugUtil {
        public static void LogInterruptOccurred(ref Interrupts.InterruptContext aContext) {
            //StartLogging();
            //WriteSerialString("<InterruptOccurred Interrupt=\"");
            //WriteNumber(aContext.Interrupt,
            //            32);
            //WriteSerialString("\" CS=\"");
            //WriteNumber(aContext.CS,
            //            32);
            //WriteSerialString("\" ESI=\"");
            //WriteNumber(aContext.ESI,
            //            32);
            //WriteSerialString("\" EDI=\"");
            //WriteNumber(aContext.EDI,
            //            32);
            //WriteSerialString("\" EBP=\"");
            //WriteNumber(aContext.EBP,
            //            32);
            //WriteSerialString("\" ESP=\"");
            //WriteNumber(aContext.ESP,
            //            32);
            //WriteSerialString("\" EAX=\"");
            //WriteNumber(aContext.EAX,
            //            32);
            //WriteSerialString("\" EBX=\"");
            //WriteNumber(aContext.EBX,
            //            32);
            //WriteSerialString("\" ECX=\"");
            //WriteNumber(aContext.ECX,
            //            32);
            //WriteSerialString("\" EDX=\"");
            //WriteNumber(aContext.EDX,
            //            32);
            //WriteSerialString("\" Param=\"");
            //WriteNumber(aContext.Param,
            //            32);
            //WriteSerialString("\" EFlags=\"");
            //WriteNumber((uint)aContext.EFlags,
            //            32);
            //WriteSerialString("\" UserESP=\"");
            //WriteNumber(aContext.UserESP,
            //            32);
            //WriteSerialString("\" EIP=\"");
            //WriteNumber(aContext.EIP,
            //            32);
            //WriteSerialString("\"/>\r\n");
            //EndLogging();
        }

	    public static void Initialize() {
		}

		public static void StartLogging() {
			// placeholder, later on, we will need some kind of locking
		}

		public static void EndLogging() {
			// placeholder, later on, we will need some kind of locking
		}


		public static void SendMessage(string aModule, string aData) {
			StartLogging();
			WriteSerialString("<Message Module=\"");
			WriteSerialString(aModule);
			WriteSerialString("\" String=\"");
			WriteSerialString(aData);
			WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendError(string aModule, string aData) {
			StartLogging();
			WriteSerialString("<Error Module=\"");
			WriteSerialString(aModule);
		    WriteSerialString("\" String=\"");
			WriteSerialString(aData);
			WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static unsafe void SendATA_BlockReceived(byte aController, byte aDrive, uint aBlock, byte[] aValue) {
			StartLogging();
			for (int i = 0; i < 4; i++) {
				WriteSerialString("<ATA_BlockPartReceived");
				WriteNumber((uint)i, 8, false);
				WriteSerialString(" Controller=\"");
				WriteNumber(aController, 8);
				WriteSerialString("\" Drive=\"");
				WriteNumber(aDrive, 8);
				WriteSerialString("\" Block=\"");
				WriteNumber(aBlock, 24);
				WriteSerialString("\" Contents=\"0x");
				for (int j = 0; j < 128; j++) {
					WriteNumber(aValue[(i * 128) + j], 8, false);
				}
				WriteSerialString("\"/>\r\n");
			}
			EndLogging();
		}

		public static void ATA_ReadBlock(byte aStep, ushort aControllerAddres, byte aDrive, uint aBlock) {
			StartLogging();
			WriteSerialString("<ATA_ReadBlock");
			WriteNumber(aStep, 8);
			WriteSerialString(" ControllerAddress=\"");
			DebugUtil.WriteNumber(aControllerAddres, 16);
			WriteSerialString("\" Drive=\"");
			DebugUtil.WriteNumber(aDrive, 8);
			WriteSerialString("\" Block=\"");
			DebugUtil.WriteNumber(aBlock, 32);
			WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendNumber(string aModule, string aDescription, uint aNumber, byte aBits) {
			StartLogging();
			WriteSerialString("<Number Module=\"");
			WriteSerialString(aModule);
			WriteSerialString("\" Description=\"");
			WriteSerialString(aDescription);
			WriteSerialString("\" Number=\"");
			DebugUtil.WriteNumber(aNumber, aBits);
			WriteSerialString("\"/>\r\n");
			EndLogging();
		}

		public static void SendATA_BlockPartReceived(byte aController, byte aDrive, uint aBlock, byte aPart, ushort aValue) {
			StartLogging();
			WriteSerialString("<ATA_BlockPartReceived Controller=\"");
			WriteNumber(aController, 8);
			WriteSerialString("\" Drive=\"");
			WriteNumber(aDrive, 8);
			WriteSerialString("\" Block=\"");
			WriteNumber(aBlock, 24);
			WriteSerialString("\" Part=\"");
			WriteNumber(aPart, 8);
			WriteSerialString("\" Value=\"");
			WriteNumber(aValue, 16);
			WriteSerialString("\"/>");
			EndLogging();
		}

		public static void WriteNumber(uint aNumber, byte aBits) {
			WriteNumber(aNumber, aBits, true);
		}
		public static void WriteNumber(uint aNumber, byte aBits, bool aWritePrefix) {
			uint xValue = aNumber;
			byte xCurrentBits = aBits;
			if (aWritePrefix) {
				WriteSerialString("0x");
			}
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
						WriteSerialString(xDigitString);
						break;
				}
			}
		}


        public static void WriteBinary(string aModule, string aMessage, byte[] aValue)
        {
            WriteBinary(aModule, aMessage,aValue, 0, aValue.Length);
        }

        public static void WriteBinary(string aModule, string aMessage, byte[] aValue, int aIndex, int aLength)
        {
            StartLogging();
            WriteSerialString("<Binary Module=\"");
            WriteSerialString(aModule);
            WriteSerialString("\" Message=\"");
            WriteSerialString(aMessage);
            WriteSerialString("\" Value=\"");
            for (int i = 0; i < aLength; i++)
            {
                WriteNumber(aValue[aIndex + i], 8, false);
            }
            WriteSerialString("\"/>\r\n");
        }
        
        public static unsafe void WriteBinary(string aModule, string aMessage, byte* aValue, int aIndex, int aLength) {
						StartLogging();
            WriteSerialString("<Binary Module=\"");
            WriteSerialString(aModule);
            WriteSerialString("\" Message=\"");
            WriteSerialString(aMessage);
            WriteSerialString("\" Value=\"");
            for (int i = 0; i < aLength; i++)
            {
                WriteNumber(aValue[aIndex + i], 8, false);
            }
            WriteSerialString("\"/>\r\n");
        }

		public static void WriteSerialString(string aData) {
#if WRITE_TO_DEBUG
			for (int i = 0; i < aData.Length; i++) {
				Serial.WriteSerial(0, (byte)aData[i]);
			}
#endif
			//Console.Write(aData);
		}
	}
}