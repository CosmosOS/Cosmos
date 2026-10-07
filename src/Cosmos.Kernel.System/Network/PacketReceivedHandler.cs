// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Delegate for handling packet received events.
/// </summary>
/// <param name="data">The received packet data.</param>
/// <param name="length">The length of the packet.</param>
internal delegate void PacketReceivedHandler(byte[] data, int length);
