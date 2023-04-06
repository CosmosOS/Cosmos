using System;
using System.Text;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Con = System.Console;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Facilitates kernel debugging over IP.
    /// </summary>
    public class NetworkDebugger
    {
        private readonly TcpListener listener;
        private TcpClient client;

        /// <summary>
        /// The remote host IP address.
        /// </summary>
        public Address Ip { get; set; }

        /// <summary>
        /// The port to use.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDebugger"/> class.
        /// </summary>
        /// <param name="port">Port used for TCP connection.</param>
        public NetworkDebugger(int port)
        {
            Port = port;
            listener = new TcpListener((ushort)port);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDebugger"/> class.
        /// </summary>
        /// <param name="ip">IP Address of the remote debugger.</param>
        /// <param name="port">Port used for TCP connection.</param>
        public NetworkDebugger(Address ip, int port)
        {
            Ip = ip;
            Port = port;
            client = new TcpClient(port);
        }

        /// <summary>
        /// Starts the debugger.
        /// </summary>
        public void Start()
        {
            if (client == null)
            {
                listener.Start();
                
                Con.WriteLine("Waiting for remote debugger connection at " + NetworkConfiguration.CurrentAddress.ToString() + ":" + Port);
                client = listener.AcceptTcpClient(); //blocking
            }
            else if (listener == null)
            {
                client.Connect(Ip, Port);
            }

            Send("--- Cosmos Network Debugger ---");
            Send("Debugger connected!");
        }

        /// <summary>
        /// Send text to the debugger.
        /// </summary>
        /// <param name="message">Text to send to the debugger.</param>
        public void Send(string message)
        {
            client.Send(Encoding.ASCII.GetBytes("[" + DateTime.Now.ToString("HH:mm:ss") + "] - " + message + "\r\n"));
        }

        /// <summary>
        /// Stops the debugger by closing the TCP connection.
        /// </summary>
        public void Stop()
        {
            Con.WriteLine("Closing Debugger connection");
            Send("Closing...");
            client.Close();
        }
    }
}
