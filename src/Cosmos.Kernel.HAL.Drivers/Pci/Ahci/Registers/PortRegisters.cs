// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;

/// <summary>
/// The registers of one AHCI port that this driver uses (AHCI 1.3.1 s3.3).
/// Carries a back-reference to the <see cref="AhciController"/> that owns
/// the port so per-port code (SATA command issue, port reset) can reach
/// controller state without going through globals.
/// </summary>
internal sealed class PortRegisters
{
    /// <summary>Offset of port 0's register bank from ABAR (AHCI spec 3.3).</summary>
    internal const ulong RegistersBaseOffset = 0x100;

    /// <summary>Size of one port register bank in bytes; port N starts at 0x100 + N * 0x80 (AHCI spec 3.3).</summary>
    internal const ulong BankBytes = 0x80;

    /// <summary>PxCLB - Command List Base Address register offset (AHCI spec 3.3.1).</summary>
    private const ulong ClbOffset = 0x00;

    /// <summary>PxCLBU - Command List Base Address Upper 32-bits register offset (AHCI spec 3.3.2).</summary>
    private const ulong ClbuOffset = 0x04;

    /// <summary>PxFB - FIS Base Address register offset (AHCI spec 3.3.3).</summary>
    private const ulong FbOffset = 0x08;

    /// <summary>PxFBU - FIS Base Address Upper 32-bits register offset (AHCI spec 3.3.4).</summary>
    private const ulong FbuOffset = 0x0C;

    /// <summary>PxIS - Interrupt Status register offset (AHCI spec 3.3.5).</summary>
    private const ulong IsOffset = 0x10;

    /// <summary>PxIE - Interrupt Enable register offset (AHCI spec 3.3.6).</summary>
    private const ulong IeOffset = 0x14;

    /// <summary>PxCMD - Command and Status register offset (AHCI spec 3.3.7).</summary>
    private const ulong CmdOffset = 0x18;

    /// <summary>PxTFD - Task File Data register offset (AHCI spec 3.3.8).</summary>
    private const ulong TfdOffset = 0x20;

    /// <summary>PxSIG - Signature register offset (AHCI spec 3.3.9).</summary>
    private const ulong SigOffset = 0x24;

    /// <summary>PxSSTS - SATA Status register offset (AHCI spec 3.3.10).</summary>
    private const ulong SstsOffset = 0x28;

    /// <summary>PxSCTL - SATA Control register offset (AHCI spec 3.3.11).</summary>
    private const ulong SctlOffset = 0x2C;

    /// <summary>PxSERR - SATA Error register offset (AHCI spec 3.3.12).</summary>
    private const ulong SerrOffset = 0x30;

    /// <summary>PxSACT - SATA Active register offset (AHCI spec 3.3.13).</summary>
    private const ulong SactOffset = 0x34;

    /// <summary>PxCI - Command Issue register offset (AHCI spec 3.3.14).</summary>
    private const ulong CiOffset = 0x38;

    // Port register field masks (AHCI spec 3.3.10 / 3.3.11); shared with
    // AhciController and Sata, which operate on this register bank.

    /// <summary>PxSSTS.DET - Device Detection 4-bit field mask (bits 3:0, AHCI spec 3.3.10).</summary>
    internal const uint SstsDetMask = 0x0F;

    /// <summary>PxSSTS.IPM - Interface Power Management field position (bits 11:8, AHCI spec 3.3.10).</summary>
    internal const int SstsIpmShift = 8;

    /// <summary>PxSSTS.IPM - Interface Power Management 4-bit field mask (AHCI spec 3.3.10).</summary>
    internal const uint SstsIpmMask = 0x0F;

    /// <summary>PxSCTL.DET - Device Detection Initialization 4-bit field mask (bits 3:0, AHCI spec 3.3.11).</summary>
    internal const uint SctlDetMask = 0xFU;

