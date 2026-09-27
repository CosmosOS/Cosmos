// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.Registers;

/// <summary>
/// The AHCI Generic Host Control registers this driver uses, at the start
/// of the mapped ABAR (AHCI 1.3.1 s3.1).
/// </summary>
internal sealed class GenericRegisters
{
    /// <summary>CAP - Host Capabilities register offset (AHCI spec 3.1.1).</summary>
    private const ulong CapabilitiesOffset = 0x00;

    /// <summary>GHC - Global Host Control register offset (AHCI spec 3.1.2).</summary>
    private const ulong GlobalHostControlOffset = 0x04;

    /// <summary>PI - Ports Implemented register offset (AHCI spec 3.1.4).</summary>
    private const ulong ImplementedPortsOffset = 0x0C;

    /// <summary>VS - AHCI Version register offset (AHCI spec 3.1.5).</summary>
    private const ulong AhciVersionOffset = 0x10;

    private readonly MmioRegion _abar;

    /// <summary>CAP - Host Capabilities.</summary>
    internal uint Capabilities => _abar.Read32(CapabilitiesOffset);

    /// <summary>GHC - Global Host Control.</summary>
    internal uint GlobalHostControl
    {
        get => _abar.Read32(GlobalHostControlOffset);
        set => _abar.Write32(GlobalHostControlOffset, value);
    }

    /// <summary>PI - Ports Implemented.</summary>
    internal uint ImplementedPorts
    {
        get => _abar.Read32(ImplementedPortsOffset);
        set => _abar.Write32(ImplementedPortsOffset, value);
    }

    /// <summary>VS - AHCI Version.</summary>
    internal uint AhciVersion => _abar.Read32(AhciVersionOffset);

    /// <summary>Creates the view of the generic registers of <paramref name="abar"/>. Touches no register.</summary>
    /// <param name="abar">BAR5, mapped.</param>
    internal GenericRegisters(MmioRegion abar)
    {
        _abar = abar;
    }
}
