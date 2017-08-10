using Cosmos.Sys.Network.TCPIP.TCP;
using System;

namespace Cosmos.Sys.Network
{
    /// <summary>
    /// Represents an established TCP connection between this host and another
    /// </summary>
    public class TcpClient
    {
        internal TCPConnection connection;
        internal ClientDataReceived dataCallback;
        internal ClientDisconnected disconnectCallback;

        internal UInt16 mss = 1360;

        protected static UInt16 NextLocalPort = 33000;

        public TcpClient(IPv4Address dest, UInt16 port)
        {
            IPv4Address source = TCPIPStack.FindNetwork(dest);
            if( source == null )
            {
                throw new ArgumentException("Destination host unreachable", "dest");
            }

            this.connection = new TCPConnection(dest, port, source, NextLocalPort++, 0x5656, TCPConnection.State.SYN_SENT);
            this.connection.client = this;

            TCPIPStack.tcpSockets.Add(connection);

            TCPPacket packet = new TCPPacket(connection, connection.LocalSequenceNumber, 0, 0x02, 8192);

            TCPIP.IPv4OutgoingBuffer.AddPacket(packet);
        }

        internal TcpClient(TCPConnection connection)
        {
            this.connection = connection;
            this.connection.client = this;
        }

        /// <summary>
        /// Data Received callback function
        /// </summary>
        public ClientDataReceived DataReceived
        {
            get { return this.dataCallback; }
            set { this.dataCallback = value; }
        }

        /// <summary>
        /// Data Received callback function
        /// </summary>
        public ClientDisconnected Disconnect
        {
            get { return this.disconnectCallback; }
            set { this.disconnectCallback = value; }
        }

        /// <summary>
        /// Send a string to the remote host
        /// </summary>
        /// <param name="data">String to be sent</param>
        public void SendString(string data)
        {
            byte[] dataBuffer = new byte[data.Length];
            for (int b = 0; b < data.Length; b++)
            {
                dataBuffer[b] = (byte)data[b];
            }

            this.SendData(dataBuffer);
        }

        /// <summary>
        /// Send a raw byte buffer to the remote host
        /// </summary>
        /// <param name="data">Byte buffer with data to send</param>
        public void SendData(byte[] data)
        {
            if (data.Length < mss)
            {
                TCPPacket packet = new TCPPacket(connection.LocalIP, connection.RemoteIP, connection.LocalPort, connection.RemotePort,
                    connection.LocalSequenceNumber, connection.RemoteSequenceNumber, 0x18, 8192, data);

                TCPIP.IPv4OutgoingBuffer.AddPacket(packet);

                connection.LocalSequenceNumber += (uint)data.Length;

                return;
            }

            int remaining_bytes = data.Length;
            int data_idx = 0;
            byte[] new_buffer;
            byte tcp_flags = 0x10;
            while (remaining_bytes > 0)
            {
                if (remaining_bytes > mss)
                {
                    new_buffer = new byte[mss];
                }
                else
                {
                    new_buffer = new byte[remaining_bytes];
                    tcp_flags = 0x18;
                }

                for (int b = 0; b < new_buffer.Length; b++)
                {
                    new_buffer[b] = data[data_idx];
                    data_idx++;
                    remaining_bytes--;
                }

                TCPPacket packet = new TCPPacket(connection.LocalIP, connection.RemoteIP, connection.LocalPort, connection.RemotePort,
                    connection.LocalSequenceNumber, connection.RemoteSequenceNumber, tcp_flags, 8192, new_buffer);

                TCPIP.IPv4OutgoingBuffer.AddPacket(packet);

                connection.LocalSequenceNumber += (uint)new_buffer.Length;
            }
        }

        /// <summary>
        /// Close down an active connection
        /// </summary>
        public void Close()
        {
            TCPPacket packet = new TCPPacket(connection, connection.LocalSequenceNumber, connection.RemoteSequenceNumber + 1, 0x11, 8192);

            TCPIP.IPv4OutgoingBuffer.AddPacket(packet);

            connection.ConnectionState = TCPConnection.State.TIMEWAIT;
        }

        internal void dataReceived(byte[] data)
        {
            if (this.dataCallback != null)
            {
                this.dataCallback(this, data);
            }
        }

        /// <summary>
        /// Remote IP Endpoint
        /// </summary>
        public IPv4EndPoint RemoteEndpoint
        {
            get { return this.connection.RemoteEndpoint; }
        }
        /// <summary>
        /// Local IP Endpoint
        /// </summary>
        public IPv4EndPoint LocalEndpoint
        {
            get { return this.connection.LocalEndpoint; }
        }

        /// <summary>
        /// Returns true if the current connection is active
        /// </summary>
        public bool Connected
        {
            get { return (this.connection.ConnectionState == TCPConnection.State.ESTABLISHED); }
        }

        internal void disconnect()
        {
            if (this.disconnectCallback != null)
            {
                this.disconnectCallback(this);
            }
        }
    }
}
