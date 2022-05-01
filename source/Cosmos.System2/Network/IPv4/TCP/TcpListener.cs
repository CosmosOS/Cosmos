using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// TcpListener class. Used to manage the TCP connection to a client.
    /// </summary>
    public class TcpListener : IDisposable
    {
        /// <summary>
        /// Tcp State machine.
        /// </summary>
        internal Tcp StateMachine;

        /// <summary>
        /// Tcp State machine.
        /// </summary>
        private ushort LocalPort;

        /// <summary>
        /// Create new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        /// <param name="address">Local address.</param>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpListener(ushort localPort)
        {
            LocalPort = localPort;
        }

        /// <summary>
        /// Receive TcpClient from remote computer.
        /// </summary>
        /// <returns>Accepted TcpClient</returns>
        /// <exception cref="Exception">Thrown if TcpListener not started.</exception>
        public TcpClient AcceptTcpClient(int timeout = -1)
        {
            if (StateMachine == null)
            {
                new Exception("TcpListener is not started.");
            }

            if (StateMachine.Status == Status.CLOSED) // if TcpListener already accepted client, remove old one.
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);

                Start();
            }

            if (timeout == -1)
            {
                while (StateMachine.WaitStatus(Status.ESTABLISHED) != true);
            }
            else
            {
                while (StateMachine.WaitStatus(Status.ESTABLISHED, timeout) != true);
            }

            return new TcpClient(StateMachine);
        }

        /// <summary>
        /// Start listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        public void Start()
        {
            StateMachine = new Tcp(LocalPort, 0, Address.Zero, Address.Zero);

            StateMachine.rxBuffer = new Queue<TCPPacket>(8);

            StateMachine.LocalEndPoint.Port = LocalPort;

            StateMachine.Status = Status.LISTEN;

            Tcp.Connections.Add(StateMachine);
        }

        /// <summary>
        /// Stop listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="Exception">Thrown if TcpListener not started.</exception>
        public void Stop()
        {
            if (StateMachine == null)
            {
                new Exception("TcpListener is not started.");
            }

            if (StateMachine.Status == Status.LISTEN)
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);

                StateMachine = null;
            }
        }

        /// <summary>
        /// Close listener
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
