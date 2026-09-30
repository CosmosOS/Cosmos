// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Threading;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="AhciDriver"/> holds for one bound HBA, hung off
/// <see cref="DeviceBinding.DriverState"/>: the ABAR window, the command
/// region in DMA memory (the command lists, the received FIS areas and the
/// command tables of every port index), the capability snapshot, the kit
/// lock every port's busy claim runs under, the published ports and the
/// counters the Storage suite reads. The port engine helpers
/// (<see cref="StopCommandEngine"/>, <see cref="StartCommandEngine"/>,
/// <see cref="ResetPort"/>, <see cref="KickPort"/>) run in thread context
/// on the kit worker, from the probe and the detach hook; the register
/// accessors are used from any thread by <see cref="AhciPort"/>. The HBA
/// stays strictly polled: no interrupt is connected to it.
/// </summary>
public sealed class AhciState
{
    // --- Constants ---

    /// <summary>How long a command may take before the port gives up on it.</summary>
    internal const uint CommandTimeoutMilliseconds = 5000;

    /// <summary>Pause between two register reads while polling, and between two busy claims.</summary>
    internal const uint PollMicroseconds = 10;

    /// <summary>How long an engine gets to stop, to start, or to report the start bit clear.</summary>
    private const uint EngineMilliseconds = 500;

    /// <summary>How long the command issue register gets to drain, and the override to clear.</summary>
    private const uint DrainMilliseconds = 5;

    /// <summary>How long the PHY gets to report a device after COMRESET, and the reset request to clear.</summary>
    private const uint PhyMilliseconds = 100;

    /// <summary>How long COMRESET is held: the specification asks for at least one millisecond.</summary>
    private const uint ComresetHoldMicroseconds = 2000;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly DmaBuffer _commandRegion;
    private readonly uint _version;
    private readonly int _commandSlots;
    private readonly bool _supports64Bit;
    private readonly bool _supportsCommandListOverride;
    private readonly uint _implementedPorts;
    private AhciPort[] _ports = [];
    private DeviceLock? _lock;
    private int _index;
    private int _commandsIssued;
    private int _timeouts;

    // --- Constructor ---

    /// <summary>
    /// Takes the binding, the ABAR window, the command region and the
    /// registers the probe read; the lock and the ports come later through
    /// the internal setters as the probe brings the HBA up. Thread context,
    /// from the probe.
    /// </summary>
    /// <param name="binding">The function's binding, for the log and the delays.</param>
    /// <param name="registers">The ABAR window.</param>
    /// <param name="commandRegion">The DMA run holding every port's command list, FIS area and command tables.</param>
    /// <param name="capabilities">CAP as read.</param>
    /// <param name="version">VS as read.</param>
    /// <param name="implementedPorts">PI as walked, after the probe's HBA reset and derivation.</param>
    internal AhciState(DeviceBinding binding, RegisterWindow registers, DmaBuffer commandRegion, uint capabilities, uint version, uint implementedPorts)
    {
        _binding = binding;
        _registers = registers;
        _commandRegion = commandRegion;
        _version = version;
        _implementedPorts = implementedPorts;
        _commandSlots = (int)((capabilities >> AhciProtocol.CapSlotsShift) & AhciProtocol.CapSlotsMask) + 1;
        _supports64Bit = (capabilities & AhciProtocol.Cap64BitAddressing) != 0;
        _supportsCommandListOverride = (capabilities & AhciProtocol.CapCommandListOverride) != 0;
    }

    // --- Properties the suite reads ---

    /// <summary>The HBA's number among the controllers this driver bound; assigned when the probe binds. Any context.</summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>How many SATA ports the probe published. Any context.</summary>
    public int PortCount => _ports.Length;

    /// <summary>VS as read: the major version in bits 31:16, the minor in bits 15:8. Any context.</summary>
    public uint Version => _version;

    /// <summary>CAP.NCS plus one: the command slots each port has. Any context.</summary>
    public int CommandSlots => _commandSlots;

    /// <summary>CAP.S64A: the HBA addresses 64 bits. Any context.</summary>
    public bool Supports64Bit => _supports64Bit;

    /// <summary>CAP.SCLO: the HBA supports the command list override. Any context.</summary>
    public bool SupportsCommandListOverride => _supportsCommandListOverride;

