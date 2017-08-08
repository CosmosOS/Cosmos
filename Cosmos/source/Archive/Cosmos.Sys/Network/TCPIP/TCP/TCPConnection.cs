using System;

namespace Cosmos.Sys.Network.TCPIP.TCP
{
    internal class TCPConnection
    {
        internal enum State { SYN_SENT, SYN_RECVD, ESTABLISHED, TIMEWAIT };

        protected UInt64 socket;
        protected IPv4EndPoint remoteEndpoint;
        protected IPv4EndPoint localEndpoint;
        protected UInt32 remoteSeqNumber;
        protected UInt32 localSeqNumber;
        protected State state;
        internal TcpClient client;

        internal TCPConnection(IPv4Address remoteIP, UInt16 remotePort, IPv4Address localIP, UInt16 localPort, 
            UInt32 initialSeqNumber, State initialState)
        {
            this.remoteEndpoint = new IPv4EndPoint(remoteIP, remotePort);
            this.localEndpoint = new IPv4EndPoint(localIP, localPort);
            this.remoteSeqNumber = initialSeqNumber;
            this.localSeqNumber = 0x3040;

            this.state = initialState;

            // TODO: have socket be a hash key we can use to find connections quickly
            this.socket = remotePort;
        }

        internal IPv4EndPoint RemoteEndpoint
        {
            get { return this.remoteEndpoint; }
        }
        internal IPv4Address RemoteIP
        {
            get { return this.remoteEndpoint.IPAddress; }
        }
        internal UInt16 RemotePort
        {
            get { return this.remoteEndpoint.Port; }
        }
        internal IPv4EndPoint LocalEndpoint
        {
            get { return this.localEndpoint; }
        }
        internal IPv4Address LocalIP
        {
            get { return this.localEndpoint.IPAddress; }
        }
        internal UInt16 LocalPort
        {
            get { return this.localEndpoint.Port; }
        }
        internal State ConnectionState
        {
            get { return this.state; }
            set { this.state = value; }
        }
        internal UInt64 Socket
        {
            get { return this.socket; }
        }
        internal UInt32 LocalSequenceNumber
        {
            get { return this.localSeqNumber; }
            set { this.localSeqNumber = value; }
        }
        internal UInt32 RemoteSequenceNumber
        {
            get { return this.remoteSeqNumber; }
            set { this.remoteSeqNumber = value; }
        }
    }
}
