// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The suite's one full driver: binds the <c>kbd</c> device, maps its
/// window and writes a pattern into it, allocates DMA memory and fills it,
/// publishes a keyboard, connects interrupt 0 to a handler that reads the
/// window and reports a key, schedules periodic work and starts a thread
/// that waits on the binding's event until teardown. Everything it holds
/// lives on a <see cref="KeyboardState"/> in <see cref="DeviceBinding.DriverState"/>.
/// </summary>
[Driver]
public sealed class KeyboardDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "kbd";

    /// <summary>The name of the driver thread, for the log.</summary>
    public const string ThreadName = "kbd-thread";

    /// <summary>Resources every probe holds: window, DMA, event, two work items and the interrupt handle.</summary>
    private const int BaseResourceCount = 6;

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(KeyboardDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        RegisterWindow window = binding.MapRegisters(0);
        for (int i = 0; i < KeyboardState.PatternLength; i++)
        {
            window.Write8((ulong)(KeyboardState.PatternOffset + i), (byte)(KeyboardState.PatternFirstByte + i));
        }

        DmaBuffer dma = binding.AllocateDma(KeyboardState.DmaBytes, KeyboardState.DmaAlignment);
        bool dmaWasZeroed = IsZeroed(dma.Span);
        Span<byte> dmaSpan = dma.Span;
        for (int i = 0; i < dmaSpan.Length; i++)
        {
            dmaSpan[i] = (byte)(KeyboardState.DmaFirstByte + i);
        }

        DmaBuffer.WriteBarrier();

        DeviceEvent keyEvent = binding.CreateEvent();
        KeyboardState state = new(binding, window, dma, keyEvent, dmaWasZeroed);
        binding.DriverState = state;

        state.Sink = binding.PublishKeyboard(state);

        if (!binding.TryRequestInterrupt(binding.Node.Interrupts[0], state.OnInterrupt, out InterruptHandle? handle))
        {
            return ProbeResult.Failed("interrupt 0 could not be connected");
        }

        state.Handle = handle;

        int held = BaseResourceCount;
        state.PeriodicScheduled = binding.TrySchedulePeriodic(KeyboardState.PeriodicIntervalMilliseconds, state.PeriodicWork);
        if (state.PeriodicScheduled)
        {
            held++;
        }

        if (binding.TryStartThread(ThreadName, state.ThreadMain, out DriverThread? thread))
        {
            state.Thread = thread;
            held++;
        }

        state.ExpectedHeldResourceCount = held;
        return ProbeResult.Bound;
    }

    /// <inheritdoc/>
    protected override void OnDetachCore(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is KeyboardState state)
        {
            state.RecordDetach();
        }
    }

    private static bool IsZeroed(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != 0)
            {
                return false;
            }
        }

        return true;
    }
}
