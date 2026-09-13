// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// One initialized AHCI HBA. Each controller owns its ABAR (HHDM-mapped),
/// capability snapshot, port list, and a per-controller command region
/// allocated from <see cref="PageAllocator"/> — kernel-virtual for memset,
/// physical (via <see cref="PageAllocator.VirtualToPhysical"/>) for the
/// HBA's CLB / FB / CTBA registers. The driver scans PCI for every
/// matching SATA/AHCI controller and instantiates one
/// <see cref="AhciController"/> per match, so a system with two HBAs gets
/// two instances and all of their ports show up in <see cref="Ahci.Ports"/>.
/// </summary>
internal unsafe class AhciController
{
    /// <summary>Stride between consecutive ports' command lists inside the command region (1 KiB; AHCI's CLB alignment).</summary>
    private const uint PortStrideCLB = 0x400;
    /// <summary>Offset of port 0's FIS-receive area inside the command region (after 32 ports' command lists).</summary>
    private const uint FBBaseOffset = 0x8000;
    /// <summary>Stride between consecutive ports' FIS-receive areas (256 B; AHCI's FB alignment).</summary>
    private const uint PortStrideFB = 0x100;
    /// <summary>Offset of port 0's command tables inside the command region (after 32 ports' FIS-receive areas).</summary>
    private const uint CTBABaseOffset = 0xA000;
    /// <summary>Stride between consecutive ports' command-table blocks (32 slots × 0x100 B each).</summary>
    private const uint PortStrideCTBA = 0x2000;
    /// <summary>Stride between consecutive command-table slots within a port (AHCI's 128 B CTBA alignment holds via page alignment).</summary>
    private const uint SlotStrideCTBA = 0x100;
    /// <summary>Command-region size: 0x4A000 (296 KiB) covers CLB + FB + 32 command-table slots for all 32 possible port indices.</summary>
    private const ulong CommandRegionBytes = 0x4A000;
    /// <summary>Command-region size in 4 KiB pages (74).</summary>
    private const ulong CommandRegionPages = (CommandRegionBytes + PageAllocator.PageSize - 1) / PageAllocator.PageSize;

    /// <summary>Bytes zeroed for one port's command list: 32 headers × 32 B (AHCI 1.3.1 s4.2.2).</summary>
    private const int CommandListBytes = 1024;
    /// <summary>Bytes zeroed for one port's FIS-receive area (AHCI 1.3.1 s4.2.1).</summary>
    private const int FisReceiveBytes = 256;
    /// <summary>Bytes zeroed for one command table slot (header + PRDT entries).</summary>
    private const int CommandTableBytes = 0x100;

    /// <summary>Maximum number of ports an AHCI HBA can implement (PI is a 32-bit mask).</summary>
    private const uint MaxPorts = 32;
    /// <summary>Command header slots per port command list (AHCI 1.3.1 s4.2.2).</summary>
    private const uint CommandSlotsPerPort = 32;
    /// <summary>PRDT entries reserved per command table, matching the 0x100-byte slot layout.</summary>
    private const ushort PrdtEntriesPerCommandTable = 8;

    /// <summary>PCI BAR index of ABAR, the AHCI MMIO region (AHCI 1.3.1 s2.1.11).</summary>
    private const int AbarBarIndex = 5;
    /// <summary>Offset of port 0's register bank from ABAR; each port adds 0x80 (AHCI 1.3.1 s3.3).</summary>
    private const ulong PortRegistersBaseOffset = 0x100;

    /// <summary>GHC.AE - AHCI Enable (bit 31, AHCI 1.3.1 s3.1.2).</summary>
    private const uint GhcAhciEnable = 1U << 31;
    /// <summary>GHC.HR - HBA Reset, self-clearing (bit 0, AHCI 1.3.1 s3.1.2).</summary>
    private const uint GhcHbaReset = 1U;
    /// <summary>Spin iterations allowed for GHC.HR to self-clear before giving up.</summary>
    private const uint HbaResetSpinLimit = 10_000_000;
    /// <summary>PI mask marking all 32 ports implemented, used when deriving PI from CAP.NP.</summary>
    private const uint AllPortsImplementedMask = 0xFFFFFFFFu;

