# Network

In this article we will discuss about Networking on Cosmos, how to use the Network Stack, send and receive packets. For now, available protocols are **ARP**, **IPv4**, **TCP**, **UDP**, **ICMP**, **DHCP** and **DNS**. Note that Cosmos devkit must be installed for this article.

All protocols here don't necessary support every feature described by their RFC and may have some bugs or architecture issues, if you find bugs or something abnormal please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository. 

Each protocol has a Client class which can be used to receive and send data. If a Receive() method is blocking, the method will timeout after 5 seconds or use the value optionally set by parameter. Please note that all finished connections should be closed using Close().

The Cosmos Network Stack mainly not uses classes and functions that are under .NET Core (except TCP). Everything described here will be under:
```csharp
using Cosmos.System.Network;
```

Before anything, a Network Configuration must be set (local machine IPv4 address, subnet mask and gateway). It can be manually set with IPConfig.Enable or dynamically set through a DHCP server. For DHCP, Cosmos will ask to the DHCP server (usually default gateway) for an address in your local network.

### Manually set IPv4 Config
```csharp
NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0"); //get network device by name
IPConfig.Enable(nic, new Address(192, 168, 1, 69), new Address(255, 255, 255, 0), new Address(192, 168, 1, 254)); //enable IPv4 configuration
```
### Dynamically set IPv4 Config through DHCP
```csharp
using(var xClient = new DHCPClient())
{
    /** Send a DHCP Discover packet **/
    //This will automatically set the IP config after DHCP response
    xClient.SendDiscoverPacket();
}
```

### Get local IP address
```csharp
Console.WriteLine(NetworkConfiguration.CurrentAddress.ToString());
```

## UDP
Before playing with packets, we have to create a client and call Connect() to specify the remote machine address. After that the client will be able to send or listen for data.
```csharp
using(var xClient = new UdpClient(4242))
{
    xClient.Connect(new Address(192, 168, 1, 70), 4242);

    /** Send data **/
    xClient.Send(Encoding.ASCII.GetBytes(message));

    /** Receive data **/
    var endpoint = new EndPoint(Address.Zero, 0);
    var data = xClient.Receive(ref endpoint);  //set endpoint to remote machine IP:port
    var data2 = xClient.NonBlockingReceive(ref endpoint); //retrieve receive buffer without waiting
}
```

## TCP
Unlike UDP, TCP is plugged with the dotnet framework. You won't have to use Cosmos.System.Network but System.Net.Sockets and System.Net. You can setup TCP network streams using TcpListener, TcpClient and NetworkStream, don't use the Stream class unless you know what you do.

Server:
```csharp
using System.Text;
using System.Net.Sockets;
using System.Net;

class TcpServer
{
    private TcpListener tcpListener;
    private int port;

    public TcpServer(int port)
    {
        this.port = port;
        var address = IPAddress.Any;
        this.tcpListener = new TcpListener(address, port);
    }

    public void Start()
    {
        this.tcpListener.Start();

        while (true)
        {
            /** Wait for new connections **/
            TcpClient client = this.tcpListener.AcceptTcpClient();
            HandleClientComm(client);
            client.Close();
        }
    }

    private void HandleClientComm(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead;

        while (true)
        {
            bytesRead = 0;

            /** Receive data **/
            bytesRead = stream.Read(buffer, 0, buffer.Length); // Blocks until a client sends a message

            if (bytesRead == 0) // The client has disconnected from the server
            {
                break;
            }

            string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            /** Send data **/
            byte[] response = Encoding.ASCII.GetBytes("ok");
            stream.Write(response, 0, response.Length);

            // stream.Flush(); useless for now
        }
        stream.Close();
    }
}
```

Client :
```csharp
string serverIp = "192.168.1.63";
int serverPort = 1312;

using(TcpClient client = new TcpClient())
{
    /**Connect to server **/
    client.Connect(serverIp, serverPort);
    NetworkStream stream = client.GetStream();
    
    /** Send data **/
    string messageToSend = "Hello from CosmosOS!";
    byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);
    stream.Write(dataToSend, 0, dataToSend.Length);
    
    /** Receive data **/
    byte[] receivedData = new byte[client.ReceiveBufferSize];
    int bytesRead = stream.Read(receivedData, 0, receivedData.Length);
    string receivedMessage = Encoding.ASCII.GetString(receivedData, 0, bytesRead);
    
    /** Close data stream **/
    stream.Close();
}
```

## FTP
Only server side is implemented in Cosmos. We recommand to use FileZilla as your FTP client.

**Your FTP client must enable active mode**. Since in Active Mode the server has to open TCP connections, **your computer firewall must be disabled** to accept incoming connection. An FTP connection is made of two TCP sockets. One for control connection (as a textual protocol) and one for data transmission. Data transmission sockets can be opened by the client (if it is in Passive Mode) or by the server (if in Active Mode). The Passive Mode is not supported yet due to current Cosmos TCP and multithreading limitation.

### Installation:

Install CosmosFtpServer package into your Cosmos kernel. For more information see [CosmosFTP readme](https://github.com/CosmosOS/CosmosFtp).

### FTP client configuration:

Use Plain FTP with an Anonymous connection.

![FTP client configuration](https://user-images.githubusercontent.com/18724279/121685499-4c71f380-cac0-11eb-8d08-6db1c0096e68.png)

### Usage:

Please note that for now only one FTP connection can be accepted, the server will shut down itself after the client disconnection.

```csharp
/** Initialize filesystem **/
var fs = new CosmosVFS();
VFSManager.RegisterVFS(fs);

using(var xServer = new FtpServer(fs, "0:\\"))
{
    /** Listen for new FTP client connections **/
    FtpServer.Listen();
}
```

## ICMP
For ICMP, we will only be able to send an ICMP echo to a distant machine and wait for its response. If another machine sends us an ICMP echo, Cosmos will automatically handle the request and reply.
```csharp
using(var xClient = new ICMPClient())
{
    xClient.Connect(new Address(192, 168, 1, 254));

    /** Send ICMP Echo **/
    xClient.SendEcho();

    /** Receive ICMP Response **/
    int time = xClient.Receive(ref endpoint); //return elapsed time / timeout if no response
}

```
## DNS
DNS can be used to get an IP address from a Domain Name string. For now DNS can only ask for one domain name at the same time.
```csharp
using(var xClient = new DnsClient())
{
    xClient.Connect(new Address(192, 168, 1, 254)); //DNS Server address

    /** Send DNS ask for a single domain name **/
    xClient.SendAsk("github.com");

    /** Receive DNS Response **/
    Address destination = xClient.Receive(); //can set a timeout value
}
```
