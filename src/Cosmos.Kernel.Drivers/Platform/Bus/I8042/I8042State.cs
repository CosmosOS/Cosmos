// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Ps2;

namespace Cosmos.Kernel.Drivers.Platform.Bus.I8042;

/// <summary>
/// Everything <see cref="I8042Driver"/> holds for one bound controller, hung
/// off <see cref="DeviceBinding.DriverState"/>, and the kit's
/// <see cref="Ps2Controller"/> for it: the two one-port register windows,
/// the kit lock, the two accesses, the two child nodes, the periodic drain
/// and the counters the Drivers suite reads. Every status read paired with
/// a data read or a command write runs under <see cref="Lock"/> (interrupts
/// disabled, any context); <see cref="DeviceBinding.Delay"/> runs outside
/// it, so every wait is a loop of "take the lock, read the status, release,
/// delay", and every wait is bounded in time with <see cref="Stopwatch"/>
/// deadlines, never in iterations. <see cref="Ps2Access.Deliver"/> is called
/// outside the lock. Sends to the two ports are serialized by their callers
/// on the kit worker, not by the controller: the 0xD4 prefix and its data
/// byte are two writes with a wait between them that the lock cannot make
/// atomic, so a later caller from another thread must add a controller-wide
/// send flag taken under the lock around the pair. The execution context of
/// each member is in its summary.
/// </summary>
public sealed class I8042State : Ps2Controller
{
    // --- Constants ---

    /// <summary>Offset of the one port in each register window.</summary>
    private const ulong PortOffset = 0;

    /// <summary>Status bit 0: a byte waits in the output buffer.</summary>
    private const byte OutputBufferFull = 0x01;

    /// <summary>Status bit 1: the input buffer still holds the last byte written; a second write is lost.</summary>
    private const byte InputBufferFull = 0x02;

    /// <summary>Status bit 5: the byte in the output buffer came from the auxiliary port.</summary>
    private const byte AuxiliaryOutputBufferFull = 0x20;

    /// <summary>Controller command: read the configuration byte.</summary>
    internal const byte ReadConfiguration = 0x20;

    /// <summary>Controller command: write the configuration byte, which follows on the data port.</summary>
    internal const byte WriteConfiguration = 0x60;

    /// <summary>Controller command: disable the auxiliary port.</summary>
    internal const byte DisableAuxiliaryPort = 0xA7;

    /// <summary>Controller command: enable the auxiliary port.</summary>
    internal const byte EnableAuxiliaryPort = 0xA8;

    /// <summary>Controller command: test the auxiliary port's interface.</summary>
    internal const byte TestAuxiliaryPort = 0xA9;

    /// <summary>Controller command: the controller's self test.</summary>
    internal const byte SelfTest = 0xAA;

    /// <summary>Controller command: test the keyboard port's interface.</summary>
    internal const byte TestKeyboardPort = 0xAB;

    /// <summary>Controller command: disable the keyboard port.</summary>
    internal const byte DisableKeyboardPort = 0xAD;

    /// <summary>Controller command: enable the keyboard port.</summary>
    internal const byte EnableKeyboardPort = 0xAE;

    /// <summary>Controller command: the next data byte goes to the auxiliary port.</summary>
    internal const byte WriteAuxiliaryPort = 0xD4;

    /// <summary>The self test's reply when it passed.</summary>
    internal const byte SelfTestPassed = 0x55;

    /// <summary>An interface test's reply when it passed.</summary>
    internal const byte PortTestPassed = 0x00;

    /// <summary>Configuration bit 0: a keyboard port byte raises IRQ 1.</summary>
    internal const byte KeyboardInterruptEnable = 0x01;

    /// <summary>Configuration bit 1: an auxiliary port byte raises IRQ 12.</summary>
    internal const byte AuxiliaryInterruptEnable = 0x02;

    /// <summary>Configuration bit 5: the auxiliary port's clock is off; set when the second port exists and is disabled.</summary>
    internal const byte AuxiliaryClockDisabled = 0x20;

    /// <summary>Configuration bit 6: the keyboard port delivers set 1 scan codes.</summary>
    internal const byte Translation = 0x40;

