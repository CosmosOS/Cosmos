// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// The access object of a Ps2 node, one per port, constructed by the 8042
/// driver: the byte stream from the port (a receive ring the leaf drains
/// from its handler with <see cref="TryReceive"/>), the command exchange
/// (<see cref="TryCommand"/>: the command byte and its arguments each
/// acknowledged by 0xFA, resent on 0xFE, then the reply bytes) and the
/// port's interrupt source, which the access raises for every stream byte.
/// The controller driver hands every byte the status register attributed
/// to the port to <see cref="Deliver"/>, which either completes the step of
/// the exchange in flight or appends the byte to the ring and raises the
/// source. The kit holds the protocol and nothing of the hardware: the
/// controller driver maps the ports and connects the lines. The execution
/// context of each member is in its summary.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class Ps2Access
{
    /// <summary>Bytes the port's receive ring holds; a full ring drops its oldest byte and counts the overrun.</summary>
    public const int ReceiveRingBytes = 16;

    /// <summary>The most reply bytes one exchange collects; a longer <c>reply</c> span is refused.</summary>
    public const int MaxReplyBytes = 8;

    /// <summary>How many times a byte the device answered 0xFE to is sent again before the exchange fails.</summary>
    public const int MaxResends = 3;

    /// <summary>The device's answer to a command byte it accepted.</summary>
    public const byte Acknowledge = 0xFA;

    /// <summary>The device's answer to a command byte it wants again.</summary>
    public const byte Resend = 0xFE;

    /// <summary>Once at least one reply byte arrived, the exchange ends when this long passes without another.</summary>
    public const uint ReplyGapMilliseconds = 20;

    /// <summary>The wait between two polls of a controller that is not interrupt driven.</summary>
    private const uint PolledWaitMilliseconds = 1;

    /// <summary>
    /// The longest wait for an acknowledgement, or for the first reply
    /// byte, on an interrupt driven controller before the deadline is
    /// checked again; a signal ends it earlier.
    /// </summary>
    private const uint StepWaitMilliseconds = 10;

    /// <summary>Milliseconds in a second, for the gap in <see cref="Stopwatch"/> ticks.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>Where the exchange in flight stands; every transition happens under the lock.</summary>
    private enum ExchangeState : byte
    {
        /// <summary>No exchange, or the exchange has every acknowledgement and wants no reply: every byte is a stream byte.</summary>
        Idle,

        /// <summary>A command or argument byte was sent and its 0xFA or 0xFE is awaited.</summary>
        AwaitingAcknowledge,

        /// <summary>The last byte was acknowledged and the reply bytes are collected.</summary>
        CollectingReply,
    }

    private readonly Ps2Controller _controller;
    private readonly Ps2InterruptSource _source;
    private readonly DeviceEvent _step = new();
    private readonly byte[] _ring = new byte[ReceiveRingBytes];
    private readonly byte[] _replyBuffer = new byte[MaxReplyBytes];
    private readonly long _gapTicks;
    private SchedSpinLock _lock;
    private int _ringHead;
    private int _ringCount;
    private volatile int _overruns;
    private ExchangeState _state;
    private bool _busy;
    private bool _acknowledged;
    private bool _resendRequested;
    private bool _lastByte;
    private int _replyCount;
    private int _replyCapacity;
    private long _lastReplyTimestamp;

    /// <summary>
    /// Creates the access of one port: the receive ring, the port's source
    /// and the exchange's event are allocated here. Thread context, the
    /// controller driver's probe.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="controller">The controller driver's contract half.</param>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is neither <see cref="Ps2Port.Keyboard"/> nor <see cref="Ps2Port.Auxiliary"/>.</exception>
    public Ps2Access(Ps2Port port, Ps2Controller controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (port != Ps2Port.Keyboard && port != Ps2Port.Auxiliary)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        Port = port;
        _controller = controller;
        _source = new Ps2InterruptSource(port);
        _gapTicks = Stopwatch.Frequency / MillisecondsPerSecond * ReplyGapMilliseconds;
    }

    /// <summary>The port this access serves. Any context.</summary>
    public Ps2Port Port { get; }

    /// <summary>
    /// True when bytes reach this access after the probe returns: the
    /// controller's lines are connected or its driver drains it
    /// periodically. A leaf declines a port that is neither
    /// (<c>no interrupt and no timer to poll with</c>). Any context.
    /// </summary>
    public bool DeliversUnattended => _controller.InterruptDriven || _controller.PolledPeriodically;

    /// <summary>How many stream bytes a full ring dropped so far. Any context.</summary>
    public int OverrunCount => _overruns;

    /// <summary>
    /// The node's one interrupt source, the port's, in a fresh array for
    /// the controller driver to hand to <see cref="DeviceBinding.PublishChild"/>
    /// exactly once: the element is the access's own source object, so the
    /// node owns exactly it, and the access never reads the array itself.
    /// Thread context.
    /// </summary>
    public InterruptSource[] InterruptsForPublish()
    {
        return [_source];
    }

    /// <summary>
    /// A byte the status register attributed to this port. Any context (the
    /// controller driver's interrupt handler, its periodic drain, or a poll
    /// inside an exchange); allocation-free; under no kit lock, the access's
    /// own spin lock excepted. Completes the step of the exchange in flight
    /// (an acknowledgement, a resend request or a reply byte) and signals
    /// the exchange's waiter, or appends the byte to the receive ring and
    /// raises the port's source. A byte does one or the other, never both.
    /// </summary>
    /// <param name="value">The byte.</param>
    public void Deliver(byte value)
    {
        bool completesStep = false;
        using (_lock.AcquireIrqSafe())
        {
            switch (_state)
            {
                case ExchangeState.AwaitingAcknowledge:
                    if (value == Acknowledge)
                    {
                        _acknowledged = true;
                        if (_lastByte)
                        {
                            // A reply byte that follows the last acknowledgement
                            // before the waiter runs again lands in the reply and
                            // not in the ring.
                            _state = _replyCapacity > 0 ? ExchangeState.CollectingReply : ExchangeState.Idle;
                        }

                        completesStep = true;
                    }
                    else if (value == Resend)
                    {
                        _resendRequested = true;
                        completesStep = true;
                    }
                    else
                    {
                        // A stream byte that overtook the exchange.
                        Append(value);
                    }

                    break;

                case ExchangeState.CollectingReply:
                    if (_replyCount < _replyCapacity)
                    {
                        // A Reset's failure code is a reply byte like any other.
                        _replyBuffer[_replyCount++] = value;
                        _lastReplyTimestamp = Stopwatch.GetTimestamp();
                        if (_replyCount == _replyCapacity)
                        {
                            _state = ExchangeState.Idle;
                        }

                        completesStep = true;
                    }
                    else
                    {
                        // The reply is full and the exchange has not read it yet.
                        Append(value);
                    }

                    break;

                default:
                    Append(value);
                    break;
            }
        }

        if (completesStep)
        {
            // Safe from interrupt context, and latched for a waiter not yet parked.
            _step.Signal();
            return;
        }

        // A real interrupt already runs masked (the scope is a no-op there);
        // a polled delivery from thread context gives the leaf's handler the
        // masked context every source promises.
        using (InternalCpu.DisableInterruptsScope())
        {
            _source.Raise();
        }
    }

    /// <summary>
    /// Takes the oldest byte off the receive ring. Allocation-free; any
    /// context, the leaf's interrupt handler in practice.
    /// </summary>
    /// <param name="value">The byte, or 0 when the ring is empty.</param>
    /// <returns>False when the ring is empty.</returns>
    public bool TryReceive(out byte value)
    {
        using (_lock.AcquireIrqSafe())
        {
            if (_ringCount == 0)
            {
                value = 0;
                return false;
            }

            value = _ring[_ringHead];
            _ringHead = (_ringHead + 1) % ReceiveRingBytes;
            _ringCount--;
            return true;
        }
    }

    /// <summary>
    /// Sends <paramref name="command"/> and then each of
    /// <paramref name="arguments"/> to the device, each acknowledged by 0xFA
    /// within the remaining time (0xFE makes the access send that byte
    /// again, up to <see cref="MaxResends"/> times), then collects up to
    /// <c>reply.Length</c> reply bytes, ending at <c>reply.Length</c>, at
    /// the first gap of <see cref="ReplyGapMilliseconds"/> once one byte
    /// arrived, or at the deadline with what arrived (an AT keyboard answers
    /// Identify with nothing: <paramref name="replyLength"/> 0 and true).
    /// The access does not judge a reply: a Reset's failure code (0xFC or
    /// 0xFD) comes back as <c>reply[0]</c> and the leaf reads it. Thread
    /// context: a probe, <c>OnDetach</c>, or the ring's indicator work item
    /// on the kit worker; never an interrupt handler. One exchange at a time
    /// per port, and the exchanges of the two ports are serialized by their
    /// callers, all on the worker.
    /// </summary>
    /// <param name="command">The command byte.</param>
    /// <param name="arguments">The bytes that follow it, each acknowledged in turn.</param>
    /// <param name="reply">Where the reply bytes go; at most <see cref="MaxReplyBytes"/> long.</param>
    /// <param name="replyLength">How many reply bytes arrived; 0 when the exchange failed.</param>
    /// <param name="timeoutMilliseconds">The time the whole exchange may take, counted from the call.</param>
    /// <returns>False when a byte is not acknowledged within <paramref name="timeoutMilliseconds"/>, when the resends ran out, when the controller could not accept a byte, or when another exchange is in flight on this port.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reply"/> is longer than <see cref="MaxReplyBytes"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool TryCommand(byte command, ReadOnlySpan<byte> arguments, Span<byte> reply, out int replyLength, uint timeoutMilliseconds)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(TryCommand));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(reply.Length, MaxReplyBytes, nameof(reply));

        using (_lock.AcquireIrqSafe())
        {
            if (_busy)
            {
                replyLength = 0;
                return false;
            }

            _busy = true;
            _state = ExchangeState.Idle;
            _replyCount = 0;
            _replyCapacity = reply.Length;
        }

        // The exchange always ends Idle and not busy, whatever the controller
        // driver did in TrySend or Poll: an exception there leaves this port
        // usable for the next exchange instead of wedging it.
        try
        {
            long deadline = KitTime.DeadlineAfter(timeoutMilliseconds);
            int byteCount = 1 + arguments.Length;
            for (int i = 0; i < byteCount; i++)
            {
                byte value = i == 0 ? command : arguments[i - 1];
                bool isLast = i == byteCount - 1;
                if (!SendAcknowledged(value, isLast, deadline))
                {
                    replyLength = 0;
                    return false;
                }
            }

            if (reply.Length == 0)
            {
                replyLength = 0;
                return true;
            }

            while (!KitTime.HasPassed(deadline))
            {
                int count;
                long lastReply;
                using (_lock.AcquireIrqSafe())
                {
                    count = _replyCount;
                    lastReply = _lastReplyTimestamp;
                }

                if (count == reply.Length)
                {
                    break;
                }

                if (count > 0 && Stopwatch.GetTimestamp() - lastReply >= _gapTicks)
                {
                    break;
                }

                // Once a byte arrived the wait ends with the next byte's signal
                // or when the gap has passed, whichever is first.
                WaitStep(count > 0 ? ReplyGapMilliseconds : StepWaitMilliseconds);
            }

            using (_lock.AcquireIrqSafe())
            {
                replyLength = _replyCount;
                _replyBuffer.AsSpan(0, replyLength).CopyTo(reply);
            }

            return true;
        }
        finally
        {
            Finish();
        }
    }

    /// <summary>
    /// Sends one byte and waits for its acknowledgement, sending it again
    /// on a resend request up to <see cref="MaxResends"/> times. The state
    /// is set before the byte is sent, so a reply arriving before the wait
    /// begins is not mistaken for a stream byte. Thread context.
    /// </summary>
    /// <param name="value">The byte.</param>
    /// <param name="isLast">True for the last byte of the sequence, after whose acknowledgement the reply is collected.</param>
    /// <param name="deadline">The exchange's deadline.</param>
    /// <returns>False when the controller could not accept the byte, the resends ran out or the deadline passed.</returns>
    private bool SendAcknowledged(byte value, bool isLast, long deadline)
    {
        for (int attempt = 0; attempt <= MaxResends; attempt++)
        {
            using (_lock.AcquireIrqSafe())
            {
                _acknowledged = false;
                _resendRequested = false;
                _lastByte = isLast;
                _state = ExchangeState.AwaitingAcknowledge;
            }

            if (!_controller.TrySend(Port, value))
            {
                return false;
            }

            bool resend = false;
            while (!KitTime.HasPassed(deadline))
            {
                bool acknowledged;
                using (_lock.AcquireIrqSafe())
                {
                    acknowledged = _acknowledged;
                    resend = _resendRequested;
                }

                if (acknowledged)
                {
                    return true;
                }

                if (resend)
                {
                    break;
                }

                WaitStep(StepWaitMilliseconds);
            }

            if (!resend)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// One wait of the exchange: on a controller that is not interrupt
    /// driven a poll first, which delivers a pending byte through
    /// <see cref="Deliver"/> and signals the event the wait then consumes
    /// at once, and a short wait so the next poll comes soon. Thread
    /// context.
    /// </summary>
    /// <param name="milliseconds">The longest wait on an interrupt driven controller.</param>
    private void WaitStep(uint milliseconds)
    {
        if (!_controller.InterruptDriven)
        {
            _controller.Poll();
            milliseconds = PolledWaitMilliseconds;
        }

        _step.Wait(milliseconds);
    }

    /// <summary>Ends the exchange: a byte arriving afterwards is a stream byte. Thread context.</summary>
    private void Finish()
    {
        using (_lock.AcquireIrqSafe())
        {
            _state = ExchangeState.Idle;
            _busy = false;
        }
    }

    /// <summary>Appends a stream byte to the ring, dropping the oldest when full. Under the lock; any context.</summary>
    /// <param name="value">The byte.</param>
    private void Append(byte value)
    {
        if (_ringCount == ReceiveRingBytes)
        {
            _ringHead = (_ringHead + 1) % ReceiveRingBytes;
            _ringCount--;
            _overruns++;
        }

        _ring[(_ringHead + _ringCount) % ReceiveRingBytes] = value;
        _ringCount++;
    }
}
