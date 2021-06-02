/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Console = global::System.Console;

namespace Cosmos.System.Network.IPv4.TCP.FTP
{
    public enum FtpCommandType
    {
        USER, //Specify user for authentication
        PASS, //Specify password for authentication
        CWD, //Change working directory
        CDUP, //Change working directory to parent directory
        QUIT, //Disconnection
        DELE, //Delete file on the server
        PWD, //Print working directory
        PASV, //Enable "passive" mode for data transfer
        PORT, //Enable "active" mode for data transfer
        HELP, //List available commands
        NOOP, //Do nothing
        RETR, //Download file from server to client
        STOR, //Upload file from client to server
        LIST //List files in the current working directory 
    }

    public class FtpCommand
    {
        public string Command;
        public string Content;
    }

    public class FtpClient
    {
        public TcpClient Server;
        public TcpClient Client;

        public string Username;
        public string Password;

        public bool Connected;

        public bool IsConnected()
        {
            if (Connected == false)
            {
                SendReply(530, "Login incorrect.");
                return Connected;
            }
            else
            {
                return Connected;
            }
        }

        /// <summary>
        /// Process CWD command.
        /// </summary>
        /// <param name="code">Reply code.</param>
        /// <param name="command">Reply content.</param>
        public void SendReply(int code, string message)
        {
            Client.Send(Encoding.ASCII.GetBytes(code + " " + message + "\r\n"));
        }
    }

    /// <summary>
    /// FtpServer class. Used to handle FTP client connections.
    /// </summary>
    public class FtpServer : IDisposable
    {
        public TcpListener tcpListener;

        private bool Listening;

        public List<FtpClient> ftpClients;

        public string CurrentDirectory { get; set; }

        /// <summary>
        /// Create new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public FtpServer(string directory)
        {
            Listening = true;
            ftpClients = new List<FtpClient>();

            CurrentDirectory = directory;
        }

        public void Listen()
        {
            tcpListener = new TcpListener(21);
            tcpListener.Start();

            while (Listening)
            {
                var client = tcpListener.AcceptTcpClient();

                global::System.Console.WriteLine("Client[0] : New connection from " + client.StateMachine.source.ToString());

                ReceiveNewClient(client);
            }
        }

        public void ReceiveNewClient(TcpClient client)
        {
            var ftpClient = new FtpClient();
            ftpClient.Client = client;
            ftpClient.Connected = false;

            ftpClient.SendReply(220, "Service ready for new user.");

            while (ftpClient.Client.IsConnected())
            {
                ReceiveRequest(ftpClient);
            }

            ftpClient.SendReply(221, "Service closing control connection.");

            ftpClient.Client.Close();

            ftpClients.Add(ftpClient);
        }

        public void ReceiveRequest(FtpClient ftpClient)
        {
            var ep = new EndPoint(Address.Zero, 0);
            var data = Encoding.ASCII.GetString(ftpClient.Client.Receive(ref ep));

            global::System.Console.WriteLine("Client[0] : " + data);

            var splitted = data.Split(' ');

            var command = new FtpCommand();
            command.Command = splitted[0];
            command.Content = splitted[1];
            command.Content.Remove(command.Content.Length - 3, 2);

            ProcessRequest(ftpClient, command);
        }

        public void ProcessRequest(FtpClient ftpClient, FtpCommand command)
        {
            switch (command.Command)
            {
                case "USER":
                    ProcessUser(ftpClient, command);
                    break;
                case "PASS":
                    ProcessPass(ftpClient, command);
                    break;
                case "CWD":
                    ProcessCwd(ftpClient, command);
                    break;
                case "CDUP":
                    break;
                case "QUIT":
                    break;
                case "DELE":
                    break;
                case "PWD":
                    break;
                case "PASV":
                    break;
                case "PORT":
                    break;
                case "HELP":
                    break;
                case "NOOP":
                    break;
                case "RETR":
                    break;
                case "STOR":
                    break;
                case "LIST":
                    break;
                default:
                    ftpClient.SendReply(200, "Unknown command.");
                    break;
            }
        }

        /// <summary>
        /// Process USER command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessUser(FtpClient ftpClient, FtpCommand command)
        {
            if (String.IsNullOrEmpty(ftpClient.Username)) {
                ftpClient.Username = command.Content;
                ftpClient.SendReply(331, "User name okay, need password.");
            }
            else if (command.Content == "anonymous")
            {
                ftpClient.Username = command.Content;
                ftpClient.Connected = true;
                ftpClient.SendReply(230, "User logged in, proceed.");
            }
        }

        /// <summary>
        /// Process PASS command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessPass(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.Username == "anonymous")
            {
                ftpClient.SendReply(530, "Login incorrect.");
            }
            else if (String.IsNullOrEmpty(ftpClient.Username))
            {
                ftpClient.SendReply(332, "Need account for login.");
            }
            else
            {
                ftpClient.Password = command.Content;
                ftpClient.Connected = true;
                ftpClient.SendReply(230, "User logged in, proceed.");
            }
        }

        /// <summary>
        /// Process CWD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessCwd(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                try
                {
                    if (Directory.Exists(CurrentDirectory + command.Content))
                    {
                        Directory.SetCurrentDirectory(CurrentDirectory);
                        CurrentDirectory = CurrentDirectory + command.Content + @"\";
                    }
                    else if (File.Exists(CurrentDirectory + command.Content))
                    {
                        ftpClient.SendReply(550, "Requested action not taken.");
                    }
                    else
                    {
                        ftpClient.SendReply(550, "Requested action not taken.");
                    }
                }
                catch
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
        }

        public void Close()
        {
            Listening = false;
            tcpListener.Stop();
        }

        /// <summary>
        /// Close Client
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
