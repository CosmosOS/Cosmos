using Cosmos.Sys.Network.TCPIP.TCP;

namespace Cosmos.Sys.Network
{
    /// <summary>
    /// Represents an established TCP connection between this host and another
    /// </summary>
    public class TcpClient
    {
        internal TCPConnection connection;
        internal ClientDataReceived dataCallback;

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
            TCPPacket packet = new TCPPacket(connection.LocalIP, connection.RemoteIP, connection.LocalPort, connection.RemotePort,
                connection.LocalSequenceNumber, connection.RemoteSequenceNumber, 0x18, 8192, data);

            TCPIP.IPv4OutgoingBuffer.AddPacket(packet);
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
    }
}
