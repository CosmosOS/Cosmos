// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.System: the network packet seam, the ring's side of the
/// driver kit seam and the members that move a thread between CPUs. An
/// experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The packet seam: the protocol packet types, which make up the
    /// <c>Cosmos.Kernel.System.Network.Protocols</c> subtree (Ethernet, ARP,
    /// the version-neutral internet layer, IPv4, ICMP, UDP, DHCP, DNS, TCP), the
    /// <see cref="Network.NetworkStack"/> members that transmit and inject
    /// them, and the client members that accept or return packet objects.
    /// </summary>
    internal const string PacketSeamDiagId = "COSMOS0002";

    /// <summary>
    /// The driver kit seam, the id the HAL's kit types carry, shared so one
    /// suppression covers the seam on both sides: the ring's
    /// <see cref="Graphics.Rendering3D.ICanvas3DFactory"/>, which a display
    /// driver implements beside the kit's facets.
    /// </summary>
    internal const string DriverKitSeamDiagId = "COSMOS0003";

    /// <summary>
    /// The members that move a thread between CPUs:
    /// <see cref="Threading.ThreadAffinity"/> and the setter of
    /// <see cref="Threading.KernelCpus.DefaultAffinity"/>. Experimental until
    /// the collector stops every CPU (SMP phase 4): until then a thread away
    /// from CPU 0 must not allocate, and the id is what tells a kernel so.
    /// The reads beside them, and the sets they take, are stable.
    /// </summary>
    internal const string AffinitySeamDiagId = "COSMOS0004";
}
