/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Server
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Text;
using Cosmos.System.FileSystem;

namespace Cosmos.System.Network.IPv4.TCP.FTP
{
    /// <summary>
    /// FTP data transfer mode.
    /// </summary>
    public enum TransferMode
    {
        /// <summary>
        /// No mode set.
        /// </summary>
        NONE,

        /// <summary>
        /// Active mode.
        /// </summary>
        ACTV,

        /// <summary>
        /// Passive Mode.
        /// </summary>
        PASV
    }

    /// <summary>
    /// FTPCommand class.
    /// </summary>
    internal class FtpCommand
    {
        /// <summary>
        /// FTP Command Type.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// FTP Command Content.
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// FtpServer class. Used to handle FTP client connections.
    /// </summary>
    public class FtpServer : IDisposable
    {
        /// <summary>
        /// Command Manager.
        /// </summary>
        internal FtpCommandManager CommandManager { get; set; }

        /// <summary>
        /// TCP Listener used to handle new FTP client connection.
        /// </summary>
        internal TcpListener tcpListener;

        /// <summary>
        /// Is FTP server listening for new FTP clients.
        /// </summary>
        internal bool Listening;

        /// <summary>
        /// Create new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public FtpServer(CosmosVFS fs, string directory)
        {
            CommandManager = new FtpCommandManager(fs, directory);

            Listening = true;
        }

        /// <summary>
        /// Listen for new FTP clients.
        /// </summary>
        public void Listen()
        {
            while (Listening)
            {
                tcpListener = new TcpListener(21);
                tcpListener.Start();
                var client = tcpListener.AcceptTcpClient();

                global::System.Console.WriteLine("Client[0] : New connection from " + client.StateMachine.LocalAddress.ToString());

                ReceiveNewClient(client);
            }
        }

        /// <summary>
        /// Handle new FTP client.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        private void ReceiveNewClient(TcpClient client)
        {
            var ftpClient = new FtpClient(client);

            ftpClient.SendReply(220, "Service ready for new user.");

            while (ftpClient.Control.IsConnected())
            {
                ReceiveRequest(ftpClient);
            }
        }

        /// <summary>
        /// Parse and execute FTP command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        internal void ReceiveRequest(FtpClient ftpClient)
        {
            var ep = new EndPoint(Address.Zero, 0);
            var data = Encoding.ASCII.GetString(ftpClient.Control.Receive(ref ep));
            data = data.Remove(data.Length - 2, 2);

            global::System.Console.WriteLine("Client[0] : " + data);

            var splitted = data.Split(' ');

            var command = new FtpCommand();
            command.Command = splitted[0];

            if (splitted.Length > 1)
            {
                int i = data.IndexOf(" ") + 1;
                command.Content = data.Substring(i);

                while (command.Content.StartsWith("/"))
                {
                    command.Content = command.Content.Remove(0, 1);
                }

                command.Content = command.Content.Replace('/', '\\');
            }

            CommandManager.ProcessRequest(ftpClient, command);
        }

        /// <summary>
        /// Close FTP server.
        /// </summary>
        public void Close()
        {
            Listening = false;
            tcpListener.Stop();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