    /// <summary>The bound on the input buffer emptying and on a reply landing.</summary>
    private const uint BufferWaitMilliseconds = 10;

    /// <summary>The <see cref="DeviceBinding.Delay"/> between two status reads.</summary>
    private const uint PollMicroseconds = 10;

    /// <summary>Bytes one <see cref="Flush"/> reads at most.</summary>
    private const int FlushLimit = 32;

    /// <summary>Bytes one handler run or one <see cref="PollCore"/> reads at most.</summary>
    private const int MaxBytesPerDrain = 16;

    /// <summary>The period of the drain when the controller is not interrupt driven.</summary>
    internal const uint PollPeriodMilliseconds = 20;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _data;
    private readonly RegisterWindow _control;
    private readonly DeviceLock _lock;
    private volatile bool _interruptDriven;
    private volatile bool _polledPeriodically;
    private volatile int _bytesDelivered;
    private volatile int _spuriousInterrupts;
    private volatile int _strayBytes;

    // --- Constructor ---

    /// <summary>Takes the binding, the two windows and the lock the probe created. Thread context, from the probe.</summary>
    /// <param name="binding">The 8042 node's binding, for the delays.</param>
    /// <param name="data">The window over port 0x60.</param>
    /// <param name="control">The window over port 0x64: the status register on read, the command register on write.</param>
    /// <param name="deviceLock">The lock every status read paired with a data read or a command write runs under.</param>
    internal I8042State(DeviceBinding binding, RegisterWindow data, RegisterWindow control, DeviceLock deviceLock)
    {
        _binding = binding;
        _data = data;
        _control = control;
        _lock = deviceLock;
    }

    // --- Properties the suites read ---

    /// <summary>True when the auxiliary port exists: its clock bit cleared once the port was enabled. Any context.</summary>
    public bool IsDualChannel { get; internal set; }

    /// <inheritdoc/>
    public override bool InterruptDriven => _interruptDriven;

    /// <inheritdoc/>
    public override bool PolledPeriodically => _polledPeriodically;

    /// <summary>How many bytes were handed to an access. Any context.</summary>
    public int BytesDelivered => _bytesDelivered;

    /// <summary>How many handler runs found the output buffer empty. Any context.</summary>
    public int SpuriousInterrupts => _spuriousInterrupts;

    /// <summary>How many bytes arrived for a port with no access. Any context.</summary>
    public int StrayBytes => _strayBytes;

    /// <summary>The <c>ps2:kbd</c> node, or null when the keyboard port failed its interface test. Any context.</summary>
    public DeviceNode? KeyboardNode { get; internal set; }

    /// <summary>The <c>ps2:aux</c> node, or null when the auxiliary port is absent or failed its interface test. Any context.</summary>
    public DeviceNode? AuxiliaryNode { get; internal set; }

    // --- Internal properties the probe reads and sets ---

    /// <summary>The lock every status read paired with a data read or a command write runs under; from the constructor, never null. Any context.</summary>
    internal DeviceLock Lock => _lock;

    /// <summary>The keyboard port's access, or null when the port failed its interface test. Any context.</summary>
    internal Ps2Access? KeyboardAccess { get; set; }

    /// <summary>The auxiliary port's access, or null when the port is absent or failed its interface test. Any context.</summary>
    internal Ps2Access? AuxiliaryAccess { get; set; }

    /// <summary>The periodic drain, or null on an interrupt driven controller. Thread context.</summary>
    internal WorkItem? DrainWork { get; set; }

    /// <summary>Records whether both lines the controller needs are connected. Thread context, from the probe.</summary>
    /// <param name="value">True once both needed lines connected.</param>
    internal void SetInterruptDriven(bool value) => _interruptDriven = value;

    /// <summary>Records whether the periodic drain is scheduled. Thread context, from the probe.</summary>
    /// <param name="value">True once the drain is scheduled.</param>
    internal void SetPolledPeriodically(bool value) => _polledPeriodically = value;

    // --- Internal helpers ---

