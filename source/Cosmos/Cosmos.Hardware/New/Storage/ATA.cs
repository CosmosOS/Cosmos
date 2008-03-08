using System;
using System.Collections.Generic;

namespace Cosmos.Hardware.New.Storage {
	public partial class ATA: BlockDevice {
		private static Action<uint> mSleep;
		private readonly string mName;
		private readonly byte mControllerIndex;
		private readonly ushort mController;
		private readonly ushort mController2;
		private readonly ushort mController_Data;
		private readonly ushort mController_Error;
		private readonly ushort mController_FeatureReg;
		private readonly ushort mController_SectorCount;
		private readonly ushort mController_SectorNumber;
		private readonly ushort mController_CylinderLow;
		private readonly ushort mController_CylinderHigh;
		private readonly ushort mController_DeviceHead;
		private readonly ushort mController_PrimaryStatus;
		private readonly ushort mController_Command;
		private readonly ushort mController_AlternateStatus;
		private readonly ushort mController_DeviceControl;
		private readonly ushort mController_DeviceAddress;
		private readonly bool mIsPrimary;
		/* 
#define CB_DATA  0   // data reg         in/out pio_base_addr1+0
#define CB_ERR   1   // error            in     pio_base_addr1+1
#define CB_FR    1   // feature reg         out pio_base_addr1+1
#define CB_SC    2   // sector count     in/out pio_base_addr1+2
#define CB_SN    3   // sector number    in/out pio_base_addr1+3
#define CB_CL    4   // cylinder low     in/out pio_base_addr1+4
#define CB_CH    5   // cylinder high    in/out pio_base_addr1+5
#define CB_DH    6   // device head      in/out pio_base_addr1+6
#define CB_STAT  7   // primary status   in     pio_base_addr1+7
#define CB_CMD   7   // command             out pio_base_addr1+7
#define CB_ASTAT 8   // alternate status in     pio_base_addr2+6
#define CB_DC    8   // device control      out pio_base_addr2+6
#define CB_DA    9   // device address   in     pio_base_addr2+7*/
		private readonly byte mDrive;
		private ATA(string aName, byte aController, byte aDrive) {
			DebugUtil.SendMessage("ATA", aName);
			mName = aName;
			mControllerIndex = aController;
			mController = mControllerAddresses1[aController];
			mController2 = mControllerAddresses2[aController];
			DebugUtil.SendNumber("ATA", "Primary address", mController, 16);
			DebugUtil.SendNumber("ATA", "Secondary address", mController2, 16);
			mType = DeviceType.Storage;
			mDrive = aDrive;
			mController_Data = mController;
			mIsPrimary = aController == 0;
			mController_Error = mController;
			//mController_Error += 1;
			DebugUtil.SendNumber("ATA", "mController_Error", mController_Error, 16);
			mController_FeatureReg = (ushort)(mController + 1);
			DebugUtil.SendNumber("ATA", "mController_FeatureReg", mController_FeatureReg, 16);
			mController_SectorCount = (ushort)(mController + 2);
			DebugUtil.SendNumber("ATA", "mController_SectorCount", mController_SectorCount, 16);
			mController_SectorNumber = (ushort)(mController + 3);
			DebugUtil.SendNumber("ATA", "mController_SectorNumber", mController_SectorNumber, 16);
			mController_CylinderLow = (ushort)(mController + 4);
			DebugUtil.SendNumber("ATA", "mController_CylinderLow", mController_CylinderLow, 16);
			mController_CylinderHigh = (ushort)(mController + 5);
			DebugUtil.SendNumber("ATA", "mController_CylinderHigh", mController_CylinderHigh, 16);
			mController_DeviceHead = (ushort)(mController + 6);
			DebugUtil.SendNumber("ATA", "mController_DeviceHead", mController_DeviceHead, 16);
			mController_PrimaryStatus = (ushort)(mController + 7);
			DebugUtil.SendNumber("ATA", "mController_PrimaryStatus", mController_PrimaryStatus, 16);
			mController_Command = (ushort)(mController + 7);
			DebugUtil.SendNumber("ATA", "mController_Command", mController_Command, 16);
			mController_AlternateStatus = (ushort)(mController2 + 8);
			DebugUtil.SendNumber("ATA", "mController_AlternateStatus", mController_AlternateStatus, 16);
			mController_DeviceControl = (ushort)(mController2 + 8);
			DebugUtil.SendNumber("ATA", "mController_DeviceControl", mController_DeviceControl, 16);
			mController_DeviceAddress = (ushort)(mController2 + 9);
			DebugUtil.SendNumber("ATA", "mController_DeviceAddress", mController_DeviceAddress, 16);
			DebugUtil.SendNumber("ATA", "Primary address", mController, 16);
			DebugUtil.SendNumber("ATA", "Secondary address", mController2, 16);
			IOWriteByte(mController_DeviceControl, 0);
		}

