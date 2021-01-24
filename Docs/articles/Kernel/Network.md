# Network

In this article we will discuss about Networking on Cosmos, how to use the Network Stack, send and received packets. For now, available protocols are **ARP**, **IPv4**, **UDP**, **ICMP**, **DHCP** and **DNS** but the team is also working on TCP. Note that Cosmos devkit must be installed for this article.

All protocols here don't necessary support every feature described by their RFC and may have some bugs or architecture issues, if you find bugs or something abnormal please [submit an issue](http://https://github.com/CosmosOS/Cosmos/issues/new/choose "repository") on our repository. 

Each protocol has a Client class which can be used to receive and send data. If a Receive() method is blocking, the method will timeout after 5 seconds or use the value optionally set by parameter. Please note that all finished connections should be closed using Close().

Cosmos Network Stack won't use Classes and Functions that are under .NET Core. Everything described here will be under:
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

    xClient.Close();  //don't forget to close!
}
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
    xClient.Receive(ref endpoint);  //set endpoint to remote machine IP:port
    xClient.NonBlockingReceive(ref endpoint); //retrieve receive buffer without waiting

    xClient.Close();
}
```

## ICMP
For ICMP, we will only able to send an ICMP echo to a distant machine and wait for its response. If another machine sends us an ICMP echo, Cosmos will automatically handle the request and reply.
```csharp
using(var xClient = new ICMPClient())
{
    xClient.Connect(new Address(192, 168, 1, 254));

    /** Send ICMP Echo **/
    xClient.SendEcho();

    /** Receive ICMP Response **/
    int time = xClient.Receive(ref endpoint); //return elapsed time / timeout if no response

    xClient.Close();
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
    
    xClient.Close();
}
```
## Utils
## Get local IP
```csharp
Console.WriteLine(NetworkConfig.CurrentConfig.Value.IPAddress.ToString());
```
