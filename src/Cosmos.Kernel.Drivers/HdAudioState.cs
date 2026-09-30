// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything one HD Audio controller holds, and the
/// <see cref="IAudioOutput"/> the ring drives it through. The frames live in
/// one cyclic DMA buffer the controller's stream engine reads on the codec's
/// clock; the ring writes into it ahead of the engine and the engine never
/// waits. <see cref="Write"/> runs in the caller's thread context under the
/// device lock, and the interrupt handler only acknowledges the stream and
/// reports the completion, so nothing allocates where it may not.
/// </summary>
internal sealed class HdAudioState : IAudioOutput
{
    // --- Constants ---

    /// <summary>The name every HD Audio output is published under.</summary>
    private const string OutputName = "hda";

    /// <summary>Periods the cyclic buffer is split into; each one is a BDL entry that interrupts on completion.</summary>
    internal const int Periods = 4;

    /// <summary>Bytes of one period. At 48 kHz stereo 16-bit that is about 21 ms of audio.</summary>
    internal const int PeriodBytes = 4096;

    /// <summary>Bytes the whole cyclic buffer spans.</summary>
    internal const int RingBytes = Periods * PeriodBytes;

    /// <summary>Alignment the cyclic buffer is allocated at.</summary>
    internal const int RingAlignment = 128;

    /// <summary>Bytes the buffer descriptor list spans.</summary>
    internal const int BdlBytes = Periods * 16;

    /// <summary>Alignment the descriptor list needs.</summary>
    internal const int BdlAlignment = 128;

    /// <summary>
    /// Bytes kept between the writer and the DMA engine, so a write never
    /// lands on the period the engine is reading out of. One period is the
    /// coarsest the engine's position can be stale by. The run is also kept
    /// silent, which is what a writer that falls behind underruns into.
    /// </summary>
    private const int GuardBytes = PeriodBytes;

    /// <summary>The stream tag the converter is told to take frames from. Any non-zero value the controller does not use.</summary>
    internal const byte StreamNumber = 1;

    /// <summary>Polls of a stream reset.</summary>
    private const int StreamResetPollCount = 100;

    /// <summary>Microseconds between two polls of a stream reset.</summary>
    private const uint StreamResetPollMicroseconds = 10;

    /// <summary>The formats this driver programs: signed 16-bit stereo, the one every codec accepts.</summary>
    private static readonly AudioFormat[] s_supportedFormats = [AudioFormat.Stereo16];

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly HdAudioCodec _codec;
    private readonly DmaBuffer _ring;
    private readonly DmaBuffer _bdl;
    private readonly DeviceLock _lock;
    private readonly ulong _streamBase;
    private readonly HdAudioOutputPath _path;

    /// <summary>The sink the ring is reported through; set once the device is published.</summary>
    private AudioSink? _sink;

    /// <summary>Where the next write goes, a byte offset into the cyclic buffer.</summary>
    private int _writePosition;

    /// <summary>Whether the stream engine is running.</summary>
    private bool _running;

    /// <summary>
    /// The format word the descriptor and the converter agree on. Kept
    /// because a stream reset clears the descriptor, and every
    /// <see cref="Start"/> resets before it programs.
    /// </summary>
    private ushort _encodedFormat;

    // --- Construction ---

    /// <summary>Records what the probe acquired. Thread context, from <see cref="HdAudioDriver.Probe"/>.</summary>
    /// <param name="binding">The binding that owns everything here.</param>
    /// <param name="registers">The controller's register window.</param>
    /// <param name="codec">The codec command layer.</param>
    /// <param name="ring">The cyclic frame buffer.</param>
    /// <param name="bdl">The buffer descriptor list over <paramref name="ring"/>.</param>
    /// <param name="streamBase">Offset of the output stream's descriptor registers in the window.</param>
    /// <param name="path">The converter and pin the codec plays through.</param>
    internal HdAudioState(
        DeviceBinding binding,
        RegisterWindow registers,
        HdAudioCodec codec,
        DmaBuffer ring,
        DmaBuffer bdl,
        ulong streamBase,
        in HdAudioOutputPath path)
    {
        _binding = binding;
        _registers = registers;
        _codec = codec;
        _ring = ring;
        _bdl = bdl;
        _streamBase = streamBase;
        _path = path;
        _lock = binding.CreateLock();

        Format = AudioFormat.Stereo16;
        SampleRate = HdAudioStreamFormat.DefaultSampleRate;
    }

    // --- Properties ---

    /// <inheritdoc/>
    public string Name => OutputName;

    /// <inheritdoc/>
    public AudioFormat Format { get; private set; }

    /// <inheritdoc/>
    public uint SampleRate { get; private set; }

    /// <inheritdoc/>
    public ReadOnlySpan<AudioFormat> SupportedFormats => s_supportedFormats;

    /// <inheritdoc/>
    public bool IsRunning => _running;

