//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Cosmos.Hardware;
//using Cosmos.Kernel;

//namespace MatthijsTest
//{
//    public class MyATAController : Device
//    {
//        private const ushort PrimaryControllerBaseAddress = 0x1F0;
//        public static void Scan()
//        {
//            var xController = new MyATAController(PrimaryControllerBaseAddress);
//            xController.Identify(true);
//            xController.Identify(false);
//            Device.Devices.Add(xController);
//        }

//        private void Identify(bool masterDrive)
//        {
//            // Select the drive to identify
//            DriveSelectPort = (byte)(masterDrive ? 0xA0 : 0xB0);
//            // read status 
//            var xStatus = StatusPort;
//            xStatus = StatusPort;
//            xStatus = StatusPort;
//            xStatus = StatusPort;
//            xStatus = StatusPort;
//            CommandPort = CommandEnum.Identify;
//            if (StatusPort == RegularStatusFlagsEnum.None)
//            {
//                return;
//            }

//            var xError = false;
//            var xReady = false;
//            while (true)
//            {
//                xStatus = StatusPort;
//                if (xStatus.HasFlags(RegularStatusFlagsEnum.Error))
//                {
//                    xError = true;
//                    break;
//                }
//                if (xStatus.HasFlags(RegularStatusFlagsEnum.DataRequest)
//                    & !xStatus.HasFlags(RegularStatusFlagsEnum.Busy))
//                {
//                    xReady = true;
//                    break;
//                }
//            }

//            if (xError)
//            {
//                // preliminary abort, means it's ATAPI or SATA
//                // todo: implement support for SATA and ATAPI detection
//                return;
//            }
//            if (!xReady)
//            {
//                Console.WriteLine("Weird error. Situation not handled in ATAController.Identify!");
//                while (true)
//                    ;
//            }
//            // not busy anymore. 
//            var xName = masterDrive
//                            ? "Master drive"
//                            : "Slave drive";
//            var xDevice = new MyATADevice(this, xName, masterDrive);
//            Devices.Add(xDevice);
//            if (masterDrive)
//            {
//                HasMasterDevice = true;
//            }
//            else
//            {
//                HasSlaveDevice = true;
//            }
//        }

//        private MyATAController(ushort baseAddress)
//        {
//            mType = DeviceType.Storage;
//            mAddressSpace = new IOAddressSpace(baseAddress, 8);
//        }

//        private IOAddressSpace mAddressSpace;
//        #region IO Ports
//        private ushort DataPort_Word
//        {
//            get
//            {
//                return mAddressSpace.Read16(0);
//            }
//            set
//            {
//                mAddressSpace.Write16(0, value);
//            }
//        }

//        private byte SectorCountPort
//        {
//            set
//            {
//                mAddressSpace.Write8(2, value);
//            }
//        }

//        private byte SectorAddress1Port
//        {
//            set
//            {
//                mAddressSpace.Write8(3, value);
//            }
//        }

//        private byte SectorAddress2Port
//        {
//            set
//            {
//                mAddressSpace.Write8(4, value);
//            }
//        }

//        private byte SectorAddress3Port
//        {
//            set
//            {
//                mAddressSpace.Write8(5, value);
//            }
//        }

//        private RegularStatusFlagsEnum StatusPort
//        {
//            get
//            {
//                return (RegularStatusFlagsEnum)mAddressSpace.Read8(7);
//            }
//        }

//        private CommandEnum CommandPort
//        {
//            set
//            {
//                mAddressSpace.Write8(7, (byte)value);
//            }
//        }

//        private byte DriveSelectPort
//        {
//            get
//            {
//                return mAddressSpace.Read8(6);
//            }
//            set
//            {
//                mAddressSpace.Write8(6, value);
//            }
//        }

//        #endregion

//        public bool HasMasterDevice
//        {
//            get;
//            private set;
//        }

//        public bool HasSlaveDevice
//        {
//            get;
//            private set;
//        }

//        public override string Name
//        {
//            get
//            {
//                return "ATA Storage Controller";
//            }
//        }

//        #region PIO
//        internal void RealReadBlock_PIO(AddressSpace target)
//        {
//            for (uint i = 0; i < 256; i++)
//            {
//                target.Write16(i * 2, DataPort_Word);
//            }
//        }

//        internal void RealWriteBlock_PIO(byte[] source)
//        {
//            for (uint i = 0; i < 256; i++)
//            {
//                DataPort_Word = (ushort)(source[i *2] | (source[(i*2)+1] << 8));
//            }
//        }

