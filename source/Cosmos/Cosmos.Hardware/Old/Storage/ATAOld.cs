using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Storage {
	public class ATAOld: Hardware {
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
		private const ushort IDE_PORT_ALTERNATESTATUS = 0x3F8;
		private const ushort IDE_PORT_DEVICECONTROL = 0x3F8;
		private const ushort IDE_PORT_DRIVEADDRESS = 0x3F9;
		private const byte IDE_STATUSREG_ERR = 0x00000001;
		private const byte IDE_STATUSREG_IDX = 0x00000002;
		private const byte IDE_STATUSREG_CORR = 0x00000004;
		private const byte IDE_STATUSREG_DRQ = 0x00000008;
		private const byte IDE_STATUSREG_DSC = 0x00000010;
		private const byte IDE_STATUSREG_DWF = 0x00000020;
		private const byte IDE_STATUSREG_DRDY = 0x00000040;
		private const byte IDE_STATUSREG_BSY = 0x00000080;
		private const byte IDE_ERRORREG_ABRT = 0x00000004;

		private static ushort[] ATAControllerInfo;

		private const int Timeout = 25;

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
			//DebugUtil.SendMessage("ATA", "Primary Controller Ready");
			IOWriteByte((ushort)(ATAControllerInfo[0] + IDE_PORT_ERROR), 0);
			PrimaryControllerInterruptOccurred = true;
		}

		private static bool PrimaryControllerInterruptOccurred;
		private static bool SecondaryControllerInterruptOccurred;

		internal static void HandleInterruptSecondary() {
			DebugUtil.SendMessage("ATA", "Secondary Controller Ready");
			IOWriteByte((ushort)(ATAControllerInfo[1] + IDE_PORT_ERROR), 0);
			SecondaryControllerInterruptOccurred = true;
		}

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
			uint xSleepCount = Timeout;
			while (((IOReadByte((ushort)(xAddr + 0x206)) & 0x80) == 0x80) && xSleepCount>0) {
				mSleep(1);
				xSleepCount--;
			}
			if(xSleepCount == 0){
				Console.WriteLine("[ATA|WriteBlockNew] Failed 1");
				return false;
			}
			IOWriteByte((ushort)(xAddr + IDE_PORT_FEATURES), 0);
			IOWriteByte((ushort)(xAddr + IDE_PORT_SECTORCOUNT), 1);
			IOWriteByte((ushort)(xAddr + IDE_PORT_SECTORNUMBER), (byte)aBlock);
			IOWriteByte((ushort)(xAddr + IDE_PORT_CYLINDERLOW), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(xAddr + IDE_PORT_CYLINDERHIGH), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(xAddr + IDE_PORT_DRIVEHEAD), (byte)(0xE0 | (aDrive << 4) | (byte)(aBlock >> 24)));
			IOWriteByte(IDE_PORT_DEVICECONTROL, 0); // receive interrupts...
			IOWriteByte((ushort)(xAddr + IDE_PORT_COMMAND), 0x30);
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(xAddr + IDE_PORT_STATUS)) & 0x80) == 0x80) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (xSleepCount == 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Failed 2");
				return false;
			}
			if ((IOReadByte((ushort)(xAddr + 0x206)) & 0x1) != 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Not valid!");
				return false;
			}
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(xAddr + 0x206)) & 0x8) == 0x8) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (xSleepCount == 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Failed 3");
				return false;
			}
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
			IOWriteByte(IDE_PORT_DEVICECONTROL, 0); // receive interrupts...
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_COMMAND), 0x30);
			uint xSleepTimes = Timeout;
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

		public static unsafe bool ReadDataNew(byte aController, byte aDrive, int aBlock, ushort* aBuffer) {
			//DebugUtil.SendNumber("ATA", "ReadData, block", (uint)aBlock, 32);
			ushort xControllerAddr = ATAControllerInfo[aController];
			// 1) Read the status register of the primary or the secondary IDE controller. 
			// 2) The BSY and DRQ bits must be zero if the controller is ready. 
			int xSleepCount = 1000;
			while (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				Console.WriteLine("[ATA#2] Read failed");
				return false;
			}
			//3) Set the DEV bit to 0 for Drive0 and to 1 for Drive1 on the selected IDE controller using 
			//   the Device/Head register and wait for approximately 400 nanoseconds using some NOP perhaps. 
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_DRIVEHEAD), (byte)(aDrive << 4));
			//4) Read the status register again. 
			//5) The BSY and DRQ bits must be 0 again for you to know that the IDE controller and the selected IDE drive are ready. 
			xSleepCount = 1000;
			while (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				Console.WriteLine("[ATA#5] Read failed");
				return false;
			}
			// 6) Write the LBA28 address to the designated IDE registers. 
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_LBABITS0TO7), (byte)aBlock);
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_LBABITS8TO15), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_LBABITS16TO23), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_LBABITS24TO27), (byte)(0xE0 | (aDrive << 4) | ((byte)((aBlock >> 24) & 0x0F))));
			// 7) Set the Sector count using the Sector Count register. 
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_SECTORCOUNT), 1);
			//8) Issue the Read Sector(s) command. 
			IOWriteByte((ushort)(xControllerAddr + IDE_PORT_COMMAND), 0x20);
			// 9) Read the Error register. If the ABRT bit is set then the Read Sector(s) command 
			//    is not supported for that IDE drive. If the ABRT bit is not set, continue to the next step. 
			if ((IOReadByte((ushort)(xControllerAddr + IDE_PORT_ERROR)) & IDE_ERRORREG_ABRT) == IDE_ERRORREG_ABRT) {
				Console.WriteLine("[ATA#9] Read failed");
				return false;
			}
			// 10) If you want to receive interrupts after reading each sector, clear the nIEN bit in the 
			//     Device Control register. If you do not clear this bit then interrupts will not be generated 
			//     after the reading of each sector which might cause an infinite loop if you are waiting for them. 
			//     The Primary IDE Controller will generate IRQ14 and the secondary IDE controller generates IRQ 15. 
			IOWriteByte(IDE_PORT_DEVICECONTROL, 0); // receive interrupts...
			// 11) Read the Alternate Status Register (you may even ignore the value that is read) 
			IOReadByte(IDE_PORT_ALTERNATESTATUS);
			// 12) Read the Status register for the selected IDE Controller. 
			IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS));
			//13) Whenever a sector of data is ready to be read from the Data Register, the BSY bit 
			//    in the status register will be set to 0 and DRQ to 1 so you might want to wait until 
			//    those bits are set to the mentioned values before attempting to read from the drive. 
			xSleepCount = 1000;
			while (((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) && xSleepCount > 0) {
				xSleepCount--;
				mSleep(1);
			}
			if ((IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) {
				Console.WriteLine("[ATA#13] Read failed");
				return false;
			}
			//14) Read one sector from the IDE Controller 16-bits at a time using the IN or the INSW instructions. 
			for (uint i = 0; i < 256; i++) {
				ushort xValue = IOReadWord(xControllerAddr);
				aBuffer[i] = xValue;
			}
			// 15) See if you have to read one more sector. If yes, repeat from step 11 again. 

			//16) If you don't need to read any more sectors, read the Alternate Status Register and ignore the byte that you read. 
			IOReadByte(IDE_PORT_ALTERNATESTATUS);
			//17) Read the status register. When the status register is read, the IDE Controller will negate 
			//    the INTRQ and you will not have pending IRQs waiting to be detected. This is a MUST to read 
			//    the status register when you are done reading from IDE ports. 
			IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS));
			//DebugUtil.SendATA_BlockReceived(aController, aDrive, (uint)aBlock, aBuffer);
			return true;
		}

		public static unsafe bool ReadData(byte aController, byte aDrive, uint aBlock, ushort* aBuffer) {
			ushort xControllerAddr = ATAControllerInfo[aController];
			//Console.WriteLine("[ATA|ReadData] Start");
			IOWriteByte((ushort)(xControllerAddr + 1), 0);
			IOWriteByte((ushort)(xControllerAddr + 2), 1);
			IOWriteByte((ushort)(xControllerAddr + 3), (byte)aBlock);
			IOWriteByte((ushort)(xControllerAddr + 4), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(xControllerAddr + 5), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(xControllerAddr + 6), (byte)(0xE0 | (aDrive << 4) | ((byte)((aBlock >> 24) & 0x0F))));
			uint xSleepTimes = 1000;
			while (((IOReadByte((ushort)(xControllerAddr + 7)) & 0x80) != 0x80) && (xSleepTimes > 0)) {
				mSleep(1);
				xSleepTimes--;
			}
			if ((IOReadByte((ushort)(xControllerAddr + 7)) & 0x80) == 0x80) {
				Console.Write("State was ");
				WriteNumber(IOReadByte((ushort)(xControllerAddr + 7)), 8);
				Console.WriteLine("");
				return false;
			}
			IOWriteByte((ushort)(xControllerAddr + 7), 0x20);
			xSleepTimes = 1000;
			while (((IOReadByte((ushort)(xControllerAddr + 7)) & 0x80) == 0x80) && (xSleepTimes > 0)) {
				mSleep(1);
				xSleepTimes--;
			}
			if ((IOReadByte((ushort)(xControllerAddr + 7)) & 0x80) == 0x80) {
				Console.Write("State was ");
				WriteNumber(IOReadByte((ushort)(xControllerAddr + 7)), 8);
				Console.WriteLine("");
				return false;
			}
			IOReadByte((ushort)(xControllerAddr + IDE_PORT_STATUS));
			ReadingDone();
			return true;
		}

		private static void ReadingDone() {
			// do nothing
		}
	}
}