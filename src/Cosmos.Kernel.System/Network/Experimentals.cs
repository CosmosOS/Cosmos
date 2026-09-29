namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.System. An experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The packet seam: the protocol packet types (Ethernet, ARP, the
    /// version-neutral internet layer, IPv4, ICMP, UDP, DHCP, DNS, TCP), the
    /// <see cref="NetworkStack"/> members that transmit and inject them, and
    /// the client members that accept or return packet objects.
    /// </summary>
    internal const string PacketSeamDiagId = "COSMOS0002";

    /// <summary>
    /// The driver kit seam, the id the HAL's kit types carry, shared so one
    /// suppression covers the seam on both sides: the ring's
    /// <see cref="Graphics.ICanvas3DFactory"/>, which a display driver
    /// implements beside the kit's facets.
    /// </summary>
    internal const string DriverKitSeamDiagId = "COSMOS0003";
}
