using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public static class DebugUtil {
        public static unsafe void LogInterruptOccurred(Interrupts.InterruptContext* aContext) {
            uint aInterrupt = aContext->Interrupt;
            Cosmos.Hardware.DebugUtil.StartLogging();
            Cosmos.Hardware.DebugUtil.WriteSerialString("<InterruptOccurred Interrupt=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->Interrupt, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" SS=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->SS, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" GS=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->GS, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" FS=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->FS, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" ES=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ES, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" DS=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->DS, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" CS=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->CS, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" ESI=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ESI, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EDI=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EDI, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EBP=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EBP, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" ESP=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ESP, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EBX=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EBX, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EDX=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EDX, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" ECX=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->ECX, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EAX=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EAX, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" Param=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->Param, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" EFlags=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->EFlags, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\" UserESP=\"");
            Cosmos.Hardware.DebugUtil.WriteNumber(aContext->UserESP, 32);
            Cosmos.Hardware.DebugUtil.WriteSerialString("\"/>\r\n");
            Cosmos.Hardware.DebugUtil.EndLogging();
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

		public static void WriteSerialString(string aData) {
			for (int i = 0; i < aData.Length; i++) {
				Serial.WriteSerial(1, (byte)aData[i]);
			}
			//Console.Write(aData);
		}
	}
}