    /// <inheritdoc/>
    public int WritableBytes
    {
        get
        {
            using (_lock.Acquire())
            {
                return FreeBytesLocked();
            }
        }
    }

    /// <summary>The codec address, converter and pin the frames travel through, for the log and the tests.</summary>
    internal HdAudioOutputPath Path => _path;

    // --- Public methods ---

    /// <inheritdoc/>
    public bool TrySetFormat(AudioFormat format, uint sampleRate)
    {
        if (_running)
        {
            return false;
        }

        if (format != AudioFormat.Stereo16)
        {
            return false;
        }

        if (!HdAudioStreamFormat.TryEncode(format, sampleRate, out ushort encoded))
        {
            return false;
        }

        using (_lock.Acquire())
        {
            _encodedFormat = encoded;
            _registers.Write16(_streamBase + HdAudioRegisters.StreamFormat, encoded);
        }

        // The converter has to agree with the stream descriptor, and both are
        // programmed while the engine is stopped.
        _codec.ProgramOutputPath(_path, StreamNumber, encoded);

        Format = format;
        SampleRate = sampleRate;
        _sink?.FormatChanged();
        return true;
    }

    /// <inheritdoc/>
    public void Start()
    {
        if (_running)
        {
            return;
        }

        // The engine's position only returns to zero through a reset, and the
        // writer's cursor below starts there. Without this a second playback
        // writes at an offset the engine is nowhere near, and what reaches
        // the codec is whatever the ring still held. The reset polls, so it
        // runs before the lock rather than under it: the lock holds
        // interrupts off.
        ResetStream();
        ProgramStream();
        _registers.Write16(_streamBase + HdAudioRegisters.StreamFormat, _encodedFormat);

        using (_lock.Acquire())
        {
            if (_running)
            {
                return;
            }

            // Silence everywhere, so the first period the engine reads is
            // quiet rather than whatever the last playback left behind.
            _ring.Span.Clear();
            _writePosition = 0;
            DmaBuffer.WriteBarrier();

            byte control = (byte)(HdAudioRegisters.StreamControlRun | HdAudioRegisters.StreamControlInterruptOnCompletion);
            _registers.Write8(_streamBase + HdAudioRegisters.StreamControlLow, control);
            _running = true;
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        using (_lock.Acquire())
        {
            if (!_running)
            {
                return;
            }

            _registers.Write8(_streamBase + HdAudioRegisters.StreamControlLow, 0);
            _running = false;
            _ring.Span.Clear();
            _writePosition = 0;
        }
    }

    /// <inheritdoc/>
    public int Write(ReadOnlySpan<byte> frames)
    {
        if (frames.IsEmpty)
        {
            return 0;
        }

        int frameSize = Format.FrameSize;

        using (_lock.Acquire())
        {
            int free = FreeBytesLocked();
            int take = frames.Length < free ? frames.Length : free;
            take -= take % frameSize;
            if (take <= 0)
            {
                return 0;
            }

            Span<byte> ring = _ring.Span;
            int firstRun = RingBytes - _writePosition;
            if (take <= firstRun)
            {
                frames[..take].CopyTo(ring[_writePosition..]);
            }
            else
            {
                frames[..firstRun].CopyTo(ring[_writePosition..]);
                frames[firstRun..take].CopyTo(ring);
            }

            DmaBuffer.WriteBarrier();
            _writePosition = (_writePosition + take) % RingBytes;
            ClearGuardLocked();
            return take;
        }
    }

    // --- Internal methods ---

    /// <summary>Remembers the sink, once the probe published the device. Thread context.</summary>
    /// <param name="sink">The sink publishing returned.</param>
    internal void AttachSink(AudioSink sink) => _sink = sink;

    /// <summary>
    /// Fills the descriptor list over the cyclic buffer, one entry per
    /// period with the completion interrupt asked for, and points the stream
    /// at it. Thread context, from the probe, with the engine stopped.
    /// </summary>
    internal void ProgramStream()
    {
        Span<HdAudioBufferDescriptor> entries = MemoryMarshal.Cast<byte, HdAudioBufferDescriptor>(_bdl.Span);
        for (int i = 0; i < Periods; i++)
        {
            entries[i].Address = _ring.PhysicalAddress + (ulong)(i * PeriodBytes);
            entries[i].Length = PeriodBytes;
            entries[i].Flags = HdAudioBufferDescriptor.InterruptOnCompletion;
        }

        DmaBuffer.WriteBarrier();

        _registers.Write32(_streamBase + HdAudioRegisters.StreamBdlLow, (uint)_bdl.PhysicalAddress);
        _registers.Write32(_streamBase + HdAudioRegisters.StreamBdlHigh, (uint)(_bdl.PhysicalAddress >> 32));
        _registers.Write32(_streamBase + HdAudioRegisters.StreamCyclicBufferLength, RingBytes);
        _registers.Write16(_streamBase + HdAudioRegisters.StreamLastValidIndex, Periods - 1);

        // The stream number in the third control byte has to match what the
        // converter was told to listen for.
        _registers.Write8(_streamBase + HdAudioRegisters.StreamControlHigh, StreamNumber << 4);
    }

    /// <summary>
    /// Takes the stream engine through its reset, which is how the
    /// specification says to reach a known state before programming it.
    /// Thread context, from the probe.
    /// </summary>
    /// <returns>False when the engine did not enter or leave reset in time.</returns>
    internal bool ResetStream()
    {
        _registers.Write8(_streamBase + HdAudioRegisters.StreamControlLow, HdAudioRegisters.StreamControlReset);
        if (!PollStreamReset(set: true))
        {
            return false;
        }

        _registers.Write8(_streamBase + HdAudioRegisters.StreamControlLow, 0);
        return PollStreamReset(set: false);
    }

    /// <summary>
    /// The stream's interrupt handler: acknowledges the completion and tells
    /// the ring room came free. Interrupt context, so it allocates nothing,
    /// takes no lock and touches only the register window and the sink.
    /// </summary>
    /// <param name="context">The kit's interrupt context.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        byte status = _registers.Read8(_streamBase + HdAudioRegisters.StreamStatus);
        if ((status & HdAudioRegisters.StreamStatusWriteClearMask) == 0)
        {
            return;
        }

        // Write the bits back to clear them.
        _registers.Write8(_streamBase + HdAudioRegisters.StreamStatus, (byte)(status & HdAudioRegisters.StreamStatusWriteClearMask));

        if ((status & HdAudioRegisters.StreamStatusBufferCompleted) != 0)
        {
            _sink?.BufferCompleted();
        }
    }