    /// <summary>PI as the probe walked it. Any context.</summary>
    public uint ImplementedPorts => _implementedPorts;

    /// <summary>How many commands completed on every port together. Any context.</summary>
    public int CommandsIssued => _commandsIssued;

    /// <summary>How many commands went unanswered for <see cref="CommandTimeoutMilliseconds"/>. Any context.</summary>
    public int Timeouts => _timeouts;

    // --- Internal properties ---

    /// <summary>The ABAR window. Any context.</summary>
    internal RegisterWindow Registers => _registers;

    /// <summary>The command region. Any context.</summary>
    internal DmaBuffer CommandRegion => _commandRegion;

    /// <summary>The published ports, in port order; set by the probe before publication.</summary>
    internal AhciPort[] Ports
    {
        get => _ports;
        set => _ports = value;
    }

    /// <summary>The lock every port's busy claim and release runs under, held only around the flag; set by the probe before any port is created.</summary>
    internal DeviceLock? Lock
    {
        get => _lock;
        set => _lock = value;
    }

    // --- Internal methods ---

    /// <summary>The lock, once the probe created it. Any context.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the lock.</exception>
    internal DeviceLock RequireLock() =>
        _lock ?? throw new InvalidOperationException("The controller's lock is not created.");

    /// <summary>Reads a port register. Any thread.</summary>
    /// <param name="port">The port index.</param>
    /// <param name="register">The register's offset within the bank.</param>
    internal uint ReadPort(uint port, ulong register) => _registers.Read32(AhciProtocol.PortBank(port) + register);

    /// <summary>Writes a port register. Any thread.</summary>
    /// <param name="port">The port index.</param>
    /// <param name="register">The register's offset within the bank.</param>
    /// <param name="value">The value.</param>
    internal void WritePort(uint port, ulong register, uint value) => _registers.Write32(AhciProtocol.PortBank(port) + register, value);

    /// <summary>Counts one completed command. Any thread.</summary>
    internal void CountCommand() => Interlocked.Increment(ref _commandsIssued);

    /// <summary>Counts one command completion timeout. Any thread.</summary>
    internal void CountTimeout() => Interlocked.Increment(ref _timeouts);

    /// <summary>Pauses the caller for <paramref name="microseconds"/>. Thread context; any thread.</summary>
    /// <param name="microseconds">How long.</param>
    internal void Delay(uint microseconds) => _binding.Delay(microseconds);

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    /// <param name="milliseconds">How far ahead.</param>
    internal static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;

    /// <summary>
    /// Stops the port's command and FIS receive engines: clears ST and
    /// waits for CR to clear, waits for PxCI to drain, clears FRE, applies
    /// the command list override when the HBA supports it and the device
    /// reads busy, then waits for CR, FR, ST and FRE to all read clear.
    /// Thread context on the kit worker.
    /// </summary>
    /// <param name="port">The port index.</param>
    /// <returns>False when an engine stayed running past its budget; the port must then be left alone.</returns>
    internal bool StopCommandEngine(uint port)
    {
        ClearPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdStart);
        if (!WaitPort(port, AhciProtocol.PxCmd, AhciProtocol.CmdListRunning, 0, EngineMilliseconds))
        {
            return false;
        }

        if (!WaitPort(port, AhciProtocol.PxCi, AhciProtocol.Rw1CClearAll, 0, DrainMilliseconds))
        {
            return false;
        }

        ClearPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdFisReceiveEnable);
        if (_supportsCommandListOverride && (ReadPort(port, AhciProtocol.PxTfd) & AhciProtocol.TfdBusy) != 0)
        {
            SetPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdCommandListOverride);
            WaitPort(port, AhciProtocol.PxCmd, AhciProtocol.CmdCommandListOverride, 0, DrainMilliseconds);
        }

        uint engineBits = AhciProtocol.CmdListRunning | AhciProtocol.CmdFisReceiveRunning | AhciProtocol.CmdStart | AhciProtocol.CmdFisReceiveEnable;
        if (!WaitPort(port, AhciProtocol.PxCmd, engineBits, 0, EngineMilliseconds))
        {
            if (_supportsCommandListOverride)
            {
                SetPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdCommandListOverride);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Starts the port's engines: waits for CR to read clear, then sets FRE
    /// and ST. Thread context on the kit worker.
    /// </summary>
    /// <param name="port">The port index.</param>
    /// <returns>False when CR stayed set past its budget; nothing was written then.</returns>
    internal bool StartCommandEngine(uint port)
    {
        if (!WaitPort(port, AhciProtocol.PxCmd, AhciProtocol.CmdListRunning, 0, EngineMilliseconds))
        {
            return false;
        }

        SetPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdFisReceiveEnable);
        SetPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdStart);
        return true;
    }

    /// <summary>
    /// Resets the port's link: clears ST and waits for it to read clear,
    /// issues COMRESET through PxSCTL.DET, holds it, releases it, waits for
    /// the PHY to report a device (a miss is ignored), clears PxSERR and
    /// waits for the reset request to read clear. Thread context on the kit
    /// worker.
    /// </summary>
    /// <param name="port">The port index.</param>
    /// <returns>False when ST stayed set past its budget; the port was not reset then.</returns>
    internal bool ResetPort(uint port)
    {
        ClearPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdStart);
        if (!WaitPort(port, AhciProtocol.PxCmd, AhciProtocol.CmdStart, 0, EngineMilliseconds))
        {
            return false;
        }

        Comreset(port);
        WritePort(port, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);
        WaitPort(port, AhciProtocol.PxSctl, AhciProtocol.SctlDetMask, 0, PhyMilliseconds);
        return true;
    }

    /// <summary>
    /// Trains a port's PHY that firmware left down: clears ST and FRE,
    /// waits for FR and CR to clear, issues COMRESET, holds it, releases it,
    /// waits for the PHY to report a device and clears PxSERR. Every miss
    /// is ignored; the caller reads PxSSTS afterwards. Thread context on the
    /// kit worker.
    /// </summary>
    /// <param name="port">The port index.</param>
    internal void KickPort(uint port)
    {
        ClearPortBits(port, AhciProtocol.PxCmd, AhciProtocol.CmdStart | AhciProtocol.CmdFisReceiveEnable);
        WaitPort(port, AhciProtocol.PxCmd, AhciProtocol.CmdFisReceiveRunning | AhciProtocol.CmdListRunning, 0, PhyMilliseconds);
        Comreset(port);
        WritePort(port, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);
    }

    // --- Private methods ---

    /// <summary>Sets PxSCTL.DET to 1, holds it, clears it and waits for PxSSTS.DET to report a device with its PHY up, a miss ignored. Thread context on the kit worker.</summary>
    private void Comreset(uint port)
    {
        uint control = ReadPort(port, AhciProtocol.PxSctl);
        WritePort(port, AhciProtocol.PxSctl, (control & ~AhciProtocol.SctlDetMask) | AhciProtocol.SctlDetComreset);
        _binding.Delay(ComresetHoldMicroseconds);
        ClearPortBits(port, AhciProtocol.PxSctl, AhciProtocol.SctlDetMask);
        WaitPort(port, AhciProtocol.PxSsts, AhciProtocol.SstsDetMask, AhciProtocol.SstsDetPresent, PhyMilliseconds);
    }

    /// <summary>Sets <paramref name="bits"/> in a port register with a read, modify, write. Any thread.</summary>
    private void SetPortBits(uint port, ulong register, uint bits) =>
        WritePort(port, register, ReadPort(port, register) | bits);

    /// <summary>Clears <paramref name="bits"/> in a port register with a read, modify, write. Any thread.</summary>
    private void ClearPortBits(uint port, ulong register, uint bits) =>
        WritePort(port, register, ReadPort(port, register) & ~bits);

    /// <summary>
    /// Polls a port register, pausing <see cref="PollMicroseconds"/> between
    /// reads, until its masked value equals <paramref name="value"/> or
    /// <paramref name="milliseconds"/> passed. Thread context; any thread.
    /// </summary>
    /// <returns>True when the value was seen in time.</returns>
    private bool WaitPort(uint port, ulong register, uint mask, uint value, uint milliseconds)
    {
        long deadline = DeadlineAfter(milliseconds);
        while (true)
        {
            if ((ReadPort(port, register) & mask) == value)
            {
                return true;
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(PollMicroseconds);
        }
    }
}
