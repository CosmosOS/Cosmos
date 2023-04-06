﻿using System;
using System.Collections.Generic;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// Used to manage a TCP connection to a client.
    /// </summary>
    public class TcpListener : IDisposable
    {
        private readonly ushort localPort;

        /// <summary>
        /// The TCP state machine.
        /// </summary>
        internal Tcp StateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        /// <param name="address">Local address.</param>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpListener(ushort localPort)
        {
            this.localPort = localPort;
        }

        /// <summary>
        /// Receives a <see cref="TcpClient"/> instance from the remote host.
        /// </summary>
        /// <returns>The accepted <see cref="TcpClient"/>.</returns>
        /// <exception cref="Exception">Thrown if TcpListener not started.</exception>
        public TcpClient AcceptTcpClient(int timeout = -1)
        {
            if (StateMachine == null)
            {
                throw new Exception("The TcpListener is not started.");
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
        /// Starts listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        public void Start()
        {
            StateMachine = new(localPort, 0, Address.Zero, Address.Zero);
            StateMachine.RxBuffer = new Queue<TCPPacket>(8);
            StateMachine.LocalEndPoint.Port = localPort;
            StateMachine.Status = Status.LISTEN;

            Tcp.Connections.Add(StateMachine);
        }

        /// <summary>
        /// Stops listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="Exception">Thrown if TcpListener not started.</exception>
        public void Stop()
        {
            if (StateMachine == null)
            {
                throw new Exception("The TcpListener is not started.");
            }

            if (StateMachine.Status == Status.LISTEN)
            {
                Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
                StateMachine = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
