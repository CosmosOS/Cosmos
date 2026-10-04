// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit;

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
    /// The driver kit seam: every public type under
    /// <c>Cosmos.Kernel.HAL.DriverKit</c> and its <c>Devices</c>,
    /// <c>Synthetic</c>, <c>Platform</c>, <c>Pci</c>, <c>Usb</c> and <c>Virtio</c> namespaces. A build
    /// target in the HAL project fails the build if a type under those
    /// namespaces loses the attribute, so promoting the kit out of the seam
    /// is a deliberate change of that target rather than of one type.
    /// </summary>
    internal const string DriverKitSeamDiagId = "COSMOS0003";
}