    /// <summary>
    /// Waits for the input buffer to empty: under the lock a status read,
    /// outside it a delay, until the bit clears or the deadline passes.
    /// Thread context.
    /// </summary>
    /// <returns>False when the buffer did not empty within <see cref="BufferWaitMilliseconds"/>.</returns>
    internal bool WaitInputBufferEmpty()
    {
        long deadline = DeadlineAfter(BufferWaitMilliseconds);
        while (true)
        {
            using (_lock.Acquire())
            {
                if ((_control.Read8(PortOffset) & InputBufferFull) == 0)
                {
                    return true;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(PollMicroseconds);
        }
    }

    /// <summary>Writes one controller command to port 0x64 once the input buffer emptied. Thread context.</summary>
    /// <param name="command">The command byte.</param>
    /// <returns>False when the input buffer did not empty in time; nothing is written then.</returns>
    internal bool WriteCommand(byte command)
    {
        if (!WaitInputBufferEmpty())
        {
            return false;
        }

        using (_lock.Acquire())
        {
            _control.Write8(PortOffset, command);
        }

        return true;
    }

    /// <summary>Writes one controller command and its argument, each once the input buffer emptied. Thread context.</summary>
    /// <param name="command">The command byte, to port 0x64.</param>
    /// <param name="argument">The byte that follows it, to port 0x60.</param>
    /// <returns>False when the input buffer did not empty in time before either write.</returns>
    internal bool WriteCommand(byte command, byte argument)
    {
        if (!WriteCommand(command))
        {
            return false;
        }

        if (!WaitInputBufferEmpty())
        {
            return false;
        }

        using (_lock.Acquire())
        {
            _data.Write8(PortOffset, argument);
        }

        return true;
    }

    /// <summary>
    /// Waits for a reply to a controller command: the same deadline loop on
    /// the output buffer, reading the data port under the same lock scope as
    /// the status. A byte the status attributes to the auxiliary port is not
    /// a controller reply: outside the lock it goes to the auxiliary access
    /// through <see cref="Ps2Access.Deliver"/>, or counts in
    /// <see cref="StrayBytes"/> when that access is null, and the wait goes
    /// on. Valid only while both interrupt enables are clear or the line
    /// handles are disconnected: a controller reply lands on the keyboard
    /// channel and raises IRQ 1 whenever configuration bit 0 is set, and the
    /// handler's drain would then take it during the delay and deliver it to
    /// the keyboard access as a stream byte. Thread context.
    /// </summary>
    /// <param name="value">The reply, or 0 at the deadline.</param>
    /// <returns>False when no reply landed within <see cref="BufferWaitMilliseconds"/>.</returns>
    internal bool TryReadReply(out byte value)
    {
        long deadline = DeadlineAfter(BufferWaitMilliseconds);
        while (true)
        {
            bool full;
            byte data = 0;
            using (_lock.Acquire())
            {
                byte status = _control.Read8(PortOffset);
                full = (status & OutputBufferFull) != 0;
                if (full)
                {
                    data = _data.Read8(PortOffset);
                    if ((status & AuxiliaryOutputBufferFull) == 0)
                    {
                        value = data;
                        return true;
                    }
                }
            }

            if (full)
            {
                Dispatch(data, auxiliary: true);
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                value = 0;
                return false;
            }

            if (!full)
            {
                _binding.Delay(PollMicroseconds);
            }
        }
    }

    /// <summary>
    /// Reads and discards whatever the output buffer holds, up to
    /// <see cref="FlushLimit"/> bytes, each status and data read under the
    /// lock. Used before the accesses exist and in
    /// <see cref="QuiesceOnDetach"/>, where the children are already torn
    /// down; the discarded bytes are not counted in any counter. Thread
    /// context.
    /// </summary>
    /// <returns>How many bytes were discarded.</returns>
    internal int Flush()
    {
        int count = 0;
        for (int i = 0; i < FlushLimit; i++)
        {
            using (_lock.Acquire())
            {
                if ((_control.Read8(PortOffset) & OutputBufferFull) == 0)
                {
                    return count;
                }

                _data.Read8(PortOffset);
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// Reads up to <see cref="MaxBytesPerDrain"/> bytes from the output
    /// buffer: under the lock the status and, while the buffer is full, the
    /// data byte and whether status bit 5 attributed it to the auxiliary
    /// port; outside the lock each byte goes to that port's access through
    /// <see cref="Ps2Access.Deliver"/> (<see cref="BytesDelivered"/>), or
    /// counts in <see cref="StrayBytes"/> when the access is null. Thread
    /// context from <see cref="PollCore"/> and the periodic work item;
    /// interrupt context from the handler (allocates nothing, blocks
    /// nowhere).
    /// </summary>
    /// <returns>How many bytes were read.</returns>
    internal int Drain()
    {
        int count = 0;
        for (int i = 0; i < MaxBytesPerDrain; i++)
        {
            byte value;
            bool auxiliary;
            using (_lock.Acquire())
            {
                byte status = _control.Read8(PortOffset);
                if ((status & OutputBufferFull) == 0)
                {
                    return count;
                }

                value = _data.Read8(PortOffset);
                auxiliary = (status & AuxiliaryOutputBufferFull) != 0;
            }

            count++;
            Dispatch(value, auxiliary);
        }

        return count;
    }

    /// <summary>
    /// The one handler for both lines: drains the output buffer, the status
    /// register's bit 5 picking the port and not the vector that fired, and
    /// counts a run that found the buffer empty as spurious. Interrupt
    /// context; allocation-free.
    /// </summary>
    /// <param name="context">The kit's handler context, unused.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the InterruptHandler shape; the status register picks the port, not the context.")]
    internal void OnInterrupt(InterruptContext context)
    {
        if (Drain() == 0)
        {
            _spuriousInterrupts++;
        }
    }

    /// <summary>The periodic work item's body: one drain. Thread context on the kit worker.</summary>
    internal void DrainOnWorker() => Drain();

    /// <summary>
    /// Quiesces the controller on the way out: both ports disabled, the
    /// output buffer flushed (a device byte left there would otherwise be
    /// read as the configuration), then the configuration read and written
    /// back with both interrupt enables clear. Every command write is
    /// bounded by <see cref="BufferWaitMilliseconds"/> and a wait that
    /// reaches its deadline ends the quiesce there, so the whole takes about
    /// 60 ms at most on a slow controller. Thread context on the kit worker,
    /// after the kit disconnected the line handles and tore the children
    /// down.
    /// </summary>
    internal void QuiesceOnDetach()
    {
        if (!WriteCommand(DisableKeyboardPort))
        {
            return;
        }

        if (!WriteCommand(DisableAuxiliaryPort))
        {
            return;
        }

        Flush();
        if (!WriteCommand(ReadConfiguration))
        {
            return;
        }

        if (!TryReadReply(out byte configuration))
        {
            return;
        }

        WriteCommand(WriteConfiguration, (byte)(configuration & ~(KeyboardInterruptEnable | AuxiliaryInterruptEnable)));
    }

    // --- Ps2Controller ---

    /// <inheritdoc/>
    protected override bool TrySendCore(Ps2Port port, byte value)
    {
        if (port == Ps2Port.Auxiliary)
        {
            if (!WriteCommand(WriteAuxiliaryPort))
            {
                return false;
            }
        }

        if (!WaitInputBufferEmpty())
        {
            return false;
        }

        using (_lock.Acquire())
        {
            _data.Write8(PortOffset, value);
        }

        return true;
    }

    /// <inheritdoc/>
    protected override void PollCore() => Drain();

    // --- Private helpers ---

    /// <summary>
    /// Hands one byte read from the output buffer to the access of the port
    /// the status attributed it to, or counts it as stray when that access
    /// is null. Outside the lock; any context; allocation-free.
    /// </summary>
    /// <param name="value">The byte.</param>
    /// <param name="auxiliary">True when status bit 5 was set for it.</param>
    private void Dispatch(byte value, bool auxiliary)
    {
        Ps2Access? access = auxiliary ? AuxiliaryAccess : KeyboardAccess;
        if (access is null)
        {
            _strayBytes++;
            return;
        }

        _bytesDelivered++;
        access.Deliver(value);
    }

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    /// <param name="milliseconds">How far ahead.</param>
    private static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;
}