    /// <summary>
    /// Turns the controller's interrupts off, stops the stream engine and
    /// stops the codec rings, so nothing is reading the DMA buffers by the
    /// time the kit frees them. Thread context, from the detach hook.
    /// </summary>
    internal void Quiesce()
    {
        // Interrupts first: a completion delivered after the device is
        // withdrawn would reach a sink that has nothing to report to.
        _registers.Write32(HdAudioRegisters.InterruptControl, 0);
        _registers.Write8(_streamBase + HdAudioRegisters.StreamControlLow, 0);
        _registers.Write8(_streamBase + HdAudioRegisters.StreamStatus, HdAudioRegisters.StreamStatusWriteClearMask);
        _running = false;
        _codec.Stop();
    }

    // --- Private methods ---

    /// <summary>Where the DMA engine has read to, a byte offset into the cyclic buffer.</summary>
    private int EnginePosition()
    {
        if (!_running)
        {
            return 0;
        }

        uint position = _registers.Read32(_streamBase + HdAudioRegisters.StreamLinkPosition);
        return (int)(position % RingBytes);
    }

    /// <summary>
    /// Bytes the writer may add before it would catch the engine, less the
    /// guard. Called under the lock.
    /// </summary>
    private int FreeBytesLocked()
    {
        if (!_running)
        {
            // Nothing is reading yet, so all but the guard is writable.
            return RingBytes - GuardBytes;
        }

        int engine = EnginePosition();
        int queued = _writePosition - engine;
        if (queued < 0)
        {
            queued += RingBytes;
        }

        int free = RingBytes - queued - GuardBytes;
        return free < 0 ? 0 : free;
    }

    /// <summary>
    /// Silences the run just past the writer: the bytes the engine reaches
    /// next if the writer does not get there first. Keeping it clear is what
    /// makes an underrun quiet, and it is safe because the run is always
    /// ahead of the writer and so never holds frames anyone has written.
    /// A swept region behind the engine cannot do the same job: the engine's
    /// position is modulo the ring, so a writer that blocks while the engine
    /// laps cannot tell one lap from four, and the sweep would clear frames
    /// it had just written. Called under the lock.
    /// </summary>
    private void ClearGuardLocked()
    {
        Span<byte> ring = _ring.Span;
        int end = _writePosition + GuardBytes;
        if (end <= RingBytes)
        {
            ring[_writePosition..end].Clear();
        }
        else
        {
            ring[_writePosition..].Clear();
            ring[..(end - RingBytes)].Clear();
        }

        DmaBuffer.WriteBarrier();
    }

    /// <summary>Polls the stream's reset bit until it reaches the wanted state.</summary>
    private bool PollStreamReset(bool set)
    {
        for (int poll = 0; poll < StreamResetPollCount; poll++)
        {
            bool asserted = (_registers.Read8(_streamBase + HdAudioRegisters.StreamControlLow) & HdAudioRegisters.StreamControlReset) != 0;
            if (asserted == set)
            {
                return true;
            }

            _binding.Delay(StreamResetPollMicroseconds);
        }

        return false;
    }
}
