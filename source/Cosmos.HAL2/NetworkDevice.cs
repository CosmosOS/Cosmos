using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;
using Cosmos.HAL.Network;

namespace Cosmos.HAL
{
    public delegate void DataReceivedHandler(byte[] packetData);

    public enum CardType
    {
        Ethernet,
        Wireless
    }

    public abstract class NetworkDevice
    {
        public static List<NetworkDevice> Devices { get; private set; }

        static NetworkDevice()
        {
            Devices = new List<NetworkDevice>();
        }

        public DataReceivedHandler DataReceived;

        protected NetworkDevice()
        {
            //mType = DeviceType.Network;
            Devices.Add(this);
        }

        public abstract CardType CardType
        {
            get;
        }

        public abstract MACAddress MACAddress
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract bool Ready
        {
            get;
        }

        public virtual bool QueueBytes(byte[] buffer)
        {
            return QueueBytes(buffer, 0, buffer.Length);
        }

        public abstract bool QueueBytes(byte[] buffer, int offset, int length);

        public abstract bool ReceiveBytes(byte[] buffer, int offset, int max);
        public abstract byte[] ReceivePacket();

        public abstract int BytesAvailable();

        public abstract bool Enable();

        public abstract bool IsSendBufferFull();
        public abstract bool IsReceiveBufferFull();
    }
}
