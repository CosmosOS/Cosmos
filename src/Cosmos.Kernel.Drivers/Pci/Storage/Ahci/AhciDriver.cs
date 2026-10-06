// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Ahci;

/// <summary>
/// The AHCI driver (AHCI 1.3.1) over the driver kit: maps ABAR, enables
/// AHCI mode, allocates one command region for every port index, walks the
/// implemented ports, trains the PHY firmware left down, rebases each port
/// onto the region, classifies its signature and publishes every SATA disk
/// to the ring as <c>sata{n}</c>. Everything it holds for one HBA lives on an
/// <see cref="AhciState"/> in <see cref="DeviceBinding.DriverState"/>. The
/// driver stays strictly polled: no interrupt is requested, GHC.IE is never
/// set and every PxIE stays 0. The legacy IDE-mode SATA function has
/// another programming interface and is never offered here.
/// <see cref="Probe"/> and <see cref="OnDetach"/> run in thread context on
/// the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Storage)]
public sealed class AhciDriver : Driver
{
    // --- Constants ---

    /// <summary>Mass storage controller, the class code of an AHCI function.</summary>
    private const byte MassStorageClass = 0x01;

    /// <summary>Serial ATA controller, the subclass.</summary>
    private const byte SerialAtaSubclass = 0x06;

    /// <summary>AHCI 1.0, the programming interface.</summary>
    private const byte AhciProgIf = 0x01;

    /// <summary>The base address register holding ABAR.</summary>
    private const int RegisterBar = 5;

    /// <summary>Bytes the register window has to span: the generic registers and port 0's bank.</summary>
    private const ulong RegisterWindowBytes = 0x180;

    /// <summary>How long the HBA gets to complete a reset.</summary>
    private const uint HbaResetMilliseconds = 1000;

    /// <summary>How long a rebased port gets to receive its first D2H FIS before SATA is assumed.</summary>
    private const uint SignatureMilliseconds = 200;

    /// <summary>The highest physical address a 32-bit HBA can reach.</summary>
    private const ulong Max32BitAddress = 0xFFFFFFFF;

