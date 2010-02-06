using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace MatthijsTest
{
    public class MyATAController : Device
    {
        private const ushort PrimaryControllerBaseAddress = 0x1F0;
        public static void Scan()
        {
            var xController = new MyATAController(PrimaryControllerBaseAddress);
            xController.Identify(true);
            xController.Identify(false);
            Device.Devices.Add(xController);
        }

        private void Identify(bool masterDrive)
        {
            // Select the drive to identify
            DriveSelectPort = (byte)(masterDrive ? 0xA0 : 0xB0);
            // read status 
            var xStatus = StatusPort;
            xStatus = StatusPort;
            xStatus = StatusPort;
            xStatus = StatusPort;
            xStatus = StatusPort;
            CommandPort = CommandEnum.Identify;
            if (StatusPort == RegularStatusFlagsEnum.None)
            {
                return;
            }

            var xError = false;
            var xReady = false;
            while (true)
            {
                xStatus = StatusPort;
                if (xStatus.HasFlags(RegularStatusFlagsEnum.Error))
                {
                    xError = true;
                    break;
                }
                if (xStatus.HasFlags(RegularStatusFlagsEnum.DataRequest)
                    & !xStatus.HasFlags(RegularStatusFlagsEnum.Busy))
                {
                    xReady = true;
                    break;
                }
            }

            if (xError)
            {
                // preliminary abort, means it's ATAPI or SATA
                // todo: implement support for SATA and ATAPI detection
                return;
            }
            if (!xReady)
            {
                Console.WriteLine("Weird error. Situation not handled in ATAController.Identify!");
                while (true)
                    ;
            }
            // not busy anymore. 
            var xName = masterDrive
                            ? "Master drive"
                            : "Slave drive";
            var xDevice = new MyATADevice(this, xName);
            Devices.Add(xDevice);
            if (masterDrive)
            {
                HasMasterDevice = true;
            }
            else
            {
                HasSlaveDevice = true;
            }
        }

        private MyATAController(ushort baseAddress)
        {
            mType = DeviceType.Storage;
            mAddressSpace = new IOAddressSpace(baseAddress, 8);
        }

        private IOAddressSpace mAddressSpace;
        #region IO Ports
        private ushort DataPort_Word
        {
            get
            {
                return mAddressSpace.Read16(0);
            }
            set
            {
                mAddressSpace.Write16(0, value);
            }
        }

        private RegularStatusFlagsEnum StatusPort
        {
            get
            {
                return (RegularStatusFlagsEnum)mAddressSpace.Read8(7);
            }
        }

        private CommandEnum CommandPort
        {
            set
            {
                mAddressSpace.Write8(7, (byte)value);
            }
        }

        private byte DriveSelectPort
        {
            get
            {
                return mAddressSpace.Read8(6);
            }
            set
            {
                mAddressSpace.Write8(6, value);
            }
        }

        #endregion

        public bool HasMasterDevice
        {
            get;
            private set;
        }

        public bool HasSlaveDevice
        {
            get;
            private set;
        }

        public override string Name
        {
            get
            {
                return "ATA Storage Controller";
            }
        }

        #region PIO
        internal void RealReadBlock(AddressSpace target)
        {
            for (uint i = 0; i < 256; i++)
            {
                target.Write16(i * 2, DataPort_Word);
            }
        }
        #endregion
    }
}
