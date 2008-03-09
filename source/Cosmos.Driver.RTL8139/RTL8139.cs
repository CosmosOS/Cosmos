using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;

namespace Cosmos.Driver.RTL8139
{
    /// <summary>
    /// Driver for networkcards using the RTL8139 chip.
    /// Some documentation can be found at: http://www.osdev.org/wiki/RTL8139
    /// </summary>
    public class RTL8139 : NetworkDevice //, DeviceDriver interface
    {
        public static Device[] FindDevices()  // DeviceDriver interface
        {
            List<Device> found = new List<Device>();

            foreach (PCIDevice device in Cosmos.Hardware.PC.Bus.PCIBus.Devices)
                if (device.VendorID == 0x10EC && device.DeviceID == 0x0139)
                    found.Add(new RTL8139(device));

            return found.ToArray();
        }


        private PCIDevice myDevice;
        public RTL8139(PCIDevice device)
        {
            myDevice = device;
            // etc
        }

        #region NetworkDevice members
        public override MACAddress MACAddress
        {
            get 
            { 
                //Polls the PCI device for the MAC address
                byte[] bytes = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    bytes[i] = myDevice.Read8((byte)i);
                }

                MACAddress mac = new MACAddress(bytes);

                return mac;            
            }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override bool RecieveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override int BytesAvailable()
        {
            throw new NotImplementedException();
        }

        public override bool IsSendBufferFull()
        {
            throw new NotImplementedException();
        }

        public override bool IsRecieveBufferFull()
        {
            throw new NotImplementedException();
        }
        #endregion

        public override string Name
        {
            get { return "Generic RTL 8139 Network device"; }
        }

        public void EnableNIC()
        {
            //Writes 0x00 to CONFIG_1 registers
            myDevice.Write8((byte)RTL8139Register.Config1, 0x00);
        }


        public void SoftResetNIC()
        {
            myDevice.Write8((byte)RTL8139Register.ChipCmd, 0x10);
            //TODO: Should check the RST bit. It is high while resetting, and low when reset complete.
        }

        private enum RTL8139Register
        {
            MAC0 = 0x00,            // Ethernet hardware address
            MAR0 = 0x08,            // Multicast filter
            TxStatus0 = 0x10,       // Transmit status (Four 32bit registers)
            TxAddr0 = 0x20,         // Tx descriptors (also four 32bit)
            RxBuf = 0x30,
            RxEarlyCnt = 0x34,
            RxEarlyStatus = 0x36,
            ChipCmd = 0x37,
            RxBufPtr = 0x38,
            RxBufAddr = 0x3A,
            IntrMask = 0x3C,
            IntrStatus = 0x3E,
            TxConfig = 0x40,
            RxConfig = 0x44,
            Timer = 0x48,           // A general-purpose counter
            RxMissed = 0x4C,        // 24 bits valid, write clears
            Cfg9346 = 0x50,
            Config0 = 0x51,
            Config1 = 0x52,
            FlashReg = 0x54,
            GPPinData = 0x58,
            GPPinDir = 0x59,
            MII_SMI = 0x5A,
            HltClk = 0x5B,
            MultiIntr = 0x5C,
            TxSummary = 0x60,
            MII_BMCR = 0x62,
            MII_BMSR = 0x64,
            NWayAdvert = 0x66,
            NWayLPAR = 0x68,
            NWayExpansion = 0x6A
        }
    }
}
