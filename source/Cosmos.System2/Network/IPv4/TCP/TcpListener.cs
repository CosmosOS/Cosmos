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
        private static Dictionary<uint, TcpListener> clients;

        public bool ExclusiveAddressUse { get; set; }
        public EndPoint LocalEndpoint { get; }
        protected bool Active { get; }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>TcpClient</returns>
        internal static TcpListener GetClient(ushort destPort)
        {
            if (clients.ContainsKey((uint)destPort))
            {
                return clients[(uint)destPort];
            }

            return null;
        }

        public TcpListener(int localPort)
        {
            StateMachine.localPort = localPort;

            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        public TcpListener(Address localaddr, int localPort)
        {
            StateMachine.source = localaddr;
            StateMachine.localPort = localPort;

            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        public static TcpListener Create(int port)
        {
            throw new NotImplementedException();
        }

        public TcpClient AcceptTcpClient()
        {
            return new TcpClient(StateMachine.localPort);
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
