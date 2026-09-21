using Cosmos.Build.API.Attributes;
using AddressFamily = System.Net.Sockets.AddressFamily;

namespace Cosmos.Kernel.Plugs.System.Net;

/// <summary>
/// Answers the BCL's "does the OS support this address family" probe without
/// opening a socket. The probe is the first thing <c>System.Net.Dns</c> and
/// <c>Socket.OSSupportsIPv6</c> evaluate; on Unix it opens and closes a
/// datagram socket through libSystem.Native, which the kernel does not carry.
/// IPv4 is what the plugged <see cref="global::System.Net.IPAddress"/> can
/// represent; IPv6 and Unix domain sockets are refused, which makes
/// <c>Dns</c> narrow an unspecified family to IPv4 before the resolver plug
/// sees it. Several assemblies compile their own copy of this type; a
/// name-addressed plug is applied to each.
/// </summary>
[Plug("System.Net.SocketProtocolSupportPal")]
public static class SocketProtocolSupportPalPlug
{
    [PlugMember]
    public static bool IsSupported(AddressFamily af) => af == AddressFamily.InterNetwork;
}
