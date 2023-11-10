using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.Helpers;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4.TCP;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    public static class SocketImpl
    {
        private static Tcp StateMachine;
        private static IPEndPoint EndPoint = null;

        public static void Ctor(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);

            Cosmos.HAL.Global.debugger.Send("Socket - ctor.");
        }

        public static void Ctor(Socket aThis, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);

            Cosmos.HAL.Global.debugger.Send("Socket - ctor.");
        }

        public static void CheckSocket(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            if (socketType != SocketType.Stream)
            {
                throw new NotImplementedException("Only stream sockets implemented.");
            }
            if (protocolType == ProtocolType.Udp)
            {
                throw new NotImplementedException("Only TCP sockets supported. UDP Coming soon ;)");
            }
            else if (protocolType != ProtocolType.Tcp)
            {
                throw new NotImplementedException("Only TCP sockets supported.");
            }
        }

        public static bool get_Connected(Socket aThis)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - get_Connected.");

            Cosmos.HAL.Global.debugger.Send("Socket - StateMachine.Status=" + Tcp.Table[(int)StateMachine.Status] );

            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static bool Poll(Socket aThis, int microSeconds, SelectMode mode)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Poll.");

            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static void Bind(Socket aThis, EndPoint localEP)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Bind.");

            EndPoint = localEP as IPEndPoint;
        }

        public static void Listen(Socket aThis)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Bind.");

            Start(aThis);
        }

        public static Socket Accept(Socket aThis, [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._remoteEndPoint")] ref EndPoint _remoteEndPoint
            , [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._remoteEndPoint")] ref EndPoint _localEndPoint)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Accept.");

            if (StateMachine == null)
            {
                throw new Exception("The TcpListener is not started.");
            }

            if (StateMachine.Status == Status.CLOSED) // if TcpListener already accepted client, remove old one.
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                Start(aThis);
            }

            while (StateMachine.WaitStatus(Status.ESTABLISHED) != true);

            Cosmos.HAL.Global.debugger.Send("Socket - Accepted.");

            Cosmos.HAL.Global.debugger.Send("Socket - Ip is " + StateMachine.RemoteEndPoint.Address.ToString() + ":" + StateMachine.RemoteEndPoint.Port);

            _remoteEndPoint = new IPEndPoint(StateMachine.RemoteEndPoint.Address.ToUInt32(), StateMachine.RemoteEndPoint.Port);

            _localEndPoint = new IPEndPoint(StateMachine.LocalEndPoint.Address.ToUInt32(), StateMachine.LocalEndPoint.Port);

            Cosmos.HAL.Global.debugger.Send("Socket - ok.");

            return aThis;
        }

        public static void Start(Socket aThis)
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
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before sending data.");
            }
            if (buffer.Length > 536)
            {
                var chunks = ArrayHelper.ArraySplit(buffer, 536);

                for (int i = 0; i < chunks.Length; i++)
                {
                    var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, i == chunks.Length - 2 ? (byte)(Flags.PSH | Flags.ACK) : (byte)Flags.ACK, StateMachine.TCB.SndWnd, 0, chunks[i]);
                    OutgoingBuffer.AddPacket(packet);
                    NetworkStack.Update();

                    StateMachine.TCB.SndNxt += (uint)chunks[i].Length;
                }
            }
            else
            {
                var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, (byte)(Flags.PSH | Flags.ACK), StateMachine.TCB.SndWnd, 0, data);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                StateMachine.TCB.SndNxt += (uint)buffer.Length;
            }

            return 0;
        }

        public static int Receive(Socket aThis, Span<byte> buffer, SocketFlags socketFlags)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Receive Span<byte>.");

            return 0;
        }

        public static int Receive(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Receive byte[].");

            while (StateMachine.Data == null || StateMachine.Data.Length == 0)
            {
                if (StateMachine.Status != Status.ESTABLISHED)
                {
                    Cosmos.HAL.Global.debugger.Send("Socket - Client must be connected before receiving data..");
                    return 0;
                }
            }

            StateMachine.RxBuffer.Dequeue();

            // Copy received buffer data in buffer arg
            Cosmos.HAL.Global.debugger.Send("Socket - Receive StateMachine.Data.Length=" + StateMachine.Data.Length);
            Cosmos.HAL.Global.debugger.Send("Socket - Receive size=" + size);
            int bytesToCopy = Math.Min(StateMachine.Data.Length, size);
            Cosmos.HAL.Global.debugger.Send("Socket - Receive bytesToCopy=" + bytesToCopy);
            Buffer.BlockCopy(StateMachine.Data, 0, buffer, offset, bytesToCopy);
            Cosmos.HAL.Global.debugger.Send("Socket - Receive copied to buffer");

            // Update buffer data by deleting read data
            byte[] remainingData = new byte[StateMachine.Data.Length - bytesToCopy];
            Buffer.BlockCopy(StateMachine.Data, bytesToCopy, remainingData, 0, remainingData.Length);
            StateMachine.Data = remainingData;

            Cosmos.HAL.Global.debugger.Send("Socket - Receive moved data.");

            return bytesToCopy;
        }

        public static void Close(Socket aThis)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Closing.");

            Close(aThis, 5000);
        }

        public static void Close(Socket aThis, int timeout)
        {
            Cosmos.HAL.Global.debugger.Send("Socket - Closing with timeout " + timeout);

            if (StateMachine == null)
            {
                throw new Exception("The Socket is not started.");
            }

            if (StateMachine.Status == Status.LISTEN)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Closing remove con");

                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                StateMachine = null;
            }
            else if (StateMachine.Status == Status.ESTABLISHED)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Closing ESTABLISHED");

                StateMachine.SendEmptyPacket(Flags.FIN | Flags.ACK);

                StateMachine.TCB.SndNxt++;

                StateMachine.Status = Status.FIN_WAIT1;

                if (StateMachine.WaitStatus(Status.CLOSED, 5000) == false)
                {
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