    /// <summary>CAP.NP - Number of Ports, 0-based 5-bit field mask (bits 4:0).</summary>
    private const uint CapNpMask = 0x1F;
    /// <summary>CAP.SXS - Supports External SATA (bit 5).</summary>
    private const int CapSxsShift = 5;
    /// <summary>CAP.EMS - Enclosure Management Supported (bit 6).</summary>
    private const int CapEmsShift = 6;
    /// <summary>CAP.CCCS - Command Completion Coalescing Supported (bit 7).</summary>
    private const int CapCccsShift = 7;
    /// <summary>CAP.NCS - Number of Command Slots field position (bits 12:8).</summary>
    private const int CapNcsShift = 8;
    /// <summary>CAP.NCS - Number of Command Slots, 0-based 5-bit field mask.</summary>
    private const uint CapNcsMask = 0x1F;
    /// <summary>CAP.PSC - Partial State Capable (bit 13).</summary>
    private const int CapPscShift = 13;
    /// <summary>CAP.SSC - Slumber State Capable (bit 14).</summary>
    private const int CapSscShift = 14;
    /// <summary>CAP.PMD - PIO Multiple DRQ Block (bit 15).</summary>
    private const int CapPmdShift = 15;
    /// <summary>CAP.FBSS - FIS-based Switching Supported (bit 16).</summary>
    private const int CapFbssShift = 16;
    /// <summary>CAP.SPM - Supports Port Multiplier (bit 17).</summary>
    private const int CapSpmShift = 17;
    /// <summary>CAP.SAM - Supports AHCI mode only (bit 18).</summary>
    private const int CapSamShift = 18;
    /// <summary>CAP.ISS - Interface Speed Support field position (bits 23:20).</summary>
    private const int CapIssShift = 20;
    /// <summary>CAP.ISS - Interface Speed Support 4-bit field mask.</summary>
    private const uint CapIssMask = 0x0F;
    /// <summary>CAP.SCLO - Supports Command List Override (bit 24).</summary>
    private const int CapScloShift = 24;
    /// <summary>CAP.SAL - Supports Activity LED (bit 25).</summary>
    private const int CapSalShift = 25;
    /// <summary>CAP.SALP - Supports Aggressive Link Power Management (bit 26).</summary>
    private const int CapSalpShift = 26;
    /// <summary>CAP.SSS - Supports Staggered Spin-up (bit 27).</summary>
    private const int CapSssShift = 27;
    /// <summary>CAP.SMPS - Supports Mechanical Presence Switch (bit 28).</summary>
    private const int CapSmpsShift = 28;
    /// <summary>CAP.SSNTF - Supports SNotification Register (bit 29).</summary>
    private const int CapSsntfShift = 29;
    /// <summary>CAP.SNCQ - Supports Native Command Queuing (bit 30).</summary>
    private const int CapSncqShift = 30;
    /// <summary>CAP.S64A - Supports 64-bit Addressing (bit 31).</summary>
    private const int CapS64aShift = 31;

    /// <summary>VS major version byte position (bits 31:24, AHCI 1.3.1 s3.1.1).</summary>
    private const int VersionMajorShift = 24;
    /// <summary>VS minor version byte position (bits 23:16).</summary>
    private const int VersionMinorShift = 16;
    /// <summary>VS patch version byte position (bits 15:8).</summary>
    private const int VersionPatchShift = 8;

    /// <summary>PxSIG value when no D2H signature FIS has been received.</summary>
    private const uint InvalidSignature = 0xFFFFFFFFu;
    /// <summary>Shift isolating the high word of PxSIG (LBA high/mid bytes) for device classification.</summary>
    private const int SignatureHighWordShift = 16;
    /// <summary>Poll attempts waiting for PxSIG to leave 0xFFFFFFFF after rebase.</summary>
    private const int SignatureRetryLimit = 200;

    /// <summary>Highest address reachable through the 32-bit CLB/FB/CTBA registers (4 GiB - 1). Internal so <see cref="Sata"/> shares it.</summary>
    internal const ulong Max32BitAddress = 0xFFFFFFFF;
    /// <summary>Mask keeping the low dword of a 64-bit DMA address. Internal so <see cref="Sata"/> shares it.</summary>
    internal const ulong Low32BitsMask = 0xFFFFFFFF;
    /// <summary>Shift extracting the high dword of a 64-bit DMA address. Internal so <see cref="Sata"/> shares it.</summary>
    internal const int High32Shift = 32;

