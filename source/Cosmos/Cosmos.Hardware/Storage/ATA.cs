using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Storage {
	public class ATA: Storage {
		private const byte IDE_PORT_DATA = 0x00000000;
		private const byte IDE_PORT_ERROR = 0x00000001;
		private const byte IDE_PORT_FEATURES = 0x00000001;
		private const byte IDE_PORT_SECTORCOUNT = 0x00000002;
		private const byte IDE_PORT_SECTORNUMBER = 0x00000003;
		private const byte IDE_PORT_LBABITS0TO7 = 0x00000003;
		private const byte IDE_PORT_CYLINDERLOW = 0x00000004;
		private const byte IDE_PORT_LBABITS8TO15 = 0x00000004;
		private const byte IDE_PORT_CYLINDERHIGH = 0x00000005;
		private const byte IDE_PORT_LBABITS16TO23 = 0x00000005;
		private const byte IDE_PORT_DRIVEHEAD = 0x00000006;
		private const byte IDE_PORT_LBABITS24TO27 = 0x00000006;
		private const byte IDE_PORT_STATUS = 0x00000007;
		private const byte IDE_PORT_COMMAND = 0x00000007;
		private const byte IDE_STATUSREG_ERR = 0x00000001;
		private const byte IDE_STATUSREG_IDX = 0x00000002;
		private const byte IDE_STATUSREG_CORR = 0x00000004;
		private const byte IDE_STATUSREG_DRQ = 0x00000008;
		private const byte IDE_STATUSREG_DSC = 0x00000010;
		private const byte IDE_STATUSREG_DWF = 0x00000020;
		private const byte IDE_STATUSREG_DRDY = 0x00000040;
		private const byte IDE_STATUSREG_BSY = 0x00000080;

		private static ushort[] ATAControllerInfo;

		public static void WriteNumber(uint aNumber, byte aBits) {
			uint xValue = aNumber;
			byte xCurrentBits = aBits;
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

		private static Action<uint> mSleep;

		internal static void HandleInterruptPrimary() {
			DebugUtil.SendMessage("ATA", "Primary Controller Ready");
			Console.WriteLine("Primary Controller Ready");
			IOWriteByte((ushort)(ATAControllerInfo[0] + IDE_PORT_ERROR), 0);
			PrimaryControllerInterruptCount++;
		}

		private static uint PrimaryControllerInterruptCount = 0;
		private static uint SecondaryControllerInterruptCount = 0;

		internal static void HandleInterruptSecondary() {
			DebugUtil.SendMessage("ATA", "Secondary Controller Ready");
			Console.WriteLine("Secondary Controller Ready");
			IOWriteByte((ushort)(ATAControllerInfo[1] + IDE_PORT_ERROR), 0);
			SecondaryControllerInterruptCount++;
		}

		//private static ushort mController;

		public static void Initialize(Action<uint> aSleep) {
			mSleep = aSleep;
			ATAControllerInfo = new ushort[] { 0x1F0, 0x170 };
			string[] xControllerDescriptions = new string[] { "Primary", "Secondary" };
			string[] xDrivesDescription = new string[] { "Master", "Slave" };
			for (int i = 0; i < ATAControllerInfo.Length; i++) {
				ushort xController = ATAControllerInfo[i];
				for (byte xDrive = 0; xDrive < 2; xDrive++) {
					IOWriteByte(0x1F0 + IDE_PORT_DRIVEHEAD, (byte)(0xA0 | (xDrive << 4)));
					mSleep(5);
					byte xValue = IOReadByte((ushort)(xController + IDE_PORT_STATUS));
					if ((xValue & 0x40) == 0x40) {
						Console.Write("[ATA] ");
						Console.Write(xControllerDescriptions[i]);
						Console.Write(" ");
						Console.Write(xDrivesDescription[xDrive]);
						Console.Write(" exists (");
						WriteNumber(xValue, 8);
						Console.WriteLine(")");
					}
				}
			}
		}

		public static bool WriteBlockNew(byte aController, byte aDrive, uint aBlock, byte[] aData) {
			ushort xAddr = ATAControllerInfo[aController];
			while ((IOReadByte((ushort)(xAddr + 0x206)) & 0x80) == 0x80)
				;
			IOWriteByte((ushort)(xAddr + IDE_PORT_FEATURES), 0);
			IOWriteByte((ushort)(xAddr + IDE_PORT_SECTORCOUNT), 1);
			IOWriteByte((ushort)(xAddr + IDE_PORT_SECTORNUMBER), (byte)aBlock);
			IOWriteByte((ushort)(xAddr + IDE_PORT_CYLINDERLOW), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(xAddr + IDE_PORT_CYLINDERHIGH), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(xAddr + IDE_PORT_DRIVEHEAD), (byte)(0xE0 | (aDrive << 4) | (byte)(aBlock >> 24)));
			IOWriteByte((ushort)(xAddr + IDE_PORT_COMMAND), 0x30);
			while ((IOReadByte((ushort)(xAddr + IDE_PORT_STATUS)) & 0x80) == 0x80)
				;
			if ((IOReadByte((ushort)(xAddr + 0x206)) & 0x1) != 0) {
				Console.WriteLine("[ATA-Write] Not valid!");
				return false;
			}
			while ((IOReadByte((ushort)(xAddr + 0x206)) & 0x8) == 0x8)
				;
			for (int i = 0; i < 256; i++) {
				IOWriteWord(xAddr, (ushort)((aData[i * 2]) | (aData[i * 2 + 1] << 8)));
			}
			return true;
		}

		public static bool WriteBlock(byte aController, byte aDrive, uint aBlock, byte[] aData) {
			if (aData.Length != 512) {
				Console.WriteLine("Incorrect buffer size!");
				return false;
			}
			Console.WriteLine("Writing Write command data");
			ushort xControllerAddr = ATAControllerInfo[aController];
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_FEATURES), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_SECTORCOUNT), 1);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_SECTORNUMBER), (byte)aBlock);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERLOW), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERHIGH), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_DRIVEHEAD), (byte)(0x20 | (aDrive << 4) | ((byte)((aBlock >> 24) & 0xF))));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_COMMAND), 0x30);
			uint xSleepTimes = 1000;
			mSleep(2);
			while (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & 0x8) != 0x8) && xSleepTimes > 0) {
				mSleep(1);
				xSleepTimes--;
			}
			if (xSleepTimes == 0) {
				byte xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS));
				Console.Write("Device state: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				return false;
			}
			for (int i = 0; i < 256; i++) {
				ushort xValue = (ushort)(aData[i * 2] | (aData[i * 2 + 1] << 8));
				IOWriteWord(xControllerAddr, xValue);
			}
			return true;
		}

		public static bool FlushCaches(byte aController, byte aDrive) {
			ushort xControllerAddr = ATAControllerInfo[aController];
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_FEATURES), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_SECTORCOUNT), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_SECTORNUMBER), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERLOW), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERHIGH), 0);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_DRIVEHEAD), (byte)(aDrive << 4));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_COMMAND), 0xE7);
			uint xSleepTimes = 1000;
			mSleep(2);
			while (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & 0x20) != 020) && xSleepTimes > 0) {
				mSleep(1);
				xSleepTimes--;
			}
			if (xSleepTimes == 0) {
				byte xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS));
				Console.WriteLine("Device State:");
				Console.Write("    Status: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_ERROR));
				Console.Write("    Error: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_SECTORNUMBER));
				Console.Write("    Sector Number: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERLOW));
				Console.Write("    Cylinder Low: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_CYLINDERHIGH));
				Console.Write("    Cylinder Low: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				xStatus = IOReadByte((ushort)(xControllerAddr + IDE_PORT_DRIVEHEAD));
				Console.Write("    Device/Head: ");
				WriteNumber(xStatus, 8);
				Console.WriteLine("");
				return false;
			}
			return true;
		}

		private static void ResetDevice(ushort aController, byte aDrive) {
			IOWriteByte((ushort)(aController + IDE_PORT_DRIVEHEAD), (byte)(aDrive << 4));
			byte xError = IOReadByte((ushort)(aController + IDE_PORT_ERROR));
		}

		private static byte[] ReadDataOld(ushort aController, byte aDrive, uint aBlock) {
			//Console.WriteLine("[ATA|ReadData] Start");
			IOWriteByte((ushort)(aController + 1), 0);
			IOWriteByte((ushort)(aController + 2), 1);
			IOWriteByte((ushort)(aController + 3), (byte)aBlock);
			IOWriteByte((ushort)(aController + 4), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(aController + 5), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(aController + 6), (byte)(0xE0 | (aDrive << 4) | ((byte)((aBlock >> 24) & 0x0F))));
			uint xCurrentInterruptCount;
			if (aController == ATAControllerInfo[0]) {
				xCurrentInterruptCount = PrimaryControllerInterruptCount;
			} else {
				xCurrentInterruptCount = SecondaryControllerInterruptCount;
			}
			IOWriteByte((ushort)(aController + 7), 0x20);
			if (aController == ATAControllerInfo[0]) {
				while (PrimaryControllerInterruptCount == xCurrentInterruptCount)
					;
			} else {
				while (SecondaryControllerInterruptCount == xCurrentInterruptCount)
					;
			}
			if ((IOReadByte((ushort)(aController + 7)) & 0x8) != 0x8)
				return null;
			byte[] xResult = new byte[512];
			for (uint i = 0; i < 256; i++) {
				ushort xValue = IOReadWord(aController);
				xResult[i * 2] = (byte)xValue;
				//    aData[(i * 2) + 1] = (byte)(xValue >> 8);
			}
			IOReadByte((ushort)(aController + IDE_PORT_STATUS));
			ReadingDone();
			return xResult;
		}

		private static void ReadingDone() {
			// do nothing
		}
	}
}