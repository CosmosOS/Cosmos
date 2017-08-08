using System;
using System.Collections.Generic;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Storage.ATA
{
    public class ATA : BlockDevice
    {
        //private static readonly ushort[] mControllerAddresses1 = new ushort[] { 
        //    0x1F0,
        //    0x170
        //};
        //private static readonly ushort[] mControllerAddresses2 = new ushort[] { 
        //0x3F0,
        //    0x370
        //};
        private static ushort GetControllerAddress1(int aIndex)
        {
            if (aIndex == 0)
            {
                return 0x1F0;
            }
            else
            {
                return 0x170;
            }
        }

        private static ushort GetControllerAddress2(int aIndex)
        {
            if (aIndex == 0)
            {
                return 0x3F0;
            }
            else
            {
                return 0x370;
            }
        }

        private static int GetControllerAddressCount()
        {
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

        private enum ATA_Commands : byte
        {
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
        private enum DeviceControlBits : byte
        {
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
        private ATA(string aName, byte aController, byte aDrive)
        {
            mName = aName;
            mControllerIndex = aController;
            mController = GetControllerAddress1(aController);
            mController2 = GetControllerAddress2(aController);
            mType = DeviceType.Storage;
            mDrive = aDrive;
            mController_Data = mController;
            mIsPrimary = aController == 0;
            mController_Error = (ushort)(mController + 1);
            mController_FeatureReg = (ushort)(mController + 1);
            mController_SectorCount = (ushort)(mController + 2);
            mController_SectorNumber = (ushort)(mController + 3);
            mController_CylinderLow = (ushort)(mController + 4);
            mController_CylinderHigh = (ushort)(mController + 5);
            mController_DeviceHead = (ushort)(mController + 6);
            mController_PrimaryStatus = (ushort)(mController + 7);
            mController_Command = (ushort)(mController + 7);
            mController_AlternateStatus = (ushort)(mController2 + 6);
            mController_DeviceControl = (ushort)(mController2 + 6);
            mController_DeviceAddress = (ushort)(mController2 + 7);
            IOWriteByte(mController_DeviceControl, 0);
            mBlockCount = GetBlockCount();
        }

        private uint GetBlockCount()
        {
            IOWriteByte(mController_DeviceHead, (byte)((mDrive << 4) + (1 << 6)));
            //uint xTimeout = Timeout;
            while ((IOReadByte(mController_Command) & IDE_STATUSREG_DRDY) == 0)
            {
                ; ; ;
                //xTimeout--;
            }    
            if ((IOReadByte(mController_Command) & IDE_STATUSREG_DRDY) == 0)
            {
                throw new Exception("[ATA#1] GetBlockCount failed!");
            }
            IOWriteByte(mController_Command, 0xF8);
            //xTimeout = Timeout;
            while ((IOReadByte(mController_Command) & IDE_STATUSREG_BSY) != 0)
            {
                //CPU.Halt();
                //xTimeout--;
                ; ; ;
            }
            if ((IOReadByte(mController_Command) & IDE_STATUSREG_BSY) != 0)
            {
                throw new Exception("[ATA#2] GetBlockCount failed!");
            }
            uint xResult = IOReadByte(mController_SectorNumber);
            xResult += (uint)IOReadByte(mController_CylinderLow) << 8;
            xResult += (uint)IOReadByte(mController_CylinderHigh) << 16;
            xResult += (uint)(IOReadByte(mController_DeviceHead) & 0xF) << 8;
            return xResult;
        }

        public static void HandleInterruptSecondary()
        {
            mSecondaryInterruptCount++;
            IOReadByte((ushort)(GetControllerAddress1(0) + 7));
        }

        private static uint mSecondaryInterruptCount;

        public static void HandleInterruptPrimary()
        {
            mPrimaryInterruptCount++;
            IOReadByte((ushort)(GetControllerAddress1(1) + 7));
        }

        private static uint mPrimaryInterruptCount;

        public static void Initialize()
        {
            for (byte xControllerBaseAIdx = 0; xControllerBaseAIdx < GetControllerAddressCount(); xControllerBaseAIdx++)
            {
                var xOldValue = IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS));
                IOWriteByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS), (byte)(xOldValue | 0x4));
                IOWriteByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS), xOldValue);

                for (byte xDrive = 0; xDrive < 2; xDrive++)
                {
                    IOWriteByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_DRIVEHEAD), 
                        (byte)((xControllerBaseAIdx << 4) | 0xA0 | (xDrive << 4)));
                    Console.Write("        Drive " + xDrive);
                    // we should wait 400ns
                    IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS));
                    IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS));
                    IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS));
                    IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS));
                    // end wait 400ns
                    if (IOReadByte((ushort)(GetControllerAddress1(xControllerBaseAIdx) + ATA_STATUS)) == 0x50)
                    {
                        ATA xATA = xATA = new ATA(String.Concat(mControllerNumbers[xControllerBaseAIdx], " ", mDriveNames[xDrive]), xControllerBaseAIdx, xDrive);
                        Device.Add(xATA);
                    }
                }
            }
        }

        public override uint BlockSize
        {
            get
            {
                return 512;
            }
        }

        private readonly ulong mBlockCount;

        public override ulong BlockCount
        {
            get
            {
                return mBlockCount;
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
        public override void ReadBlock(ulong aBlock, byte[] aBuffer)
        {
            // 1) Read the status register of the primary or the secondary IDE controller. 
            // 2) The BSY and DRQ bits must be zero if the controller is ready. 
            uint xSleepCount = Timeout;
            while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0)
            {
                //CPU.Halt();
                // xSleepCount--;
                ; ; ;
            }
            if (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0)
            {
                throw new Exception("[ATA#2] Read failed");
            }
            //3) Set the DEV bit to 0 for Drive0 and to 1 for Drive1 on the selected IDE controller using 
            //   the Device/Head register and wait for approximately 400 nanoseconds using some NOP perhaps. 
            IOWriteByte(mController_DeviceHead, (byte)(mDrive << 4));
            //4) Read the status register again. 
            //5) The BSY and DRQ bits must be 0 again for you to know that the IDE controller and the selected IDE drive are ready. 
            xSleepCount = Timeout;
            while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0)
            {
                //CPU.Halt();
                //xSleepCount--;
                ; ; ;
            }
            if (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != 0) && xSleepCount > 0)
            {
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
            if ((IOReadByte(mController_Error) & IDE_ERRORREG_ABRT) == IDE_ERRORREG_ABRT)
            {
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
            while (((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ) && xSleepCount != 0)
            {
                //xSleepCount--;
                //CPU.Halt();
                ; ; ;
            }
            if ((IOReadByte(mController_Command) & (IDE_STATUSREG_BSY | IDE_STATUSREG_DRQ)) != IDE_STATUSREG_DRQ)
            {
                throw new Exception("[ATA#13] Read failed");
            }
            //14) Read one sector from the IDE Controller 16-bits at a time using the IN or the INSW instructions. 
            for (uint i = 0; i < 256; i++)
            {
                ushort xValue = IOReadWord(mController);
                aBuffer[i * 2] = (byte)xValue;
                aBuffer[(i * 2) + 1] = (byte)(xValue >> 8);
            }
            // 15) See if you have to read one more sector. If yes, repeat from step 11 again. 
            //16) If you don't need to read any more sectors, read the Alternate Status Register and ignore the byte that you read. 
            IOReadByte(mController_AlternateStatus);
            //17) Read the status register. When the status register is read, the IDE Controller will negate 
            //    the INTRQ and you will not have pending IRQs waiting to be detected. This is a MUST to read 
            //    the status register when you are done reading from IDE ports. 
            IOReadByte(mController_Command);
        }

        public override void WriteBlock(ulong aBlock, byte[] aContents)
        {
            uint xSleepCount = Timeout;
            while (((IOReadByte(mController_Command) & 0x80) == 0x80) && xSleepCount > 0)
            {
                //CPU.Halt();
                //xSleepCount--;
                ; ; ;
            }
            if (xSleepCount == 0)
            {
                throw new Exception("[ATA|WriteBlockNew] Failed 1");
            }
            IOWriteByte(mController_FeatureReg, 0);
            IOWriteByte(mController_SectorCount, 1);
            IOWriteByte(mController_SectorNumber, (byte)aBlock);
            IOWriteByte(mController_CylinderLow, (byte)(aBlock >> 8));
            IOWriteByte(mController_CylinderHigh, (byte)(aBlock >> 16));
            IOWriteByte(mController_DeviceHead, (byte)(0xE0 | (mDrive << 4) | (byte)(aBlock >> 24)));
            IOWriteByte(mController_DeviceControl, 0); // receive interrupts...
            IOWriteByte(mController_Command, 0x30);
            xSleepCount = Timeout;
            while (((IOReadByte(mController_Command) & 0x80) == 0x80) && xSleepCount > 0)
            {
                //CPU.Halt();
                //xSleepCount--;
                ; ; ;
            }
            if (xSleepCount == 0)
            {
                throw new Exception("[ATA|WriteBlockNew] Failed 2");
            }
            xSleepCount = Timeout;
            while (((IOReadByte(mController_AlternateStatus) & 0x8) == 0x8) && xSleepCount > 0)
            {
                //CPU.Halt();
                //xSleepCount--;
                ; ; ;
            }
            if (xSleepCount == 0)
            {
                throw new Exception("[ATA|WriteBlockNew] Failed 3");
            }
            for (int i = 0; i < 256; i++)
            {
                IOWriteWord(mController_Data, (ushort)((aContents[i * 2]) | (aContents[i * 2 + 1] << 8)));
            }
        }

        public override string Name
        {
            get
            {
                return mName;
            }
        }
    }
}
