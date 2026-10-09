// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

/// <summary>
/// Bridge to the native ACPI IORT resolver
/// (<c>src/Cosmos.Kernel.Native.MultiArch/ACPI/acpi_wrapper.c</c>). Maps a
/// PCI requester ID (segment, BDF) to the ITS DeviceID the platform's
/// IORT advertises. Returns non-zero when no IORT mapping is present, in
/// which case callers fall back to <c>DeviceID = BDF</c>.
/// </summary>
internal static unsafe partial class AcpiIortNative
{
    /// <summary>
    /// Resolves <paramref name="bdf"/> on PCI segment <paramref name="segment"/>
    /// to an ITS DeviceID via IORT. Returns 0 on success.
    /// </summary>
    [LibraryImport("*", EntryPoint = "acpi_iort_resolve_device_id")]
    [SuppressGCTransition]
    public static partial int ResolveDeviceId(uint segment, uint bdf, out uint deviceId);
}