    /// <summary>Per-iteration poll delay in AHCI ticks (~µs) for PHY/engine waits in KickPort and PxSIG polls.</summary>
    private const int PollDelayTicks = 1000;
    /// <summary>COMRESET hold time in AHCI ticks (~µs); spec requires DET=1 held ≥1 ms.</summary>
    private const int ComresetHoldTicks = 2000;
    /// <summary>Per-iteration poll delay in AHCI ticks (~µs) for command-engine state polls.</summary>
    private const int EnginePollDelayTicks = 5000;
    /// <summary>Short per-iteration poll delay in AHCI ticks (~µs) for CI-drain and CLO polls.</summary>
    private const int ShortPollDelayTicks = 50;
    /// <summary>Poll iteration budget for engine-stop and PHY-ready waits in KickPort.</summary>
    private const int KickPortPollLimit = 100;
    /// <summary>Poll iteration budget (exhaustion sentinel) for StartCMD/StopCMD register waits.</summary>
    private const int EnginePollLimit = 101;

    private readonly PciDevice _pci;
    private ulong _abarPhys;
    private ulong _abarVirt;
    private GenericRegisters? _generic;
    private readonly List<BlockDevice> _ports = new();
    private ulong _cmdRegionVirt;
    private ulong _cmdRegionPhys;

    // Capability snapshot (read once at init from CAP)
    private bool _supports64bitAddressing;
    private bool _supportsNativeCommandQueuing;
    private bool _supportsSNotificationRegister;
    private bool _supportsMechanicalPresenceSwitch;
    private bool _supportsStaggeredSpinup;
    private bool _supportsAggressiveLinkPowerManagement;
    private bool _supportsActivityLED;
    private bool _supportsCommandListOverride;
    private uint _interfaceSpeedSupport;
    private bool _supportsAhciModeOnly;
    private bool _supportsPortMultiplier;
    private bool _fisBasedSwitchingSupported;
    private bool _pioMultipleDRQBlock;
    private bool _slumberStateCapable;
    private bool _partialStateCapable;
    private uint _numOfCommandSlots;
    private bool _commandCompletionCoalescingSupported;
    private bool _enclosureManagementSupported;
    private bool _supportsExternalSATA;
    private uint _numOfPorts;

    public PciDevice Device => _pci;
    public IReadOnlyList<BlockDevice> Ports => _ports;
    public uint AhciVersion => _generic?.AhciVersion ?? 0;
    public uint NumberOfCommandSlots => _numOfCommandSlots;
    public bool SupportsCommandListOverride => _supportsCommandListOverride;
    public bool Supports64BitAddressing => _supports64bitAddressing;

    public AhciController(PciDevice device)
    {
        _pci = device;
    }

    /// <summary>Kernel-virtual base of port <paramref name="portNumber"/>'s command list.</summary>
    internal ulong PortCommandListVirt(uint portNumber) =>
        _cmdRegionVirt + PortStrideCLB * portNumber;

    /// <summary>Physical (DMA) base of port <paramref name="portNumber"/>'s command list.</summary>
    internal ulong PortCommandListPhys(uint portNumber) =>
        _cmdRegionPhys + PortStrideCLB * portNumber;

    /// <summary>Kernel-virtual base of port <paramref name="portNumber"/>'s FIS-receive area.</summary>
    internal ulong PortFisReceiveVirt(uint portNumber) =>
        _cmdRegionVirt + FBBaseOffset + PortStrideFB * portNumber;

    /// <summary>Physical (DMA) base of port <paramref name="portNumber"/>'s FIS-receive area.</summary>
    internal ulong PortFisReceivePhys(uint portNumber) =>
        _cmdRegionPhys + FBBaseOffset + PortStrideFB * portNumber;

    /// <summary>Kernel-virtual base of port/slot's command table.</summary>
    internal ulong PortCommandTableVirt(uint portNumber, uint slot) =>
        _cmdRegionVirt + CTBABaseOffset + PortStrideCTBA * portNumber + SlotStrideCTBA * slot;

    /// <summary>Physical (DMA) base of port/slot's command table.</summary>
    internal ulong PortCommandTablePhys(uint portNumber, uint slot) =>
        _cmdRegionPhys + CTBABaseOffset + PortStrideCTBA * portNumber + SlotStrideCTBA * slot;

