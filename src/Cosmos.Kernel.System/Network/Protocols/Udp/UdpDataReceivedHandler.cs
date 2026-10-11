// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network.Protocols.Udp;

/// <summary>
/// Delegate for UDP data received events.
/// </summary>
/// <param name="packet">The received datagram.</param>
internal delegate void UdpDataReceivedHandler(UdpPacket packet);