//        internal void RealWriteBlock_PIO(AddressSpace source)
//        {
//            for (uint i = 0; i < 256; i++)
//            {
//                DataPort_Word = source.Read16Unchecked(i * 2);
//            }
//        }

//        internal void ReadSector_LBA28(bool masterDrive, uint block, AddressSpace target)
//        {
//            var xDriveSelect = (byte)0xE0;
//            if (!masterDrive)
//            {
//                xDriveSelect = 0xF0;
//            }
//            DriveSelectPort = (byte)(xDriveSelect | ((block >> 24) & 0xF));
//            SectorCountPort = 1;
//            SectorAddress1Port = (byte)block;
//            SectorAddress2Port = (byte)(block >> 8);
//            SectorAddress3Port = (byte)(block >> 16);
//            CommandPort = CommandEnum.ReadSectors;
//            bool xPoll = true;
//            bool xError = false;
//            do
//            {
//                var xStatus = StatusPort;
//                if ((!xStatus.HasFlags(RegularStatusFlagsEnum.Busy))
//                    && xStatus.HasFlags(RegularStatusFlagsEnum.DataRequest))
//                {
//                    xPoll = false;
//                }
//                if (xStatus.HasFlags(RegularStatusFlagsEnum.Error)
//                    || xStatus.HasFlags(RegularStatusFlagsEnum.DriveFault))
//                {
//                    xPoll = false;
//                    xError = true;
//                }

//            }
//            while (xPoll);
//            if (xError)
//            {
//                throw new Exception("Error while reading sector!");
//            }
//            RealReadBlock_PIO(target);
//        }

//        internal void WriteSector_LBA28(bool masterDrive, uint block, byte[] source)
//        {
//            var xDriveSelect = (byte)0xE0;
//            if (!masterDrive)
//            {
//                xDriveSelect = 0xF0;
//            }
//            DriveSelectPort = (byte)(xDriveSelect | ((block >> 24) & 0xF));
//            SectorCountPort = 1;
//            SectorAddress1Port = (byte)block;
//            SectorAddress2Port = (byte)(block >> 8);
//            SectorAddress3Port = (byte)(block >> 16);
//            CommandPort = CommandEnum.WriteSectors;
//            bool xPoll = true;
//            bool xError = false;
//            do
//            {
//                var xStatus = StatusPort;
//                if ((!xStatus.HasFlags(RegularStatusFlagsEnum.Busy))
//                    && xStatus.HasFlags(RegularStatusFlagsEnum.DataRequest))
//                {
//                    xPoll = false;
//                }
//                if (xStatus.HasFlags(RegularStatusFlagsEnum.Error)
//                    || xStatus.HasFlags(RegularStatusFlagsEnum.DriveFault))
//                {
//                    xPoll = false;
//                    xError = true;
//                }

//            }
//            while (xPoll);
//            if (xError)
//            {
//                throw new Exception("Error while writing sector!");
//            }
//            RealWriteBlock_PIO(source);
//        }

//        internal void WriteSector_LBA28(bool masterDrive, uint block, AddressSpace source)
//        {
//            var xDriveSelect = (byte)0xE0;
//            if (!masterDrive)
//            {
//                xDriveSelect = 0xF0;
//            }
//            DriveSelectPort = (byte)(xDriveSelect | ((block >> 24) & 0xF));
//            SectorCountPort = 1;
//            SectorAddress1Port = (byte)block;
//            SectorAddress2Port = (byte)(block >> 8);
//            SectorAddress3Port = (byte)(block >> 16);
//            CommandPort = CommandEnum.WriteSectors;
//            bool xPoll = true;
//            bool xError = false;
//            do
//            {
//                var xStatus = StatusPort;
//                if ((!xStatus.HasFlags(RegularStatusFlagsEnum.Busy))
//                    && xStatus.HasFlags(RegularStatusFlagsEnum.DataRequest))
//                {
//                    xPoll = false;
//                }
//                if (xStatus.HasFlags(RegularStatusFlagsEnum.Error)
//                    || xStatus.HasFlags(RegularStatusFlagsEnum.DriveFault))
//                {
//                    xPoll = false;
//                    xError = true;
//                }

//            }
//            while (xPoll);
//            if (xError)
//            {
//                throw new Exception("Error while writing sector!");
//            }
//            RealWriteBlock_PIO(source);
//        }
//        #endregion
//    }
//}