		private void Initialize() {
		}

		public static void HandleInterruptSecondary() {
			mSecondaryInterruptCount++;
			IOReadByte((ushort)(mControllerAddresses1[0] + 7));
		}

		private static uint mSecondaryInterruptCount;

		public static void HandleInterruptPrimary() {
			mPrimaryInterruptCount++;
			IOReadByte((ushort)(mControllerAddresses1[1] + 7));
		}

		private static uint mPrimaryInterruptCount;

		private uint mBlockCount;

		public static void Initialize(Action<uint> aSleep) {
			if (aSleep == null) {
				throw new ArgumentNullException("aSleep");
			}
			mSleep = aSleep;
			DebugUtil.SendMessage("ATA", "Start Device Detection");
			for (byte xControllerBaseAIdx = 0; xControllerBaseAIdx < mControllerAddresses1.Length; xControllerBaseAIdx++) {
				for (byte xDrive = 0; xDrive < 2; xDrive++) {
					IOWriteByte((ushort)(mControllerAddresses1[xControllerBaseAIdx] + ATA_DRIVEHEAD), (byte)((xControllerBaseAIdx << 4) | 0xA0 | (xDrive << 4)));
					mSleep(1);
					if (IOReadByte((ushort)(mControllerAddresses1[xControllerBaseAIdx] + ATA_STATUS)) == 0x50) {
						Device.Add(new ATA(String.Concat(mControllerNumbers[xControllerBaseAIdx], " ", mDriveNames[xDrive]), xControllerBaseAIdx, xDrive));
					}
				}
			}
		}
		public override uint BlockSize {
			get {
				return 512;
			}
		}