    /// <summary>PxSCTL.DET value 1: perform interface communication initialization (COMRESET, AHCI spec 3.3.11).</summary>
    internal const uint SctlDetComreset = 1U;

    /// <summary>All-ones write for RW1C registers (PxSERR/PxIS): clears every latched bit.</summary>
    internal const uint Rw1CClearAll = 0xFFFFFFFFu;

    private readonly MmioRegion _abar;
    private readonly ulong _offset;

    /// <summary>The port index, 0 to 31.</summary>
    internal uint PortNumber { get; }

    /// <summary>What the port's signature said is attached; <see cref="PortType.Nothing"/> until the port is classified.</summary>
    internal PortType PortType { get; set; } = PortType.Nothing;

    /// <summary>The controller the port belongs to.</summary>
    internal AhciController Controller { get; }

    /// <summary>Command List Base Address</summary>
    internal uint CLB
    {
        get => _abar.Read32(_offset + ClbOffset);
        set => _abar.Write32(_offset + ClbOffset, value);
    }

    /// <summary>Command List Base Address Upper</summary>
    internal uint CLBU
    {
        get => _abar.Read32(_offset + ClbuOffset);
        set => _abar.Write32(_offset + ClbuOffset, value);
    }

    /// <summary>FIS Base Address</summary>
    internal uint FB
    {
        get => _abar.Read32(_offset + FbOffset);
        set => _abar.Write32(_offset + FbOffset, value);
    }

    /// <summary>FIS Base Address Upper</summary>
    internal uint FBU
    {
        get => _abar.Read32(_offset + FbuOffset);
        set => _abar.Write32(_offset + FbuOffset, value);
    }

    /// <summary>Interrupt Status</summary>
    internal uint IS
    {
        get => _abar.Read32(_offset + IsOffset);
        set => _abar.Write32(_offset + IsOffset, value);
    }

    /// <summary>Interrupt Enable</summary>
    internal uint IE
    {
        get => _abar.Read32(_offset + IeOffset);
        set => _abar.Write32(_offset + IeOffset, value);
    }

    /// <summary>Command</summary>
    internal uint CMD
    {
        get => _abar.Read32(_offset + CmdOffset);
        set => _abar.Write32(_offset + CmdOffset, value);
    }

    /// <summary>Task File Data</summary>
    internal uint TFD => _abar.Read32(_offset + TfdOffset);

    /// <summary>Signature</summary>
    internal uint SIG => _abar.Read32(_offset + SigOffset);

    /// <summary>SATA Status</summary>
    internal uint SSTS => _abar.Read32(_offset + SstsOffset);

    /// <summary>SATA Control</summary>
    internal uint SCTL
    {
        get => _abar.Read32(_offset + SctlOffset);
        set => _abar.Write32(_offset + SctlOffset, value);
    }

    /// <summary>SATA Error</summary>
    internal uint SERR
    {
        get => _abar.Read32(_offset + SerrOffset);
        set => _abar.Write32(_offset + SerrOffset, value);
    }

    /// <summary>SATA Active</summary>
    internal uint SACT => _abar.Read32(_offset + SactOffset);

    /// <summary>Command Issue</summary>
    internal uint CI
    {
        get => _abar.Read32(_offset + CiOffset);
        set => _abar.Write32(_offset + CiOffset, value);
    }

    /// <summary>Creates the view of port <paramref name="portNumber"/>'s registers. Touches no register.</summary>
    /// <param name="abar">BAR5, mapped; the caller checked the port's bank lies inside it.</param>
    /// <param name="portNumber">The port index, 0 to 31.</param>
    /// <param name="controller">The controller the port belongs to.</param>
    internal PortRegisters(MmioRegion abar, uint portNumber, AhciController controller)
    {
        _abar = abar;
        _offset = RegistersBaseOffset + BankBytes * portNumber;
        PortNumber = portNumber;
        Controller = controller;
    }
}
