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
            xController.SendIdentity(true);
        }

        private void SendIdentity(bool masterDrive)
        {
            DriveSelectPort = (byte)(masterDrive ? 0xA0 : 0xB0);
            var xStatus = RegularStatusPort;
            xStatus = RegularStatusPort;
            xStatus = RegularStatusPort;
            xStatus = RegularStatusPort;
            
        }

        private MyATAController(ushort baseAddress)
        {
            mType = DeviceType.Storage;
            mAddressSpace = new IOAddressSpace(baseAddress, 8);
        }

        private IOAddressSpace mAddressSpace;
        #region IO Ports
        private byte DataPort
        {
            get
            {
                return mAddressSpace.Read8(0);
            }
            set
            {
                mAddressSpace.Write8(0, value);
            }
        }

        private RegularStatusFlagsEnum RegularStatusPort {
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

        public override string Name
        {
            get
            {
                return "PATA Storage Controller";
            }
        }
    }
}
