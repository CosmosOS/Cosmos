/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Abstract Network Device Class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static NetworkDevice GetDeviceByName(string nameID)
        {
            foreach (var device in Devices)
            {
                if (device.NameID == nameID)
                {
                    return device;
                }
            }
            return null;
        }

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

        public string NameID
        {
            get; set;
        }

        public abstract string Name
        {
            get;
        }

        public abstract bool Ready
        {
            get;
        }

        /// <summary>
        /// Add bytes to the transmit buffer queue.
        /// </summary>
        /// <param name="buffer">bytes array to queue.</param>
        /// <returns>TRUE on success.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if buffer length is bigger than Int32.MaxValue.</exception>
        public virtual bool QueueBytes(byte[] buffer)
        {
            return QueueBytes(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Add bytes to the transmit buffer queue.
        /// </summary>
        /// <param name="buffer">bytes array to queue.</param>
        /// <param name="offset">Offset of the data in the buffer.</param>
        /// <param name="length">Data length.</param>
        /// <returns>TRUE on success.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if length is bigger than Int32.MaxValue.</exception>
        public abstract bool QueueBytes(byte[] buffer, int offset, int length);

        public abstract bool ReceiveBytes(byte[] buffer, int offset, int max);
        public abstract byte[] ReceivePacket();

        public abstract int BytesAvailable();

        public abstract bool Enable();

        public abstract bool IsSendBufferFull();
        public abstract bool IsReceiveBufferFull();
    }
}
