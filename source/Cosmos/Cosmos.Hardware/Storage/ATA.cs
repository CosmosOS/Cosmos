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
		private const uint Timeout = 5000;

		private readonly ushort mControllerAddress;
		private readonly byte mDrive;
		private static readonly ushort[] ATAControllerInfo = new ushort[] { 0x1F0, 0x170 };
		private static Action<uint> mSleep;
		public static void Initialize(Action<uint> aSleep) {
			mSleep = aSleep;
		}
		public ATA(int aController, byte aDrive) {
			mControllerAddress = ATAControllerInfo[aController];
			mDrive = aDrive;
		}

		public override uint BlockSize {
			get {
				return 512;
			}
		}

		public override unsafe bool ReadBlock(uint aBlock, byte* aBuffer) {
			DebugUtil.SendNumber("ATA", "ReadData, block", aBlock, 32);
			// 1) Read the status register of the primary or the secondary IDE controller. 
			// 2) The BSY and DRQ bits must be zero if the controller is ready. 
			uint xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				Console.WriteLine("[ATA#2] Read failed");
				return false;
			}
			//3) Set the DEV bit to 0 for Drive0 and to 1 for Drive1 on the selected IDE controller using 
			//   the Device/Head register and wait for approximately 400 nanoseconds using some NOP perhaps. 
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_DRIVEHEAD), (byte)(mDrive << 4));
			//4) Read the status register again. 
			//5) The BSY and DRQ bits must be 0 again for you to know that the IDE controller and the selected IDE drive are ready. 
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				Console.WriteLine("[ATA#5] Read failed");
				return false;
			}
			// 6) Write the LBA28 address to the designated IDE registers. 
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_LBABITS0TO7), (byte)aBlock);
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_LBABITS8TO15), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_LBABITS16TO23), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_LBABITS24TO27), (byte)(0xE0 | (mDrive << 4) | ((byte)((aBlock >> 24) & 0x0F))));
			// 7) Set the Sector count using the Sector Count register. 
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_SECTORCOUNT), 1);
			//8) Issue the Read Sector(s) command. 
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_COMMAND), 0x20);
			// 9) Read the Error register. If the ABRT bit is set then the Read Sector(s) command 
			//    is not supported for that IDE drive. If the ABRT bit is not set, continue to the next step. 
			if ((IOReadByte((ushort)(mControllerAddress + IDE_PORT_ERROR)) & IDE_ERRORREG_ABRT) == IDE_ERRORREG_ABRT) {
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
			IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS));
			//13) Whenever a sector of data is ready to be read from the Data Register, the BSY bit 
			//    in the status register will be set to 0 and DRQ to 1 so you might want to wait until 
			//    those bits are set to the mentioned values before attempting to read from the drive. 
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) && xSleepCount != 0) {
				xSleepCount--;
				mSleep(1);
			}
			if ((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) {
				Console.WriteLine("[ATA#13] Read failed");
				return false;
			}
			//14) Read one sector from the IDE Controller 16-bits at a time using the IN or the INSW instructions. 
			ushort* xBuffer = (ushort*)aBuffer;
			for (uint i = 0; i < 256; i++) {
				ushort xValue = IOReadWord(mControllerAddress);
				xBuffer[i] = xValue;
			}
			// 15) See if you have to read one more sector. If yes, repeat from step 11 again. 

			//16) If you don't need to read any more sectors, read the Alternate Status Register and ignore the byte that you read. 
			IOReadByte(IDE_PORT_ALTERNATESTATUS);
			//17) Read the status register. When the status register is read, the IDE Controller will negate 
			//    the INTRQ and you will not have pending IRQs waiting to be detected. This is a MUST to read 
			//    the status register when you are done reading from IDE ports. 
			IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS));
			//DebugUtil.SendATA_BlockReceived(aController, aDrive, (uint)aBlock, aBuffer);
			return true;
		}

		public override bool WriteBlock(uint aBlock, byte[] aData) {
			uint xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + 0x206)) & 0x80) == 0x80) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (xSleepCount == 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Failed 1");
				return false;
			}
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_FEATURES), 0);
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_SECTORCOUNT), 1);
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_SECTORNUMBER), (byte)aBlock);
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_CYLINDERLOW), (byte)(aBlock >> 8));
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_CYLINDERHIGH), (byte)(aBlock >> 16));
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_DRIVEHEAD), (byte)(0xE0 | (mDrive << 4) | (byte)(aBlock >> 24)));
			IOWriteByte(IDE_PORT_DEVICECONTROL, 0); // receive interrupts...
			IOWriteByte((ushort)(mControllerAddress + IDE_PORT_COMMAND), 0x30);
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + IDE_PORT_STATUS)) & 0x80) == 0x80) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (xSleepCount == 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Failed 2");
				return false;
			}
			if ((IOReadByte((ushort)(mControllerAddress + 0x206)) & 0x1) != 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Not valid!");
				return false;
			}
			xSleepCount = Timeout;
			while (((IOReadByte((ushort)(mControllerAddress + 0x206)) & 0x8) == 0x8) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (xSleepCount == 0) {
				Console.WriteLine("[ATA|WriteBlockNew] Failed 3");
				return false;
			}
			for (int i = 0; i < 256; i++) {
				IOWriteWord(mControllerAddress, (ushort)((aData[i * 2]) | (aData[i * 2 + 1] << 8)));
			}
			return true;
		}
	}
}