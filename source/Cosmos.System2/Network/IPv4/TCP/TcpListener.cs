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
        /// Create new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        /// <param name="address">Local address.</param>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpListener(int localPort)
        {
            StateMachine = new Tcp((ushort)localPort, 0, Address.Zero, Address.Zero);

            StateMachine.rxBuffer = new Queue<TCPPacket>(8);

            StateMachine.LocalEndPoint.Port = (ushort)localPort;
        }

        /// <summary>
        /// Receive TcpClient from remote computer.
        /// </summary>
        /// <returns>Accepted TcpClient</returns>
        public TcpClient AcceptTcpClient(int timeout = -1)
        {
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
            StateMachine.Status = Status.LISTEN;

            Tcp.Connections.Add(StateMachine);
        }

        /// <summary>
        /// Stop listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        public void Stop()
        {
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
