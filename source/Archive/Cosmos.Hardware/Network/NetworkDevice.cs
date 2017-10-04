using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2.Network
{    
    public delegate void DataReceivedHandler(byte[] packetData);

    public abstract class NetworkDevice : Device
    {
        protected NetworkDevice()
        {
            mType = DeviceType.Network;
        }

        public abstract MACAddress MACAddress
        {
            get;
        }

        public abstract bool Ready
        {
            get;
        }

        public DataReceivedHandler DataReceived;

        public virtual bool QueueBytes(byte[] buffer)
        {
            return QueueBytes(buffer, 0, buffer.Length);
        }

        public abstract bool QueueBytes(byte[] buffer, int offset, int length);

        public abstract bool ReceiveBytes(byte[] buffer, int offset, int max);
        public abstract byte[] ReceivePacket();

        public abstract int BytesAvailable();

        public abstract bool IsSendBufferFull();
        public abstract bool IsReceiveBufferFull();

        public static List<NetworkDevice> NetworkDevices
        {
            get
            {
                List<NetworkDevice> netDevices = new List<NetworkDevice>();

                for (int d = 0; d < Devices.Count; d++)
                {
                    Device dev = Devices[d];
                    if (dev.Type == DeviceType.Network)
                    {
                        netDevices.Add((NetworkDevice)dev);
                    }
                }

                return netDevices;
            }
        }
    }
}