    /// <summary>
    /// Bring this controller up: enable bus master + memory decoding, map
    /// ABAR via HHDM, allocate a command region via
    /// <see cref="PageAllocator"/>, enable AHCI mode, snapshot CAP, and
    /// probe every implemented port. After a successful call,
    /// <see cref="Ports"/> contains one <see cref="Sata"/> per attached
    /// SATA drive. Returns <c>false</c> if any precondition fails — the
    /// caller (<see cref="Ahci.Initialize"/>) then drops this controller
    /// and moves on. Init-time failures are reported via return value
    /// rather than thrown so a misconfigured device can't take the boot
    /// down.
    /// </summary>
    public bool Initialize()
    {
        _pci.EnableBusMaster(true);
        _pci.EnableMemory(true);

        _abarPhys = _pci.GetBar64Address(AbarBarIndex);
        if (_abarPhys == 0)
        {
            Serial.WriteString("[AHCI] BAR5 is not a memory BAR\n");
            return false;
        }

        // ARM64 needs the ABAR page installed as Device memory in TTBR1
        // before the HHDM virtual is dereferenceable for MMIO. No-op on x64.
        PlatformHAL.Initializer?.EnsureMmioMapped(_abarPhys);

        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        _abarVirt = _abarPhys + hhdmOffset;

        Serial.WriteString("[AHCI] ABAR phys=0x");
        Serial.WriteHex(_abarPhys);
        Serial.WriteString(" virt=0x");
        Serial.WriteHex(_abarVirt);
        Serial.WriteString("\n");

        _cmdRegionVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, CommandRegionPages, true);
        if (_cmdRegionVirt == 0)
        {
            Serial.WriteString("[AHCI] Failed to allocate command region\n");
            return false;
        }
        _cmdRegionPhys = PageAllocator.VirtualToPhysical(_cmdRegionVirt);

        Serial.WriteString("[AHCI] Cmd region phys=0x");
        Serial.WriteHex(_cmdRegionPhys);
        Serial.WriteString(" virt=0x");
        Serial.WriteHex(_cmdRegionVirt);
        Serial.WriteString("\n");

        _generic = new GenericRegisters(_abarVirt);

        // AHCI 1.3.1 s10.1.2: with CAP.SAM=0, software must set GHC.AE=1
        // before accessing any other AHCI register. SeaBIOS/EDK2 normally
        // leave AE set, but firmware that bound the device in AHCI ProgIf
        // with AE=0 would make all port MMIO below undefined. With SAM=1
        // the bit is read-only 1, so the OR is harmless either way —
        // matches Linux's ahci_enable_ahci.
        if ((_generic.GlobalHostControl & GhcAhciEnable) == 0)
        {
            _generic.GlobalHostControl |= GhcAhciEnable;
        }

        Serial.WriteString("[AHCI] CAP=0x");
        Serial.WriteHex(_generic.Capabilities);
        Serial.WriteString(" PI=0x");
        Serial.WriteHex(_generic.ImplementedPorts);
        Serial.WriteString(" GHC=0x");
        Serial.WriteHex(_generic.GlobalHostControl);
        Serial.WriteString(" VS=0x");
        Serial.WriteHex(_generic.AhciVersion);
        Serial.WriteString("\n");

        // Only reset the HBA when firmware didn't enumerate ports — EDK2 on
        // aarch64 boots via virtio and leaves PI at 0; SeaBIOS on x86 already
        // populated PI via its own AHCI init. Driving an HR there zeroes the
        // working PI on ich9-ahci (PI is nominally HwInit but QEMU doesn't
        // restore the firmware-populated bits across HR). When we do reset,
        // poll HR for self-clear then re-set AE — matches the sequence in
        // Linux's ahci_reset_controller.
        if (_generic.ImplementedPorts == 0)
        {
            Serial.WriteString("[AHCI] PI=0, doing HBA reset\n");
            _generic.GlobalHostControl = GhcAhciEnable | GhcHbaReset;
            uint resetSpin = 0;
            while ((_generic.GlobalHostControl & GhcHbaReset) != 0)
            {
                if (++resetSpin > HbaResetSpinLimit)
                {
                    Serial.WriteString("[AHCI] HBA reset did not complete\n");
                    ReleaseCommandRegion();
                    return false;
                }
            }
            _generic.GlobalHostControl |= GhcAhciEnable; // Re-enable AHCI mode
            Serial.WriteString("[AHCI] After reset: CAP=0x");
            Serial.WriteHex(_generic.Capabilities);
            Serial.WriteString(" PI=0x");
            Serial.WriteHex(_generic.ImplementedPorts);
            Serial.WriteString("\n");
        }

        // QEMU's ich9-ahci on aarch64 virt leaves PI at 0 even after HR
        // (the bits would normally be HwInit-restored). When PI is still
        // empty, write the mask derived from CAP.NP so we can enumerate
        // the controller's ports — CheckPortType filters out empty slots.
        if (_generic.ImplementedPorts == 0)
        {
            uint cap = _generic.Capabilities;
            uint nports = (cap & CapNpMask) + 1;
            uint piMask = nports >= MaxPorts ? AllPortsImplementedMask : ((1u << (int)nports) - 1u);
            Serial.WriteString("[AHCI] PI still 0; deriving from CAP.NP=");
            Serial.WriteNumber(nports);
            Serial.WriteString(" → PI=0x");
            Serial.WriteHex(piMask);
            Serial.WriteString("\n");
            _generic.ImplementedPorts = piMask;
            Serial.WriteString("[AHCI] PI readback=0x");
            Serial.WriteHex(_generic.ImplementedPorts);
            Serial.WriteString("\n");
        }

