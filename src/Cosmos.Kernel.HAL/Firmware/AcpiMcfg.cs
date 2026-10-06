// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// C# bridge to native ACPI MCFG discovery (acpi_wrapper.c in MultiArch).
/// The native code is called during early boot (kmain) and parses the MCFG
/// table to extract the PCI ECAM window. This class just retrieves the result.
/// Native import lives in Cosmos.Kernel.Core/Bridge/Import/AcpiMcfgNative.cs.
/// </summary>
internal static unsafe class AcpiMcfg
{
    /// <summary>
    /// Mirrors the C struct acpi_mcfg_info_t from ACPI/acpi_wrapper.c.
    /// Must match the native layout exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct McfgInfo
    {
        public byte Found;
        public byte StartBus;
        public byte EndBus;
        private byte _pad1;
        public ushort Segment;
        private ushort _pad2;
        public ulong BaseAddress;  // ECAM physical base
    }

    /// <summary>
    /// Copies the MCFG entry ACPI reported (base address, segment, first and
    /// last bus), for the machine description that publishes the PCI host
    /// node. Thread context; allocation-free.
    /// </summary>
    /// <param name="info">The entry, or default when there is none.</param>
    /// <returns>False when ACPI was unavailable or the MCFG table was not found.</returns>
    public static bool TryGetInfo(out McfgInfo info)
    {
        McfgInfo* mcfg = (McfgInfo*)AcpiMcfgNative.GetMcfgInfo();
        if (mcfg != null && mcfg->Found != 0)
        {
            info = *mcfg;
            return true;
        }

        info = default;
        return false;
    }
}
