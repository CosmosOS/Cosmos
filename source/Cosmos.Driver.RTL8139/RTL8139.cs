using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;

namespace Cosmos.Driver.RTL8139
{
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
        
        public override MACAddress MACAddress
        {
            get { throw new NotImplementedException(); }
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

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
