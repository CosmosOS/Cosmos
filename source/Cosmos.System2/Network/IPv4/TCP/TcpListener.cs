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

        // <summary>
        /// Clients dictionary.
        /// </summary>
        private static Dictionary<uint, TcpListener> listeners;

        /// <summary>
        /// Assign listeners dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static TcpListener()
        {
            listeners = new Dictionary<uint, TcpListener>();
        }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>TcpClient</returns>
        internal static TcpListener GetListener(ushort destPort)
        {
            TcpListener listener;

            if (listeners.TryGetValue(destPort, out listener))
            {
                return listener;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpListener(int localPort)
        {
            StateMachine = new Tcp();

            StateMachine.rxBuffer = new Queue<TCPPacket>(8);

            StateMachine.localPort = localPort;

            if (localPort > 0)
            {
                listeners.Add((uint)localPort, this);
            }
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
        }

        /// <summary>
        /// Stop listening for new TCP connections.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        public void Stop()
        {
            if (listeners.ContainsKey((uint)StateMachine.localPort))
            {
                listeners.Remove((uint)StateMachine.localPort);
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
