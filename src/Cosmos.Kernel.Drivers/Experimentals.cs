// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.Drivers. An experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The driver kit seam, the id the HAL's kit types and the ring's
    /// <c>ICanvas3DFactory</c> carry, shared so one suppression covers the
    /// seam on every side: here it marks <see cref="ISvgaAdapter"/>, the
    /// VMware SVGA II adapter's test and tooling facet.
    /// </summary>
    internal const string DriverKitSeamDiagId = "COSMOS0003";
}