    /// <summary>Bits in the low dword of a 64-bit address.</summary>
    private const int DwordBits = 32;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: MassStorageClass, subclass: SerialAtaSubclass, progIf: AhciProgIf),
    ];

    /// <summary>The next controller number; probes are serialized on the kit worker, so no lock.</summary>
    private int _nextControllerIndex;

    /// <summary>The next <c>sata{n}</c> number, global across controllers and never reused: a port whose bring-up fails consumes it.</summary>
    private int _nextPortIndex;

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(AhciDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the HBA up and publishes its SATA ports. Thread context on the
    /// kit worker; a declined or failed result makes the kit release
    /// everything acquired here and quiet the function again.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the ports published, a controller with no usable port included; declined when BAR5 is not a memory window or a 32-bit HBA cannot reach the region; failed when the HBA reset did not complete.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The wire structs, then BAR5 as the register window.
        if (Unsafe.SizeOf<HbaCommandHeader>() != AhciProtocol.CommandHeaderBytes
            || Unsafe.SizeOf<HbaPrdtEntry>() != AhciProtocol.PrdtEntryBytes
            || Unsafe.SizeOf<FisRegisterH2D>() != AhciProtocol.FisRegisterH2DBytes)
        {
            return ProbeResult.Failed("the wire structs have the wrong size");
        }

        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar5 = pci.Bars[RegisterBar];
        if (!bar5.IsAssigned || bar5.IsIo || bar5.Length < RegisterWindowBytes)
        {
            return ProbeResult.Declined("BAR5 is not a memory window");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. The command region: page aligned, which satisfies the 1 KiB,
        //    256-byte and 128-byte alignments of its three areas.
        DmaBuffer region = binding.AllocateDma(AhciProtocol.CommandRegionBytes, AhciProtocol.CommandRegionAlignment);

        // 4. AHCI mode, then the registers.
        uint ghc = registers.Read32(AhciProtocol.Ghc);
        if ((ghc & AhciProtocol.GhcAhciEnable) == 0)
        {
            registers.Write32(AhciProtocol.Ghc, ghc | AhciProtocol.GhcAhciEnable);
        }

        uint capabilities = registers.Read32(AhciProtocol.Cap);
        uint implementedPorts = registers.Read32(AhciProtocol.Pi);
        ghc = registers.Read32(AhciProtocol.Ghc);
        uint version = registers.Read32(AhciProtocol.Vs);

        // 5. Firmware that left PI empty: reset the HBA, and derive PI from
        //    CAP.NP when the reset did not restore it.
        if (implementedPorts == 0)
        {
            registers.Write32(AhciProtocol.Ghc, AhciProtocol.GhcAhciEnable | AhciProtocol.GhcHbaReset);
            if (!WaitHbaReset(binding, registers))
            {
                return ProbeResult.Failed("the HBA reset did not complete");
            }

            registers.Write32(AhciProtocol.Ghc, registers.Read32(AhciProtocol.Ghc) | AhciProtocol.GhcAhciEnable);
            capabilities = registers.Read32(AhciProtocol.Cap);
            implementedPorts = registers.Read32(AhciProtocol.Pi);
            if (implementedPorts == 0)
            {
                uint portsMinusOne = capabilities & AhciProtocol.CapPortsMask;
                implementedPorts = portsMinusOne == AhciProtocol.CapPortsAll ? 0xFFFFFFFFu : (1u << (int)(portsMinusOne + 1)) - 1;
                registers.Write32(AhciProtocol.Pi, implementedPorts);
            }
        }

        // 6. The capability snapshot, the 32-bit reach, the lock.
        AhciState state = new(binding, registers, region, capabilities, version, implementedPorts);
        if (!state.Supports64Bit && region.PhysicalAddress + AhciProtocol.CommandRegionBytes - 1 > Max32BitAddress)
        {
            return ProbeResult.Declined("the controller addresses 32 bits and the command region lies above 4 GiB");
        }

        state.Lock = binding.CreateLock();

        // 7. The ports, in ascending order of their PI bit.
        List<AhciPort> ports = [];
        for (uint port = 0; port < AhciProtocol.MaxPorts; port++)
        {
            if ((implementedPorts & (1u << (int)port)) == 0)
            {
                continue;
            }

            if (AhciProtocol.PortBankBase + AhciProtocol.PortBankBytes * (port + 1) > registers.Length)
            {
                binding.Log($"port {port}: outside the register window, skipped");
                continue;
            }

            AhciPort? published = BringUpPort(binding, state, port);
            if (published is not null)
            {
                ports.Add(published);
            }
        }

        // 8. The state, then the ring, one port at a time in port order.
        state.Index = _nextControllerIndex++;
        state.Ports = ports.ToArray();
        binding.DriverState = state;
        for (int i = 0; i < ports.Count; i++)
        {
            binding.PublishBlockDevice(ports[i]);
        }

        // 9.
        int implemented = BitOperations.PopCount(implementedPorts);
        uint major = version >> AhciProtocol.VersionMajorShift;
        uint minor = (version >> AhciProtocol.VersionMinorShift) & AhciProtocol.VersionByteMask;
        binding.Log($"{state.PortCount} sata ports of {implemented} implemented, version {major}.{minor}, {state.CommandSlots} slots, {(state.Supports64Bit ? "64" : "32")}-bit");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Stops the command engine of every published port (a miss ignored)
    /// and turns bus mastering off before the kit frees the command region
    /// and unmaps ABAR. Thread context on the kit worker; the ring was told
    /// through the consumer's withdrawal before this runs; nothing is
    /// written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not AhciState state || !reason.HardwarePresent)
        {
            return;
        }

        AhciPort[] ports = state.Ports;
        for (int i = 0; i < ports.Length; i++)
        {
            state.StopCommandEngine(ports[i].PortNumber);
        }

        binding.Node.Access<PciAccess>().EnableBusMastering(false);
    }

    // --- Private methods ---

    /// <summary>Polls GHC.HR until it clears within <see cref="HbaResetMilliseconds"/>. Thread context on the kit worker.</summary>
    /// <returns>False when the bit stayed set.</returns>
    private static bool WaitHbaReset(DeviceBinding binding, RegisterWindow registers)
    {
        long deadline = AhciState.DeadlineAfter(HbaResetMilliseconds);
        while ((registers.Read32(AhciProtocol.Ghc) & AhciProtocol.GhcHbaReset) != 0)
        {
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            binding.Delay(AhciState.PollMicroseconds);
        }

        return true;
    }

    /// <summary>
    /// Brings one implemented port up: trains the PHY when firmware left it
    /// down, skips a port without a device, rebases it onto the command
    /// region, starts its engine, reads its signature and creates the
    /// <see cref="AhciPort"/> for a SATA disk. A rebased port that is not
    /// published is stopped again before it is skipped, so no engine keeps
    /// a FIS area inside memory the kit frees. Thread context on the kit
    /// worker.
    /// </summary>
    /// <returns>The port to publish, or null when it was skipped; the reason was logged.</returns>
    private AhciPort? BringUpPort(DeviceBinding binding, AhciState state, uint port)
    {
        uint status = state.ReadPort(port, AhciProtocol.PxSsts);
        if ((status & AhciProtocol.SstsDetMask) != AhciProtocol.SstsDetPresent)
        {
            state.KickPort(port);
        }

        status = state.ReadPort(port, AhciProtocol.PxSsts);
        uint powerManagement = (status >> AhciProtocol.SstsIpmShift) & AhciProtocol.SstsIpmMask;
        uint detection = status & AhciProtocol.SstsDetMask;
        if (powerManagement != AhciProtocol.SstsIpmActive || detection != AhciProtocol.SstsDetPresent)
        {
            binding.Log($"port {port}: no device (SSTS 0x{status:X})");
            return null;
        }

        if (!state.StopCommandEngine(port) && !state.ResetPort(port))
        {
            binding.Log($"port {port}: engine still running, skipped");
            return null;
        }

        RebasePort(state, port);
        if (!state.StartCommandEngine(port))
        {
            state.ResetPort(port);
            if (!state.StartCommandEngine(port))
            {
                binding.Log($"port {port}: engine did not start, skipped");
                state.StopCommandEngine(port);
                return null;
            }
        }

        state.WritePort(port, AhciProtocol.PxIs, AhciProtocol.Rw1CClearAll);
        state.WritePort(port, AhciProtocol.PxIe, 0);

        uint signature = state.ReadPort(port, AhciProtocol.PxSig);
        long deadline = AhciState.DeadlineAfter(SignatureMilliseconds);
        while (signature == AhciProtocol.InvalidSignature && Stopwatch.GetTimestamp() < deadline)
        {
            binding.Delay(AhciState.PollMicroseconds);
            signature = state.ReadPort(port, AhciProtocol.PxSig);
        }

        uint kind = signature == AhciProtocol.InvalidSignature ? AhciProtocol.SignatureSata : signature >> AhciProtocol.SignatureHighShift;
        if (kind == AhciProtocol.SignatureSata)
        {
            int index = _nextPortIndex++;
            try
            {
                return new AhciPort(state, binding, port, index);
            }
            catch (Exception exception)
            {
                binding.Log($"port {port}: bring-up failed: {exception.Message}");
                state.StopCommandEngine(port);
                return null;
            }
        }

        if (kind == AhciProtocol.SignatureSatapi)
        {
            binding.Log($"port {port}: satapi not supported");
        }
        else if (kind == AhciProtocol.SignatureSemb)
        {
            binding.Log($"port {port}: semb not supported");
        }
        else if (kind == AhciProtocol.SignaturePortMultiplier)
        {
            binding.Log($"port {port}: port multiplier not supported");
        }
        else
        {
            binding.Log($"port {port}: unknown signature 0x{signature:X}, skipped");
        }

        state.StopCommandEngine(port);
        return null;
    }

    /// <summary>
    /// Points a stopped port at its command list and FIS area in the
    /// region, clears its latched errors and events, keeps its interrupts
    /// masked, zeroes the list and the FIS area, writes the 32 headers with
    /// their table addresses and zeroes each table. Thread context on the
    /// kit worker.
    /// </summary>
    private static void RebasePort(AhciState state, uint port)
    {
        DmaBuffer region = state.CommandRegion;
        ulong listPhysical = region.PhysicalAddress + (ulong)AhciProtocol.CommandListOffset(port);
        ulong fisPhysical = region.PhysicalAddress + (ulong)AhciProtocol.FisReceiveOffset(port);
        state.WritePort(port, AhciProtocol.PxClb, (uint)listPhysical);
        state.WritePort(port, AhciProtocol.PxClbu, (uint)(listPhysical >> DwordBits));
        state.WritePort(port, AhciProtocol.PxFb, (uint)fisPhysical);
        state.WritePort(port, AhciProtocol.PxFbu, (uint)(fisPhysical >> DwordBits));
        state.WritePort(port, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);
        state.WritePort(port, AhciProtocol.PxIs, AhciProtocol.Rw1CClearAll);
        state.WritePort(port, AhciProtocol.PxIe, 0);

        Span<byte> list = region.Span.Slice(AhciProtocol.CommandListOffset(port), AhciProtocol.CommandListBytes);
        list.Clear();
        region.Span.Slice(AhciProtocol.FisReceiveOffset(port), AhciProtocol.FisReceiveBytes).Clear();

        Span<HbaCommandHeader> headers = MemoryMarshal.Cast<byte, HbaCommandHeader>(list);
        for (int slot = 0; slot < AhciProtocol.CommandSlotsPerPort; slot++)
        {
            int tableOffset = AhciProtocol.CommandTableOffset(port, slot);
            ulong tablePhysical = region.PhysicalAddress + (ulong)tableOffset;
            headers[slot] = new HbaCommandHeader
            {
                PrdtLength = AhciProtocol.PrdtEntriesPerTable,
                CommandTableBase = (uint)tablePhysical,
                CommandTableBaseUpper = (uint)(tablePhysical >> DwordBits),
            };
            region.Span.Slice(tableOffset, AhciProtocol.CommandTableBytes).Clear();
        }
    }
}
