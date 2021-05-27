using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// TcpListener class. Used to manage the TCP connection to a client.
    /// </summary>
    public class TcpListener
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
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>TcpClient</returns>
        internal static TcpListener GetListener(ushort destPort)
        {
            if (listeners.ContainsKey((uint)destPort))
            {
                return listeners[(uint)destPort];
            }

            return null;
        }

        public TcpListener(int localPort)
        {
            StateMachine.localPort = localPort;

            if (localPort > 0)
            {
                listeners.Add((uint)localPort, this);
            }
        }

        public TcpClient AcceptTcpClient()
        {
            while (StateMachine.WaitStatus(Status.ESTABLISHED, 5000) != true);

            return new TcpClient(StateMachine);
        }

        public void Start()
        {
            StateMachine.Status = Status.LISTEN;
        }

        public void Stop()
        {
            StateMachine.Status = Status.CLOSED;
        }
    }
}
