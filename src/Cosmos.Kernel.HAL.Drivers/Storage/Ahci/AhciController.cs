// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.Ata;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.CommandList;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.Registers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci;

/// <summary>
/// One AHCI HBA as <see cref="AhciDriver"/> brings it up: its ABAR, the
/// capabilities read from it, its SATA disks and its command region. The
/// command region is one DMA buffer holding every port's command list,
/// FIS-receive area and command tables at fixed offsets, for all 32
/// possible port indices whatever PI says, so a port's addresses never
/// depend on which other ports exist.
/// </summary>
internal sealed class AhciController
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
    /// <summary>
    /// Command-region size: 0x4A000 (296 KiB, 74 pages) covers CLB + FB + 32
    /// command-table slots for all 32 possible port indices. The kit hands
    /// out whole zeroed pages, so every structure in it keeps the alignment
    /// the offsets above assume.
    /// </summary>
    private const int CommandRegionBytes = 0x4A000;

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

    /// <summary>Config-space offset of BAR5, which holds ABAR's physical address, for the log.</summary>
    private const ushort Bar5ConfigOffset = 0x24;
    /// <summary>Address bits of a memory BAR; the low four hold its type flags.</summary>
    private const uint MemoryBarAddressMask = 0xFFFFFFF0;

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
    /// <summary>CAP.NCS - Number of Command Slots field position (bits 12:8).</summary>
    private const int CapNcsShift = 8;
    /// <summary>CAP.NCS - Number of Command Slots, 0-based 5-bit field mask.</summary>
    private const uint CapNcsMask = 0x1F;
    /// <summary>CAP.SCLO - Supports Command List Override (bit 24).</summary>
    private const int CapScloShift = 24;
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

    /// <summary>Per-iteration poll delay in µs for PHY/engine waits in KickPort and PxSIG polls.</summary>
    private const int PollDelayMicroseconds = 1000;
    /// <summary>COMRESET hold time in µs; spec requires DET=1 held ≥1 ms.</summary>
    private const int ComresetHoldMicroseconds = 2000;
    /// <summary>Per-iteration poll delay in µs for command-engine state polls.</summary>
    private const int EnginePollDelayMicroseconds = 5000;
    /// <summary>Short per-iteration poll delay in µs for CI-drain and CLO polls.</summary>
    private const int ShortPollDelayMicroseconds = 50;
    /// <summary>Poll iteration budget for engine-stop and PHY-ready waits in KickPort.</summary>
    private const int KickPortPollLimit = 100;
    /// <summary>Poll iteration budget (exhaustion sentinel) for StartCMD/StopCMD register waits.</summary>
    private const int EnginePollLimit = 101;

    private readonly MmioRegion _abar;
    private readonly GenericRegisters _generic;
    private readonly List<Sata> _ports = [];
    private DmaBuffer? _commandRegion;

    // What the rest of the driver reads from CAP, snapshot once at init.
    private bool _supports64BitAddressing;
    private bool _supportsCommandListOverride;
    private uint _numberOfCommandSlots;
    private uint _numberOfPorts;

    /// <summary>The binding attempt the controller belongs to, which logs its lines and busy-waits for it.</summary>
    internal PciDeviceContext Context { get; }

    /// <summary>The SATA disks <see cref="Initialize"/> brought up, in port order.</summary>
    internal IReadOnlyList<Sata> Ports => _ports;

    /// <summary>CAP.NCS + 1: the command slots each port implements.</summary>
    internal uint NumberOfCommandSlots => _numberOfCommandSlots;

    /// <summary>
    /// Highest address the HBA can DMA to: 4 GiB - 1 unless CAP.S64A says
    /// it generates 64-bit addresses.
    /// </summary>
    internal ulong MaximumDeviceAddress => _supports64BitAddressing ? ulong.MaxValue : Max32BitAddress;

    /// <summary>The command region, allocated by <see cref="Initialize"/> before any port is rebased.</summary>
    /// <exception cref="InvalidOperationException">Read before <see cref="Initialize"/> allocated it.</exception>
    internal DmaBuffer CommandRegion =>
        _commandRegion ?? throw new InvalidOperationException("The AHCI command region is allocated by Initialize.");

    /// <summary>
    /// Creates the controller <paramref name="context"/> offers, over its
    /// mapped ABAR. Touches no register: <see cref="Initialize"/> does.
    /// </summary>
    /// <param name="context">The binding attempt, which logs, busy-waits and allocates the DMA memory.</param>
    /// <param name="abar">BAR5, mapped.</param>
    internal AhciController(PciDeviceContext context, MmioRegion abar)
    {
        Context = context;
        _abar = abar;
        _generic = new GenericRegisters(abar);
    }

    /// <summary>Offset of port <paramref name="portNumber"/>'s command list in the command region.</summary>
    internal static int CommandListOffset(uint portNumber) => (int)(PortStrideCLB * portNumber);

    /// <summary>Offset of port <paramref name="portNumber"/>'s FIS-receive area in the command region.</summary>
    internal static int FisReceiveOffset(uint portNumber) => (int)(FBBaseOffset + PortStrideFB * portNumber);

    /// <summary>Offset of the command table of port <paramref name="portNumber"/>'s <paramref name="slot"/> in the command region.</summary>
    internal static int CommandTableOffset(uint portNumber, uint slot) =>
        (int)(CTBABaseOffset + PortStrideCTBA * portNumber + SlotStrideCTBA * slot);

    /// <summary>The address the HBA uses for byte <paramref name="offset"/> of the command region.</summary>
    internal ulong DeviceAddressOf(int offset) => CommandRegion.DeviceAddress + (ulong)offset;

    /// <summary>
    /// Busy-waits <paramref name="microseconds"/>. The waits are the ones the
    /// AHCI bring-up always had; they run on the probing thread, which
    /// cannot sleep, and during I/O, which spins the same way.
    /// </summary>
    internal void Wait(int microseconds) =>
        Context.Delay(TimeSpan.FromTicks(microseconds * TimeSpan.TicksPerMicrosecond));

    /// <summary>
    /// Brings the controller up: enables AHCI mode, resets the HBA when
    /// firmware left no port implemented, snapshots CAP, allocates the
    /// command region, and probes every implemented port. After a call that
    /// returned true, <see cref="Ports"/> holds one <see cref="Sata"/> per
    /// SATA disk found, possibly none. Probe only: it allocates DMA memory.
    /// </summary>
    /// <returns>
    /// False when the controller cannot be driven: the HBA reset never
    /// completed, or no command region could be allocated where the HBA can
    /// reach it. Reported rather than thrown, so a misconfigured controller
    /// fails its own attempt and nothing else.
    /// </returns>
    internal bool Initialize()
    {
        uint abarPhysical = Context.Function.ReadConfig32(Bar5ConfigOffset) & MemoryBarAddressMask;
        Context.WriteLog($"ABAR phys=0x{abarPhysical:X}, 0x{_abar.Length:X} bytes mapped");

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

        Context.WriteLog($"CAP=0x{_generic.Capabilities:X} PI=0x{_generic.ImplementedPorts:X} GHC=0x{_generic.GlobalHostControl:X} VS=0x{_generic.AhciVersion:X}");

        // Only reset the HBA when firmware didn't enumerate ports — EDK2 on
        // aarch64 boots via virtio and leaves PI at 0; SeaBIOS on x86 already
        // populated PI via its own AHCI init. Driving an HR there zeroes the
        // working PI on ich9-ahci (PI is nominally HwInit but QEMU doesn't
        // restore the firmware-populated bits across HR). When we do reset,
        // poll HR for self-clear then re-set AE — matches the sequence in
        // Linux's ahci_reset_controller.
        if (_generic.ImplementedPorts == 0)
        {
            Context.WriteLog("PI=0, doing HBA reset");
            _generic.GlobalHostControl = GhcAhciEnable | GhcHbaReset;
            uint resetSpin = 0;
            while ((_generic.GlobalHostControl & GhcHbaReset) != 0)
            {
                if (++resetSpin > HbaResetSpinLimit)
                {
                    Context.WriteLog("HBA reset did not complete");
                    return false;
                }
            }

            _generic.GlobalHostControl |= GhcAhciEnable;
            Context.WriteLog($"After reset: CAP=0x{_generic.Capabilities:X} PI=0x{_generic.ImplementedPorts:X}");
        }

        // QEMU's ich9-ahci on aarch64 virt leaves PI at 0 even after HR
        // (the bits would normally be HwInit-restored). When PI is still
        // empty, write the mask derived from CAP.NP so we can enumerate
        // the controller's ports — the SSTS checks filter out empty slots.
        if (_generic.ImplementedPorts == 0)
        {
            uint capabilities = _generic.Capabilities;
            uint portCount = (capabilities & CapNpMask) + 1;
            uint piMask = portCount >= MaxPorts ? AllPortsImplementedMask : ((1u << (int)portCount) - 1u);
            Context.WriteLog($"PI still 0; deriving from CAP.NP={portCount} -> PI=0x{piMask:X}");
            _generic.ImplementedPorts = piMask;
            Context.WriteLog($"PI readback=0x{_generic.ImplementedPorts:X}");
        }

        GetCapabilities();

        // Allocated once CAP.S64A is known, so a 32-bit HBA gets a region it
        // can reach or none at all. The kit checks the region's END, not just
        // its base: a region based just under 4 GiB spans the boundary, and
        // high ports' CLB/FB/CTBA would silently truncate in the 32-bit
        // registers. No register access depends on the region before
        // GetPorts, so allocating here rather than first changes nothing the
        // HBA sees.
        if (!Context.TryAllocateDma(CommandRegionBytes, MaximumDeviceAddress, out DmaBuffer? commandRegion))
        {
            Context.WriteLog(_supports64BitAddressing
                ? "Failed to allocate command region"
                : "Controller is 32-bit only and no command region below 4 GiB could be allocated");
            return false;
        }

        _commandRegion = commandRegion;
        Context.WriteLog($"Cmd region phys=0x{commandRegion.DeviceAddress:X}");

        _ports.Capacity = (int)_numberOfPorts;
        GetPorts();

        uint version = _generic.AhciVersion;
        Context.WriteLog($"Version: {(byte)(version >> VersionMajorShift)}.{(byte)(version >> VersionMinorShift)}.{(byte)(version >> VersionPatchShift)}");
        Context.WriteLog($"Ports discovered: {_ports.Count}");
        return true;
    }

    private void GetCapabilities()
    {
        uint capabilities = _generic.Capabilities;
        _numberOfPorts = (capabilities & CapNpMask) + 1;
        _numberOfCommandSlots = ((capabilities >> CapNcsShift) & CapNcsMask) + 1;
        _supportsCommandListOverride = ((capabilities >> CapScloShift) & 1) == 1;
        _supports64BitAddressing = ((capabilities >> CapS64aShift) & 1) == 1;
    }

    private void GetPorts()
    {
        uint implementedPort = PortsInsideAbar(_generic.ImplementedPorts);

        for (uint port = 0; port < MaxPorts; port++)
        {
            if ((implementedPort & 1) != 0)
            {
                PortRegisters portReg = new(_abar, port, this);

                // Only run a COMRESET when the PHY isn't already up — on x64
                // SeaBIOS trained the link and captured the device's D2H FIS
                // (so PxSIG holds 0x00000101 for SATA). Kicking that port
                // would clear PxSIG to 0xFFFFFFFF and we'd lose the type
                // classification. EDK2 on aarch64 leaves DET=0, so we kick
                // there to train the PHY ourselves.
                if ((portReg.SSTS & PortRegisters.SstsDetMask) != (uint)DeviceDetectionStatus.DeviceDetectedWithPhy)
                {
                    KickPort(portReg);
                }

                uint ssts = portReg.SSTS;
                Context.WriteLog($"Port {port} SSTS=0x{ssts:X} SIG=0x{portReg.SIG:X}");

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
                    Wait(PollDelayMicroseconds);
                    sigRaw = portReg.SIG;
                }

                PortType portType;
                if (sigRaw == InvalidSignature)
                {
                    // No D2H FIS arrived even though PHY is up. Assume SATA —
                    // the only type we currently support — so the test suite
                    // still exercises this port. SATAPI / SEMB drives would
                    // misbehave here; not worth guarding until they're real.
                    Context.WriteLog($"Port {port} PxSIG never populated; assuming SATA");
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
                    // times out must be skipped, not fail the whole
                    // controller: Initialize's contract is report-don't-throw.
                    try
                    {
                        Sata sataPort = new(portReg);
                        _ports.Add(sataPort);
                        Context.WriteLog($"Initialized SATA port {port}");
                    }
                    catch (Exception ex)
                    {
                        Context.WriteLog($"Port {port} bring-up failed: {ex.Message}");
                    }
                }
                else if (portType == PortType.Satapi)
                {
                    Context.WriteLog($"Found SATAPI port {port} (not supported yet)");
                }
                else if (portType == PortType.Semb)
                {
                    Context.WriteLog($"Found SEMB port {port} (not supported yet)");
                }
                else if (portType == PortType.PM)
                {
                    Context.WriteLog($"Found Port Multiplier at port {port} (not supported yet)");
                }
            }

            implementedPort >>= 1;
        }
    }

    /// <summary>
    /// Keeps only the ports of <paramref name="implementedPorts"/> whose
    /// register bank lies inside the mapped ABAR. Every region access is
    /// bounds-checked and throws past its end; a PI mask derived from CAP.NP
    /// can name more ports than a small ABAR holds, and those ports would
    /// otherwise fail the whole controller on their first register read.
    /// </summary>
    private uint PortsInsideAbar(uint implementedPorts)
    {
        ulong portsInside = _abar.Length <= PortRegisters.RegistersBaseOffset
            ? 0
            : (_abar.Length - PortRegisters.RegistersBaseOffset) / PortRegisters.BankBytes;
        if (portsInside >= MaxPorts)
        {
            return implementedPorts;
        }

        uint inside = (1u << (int)portsInside) - 1u;
        if ((implementedPorts & ~inside) != 0)
        {
            Context.WriteLog($"PI=0x{implementedPorts:X} names ports past the 0x{_abar.Length:X}-byte ABAR; only ports below {portsInside} are used");
        }

        return implementedPorts & inside;
    }

    private PortType ClassifySignature(uint sig, uint port)
    {
        uint sigHi = sig >> SignatureHighWordShift;
        switch ((AhciSignature)sigHi)
        {
            case AhciSignature.Sata:
                return PortType.Sata;
            case AhciSignature.Satapi:
                return PortType.Satapi;
            case AhciSignature.Semb:
                return PortType.Semb;
            case AhciSignature.PortMultiplier:
                return PortType.PM;
            case AhciSignature.Nothing:
                return PortType.Nothing;
            default:
                Context.WriteLog($"Unknown drive signature 0x{sig:X} at port {port} — skipping");
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
    private void KickPort(PortRegisters port)
    {
        port.CMD &= ~(uint)CommandAndStatus.StartProcess; // ST
        port.CMD &= ~(uint)CommandAndStatus.FISReceiveEnable; // FRE
        for (int i = 0; i < KickPortPollLimit; i++)
        {
            if ((port.CMD & (uint)(CommandAndStatus.FISReceiveRunning | CommandAndStatus.CMDListRunning)) == 0)
            {
                break;
            }

            Wait(PollDelayMicroseconds);
        }

        port.SCTL = (port.SCTL & ~PortRegisters.SctlDetMask) | PortRegisters.SctlDetComreset;
        Wait(ComresetHoldMicroseconds); // hold COMRESET ≥1 ms before clearing
        port.SCTL &= ~PortRegisters.SctlDetMask;

        for (int i = 0; i < KickPortPollLimit; i++)
        {
            if ((port.SSTS & PortRegisters.SstsDetMask) == (uint)DeviceDetectionStatus.DeviceDetectedWithPhy)
            {
                break;
            }

            Wait(PollDelayMicroseconds);
        }

        port.SERR = PortRegisters.Rw1CClearAll;
    }

    private bool PortRebase(PortRegisters port, uint portNumber)
    {
        Context.WriteLog($"Rebasing port {portNumber}...");
        if (!StopCMD(port) && !Sata.PortReset(port))
        {
            // The command engine never stopped: reprogramming CLB/FB with a
            // live engine violates AHCI 10.1.2 (the HBA could fetch garbage
            // command headers and DMA anywhere). Leave the port untouched
            // and report failure — the caller must skip the port, because
            // issuing commands against the firmware's stale command list
            // could "succeed" reading a zeroed bounce buffer and register
            // a bogus zero-capacity device.
            Context.WriteLog("Skipping rebase; port engine still running");
            return false;
        }

        int clbOffset = CommandListOffset(portNumber);
        int fbOffset = FisReceiveOffset(portNumber);
        ulong clbPhys = DeviceAddressOf(clbOffset);
        ulong fbPhys = DeviceAddressOf(fbOffset);

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

        Span<byte> region = CommandRegion.Span;
        region.Slice(clbOffset, CommandListBytes).Clear();
        region.Slice(fbOffset, FisReceiveBytes).Clear();

        GetCommandHeader(portNumber);

        if (!StartCMD(port))
        {
            // PortReset stops the engine; without a StartCMD retry the port
            // would be left with ST=0 and every future doorbell write would
            // hang the command wait.
            if (!Sata.PortReset(port) || !StartCMD(port))
            {
                // Same containment as the engine-still-running case: an
                // offline port must be skipped, not handed to Sata.
                Context.WriteLog("Port failed to start after reset; leaving it offline");
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

        Context.WriteLog($"Port {portNumber} rebased");
        return true;
    }

    private void GetCommandHeader(uint portNumber)
    {
        DmaBuffer region = CommandRegion;
        int clbOffset = CommandListOffset(portNumber);
        for (uint i = 0; i < CommandSlotsPerPort; i++)
        {
            int ctbaOffset = CommandTableOffset(portNumber, i);
            ulong ctbaPhys = DeviceAddressOf(ctbaOffset);
            HbaCommandHeader cmdHeader = new(region, clbOffset, i);
            cmdHeader.PRDTL = PrdtEntriesPerCommandTable;
            cmdHeader.CTBA = (uint)(ctbaPhys & Low32BitsMask);
            cmdHeader.CTBAU = (uint)(ctbaPhys >> High32Shift);
            region.Span.Slice(ctbaOffset, CommandTableBytes).Clear();
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

            Wait(EnginePollDelayMicroseconds);
        }

        if (spin == EnginePollLimit)
        {
            return false;
        }

        port.CMD |= (uint)CommandAndStatus.FISReceiveEnable;
        port.CMD |= (uint)CommandAndStatus.StartProcess;

        return true;
    }

    private bool StopCMD(PortRegisters port)
    {
        int spin;
        port.CMD &= ~(uint)CommandAndStatus.StartProcess;

        for (spin = 0; spin < EnginePollLimit; spin++)
        {
            if ((port.CMD & (uint)CommandAndStatus.CMDListRunning) == 0)
            {
                break;
            }

            Wait(EnginePollDelayMicroseconds);
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

            Wait(ShortPollDelayMicroseconds);
        }

        if (spin == EnginePollLimit)
        {
            return false;
        }

        port.CMD &= ~(uint)CommandAndStatus.FISReceiveEnable;

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

                    Wait(ShortPollDelayMicroseconds);
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

            Wait(EnginePollDelayMicroseconds);
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
