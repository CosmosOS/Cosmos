using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.Helpers;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.TCP;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    [PlugField(FieldId = StateMachineFieldId, FieldType = typeof(Tcp))]
    [PlugField(FieldId = EndPointFieldId, FieldType = typeof(IPEndPoint))]
    public static class SocketImpl
    {
        private const string StateMachineFieldId = "$$StateMachine$$";
        private const string EndPointFieldId = "$$EndPoint$$";

        private const int MinPort = 49152;
        private const int MaxPort = 65535;

        public static void Ctor(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);
        }

        public static void Ctor(Socket aThis, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            CheckSocket(aThis, socketType, protocolType);
        }

        private static void CheckSocket(Socket aThis, SocketType socketType, ProtocolType protocolType)
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

        public static bool get_Connected(Socket aThis,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static EndPoint get_LocalEndPoint(Socket aThis,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._localEndPoint")] ref EndPoint _localEndPoint)
        {
            return _localEndPoint;
        }

        public static EndPoint get_RemoteEndPoint(Socket aThis,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._remoteEndPoint")] ref EndPoint _remoteEndPoint)
        {
            return _remoteEndPoint;
        }

        public static bool Poll(Socket aThis, int microSeconds, SelectMode mode,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        public static void Bind(Socket aThis, EndPoint localEP,
            [FieldAccess(Name = EndPointFieldId)] ref EndPoint EndPoint)
        {
            EndPoint = localEP as IPEndPoint;
        }

        public static void Listen(Socket aThis,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine,
            [FieldAccess(Name = EndPointFieldId)] ref EndPoint EndPoint)
        {
            Start(aThis, ref StateMachine, ref EndPoint);
        }

        public static Socket Accept(Socket aThis,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine,
            [FieldAccess(Name = EndPointFieldId)] ref EndPoint EndPoint,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._remoteEndPoint")] ref EndPoint _remoteEndPoint,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._localEndPoint")] ref EndPoint _localEndPoint)
        {
            if (StateMachine == null)
            {
                Cosmos.HAL.Global.debugger.Send("The TcpListener is not started, starting...");

                Start(aThis, ref StateMachine, ref EndPoint);
            }

            if (StateMachine.Status == Status.CLOSED) // if TcpListener already accepted client, remove old one.
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                Start(aThis, ref StateMachine, ref EndPoint);
            }

            while (StateMachine.WaitStatus(Status.ESTABLISHED) != true) ;

            _remoteEndPoint = new IPEndPoint(StateMachine.RemoteEndPoint.Address.ToUInt32(), StateMachine.RemoteEndPoint.Port);
            _localEndPoint = new IPEndPoint(StateMachine.LocalEndPoint.Address.ToUInt32(), StateMachine.LocalEndPoint.Port);

            return aThis;
        }

        private static void Start(Socket aThis,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine,
            [FieldAccess(Name = EndPointFieldId)] ref EndPoint EndPoint)
        {
            StateMachine = new((ushort)((IPEndPoint)EndPoint).Port, 0, Cosmos.System.Network.IPv4.Address.Zero, Cosmos.System.Network.IPv4.Address.Zero);
            StateMachine.LocalEndPoint.Port = (ushort)((IPEndPoint)EndPoint).Port;
            StateMachine.Status = Status.LISTEN;

            Tcp.Connections.Add(StateMachine);
        }

        public static void Connect(Socket aThis, IPAddress address, int port,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine,
            [FieldAccess(Name = EndPointFieldId)] ref EndPoint EndPoint,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._remoteEndPoint")] ref EndPoint _remoteEndPoint,
            [FieldAccess(Name = "System.Net.EndPoint System.Net.Sockets.Socket._localEndPoint")] ref EndPoint _localEndPoint)
        {
            Start(aThis, ref StateMachine, ref EndPoint);

            if (StateMachine.Status == Status.ESTABLISHED)
            {
                Cosmos.HAL.Global.debugger.Send("Socket - Client must be closed before setting a new connection..");
                throw new Exception("Client must be closed before setting a new connection.");
            }

            StateMachine.RemoteEndPoint.Address = Cosmos.System.Network.IPv4.Address.Parse(((IPEndPoint)EndPoint).Address.ToString());
            StateMachine.RemoteEndPoint.Port = (ushort)port;
            StateMachine.LocalEndPoint.Address = NetworkConfiguration.CurrentAddress;
            StateMachine.LocalEndPoint.Port = Tcp.GetDynamicPort();

            _remoteEndPoint = new IPEndPoint(address, StateMachine.RemoteEndPoint.Port);
            _localEndPoint = new IPEndPoint(StateMachine.LocalEndPoint.Address.ToUInt32(), StateMachine.LocalEndPoint.Port);

            //Generate Random Sequence Number
            Random rnd = new();
            var SequenceNumber = (uint)(rnd.Next(0, int.MaxValue) << 32) | (uint)rnd.Next(0, int.MaxValue);

            //Fill TCB
            StateMachine.TCB.SndUna = SequenceNumber;
            StateMachine.TCB.SndNxt = SequenceNumber;
            StateMachine.TCB.SndWnd = Tcp.TcpWindowSize;
            StateMachine.TCB.SndUp = 0;
            StateMachine.TCB.SndWl1 = 0;
            StateMachine.TCB.SndWl2 = 0;
            StateMachine.TCB.ISS = SequenceNumber;

            StateMachine.TCB.RcvNxt = 0;
            StateMachine.TCB.RcvWnd = Tcp.TcpWindowSize;
            StateMachine.TCB.RcvUp = 0;
            StateMachine.TCB.IRS = 0;

            Tcp.Connections.Add(StateMachine);
            StateMachine.SendEmptyPacket(Flags.SYN);
            StateMachine.Status = Status.SYN_SENT;

            if (StateMachine.WaitStatus(Status.ESTABLISHED, 5000) == false)
            {
                throw new Exception("Failed to open TCP connection!");
            }
        }

        public static int Send(Socket aThis, ReadOnlySpan<byte> buffer, SocketFlags socketFlags,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
        {
            return Send(aThis, buffer.ToArray(), 0, buffer.Length, socketFlags, ref StateMachine); ;
        }

        public static int Send(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
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

                    WaitAck(ref StateMachine);
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

                bytesSent = size;

                WaitAck(ref StateMachine);
            }

            return bytesSent;
        }

        private static void WaitAck(
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
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


        public static int Receive(Socket aThis, Span<byte> buffer, SocketFlags socketFlags,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
        {
            return Receive(aThis, buffer.ToArray(), 0, buffer.Length, socketFlags, ref StateMachine);
        }

        public static int Receive(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
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
                    break;
                }
            }

            int bytesToCopy = Math.Min(StateMachine.Data.Length, size);
            Buffer.BlockCopy(StateMachine.Data, 0, buffer, offset, bytesToCopy);

            byte[] remainingData = new byte[StateMachine.Data.Length - bytesToCopy];
            Buffer.BlockCopy(StateMachine.Data, bytesToCopy, remainingData, 0, remainingData.Length);
            StateMachine.Data = remainingData;

            return bytesToCopy;
        }

        public static void Close(Socket aThis,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
        {
            Close(aThis, 5000, ref StateMachine);
        }

        public static void Close(Socket aThis, int timeout,
            [FieldAccess(Name = StateMachineFieldId)] ref Tcp StateMachine)
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
