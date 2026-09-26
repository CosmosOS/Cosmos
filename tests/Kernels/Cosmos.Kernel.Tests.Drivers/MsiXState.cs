// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// Reads a function's MSI-X state the way a driver can, through the kit's
/// config and BAR accessors, so the interrupt cells check what the kit
/// programmed in the hardware rather than what it says it did. Layout from
/// PCI 3.0 §6.8.2: the capability's Message Control and Table Offset/BIR
/// registers, and 16-byte table entries whose last dword is Vector Control.
/// </summary>
internal static class MsiXState
{
    private const byte CapabilityId = 0x11;
    private const ushort MessageControlOffset = 0x02;
    private const ushort TableLocationOffset = 0x04;
    private const ushort EnableBit = 0x8000;
    private const uint TableBarMask = 0x7;
    private const uint TableOffsetMask = 0xFFFF_FFF8;
    private const ulong VectorControlOffset = 12;

    /// <summary>Vector Control bit 0: the entry is masked.</summary>
    public const uint EntryMaskBit = 0x1;

    /// <summary>True when the function has an MSI-X capability with MSI-X Enable set.</summary>
    public static bool IsEnabled(PciFunction function)
    {
        if (!function.TryFindCapability(CapabilityId, out ushort capability))
        {
            return false;
        }

        return (function.ReadConfig16((ushort)(capability + MessageControlOffset)) & EnableBit) != 0;
    }

    /// <summary>
    /// Maps the BAR holding the function's MSI-X table, during Probe, and
    /// gives where entry 0's Vector Control register sits in it, so the
    /// entry can be read during Probe and again once the binding is Bound.
    /// </summary>
    /// <returns>False when the function has no MSI-X, or its table BAR does not map.</returns>
    public static bool TryMapEntry0Control(PciDeviceContext context, [NotNullWhen(true)] out MmioRegion? region, out ulong offset)
    {
        region = null;
        offset = 0;
        PciFunction function = context.Function;
        if (!function.TryFindCapability(CapabilityId, out ushort capability))
        {
            return false;
        }

        uint location = function.ReadConfig32((ushort)(capability + TableLocationOffset));
        if (!context.TryMapBar((int)(location & TableBarMask), out MmioRegion? table))
        {
            return false;
        }

        region = table;
        offset = (location & TableOffsetMask) + VectorControlOffset;
        return true;
    }
}
