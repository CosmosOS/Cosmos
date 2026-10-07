// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.HAL. An experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The driver kit seam: every public type in Cosmos.Kernel.HAL, the kit
    /// under <c>Cosmos.Kernel.HAL.DriverKit</c> and the device categories
    /// under <c>Cosmos.Kernel.HAL.Devices</c>, except the two stable device
    /// contracts, <c>IBlockDevice</c> and <c>MacAddress</c>. A build target in
    /// the HAL project fails the build if any other public type loses the
    /// attribute, so promoting a type out of the seam is a deliberate change
    /// of that target's list rather than of one type.
    /// </summary>
    internal const string DriverKitSeamDiagId = "COSMOS0003";
}
