// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.HAL;

/// <summary>
/// C# bridge to native ACPI MCFG discovery (acpi_wrapper.c in MultiArch).
/// The native code is called during early boot (kmain) and parses the MCFG
/// table to extract the PCI ECAM base address. This class just retrieves the result.
/// Native import lives in Cosmos.Kernel.Core/Bridge/Import/AcpiNative.cs.
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
    /// Gets MCFG information discovered from ACPI during early boot.
    /// Returns null if ACPI was not available or MCFG table wasn't found.
    /// </summary>
    public static McfgInfo* GetMcfgInfo()
    {
        return (McfgInfo*)AcpiMcfgNative.GetMcfgInfo();
    }

    /// <summary>
    /// Gets the PCI ECAM physical base address from ACPI MCFG.
    /// Returns 0 if MCFG table was not found.
    /// </summary>
    public static ulong GetEcamBase()
    {
        McfgInfo* mcfg = (McfgInfo*)AcpiMcfgNative.GetMcfgInfo();
        if (mcfg != null && mcfg->Found != 0)
        {
            return mcfg->BaseAddress;
        }

        return 0;
    }
}