        GetCapabilities();

        // Check the region's END, not just its base: a region based just
        // under 4 GiB spans the boundary and high ports' CLB/FB/CTBA would
        // silently truncate in the 32-bit registers.
        if (!_supports64bitAddressing && _cmdRegionPhys + CommandRegionBytes - 1 > Max32BitAddress)
        {
            Serial.WriteString("[AHCI] Controller is 32-bit only but command region crosses 4 GiB\n");
            ReleaseCommandRegion();
            return false;
        }

        _ports.Capacity = (int)_numOfPorts;
        GetPorts();

        Serial.WriteString("[AHCI] Version: ");
        PrintVersion();
        Serial.WriteString("\n[AHCI] Ports discovered: ");
        Serial.WriteNumber((uint)_ports.Count);
        Serial.WriteString("\n");
        return true;
    }

    private void PrintVersion()
    {
        if (_generic == null)
        {
            Serial.WriteString("Unknown");
            return;
        }
        Serial.WriteNumber((byte)(_generic.AhciVersion >> VersionMajorShift));
        Serial.WriteString(".");
        Serial.WriteNumber((byte)(_generic.AhciVersion >> VersionMinorShift));
        Serial.WriteString(".");
        Serial.WriteNumber((byte)(_generic.AhciVersion >> VersionPatchShift));
    }

    private void GetCapabilities()
    {
        if (_generic == null)
        {
            return;
        }

        _numOfPorts = (_generic.Capabilities & CapNpMask) + 1;
        _supportsExternalSATA = ((_generic.Capabilities >> CapSxsShift) & 1) == 1;
        _enclosureManagementSupported = ((_generic.Capabilities >> CapEmsShift) & 1) == 1;
        _commandCompletionCoalescingSupported = ((_generic.Capabilities >> CapCccsShift) & 1) == 1;
        _numOfCommandSlots = ((_generic.Capabilities >> CapNcsShift) & CapNcsMask) + 1;
        _partialStateCapable = ((_generic.Capabilities >> CapPscShift) & 1) == 1;
        _slumberStateCapable = ((_generic.Capabilities >> CapSscShift) & 1) == 1;
        _pioMultipleDRQBlock = ((_generic.Capabilities >> CapPmdShift) & 1) == 1;
        _fisBasedSwitchingSupported = ((_generic.Capabilities >> CapFbssShift) & 1) == 1;
        _supportsPortMultiplier = ((_generic.Capabilities >> CapSpmShift) & 1) == 1;
        _supportsAhciModeOnly = ((_generic.Capabilities >> CapSamShift) & 1) == 1;
        _interfaceSpeedSupport = (_generic.Capabilities >> CapIssShift) & CapIssMask;
        _supportsCommandListOverride = ((_generic.Capabilities >> CapScloShift) & 1) == 1;
        _supportsActivityLED = ((_generic.Capabilities >> CapSalShift) & 1) == 1;
        _supportsAggressiveLinkPowerManagement = ((_generic.Capabilities >> CapSalpShift) & 1) == 1;
        _supportsStaggeredSpinup = ((_generic.Capabilities >> CapSssShift) & 1) == 1;
        _supportsMechanicalPresenceSwitch = ((_generic.Capabilities >> CapSmpsShift) & 1) == 1;
        _supportsSNotificationRegister = ((_generic.Capabilities >> CapSsntfShift) & 1) == 1;
        _supportsNativeCommandQueuing = ((_generic.Capabilities >> CapSncqShift) & 1) == 1;
        _supports64bitAddressing = ((_generic.Capabilities >> CapS64aShift) & 1) == 1;
    }

    private void GetPorts()
    {
        if (_generic == null)
        {
            return;
        }

        uint implementedPort = _generic.ImplementedPorts;

        for (uint port = 0; port < MaxPorts; port++)
        {
            if ((implementedPort & 1) != 0)
            {
                PortRegisters portReg = new(_abarVirt + PortRegistersBaseOffset, port, this);

                // Only run a COMRESET when the PHY isn't already up — on x64
                // SeaBIOS trained the link and captured the device's D2H FIS
                // (so PxSIG holds 0x00000101 for SATA). Kicking that port
                // would clear PxSIG to 0xFFFFFFFF and we'd lose the type
                // classification. EDK2 on aarch64 leaves DET=0, so we kick
                // there to train the PHY ourselves.
                if ((DeviceDetectionStatus)(portReg.SSTS & PortRegisters.SstsDetMask) != DeviceDetectionStatus.DeviceDetectedWithPhy)
                {
                    KickPort(portReg);
                }

                uint ssts = portReg.SSTS;
                Serial.WriteString("[AHCI] Port ");
                Serial.WriteNumber(port);
                Serial.WriteString(" SSTS=0x");
                Serial.WriteHex(ssts);
                Serial.WriteString(" SIG=0x");
                Serial.WriteHex(portReg.SIG);
                Serial.WriteString("\n");

                InterfacePowerManagementStatus ipm = (InterfacePowerManagementStatus)((ssts >> PortRegisters.SstsIpmShift) & PortRegisters.SstsIpmMask);
                DeviceDetectionStatus det = (DeviceDetectionStatus)(ssts & PortRegisters.SstsDetMask);
                if (ipm != InterfacePowerManagementStatus.Active ||
                    det != DeviceDetectionStatus.DeviceDetectedWithPhy)
                {
                    implementedPort >>= 1;
                    continue;
                }

                // PortRebase sets FB and enables FIS Receive so the device's
                // post-reset D2H signature FIS gets captured. We use PxSIG
                // afterwards to distinguish SATA from SATAPI/SEMB. The reset
                // path (KickPort + PHY retrain) clears PxSIG to 0xFFFFFFFF
                // first, so re-reading it before FRE is meaningless.
                if (!PortRebase(portReg, port))
                {
                    // Un-rebased port: CLB/FB still hold firmware values, so
                    // doorbells would run the firmware's stale command list.
                    implementedPort >>= 1;
                    continue;
                }

                uint sigRaw = portReg.SIG;
                for (int retry = 0; retry < SignatureRetryLimit && sigRaw == InvalidSignature; retry++)
                {
                    Ahci.Wait(PollDelayTicks);
                    sigRaw = portReg.SIG;
                }

                PortType portType;
                if (sigRaw == InvalidSignature)
                {
                    // No D2H FIS arrived even though PHY is up. Assume SATA —
                    // the only type we currently support — so the test suite
                    // still exercises this port. SATAPI / SEMB drives would
                    // misbehave here; not worth guarding until they're real.
                    Serial.WriteString("[AHCI] Port ");
                    Serial.WriteNumber(port);
                    Serial.WriteString(" PxSIG never populated; assuming SATA\n");
                    portType = PortType.Sata;
                }
                else
                {
                    portType = ClassifySignature(sigRaw, port);
                }
                portReg.PortType = portType;

                if (portType == PortType.Sata)
                {
                    // Per-port containment: a port that misidentifies (the
                    // assume-SATA fallback can hit ATAPI) or whose Identify
                    // times out must be skipped, not unwind the whole boot —
                    // Initialize()'s contract is report-don't-throw.
                    try
                    {
                        Sata sataPort = new(portReg);
                        _ports.Add(sataPort);
                        Serial.WriteString("[AHCI] Initialized SATA port ");
                        Serial.WriteNumber(port);
                        Serial.WriteString("\n");
                    }
                    catch (Exception ex)
                    {
                        Serial.WriteString("[AHCI] Port ");
                        Serial.WriteNumber(port);
                        Serial.WriteString(" bring-up failed: ");
                        Serial.WriteString(ex.Message);
                        Serial.WriteString("\n");
                    }
                }
                else if (portType == PortType.Satapi)
                {
                    Serial.WriteString("[AHCI] Found SATAPI port ");
                    Serial.WriteNumber(port);
                    Serial.WriteString(" (not supported yet)\n");
                }
                else if (portType == PortType.Semb)
                {
                    Serial.WriteString("[AHCI] Found SEMB port ");
                    Serial.WriteNumber(port);
                    Serial.WriteString(" (not supported yet)\n");
                }
                else if (portType == PortType.PM)
                {
                    Serial.WriteString("[AHCI] Found Port Multiplier at port ");
                    Serial.WriteNumber(port);
                    Serial.WriteString(" (not supported yet)\n");
                }
            }
            implementedPort >>= 1;
        }
    }

    private static PortType ClassifySignature(uint sig, uint port)
    {
        uint sigHi = sig >> SignatureHighWordShift;
        switch ((AhciSignature)sigHi)
        {
            case AhciSignature.Sata: return PortType.Sata;
            case AhciSignature.Satapi: return PortType.Satapi;
            case AhciSignature.Semb: return PortType.Semb;
            case AhciSignature.PortMultiplier: return PortType.PM;
            case AhciSignature.Nothing: return PortType.Nothing;
            default:
                Serial.WriteString("[AHCI] Unknown drive signature 0x");
                Serial.WriteHex(sig);
                Serial.WriteString(" at port ");
                Serial.WriteNumber(port);
                Serial.WriteString(" — skipping\n");
                return PortType.Nothing;
        }
    }

    /// <summary>
    /// Triggers a SATA COMRESET on the port so its PHY runs OOB even when
    /// firmware (EDK2 on aarch64 virt) didn't bring it up. Stops the port's
    /// command engine first, writes <c>PxSCTL.DET=1</c> for ≥1 ms to issue
    /// COMRESET, then clears DET so the PHY trains. Returns early instead of
    /// hanging if no device responds — empty slots stay empty.
    /// </summary>
    private static void KickPort(PortRegisters port)
    {
        port.CMD &= ~(uint)CommandAndStatus.StartProcess; // ST
        port.CMD &= ~(uint)CommandAndStatus.FISReceiveEnable; // FRE
        for (int i = 0; i < KickPortPollLimit; i++)
        {
            if ((port.CMD & (uint)(CommandAndStatus.FISReceiveRunning | CommandAndStatus.CMDListRunning)) == 0)
            {
                break;
            }
            Ahci.Wait(PollDelayTicks);
        }

        port.SCTL = (port.SCTL & ~PortRegisters.SctlDetMask) | PortRegisters.SctlDetComreset;
        Ahci.Wait(ComresetHoldTicks); // hold COMRESET ≥1 ms before clearing
        port.SCTL &= ~PortRegisters.SctlDetMask;

        for (int i = 0; i < KickPortPollLimit; i++)
        {
            if ((DeviceDetectionStatus)(port.SSTS & PortRegisters.SstsDetMask) == DeviceDetectionStatus.DeviceDetectedWithPhy)
            {
                break;
            }
            Ahci.Wait(PollDelayTicks);
        }

        port.SERR = PortRegisters.Rw1CClearAll;
    }

    private bool PortRebase(PortRegisters port, uint portNumber)
    {
        Serial.WriteString("[AHCI] Rebasing port...\n");
        if (!StopCMD(port) && !Sata.PortReset(port))
        {
            // The command engine never stopped: reprogramming CLB/FB with a
            // live engine violates AHCI 10.1.2 (the HBA could fetch garbage
            // command headers and DMA anywhere). Leave the port untouched
            // and report failure — the caller must skip the port, because
            // issuing commands against the firmware's stale command list
            // could "succeed" reading a zeroed bounce buffer and register
            // a bogus zero-capacity device.
            Serial.WriteString("[AHCI] Skipping rebase; port engine still running\n");
            return false;
        }

        ulong clbPhys = PortCommandListPhys(portNumber);
        ulong fbPhys = PortFisReceivePhys(portNumber);
        ulong clbVirt = PortCommandListVirt(portNumber);
        ulong fbVirt = PortFisReceiveVirt(portNumber);

        port.CLB = (uint)(clbPhys & Low32BitsMask);
        port.CLBU = (uint)(clbPhys >> High32Shift);
        port.FB = (uint)(fbPhys & Low32BitsMask);
        port.FBU = (uint)(fbPhys >> High32Shift);

        // PxSERR / PxIS are RW1C: writing all ones clears every latched
        // bit (AHCI 10.1.2 requires SERR fully cleared before ST=1);
        // writing 0 or a partial mask clears nothing.
        port.SERR = PortRegisters.Rw1CClearAll;
        port.IS = PortRegisters.Rw1CClearAll;
        port.IE = 0;

        MemoryOp.MemSet((byte*)clbVirt, 0, CommandListBytes);
        MemoryOp.MemSet((byte*)fbVirt, 0, FisReceiveBytes);

        GetCommandHeader(port, portNumber);

        if (!StartCMD(port))
        {
            // PortReset stops the engine; without a StartCMD retry the port
            // would be left with ST=0 and every future doorbell write would
            // hang the command wait.
            if (!Sata.PortReset(port) || !StartCMD(port))
            {
                // Same containment as the engine-still-running case: an
                // offline port must be skipped, not handed to Sata.
                Serial.WriteString("[AHCI] Port failed to start after reset; leaving it offline\n");
                return false;
            }
        }

        // Clear latched events (RW1C) but keep every interrupt source
        // masked: this driver is strictly polled (IssueCommandCore clears
        // PxIS itself) and no AHCI ISR exists. Enabling PxIE here was only
        // benign while GHC.IE stayed 0 — any future GHC.IE=1 would turn the
        // latched events into an interrupt storm with no handler.
        port.IS = PortRegisters.Rw1CClearAll;
        port.IE = 0;

        Serial.WriteString("[AHCI] Port rebased\n");
        return true;
    }

    // Frees the per-controller command region on init-failure paths so a
    // dropped controller doesn't leak its 74 pages.
    private unsafe void ReleaseCommandRegion()
    {
        if (_cmdRegionVirt != 0)
        {
            PageAllocator.Free((void*)_cmdRegionVirt);
            _cmdRegionVirt = 0;
            _cmdRegionPhys = 0;
        }
    }

    private void GetCommandHeader(PortRegisters port, uint portNumber)
    {
        ulong clbVirt = PortCommandListVirt(portNumber);
        for (uint i = 0; i < CommandSlotsPerPort; i++)
        {
            ulong ctbaPhys = PortCommandTablePhys(portNumber, i);
            ulong ctbaVirt = PortCommandTableVirt(portNumber, i);
            HbaCommandHeader cmdHeader = new(clbVirt, i)
            {
                PRDTL = PrdtEntriesPerCommandTable,
                CTBA = (uint)(ctbaPhys & Low32BitsMask),
                CTBAU = (uint)(ctbaPhys >> High32Shift)
            };
            MemoryOp.MemSet((byte*)ctbaVirt, 0, CommandTableBytes);
        }
    }

    private bool StartCMD(PortRegisters port)
    {
        int spin;
        for (spin = 0; spin < EnginePollLimit; spin++)
        {
            if ((port.CMD & (uint)CommandAndStatus.CMDListRunning) == 0)
            {
                break;
            }
            Ahci.Wait(EnginePollDelayTicks);
        }
        if (spin == EnginePollLimit)
        {
            return false;
        }

        port.CMD |= (uint)CommandAndStatus.FISReceiveEnable; // FIS Receive Enable
        port.CMD |= (uint)CommandAndStatus.StartProcess; // Start

        return true;
    }

    private bool StopCMD(PortRegisters port)
    {
        int spin;
        port.CMD &= ~(uint)CommandAndStatus.StartProcess; // Clear Start bit

        for (spin = 0; spin < EnginePollLimit; spin++)
        {
            if ((port.CMD & (uint)CommandAndStatus.CMDListRunning) == 0)
            {
                break;
            }
            Ahci.Wait(EnginePollDelayTicks);
        }
        if (spin == EnginePollLimit)
        {
            return false;
        }

        for (spin = 0; spin < EnginePollLimit; spin++)
        {
            if (port.CI == 0)
            {
                break;
            }
            Ahci.Wait(ShortPollDelayTicks);
        }
        if (spin == EnginePollLimit)
        {
            return false;
        }

        port.CMD &= ~(uint)CommandAndStatus.FISReceiveEnable; // Clear FIS Receive Enable

        if (_supportsCommandListOverride)
        {
            if ((port.TFD & (uint)AtaDeviceStatus.Busy) != 0)
            {
                port.CMD |= (uint)CommandAndStatus.CMDListOverride;
                // AHCI 1.3.1 s3.3.7: software must wait for CLO to read
                // back 0 before setting ST again; proceeding early would
                // reprogram the port mid-override.
                for (spin = 0; spin < EnginePollLimit; spin++)
                {
                    if ((port.CMD & (uint)CommandAndStatus.CMDListOverride) == 0)
                    {
                        break;
                    }
                    Ahci.Wait(ShortPollDelayTicks);
                }
            }
        }

        for (spin = 0; spin < EnginePollLimit; spin++)
        {
            if ((port.CMD & (uint)CommandAndStatus.CMDListRunning) == 0 &&
                (port.CMD & (uint)CommandAndStatus.FISReceiveRunning) == 0 &&
                (port.CMD & (uint)CommandAndStatus.StartProcess) == 0 &&
                (port.CMD & (uint)CommandAndStatus.FISReceiveEnable) == 0)
            {
                break;
            }
            Ahci.Wait(EnginePollDelayTicks);
        }
        if (spin == EnginePollLimit)
        {
            // Last-ditch CLO on the way out for HBAs that support it; the
            // old else-arm "clear CLO" write was a no-op (CLO is cleared by
            // hardware, not by software writing 0).
            if (_supportsCommandListOverride)
            {
                port.CMD |= (uint)CommandAndStatus.CMDListOverride;
            }
            return false;
        }

        return true;
    }
}