		public override ulong BlockCount {
			get {
				throw new NotImplementedException();
			}
		}
		/* 
#define CB_DATA  0   // data reg         in/out pio_base_addr1+0
#define CB_ERR   1   // error            in     pio_base_addr1+1
#define CB_FR    1   // feature reg         out pio_base_addr1+1
#define CB_SC    2   // sector count     in/out pio_base_addr1+2
#define CB_SN    3   // sector number    in/out pio_base_addr1+3
#define CB_CL    4   // cylinder low     in/out pio_base_addr1+4
#define CB_CH    5   // cylinder high    in/out pio_base_addr1+5
#define CB_DH    6   // device head      in/out pio_base_addr1+6
#define CB_STAT  7   // primary status   in     pio_base_addr1+7
#define CB_CMD   7   // command             out pio_base_addr1+7
#define CB_ASTAT 8   // alternate status in     pio_base_addr2+6
#define CB_DC    8   // device control      out pio_base_addr2+6
#define CB_DA    9   // device address   in     pio_base_addr2+7*/
		public override byte[] ReadBlock(ulong aBlock) {
			// 1) Read the status register of the primary or the secondary IDE controller. 
			// 2) The BSY and DRQ bits must be zero if the controller is ready. 
			DebugUtil.SendNumber("ATA", "ReadBlock", (ushort)aBlock, 32);
			uint xSleepCount = Timeout;
			while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				throw new Exception("[ATA#2] Read failed");
			}
			//3) Set the DEV bit to 0 for Drive0 and to 1 for Drive1 on the selected IDE controller using 
			//   the Device/Head register and wait for approximately 400 nanoseconds using some NOP perhaps. 
			IOWriteByte(mController_DeviceHead, (byte)(mDrive << 4));
			//4) Read the status register again. 
			//5) The BSY and DRQ bits must be 0 again for you to know that the IDE controller and the selected IDE drive are ready. 
			xSleepCount = Timeout;
			while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				mSleep(1);
				xSleepCount--;
			}
			if (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0) {
				throw new Exception("[ATA#5] Read failed");
			}
			// 6) Write the LBA28 address to the designated IDE registers. 
			IOWriteByte(mController_SectorNumber, (byte)aBlock);
			IOWriteByte(mController_CylinderLow, (byte)(aBlock >> 8));
			IOWriteByte(mController_CylinderHigh, (byte)(aBlock >> 16));
			IOWriteByte(mController_DeviceHead, (byte)(0xE0 | (mDrive << 4) | ((byte)((aBlock >> 24) & 0x0F))));
			// 7) Set the Sector count using the Sector Count register. 
			IOWriteByte(mController_SectorCount, 1);
			//8) Issue the Read Sector(s) command. 
			IOWriteByte(mController_Command, 0x20);
			// 9) Read the Error register. If the ABRT bit is set then the Read Sector(s) command 
			//    is not supported for that IDE drive. If the ABRT bit is not set, continue to the next step. 
			if ((IOReadByte(mController_Error) & IDE_ERRORREG_ABRT) == IDE_ERRORREG_ABRT) {
				throw new Exception("[ATA#9] Read failed");
			}
			// 10) If you want to receive interrupts after reading each sector, clear the nIEN bit in the 
			//     Device Control register. If you do not clear this bit then interrupts will not be generated 
			//     after the reading of each sector which might cause an infinite loop if you are waiting for them. 
			//     The Primary IDE Controller will generate IRQ14 and the secondary IDE controller generates IRQ 15. 
			IOWriteByte(mController_DeviceControl, 0); // receive interrupts...
			// 11) Read the Alternate Status Register (you may even ignore the value that is read) 
			IOReadByte(mController_AlternateStatus);
			// 12) Read the Status register for the selected IDE Controller. 
			IOReadByte(mController_Command);
			//13) Whenever a sector of data is ready to be read from the Data Register, the BSY bit 
			//    in the status register will be set to 0 and DRQ to 1 so you might want to wait until 
			//    those bits are set to the mentioned values before attempting to read from the drive. 
			xSleepCount = Timeout;
			while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) && xSleepCount != 0) {
				xSleepCount--;
				mSleep(1);
			}
			if ((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) {
				throw new Exception("[ATA#13] Read failed");
			}
			//14) Read one sector from the IDE Controller 16-bits at a time using the IN or the INSW instructions. 
			byte[] xResult = new byte[512];
			for (uint i = 0; i < 256; i++) {
				ushort xValue = IOReadWord(mController);
				if (xValue > 0) {
					Console.Write("Block ");
					Console.Write(aBlock.ToString());
					Console.WriteLine(" Contains nonzero information!");
				}
				xResult[i * 2] = (byte)xValue;
				xResult[(i * 2) + 1] = (byte)(xValue >> 8);
			}
			// 15) See if you have to read one more sector. If yes, repeat from step 11 again. 
			//16) If you don't need to read any more sectors, read the Alternate Status Register and ignore the byte that you read. 
			IOReadByte(mController_AlternateStatus);
			//17) Read the status register. When the status register is read, the IDE Controller will negate 
			//    the INTRQ and you will not have pending IRQs waiting to be detected. This is a MUST to read 
			//    the status register when you are done reading from IDE ports. 
			IOReadByte(mController_Command);
			DebugUtil.SendATA_BlockReceived(mControllerIndex, mDrive, (uint)aBlock, xResult);
			return xResult;
		}

		public override void WriteBlock(ulong aBlock, byte[] aContents) {
			throw new NotImplementedException();
		}

		public override string Name {
			get {
				return mName;
			}
		}
	}
}
