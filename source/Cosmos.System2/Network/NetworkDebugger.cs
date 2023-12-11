using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.Network.Config;
using Con = System.Console;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Facilitates kernel debugging over IP.
    /// </summary>
    public class NetworkDebugger
    {
        private readonly TcpListener listener;
        private NetworkStream stream;
        private TcpClient client;

        /// <summary>
        /// The remote host IP address.
        /// </summary>
        public IPAddress Ip { get; set; }

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
            listener = new TcpListener(IPAddress.Any, (ushort)port);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDebugger"/> class.
        /// </summary>
        /// <param name="ip">IP Address of the remote debugger.</param>
        /// <param name="port">Port used for TCP connection.</param>
        public NetworkDebugger(IPAddress ip, int port)
        {
            Ip = ip;
            Port = port;
            client = new TcpClient(new IPEndPoint(Ip, port));
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
                client = listener.AcceptTcpClient();
            }
            else if (listener == null)
            {
                client.Connect(Ip, Port);
            }

            stream = client.GetStream();

            Send("--- Cosmos Network Debugger ---");
            Send("Debugger connected!");
        }

        /// <summary>
        /// Send text to the debugger.
        /// </summary>
        /// <param name="message">Text to send to the debugger.</param>
        public void Send(string message)
        {
            byte[] dataToSend = Encoding.ASCII.GetBytes("[" + DateTime.Now.ToString("HH:mm:ss") + "] - " + message + "\r\n");
            stream.Write(dataToSend, 0, dataToSend.Length);
        }

        /// <summary>
        /// Stops the debugger by closing the TCP connection.
        /// </summary>
        public void Stop()
        {
            Con.WriteLine("Closing Debugger connection");
            Send("Closing...");
            stream.Close();
            client.Close();
        }
    }
}
