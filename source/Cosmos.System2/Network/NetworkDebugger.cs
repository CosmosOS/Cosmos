using System;
using System.Text;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Con = System.Console;

namespace Cosmos.System.Network;

public class NetworkDebugger
{
    /// <summary>
    ///     TCP Server.
    /// </summary>
    private readonly TcpListener xListener;

    /// <summary>
    ///     TCP Client.
    /// </summary>
    private TcpClient xClient;

    /// <summary>
    ///     Create NetworkDebugger class (used to listen for a debugger connection)
    /// </summary>
    /// <param name="port">Port used for TCP connection.</param>
    public NetworkDebugger(int port)
    {
        Port = port;
        xListener = new TcpListener((ushort)port);
    }

    /// <summary>
    ///     Create NetworkDebugger class (used to connect to a remote debugger)
    /// </summary>
    /// <param name="ip">IP Address of the remote debugger.</param>
    /// <param name="port">Port used for TCP connection.</param>
    public NetworkDebugger(Address ip, int port)
    {
        Ip = ip;
        Port = port;
        xClient = new TcpClient(port);
    }

    /// <summary>
    ///     Remote IP Address
    /// </summary>
    public Address Ip { get; set; }

    /// <summary>
    ///     Port used
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Start debugger
    /// </summary>
    public void Start()
    {
        if (xClient == null)
        {
            xListener.Start();

            Con.WriteLine("Waiting for remote debugger connection at " + NetworkConfiguration.CurrentAddress + ":" +
                          Port);
            xClient = xListener.AcceptTcpClient(); //blocking
        }
        else if (xListener == null)
        {
            xClient.Connect(Ip, Port);
        }

        Send("--- Cosmos Network Debugger ---");
        Send("Debugger Connected!");
    }

    /// <summary>
    ///     Send text to the debugger
    /// </summary>
    /// <param name="message">Text to send to the debugger.</param>
    public void Send(string message) =>
        xClient.Send(Encoding.ASCII.GetBytes("[" + DateTime.Now.ToString("HH:mm:ss") + "] - " + message + "\r\n"));

    /// <summary>
    ///     Stop the debugger by closing TCP Connection
    /// </summary>
    public void Stop()
    {
        Con.WriteLine("Closing Debugger connection");
        Send("Closing...");
        xClient.Close();
    }
}
