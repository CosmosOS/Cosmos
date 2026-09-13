using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.Kernel.Plugs.System.Net;

[Plug(typeof(IPAddress))]
public static class IPAddressPlug
{
    public const int IPv4AddressBytes = 4;
    public const int IPv6AddressBytes = 16;

    // Store address data per instance (public for cross-assembly access when patched)
    public static readonly Dictionary<int, uint> _addresses = new();

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(IPAddress aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(IPAddress aThis, long address)
    {
        int id = GetId(aThis);
        _addresses[id] = (uint)address;
    }

    [PlugMember(".ctor")]
    public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address)
    {
        Ctor(aThis, address.ToArray());
    }

    [PlugMember(".ctor")]
    public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address, long scopeId)
    {
        Ctor(aThis, address.ToArray());
    }

    [PlugMember(".ctor")]
    public static void Ctor(IPAddress aThis, byte[] address)
    {
        int id = GetId(aThis);
        if (address.Length == IPv4AddressBytes)
        {
            _addresses[id] = (uint)((address[0] << 0) | (address[1] << 8) | (address[2] << 16) | (address[3] << 24));
        }
        else if (address.Length == IPv6AddressBytes)
        {
            // IPv6 not fully supported yet
            _addresses[id] = 0;
        }
        else
        {
            Log.WriteString("[IPAddressPlug] Bad IP address format\n");
            throw new ArgumentException("Bad IP address format");
        }
    }

    [PlugMember("get_AddressFamily")]
    public static AddressFamily get_AddressFamily(IPAddress aThis)
    {
        return AddressFamily.InterNetwork;
    }

    [PlugMember]
    public static byte[] GetAddressBytes(IPAddress aThis)
    {
        int id = GetId(aThis);
        uint addr = _addresses.TryGetValue(id, out uint a) ? a : 0;
        return new byte[]
        {
            (byte)(addr & 0xFF),
            (byte)((addr >> 8) & 0xFF),
            (byte)((addr >> 16) & 0xFF),
            (byte)((addr >> 24) & 0xFF)
        };
    }

    [PlugMember]
    public static string ToString(IPAddress aThis)
    {
        int id = GetId(aThis);
        uint addr = _addresses.TryGetValue(id, out uint a) ? a : 0;
        // Use simple string concatenation with manual byte conversion to avoid resource loading
        return ByteToString((byte)(addr & 0xFF)) + "." +
               ByteToString((byte)((addr >> 8) & 0xFF)) + "." +
               ByteToString((byte)((addr >> 16) & 0xFF)) + "." +
               ByteToString((byte)((addr >> 24) & 0xFF));
    }

    // Simple byte to string without using .NET formatting (avoids resource loading)
    private static string ByteToString(byte value)
    {
        if (value == 0)
        {
            return "0";
        }

        if (value < 10)
        {
            return ((char)('0' + value)).ToString();
        }

        if (value < 100)
        {
            char d1 = (char)('0' + value / 10);
            char d0 = (char)('0' + value % 10);
            return new string(new[] { d1, d0 });
        }
        else
        {
            char d2 = (char)('0' + value / 100);
            char d1 = (char)('0' + (value / 10) % 10);
            char d0 = (char)('0' + value % 10);
            return new string(new[] { d2, d1, d0 });
        }
    }

    [PlugMember]
    public static IPAddress? Parse(string ipString)
    {
        string[] fragments = ipString.Split('.');
        if (fragments.Length == 4)
        {
            try
            {
                byte[] addressArray = new byte[4];
                addressArray[0] = byte.Parse(fragments[0]);
                addressArray[1] = byte.Parse(fragments[1]);
                addressArray[2] = byte.Parse(fragments[2]);
                addressArray[3] = byte.Parse(fragments[3]);
                return new IPAddress(addressArray);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    [PlugMember]
    public static bool TryParse(string ipString, [NotNullWhen(true)] out IPAddress? address)
    {
        address = Parse(ipString);
        return address != null;
    }

    // Static property plugs for well-known addresses
    [PlugMember("get_Any")]
    public static IPAddress get_Any()
    {
        return new IPAddress(0); // 0.0.0.0
    }

    [PlugMember("get_Loopback")]
    public static IPAddress get_Loopback()
    {
        return new IPAddress(new byte[] { 127, 0, 0, 1 });
    }

    [PlugMember("get_Broadcast")]
    public static IPAddress get_Broadcast()
    {
        return new IPAddress(new byte[] { 255, 255, 255, 255 });
    }

    [PlugMember("get_None")]
    public static IPAddress get_None()
    {
        return new IPAddress(new byte[] { 255, 255, 255, 255 });
    }

    // Helper to get raw address value (public for cross-assembly access)
    public static uint GetAddress(IPAddress aThis)
    {
        int id = GetId(aThis);
        return _addresses.TryGetValue(id, out uint a) ? a : 0;
    }
}
