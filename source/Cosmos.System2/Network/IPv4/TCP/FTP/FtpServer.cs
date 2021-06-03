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
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
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

    public enum TransferMode
    {
        NONE,
        ACTV,
        PASV
    }

    public class FtpCommand
    {
        public string Command;
        public string Content;
    }

    public class FtpClient
    {
        public TcpClient Discussion;

        public TcpClient Data;
        public TcpListener DataListener;

        public TransferMode Mode;

        public string Username;
        public string Password;

        public bool Connected;

        public FtpClient(TcpClient client)
        {
            Discussion = client;
            Connected = false;
            Mode = TransferMode.NONE;
        }

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
            message = message.Replace('\\', '/');
            Discussion.Send(Encoding.ASCII.GetBytes(code + " " + message + "\r\n"));
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

        CosmosVFS FileSystem { get; set; }

        /// <summary>
        /// Create new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public FtpServer(CosmosVFS fs, string directory)
        {
            Listening = true;
            ftpClients = new List<FtpClient>();

            CurrentDirectory = directory;
            FileSystem = fs;
        }

        public void Listen()
        {
            tcpListener = new TcpListener(21);
            tcpListener.Start();

            while (Listening)
            {
                var client = tcpListener.AcceptTcpClient();

                global::System.Console.WriteLine("Client[0] : New connection from " + client.StateMachine.LocalAddress.ToString());

                ReceiveNewClient(client);
            }
        }

        public void ReceiveNewClient(TcpClient client)
        {
            var ftpClient = new FtpClient(client);

            ftpClient.SendReply(220, "Service ready for new user.");

            while (ftpClient.Discussion.IsConnected())
            {
                ReceiveRequest(ftpClient);
            }

            ftpClient.SendReply(221, "Service closing control connection.");

            ftpClient.Discussion.Close();

            ftpClients.Add(ftpClient);
        }

        public void ReceiveRequest(FtpClient ftpClient)
        {
            var ep = new EndPoint(Address.Zero, 0);
            var data = Encoding.ASCII.GetString(ftpClient.Discussion.Receive(ref ep));
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
                case "SYST":
                    ftpClient.SendReply(215, "CosmosOS");
                    break;
                case "CDUP":
                    ProcessCdup(ftpClient, command);
                    break;
                case "QUIT":
                    break;
                case "DELE":
                    ProcessDele(ftpClient, command);
                    break;
                case "PWD":
                    ProcessPwd(ftpClient, command);
                    break;
                case "PASV":
                    ProcessPasv(ftpClient, command);
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
                    ProcessStor(ftpClient, command);
                    break;
                case "RMD":
                    ProcessRmd(ftpClient, command);
                    break;
                case "MKD":
                    ProcessMkd(ftpClient, command);
                    break;
                case "LIST":
                    ProcessList(ftpClient, command);
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
            if (String.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            if (command.Content == "anonymous")
            {
                ftpClient.Username = command.Content;
                ftpClient.Connected = true;
                ftpClient.SendReply(230, "User logged in, proceed.");
            }
            else if (String.IsNullOrEmpty(ftpClient.Username)) {
                ftpClient.Username = command.Content;
                ftpClient.SendReply(331, "User name okay, need password.");
            }
            else
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process PASS command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessPass(FtpClient ftpClient, FtpCommand command)
        {
            if (String.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
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
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                try
                {
                    if (Directory.Exists(CurrentDirectory + "\\" + command.Content))
                    {
                        CurrentDirectory = CurrentDirectory + "\\" + command.Content;
                        Directory.SetCurrentDirectory(CurrentDirectory);
                        ftpClient.SendReply(250, "Requested file action okay.");
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

        /// <summary>
        /// Process PWD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessPwd(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                ftpClient.SendReply(257, "/" + CurrentDirectory + " created.");
            }
        }

        /// <summary>
        /// Process PASV command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessPasv(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                //TODO: Find port dynamically.
                int port = 20;
                var address = ftpClient.Discussion.StateMachine.LocalAddress.ToByteArray();

                ftpClient.SendReply(227, $"Entering Passive Mode ({address[0]},{address[1]},{address[2]},{address[3]},{port / 256},{port % 256})");

                ftpClient.DataListener = new TcpListener(port);
                ftpClient.DataListener.Start();

                ftpClient.Mode = TransferMode.PASV;
            }
        }

        /// <summary>
        /// Process LIST command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessList(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                    throw new NotImplementedException("FTP LIST command currently not supported in ACTV mode.");
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.SendReply(150, "Opening data connection.");

                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataListener.Stop();

                    var directory_list = FileSystem.GetDirectoryListing(CurrentDirectory + "\\" + command.Content);

                    var sb = new StringBuilder();
                    foreach (var directoryEntry in directory_list)
                    {
                        if (directoryEntry.mEntryType == DirectoryEntryTypeEnum.Directory)
                        {
                            sb.Append("d");
                        }
                        else
                        {
                            sb.Append("-");
                        }
                        sb.Append("rwxrwxrwx 1 unknown unknown ");
                        sb.Append(directoryEntry.mSize);
                        sb.Append(" Jan 1 09:00 ");
                        sb.AppendLine(directoryEntry.mName);
                    }

                    ftpClient.Data.Send(Encoding.ASCII.GetBytes(sb.ToString()));

                    ftpClient.Data.Close();

                    ftpClient.SendReply(226, "Transfer complete.");
                }
            }
        }

        /// <summary>
        /// Process DELE command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessDele(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                try
                {
                    if (File.Exists(CurrentDirectory + "\\" + command.Command))
                    {
                        File.Delete(CurrentDirectory + "\\" + command.Command);
                        ftpClient.SendReply(250, "Requested file action okay, completed.");
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

        /// <summary>
        /// Process RMD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessRmd(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                try
                {
                    if (Directory.Exists(CurrentDirectory + "\\" + command.Command))
                    {
                        Directory.Delete(CurrentDirectory + "\\" + command.Command, true);
                        ftpClient.SendReply(200, "Command okay.");
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

        /// <summary>
        /// Process MKD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessMkd(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                try
                {
                    if (Directory.Exists(CurrentDirectory + "\\" + command.Command))
                    {
                        ftpClient.SendReply(550, "Requested action not taken.");
                    }
                    else
                    {
                        Directory.CreateDirectory(CurrentDirectory + "\\" + command.Command);
                        ftpClient.SendReply(200, "Command okay.");
                    }
                }
                catch
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
        }

        /// <summary>
        /// Process CDUP command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessCdup(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                try
                {
                    CurrentDirectory = new DirectoryInfo(CurrentDirectory).Parent.FullName;
                    ftpClient.SendReply(250, "Requested file action okay.");
                }
                catch
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
        }

        /// <summary>
        /// Process STOR command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessStor(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                    throw new NotImplementedException("FTP STOR command currently not supported in ACTV mode.");
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.SendReply(150, "Opening data connection.");

                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataListener.Stop();

                    var ep = new EndPoint(Address.Zero, 0);
                    var data = ftpClient.Data.Receive(ref ep);

                    try
                    {
                        File.WriteAllBytes(CurrentDirectory + "\\" + command.Content, data);
                    }
                    catch
                    {
                        ftpClient.SendReply(550, "Requested action not taken.");
                    }

                    ftpClient.Data.Close();

                    ftpClient.SendReply(226, "Transfer complete.");
                }
            }
        }

        /// <summary>
        /// Process RETR command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        public void ProcessRetr(FtpClient ftpClient, FtpCommand command)
        {
            if (ftpClient.IsConnected())
            {
                if (String.IsNullOrEmpty(command.Content))
                {
                    ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                    return;
                }
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                    throw new NotImplementedException("FTP RETR command currently not supported in ACTV mode.");
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.SendReply(150, "Opening data connection.");

                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataListener.Stop();

                    try
                    {
                        var data = File.ReadAllBytes(CurrentDirectory + "\\" + command.Content);
                        ftpClient.Data.Send(data);
                    }
                    catch
                    {
                        ftpClient.SendReply(550, "Requested action not taken.");
                    }

                    ftpClient.Data.Close();

                    ftpClient.SendReply(226, "Transfer complete.");
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
