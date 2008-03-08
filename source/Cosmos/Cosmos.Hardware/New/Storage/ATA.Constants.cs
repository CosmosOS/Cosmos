using System;
using System.Collections.Generic;

namespace Cosmos.Hardware.New.Storage {
	partial class ATA {
		//private static readonly ushort[] mControllerAddresses1 = new ushort[] { 
		//    0x1F0,
		//    0x170
		//};
		//private static readonly ushort[] mControllerAddresses2 = new ushort[] { 
		//0x3F0,
		//    0x370
		//};
		private static ushort GetControllerAddress1(int aIndex) {
			if (aIndex == 0) {
				return 0x1F0;
			} else {
				return 0x170;
			}
		}

		private static ushort GetControllerAddress2(int aIndex) {
			if (aIndex == 0) {
				return 0x3F0;
			} else {
				return 0x370;
			}
		}

		private static int GetControllerAddressCount() {
			return 2;
		}
		private static string[] mControllerNumbers = new string[] { "First", "Second", "Third", "Fourth" };
		private static string[] mControllerChannels = new string[] { "Primary", "Secondary" };
		private static string[] mDriveNames = new string[] { "Master", "Slave" };
		private const byte ATA_DELAY_DEFAULT = 5;
		private const byte ATA_DATA = 0;
		private const byte ATA_ERROR = 1;
		private const byte ATA_SECTORCOUNT = 2;
		private const byte ATA_SECTORNUMBER = 3;
		private const byte ATA_CYLINDERLOW = 4;
		private const byte ATA_CYLINDERHIGH = 5;
		private const byte ATA_DRIVEHEAD = 6;
		private const byte ATA_STATUS = 7;
		private const byte ATA_COMMAND = 8;

		private const byte IDE_STATUSREG_ERR = 0x00000001;
		private const byte IDE_STATUSREG_IDX = 0x00000002;
		private const byte IDE_STATUSREG_CORR = 0x00000004;
		private const byte IDE_STATUSREG_DRQ = 0x00000008;
		private const byte IDE_STATUSREG_DSC = 0x00000010;
		private const byte IDE_STATUSREG_DWF = 0x00000020;
		private const byte IDE_STATUSREG_DRDY = 0x00000040;
		private const byte IDE_STATUSREG_BSY = 0x00000080;
		private const byte IDE_ERRORREG_ABRT = 0x00000004;

		private const uint Timeout = 60;

		private enum ATA_Commands: byte {
			Nop = 0x00,
			CfaRequestExtErrCode = 0x03,
			DeviceReset = 0x08,
			Recalibrate = 0x10,
			ReadSectors = 0x20,
			ReadSectorsExt = 0x24,
			ReadDmaExt = 0x25,
			ReadDmaQueuedExt = 0x26,
			ReadMultipleExt = 0x29,
			WriteSectors = 0x30,
			WriteSectorsExt = 0x34,
			WriteDmaExt = 0x35,
			WriteDmaQueuedExt = 0x36,
			CfaWriteSectorsWoErase = 0x38,
			WriteMultipleExt = 0x39,
			WriteVerify = 0x3C,
			ReadVerifySectors = 0x40,
			ReadVerifySectorsExt = 0x42,
			FormatTrack = 0x50,
			Seek = 0x70,
			CfaTranslateSector = 0x87,
			ExecuteDeviceDiagnostic = 0x90,
			InitializeDriveParameters = 0x91,
			InitializeDeviceParameters = 0x91,
			StandbyImmediate2 = 0x94,
			IdleImmediate2 = 0x95,
			Standby2 = 0x96,
			Idle2 = 0x97,
			CheckPowerMode2 = 0x98,
			Sleep2 = 0x99,
			Packet = 0xA0,
			IdentifyDevicePacket = 0xA1,
			IdentifyPacketDevice = 0xA1,
			Smart = 0xB0,
			CfaEraseSectors = 0xC0,
			ReadMultiple = 0xC4,
			WriteMultiple = 0xC5,
			SetMultipleMode = 0xC6,
			ReadDmaQueued = 0xC7,
			ReadDma = 0xC8,
			WriteDma = 0xCA,
			WriteDmaQueued = 0xCC,
			CfaWriteMultipleWoErase = 0xCD,
			StandbyImmediate1 = 0xE0,
			IdleImmediate1 = 0xE1,
			Standby1 = 0xE2,
			Idle1 = 0xE3,
			ReadBuffer = 0xE4,
			CheckPowerMode1 = 0xE5,
			Sleep1 = 0xE6,
			FlushCache = 0xE7,
			WriteBuffer = 0xE8,
			FlushCacheExt = 0xEA,
			IdentifyDevice = 0xEC,
			SetFeatures = 0xEF,
		}
		private enum DeviceControlBits: byte {
			/// <summary>
			/// High Order Byte (48-bit LBA)
			/// </summary>
			HighOrderByte = 0x80,
			/// <summary>
			/// Soft reset
			/// </summary>
			CB_DC_SRST = 0x04,// soft reset
			/// <summary>
			/// Disable interrupts
			/// </summary>
			CB_DC_NIEN = 0x02  // disable interrupts

		}
	}
}
