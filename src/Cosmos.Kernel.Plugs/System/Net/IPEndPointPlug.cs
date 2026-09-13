using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.Net;

[Plug(typeof(IPEndPoint))]
public static class IPEndPointPlug
{
    // Store endpoint data per instance in separate dictionaries
    public static readonly Dictionary<int, IPAddress> _addresses = [];
    public static readonly Dictionary<int, int> _ports = [];

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(IPEndPoint aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(IPEndPoint aThis, long address, int port)
    {
        int id = GetId(aThis);
        _addresses[id] = new IPAddress(address);
        _ports[id] = port;
    }

    [PlugMember(".ctor")]
    public static void Ctor(IPEndPoint aThis, IPAddress address, int port)
    {
        int id = GetId(aThis);
        _addresses[id] = address;
        _ports[id] = port;
    }

    [PlugMember("get_Address")]
    public static IPAddress? get_Address(IPEndPoint aThis)
    {
        int id = GetId(aThis);
        if (_addresses.TryGetValue(id, out var addr))
        {
            return addr;
        }

        return null;
    }

    [PlugMember("set_Address")]
    public static void set_Address(IPEndPoint aThis, IPAddress value)
    {
        int id = GetId(aThis);
        _addresses[id] = value;
    }

    [PlugMember("get_Port")]
    public static int get_Port(IPEndPoint aThis)
    {
        int id = GetId(aThis);
        if (_ports.TryGetValue(id, out int port))
        {
            return port;
        }

        return 0;
    }

    [PlugMember("set_Port")]
    public static void set_Port(IPEndPoint aThis, int value)
    {
        int id = GetId(aThis);
        _ports[id] = value;
    }

    [PlugMember("get_AddressFamily")]
    public static AddressFamily get_AddressFamily(IPEndPoint aThis)
    {
        int id = GetId(aThis);
        if (_addresses.TryGetValue(id, out IPAddress? addr))
        {
            return addr.AddressFamily;
        }
        return AddressFamily.InterNetwork; // Default to IPv4
    }

    [PlugMember]
    public static string ToString(IPEndPoint aThis)
    {
        int id = GetId(aThis);
        if (_addresses.TryGetValue(id, out var addr) && _ports.TryGetValue(id, out int port))
        {
            return $"{addr}:{port}";
        }

        return "0.0.0.0:0";
    }
}
