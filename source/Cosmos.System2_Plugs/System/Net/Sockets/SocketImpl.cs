using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.Helpers;
using Cosmos.System.Network.IPv4.TCP;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    public static class SocketImpl
    {
        private static Tcp StateMachine;
        private static IPEndPoint EndPoint = null;

        private static EndPoint _localEndPoint;
        private static EndPoint _remoteEndPoint;

        public static void Ctor(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);
        }

        public static void Ctor(Socket aThis, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);
        }

        public static void CheckSocket(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            if (socketType != SocketType.Stream)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Only stream sockets implemented.");

                throw new NotImplementedException("Only stream sockets implemented.");
            }
            if (protocolType == ProtocolType.Udp)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Only TCP sockets supported. UDP Coming soon ;)");

                throw new NotImplementedException("Only TCP sockets supported. UDP Coming soon ;)");
            }
            else if (protocolType != ProtocolType.Tcp)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Only TCP sockets supported.");

                throw new NotImplementedException("Only TCP sockets supported.");
            }
        }

        public static bool get_Connected(Socket aThis)
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static bool Poll(Socket aThis, int microSeconds, SelectMode mode)
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static void Bind(Socket aThis, EndPoint localEP)
        {
            EndPoint = localEP as IPEndPoint;
        }

        public static void Listen(Socket aThis)
        {
            Start();
        }

        public static Socket Accept(Socket aThis)
        {
            if (StateMachine == null)
            {
                Cosmos.HAL.Global.debugger.Send("The TcpListener is not started, starting...");

                Start();
            }

            if (StateMachine.Status == Status.CLOSED) // if TcpListener already accepted client, remove old one.
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                Start();
            }

            while (StateMachine.WaitStatus(Status.ESTABLISHED) != true);

            _remoteEndPoint = new IPEndPoint(StateMachine.RemoteEndPoint.Address.ToUInt32(), StateMachine.RemoteEndPoint.Port);

            _localEndPoint = new IPEndPoint(StateMachine.LocalEndPoint.Address.ToUInt32(), StateMachine.LocalEndPoint.Port);

            return aThis;
        }

        private static void Start()
        {
            StateMachine = new((ushort)EndPoint.Port, 0, Cosmos.System.Network.IPv4.Address.Zero, Cosmos.System.Network.IPv4.Address.Zero);
            StateMachine.RxBuffer = new Queue<TCPPacket>(8);
            StateMachine.LocalEndPoint.Port = (ushort)EndPoint.Port;
            StateMachine.Status = Status.LISTEN;

            Tcp.Connections.Add(StateMachine);
        }

        public static void Connect(Socket aThis, IPAddress address, int port)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Connect.");

            throw new NotImplementedException();
        }

        public static int Send(Socket aThis, ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }

        public static int Send(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (StateMachine.RemoteEndPoint.Address == null || StateMachine.RemoteEndPoint.Port == 0)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Must establish a default remote host by calling Connect() before using this Send() overload.");
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Client must be connected before sending data..");
                throw new Exception("Client must be connected before sending data.");
            }

            if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Invalid offset or size");
                throw new ArgumentOutOfRangeException("Invalid offset or size");
            }

            int bytesSent = 0;

            if (size > 536) // why 536 bytes for chunks size??
            {
                byte[] data = new byte[size];
                Buffer.BlockCopy(buffer, offset, data, 0, size);

                var chunks = ArrayHelper.ArraySplit(data, 536);

                for (int i = 0; i < chunks.Length; i++)
                {
                    var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, i == chunks.Length - 1 ? (byte)(Flags.PSH | Flags.ACK) : (byte)Flags.ACK, StateMachine.TCB.SndWnd, 0, chunks[i]);
                    Cosmos.System.Network.IPv4.OutgoingBuffer.AddPacket(packet);
                    Cosmos.System.Network.NetworkStack.Update();

                    StateMachine.TCB.SndNxt += (uint)chunks[i].Length;
                    bytesSent += chunks[i].Length;

                    WaitAck();
                }
                
                bytesSent = size;
            }
            else
            {
                byte[] data = new byte[size];
                Buffer.BlockCopy(buffer, offset, data, 0, size);

                var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, (byte)(Flags.PSH | Flags.ACK), StateMachine.TCB.SndWnd, 0, data);
                Cosmos.System.Network.IPv4.OutgoingBuffer.AddPacket(packet);
                Cosmos.System.Network.NetworkStack.Update();

                StateMachine.TCB.SndNxt += (uint)size;

                StateMachine.WaitingSendAck = true;

                bytesSent = size;

                WaitAck();
            }

            return bytesSent;
        }

        private static void WaitAck()
        {
            bool ackReceived = false;
            uint expectedAckNumber = StateMachine.TCB.SndNxt;

            while (!ackReceived)
            {
                if (StateMachine.TCB.SndUna >= expectedAckNumber)
                {
                    ackReceived = true;
                }
            }
        }


        public static int Receive(Socket aThis, Span<byte> buffer, SocketFlags socketFlags)
        {
            return Receive(aThis, buffer.ToArray(), 0, buffer.Length, socketFlags);
        }

        public static int Receive(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Receive Invalid offset or size");
                throw new ArgumentOutOfRangeException("Invalid offset or size");
            }

            while (StateMachine.Data == null || StateMachine.Data.Length == 0)
            {
                if (StateMachine.Status != Status.ESTABLISHED)
                {
                    Cosmos.HAL.Global.debugger.Send("Socket - Client must be connected before receiving data..");
                    return 0;
                }
            }

            StateMachine.RxBuffer.Dequeue();

            int bytesToCopy = Math.Min(StateMachine.Data.Length, size);
            Buffer.BlockCopy(StateMachine.Data, 0, buffer, offset, bytesToCopy);

            byte[] remainingData = new byte[StateMachine.Data.Length - bytesToCopy];
            Buffer.BlockCopy(StateMachine.Data, bytesToCopy, remainingData, 0, remainingData.Length);
            StateMachine.Data = remainingData;

            return bytesToCopy;
        }

        public static void Close(Socket aThis)
        {
            Close(aThis, 5000);
        }

        public static void Close(Socket aThis, int timeout)
        {
            if (StateMachine == null)
            {
                return;
            }

            if (StateMachine.Status == Status.CLOSED)
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);

                StateMachine = null;

                return;
            }
            else if (StateMachine.Status == Status.CLOSING || StateMachine.Status == Status.CLOSE_WAIT)
            {
                while (StateMachine.WaitStatus(Status.CLOSED) != true) ;

                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);

                StateMachine = null;

                return;
            }

            if (StateMachine.Status == Status.LISTEN)
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                StateMachine = null;
            }
            else if (StateMachine.Status == Status.ESTABLISHED)
            {
                StateMachine.SendEmptyPacket(Flags.FIN | Flags.ACK);

                StateMachine.TCB.SndNxt++;

                StateMachine.Status = Status.FIN_WAIT1;

                if (StateMachine.WaitStatus(Status.CLOSED, 5000) == false)
                {
                    Cosmos.HAL.Global.debugger.Send("Socket - Close Failed to close TCP connection!");
                    throw new Exception("Failed to close TCP connection!");
                }

                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
            }
        }

        public static void Dispose(Socket aThis)
        {
            aThis.Close(5000);
        }
    }
}
