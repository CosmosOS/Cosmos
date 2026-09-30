// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The Intel HD Audio driver over the driver kit: binds any controller that
/// reports the HD Audio programming interface, takes it out of reset, brings
/// up the command and response rings, walks the codecs for a converter and a
/// pin it can play out of, programs one output stream over a cyclic DMA
/// buffer, and publishes the output to the ring. It matches by PCI class
/// rather than by device id, because the specification fixes the register
/// layout and every vendor's controller answers the same programming
/// interface; that is what makes the one driver cover QEMU's
/// <c>intel-hda</c>, Intel's own PCH controllers and the AMD and NVIDIA
/// controllers that follow the same spec. Everything it holds for one
/// controller lives on an <see cref="HdAudioState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Audio)]
public sealed class HdAudioDriver : Driver
{
    // --- Constants ---

    /// <summary>PCI class of a multimedia device.</summary>
    private const byte MultimediaClass = 0x04;

    /// <summary>PCI subclass of an HD Audio controller.</summary>
    private const byte HdAudioSubclass = 0x03;

    /// <summary>PCI programming interface of an HD Audio controller.</summary>
    private const byte HdAudioProgrammingInterface = 0x00;

    /// <summary>The base address register holding the controller's registers.</summary>
    private const int RegisterBar = 0;

    /// <summary>Bytes the register window has to span: the controller block plus a few stream descriptors.</summary>
    private const ulong RegisterWindowBytes = 0x180;

    /// <summary>Polls of the controller leaving or entering reset.</summary>
    private const int ResetPollCount = 100;

    /// <summary>Microseconds between two polls of the controller's reset.</summary>
    private const uint ResetPollMicroseconds = 100;

    /// <summary>
    /// Microseconds waited after the controller leaves reset, before the
    /// codecs are read. The specification asks for at least 521 us for every
    /// codec on the link to finish announcing itself.
    /// </summary>
    private const uint CodecEnumerationMicroseconds = 1000;

    /// <summary>The interrupt source index of the function's legacy line.</summary>
    private const int LineInterruptIndex = 0;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: MultimediaClass, subclass: HdAudioSubclass, progIf: HdAudioProgrammingInterface),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(HdAudioDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up and publishes its output. Thread context on
    /// the kit worker; a declined or failed result makes the kit release
    /// everything acquired here and quiet the function again.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the output published; declined when the function is not one this driver can operate; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. BAR0 is the register window.
        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar0 = pci.Bars[RegisterBar];
        if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < RegisterWindowBytes)
        {
            return ProbeResult.Declined($"BAR0 is not a memory window of {RegisterWindowBytes} bytes");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on: the rings and the frame buffer are all read
        //    by the controller itself.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. Reset. Interrupts go off first, so a controller firmware left
        //    armed cannot deliver one into a handler that is not connected.
        registers.Write32(HdAudioRegisters.InterruptControl, 0);
        if (!Reset(binding, registers))
        {
            return ProbeResult.Failed("the controller did not leave reset");
        }

        // 4. The link needs a moment for every codec to announce itself. The
        //    status bits are set by the reset itself and are write-to-clear,
        //    so they are read as they stand: clearing them first would throw
        //    away the very announcement this is waiting for.
        binding.Delay(CodecEnumerationMicroseconds);
        ushort codecs = registers.Read16(HdAudioRegisters.StateChangeStatus);
        if (codecs == 0)
        {
            return ProbeResult.Declined("no codec answered on the link");
        }

        // 5. Which stream descriptor is the first output one: the input
        //    streams come first in the descriptor block.
        uint capabilities = registers.Read16(HdAudioRegisters.GlobalCapabilities);
        int inputStreams = (int)((capabilities >> HdAudioRegisters.CapabilitiesInputStreamsShift) & HdAudioRegisters.CapabilitiesStreamsMask);
        int outputStreams = (int)((capabilities >> HdAudioRegisters.CapabilitiesOutputStreamsShift) & HdAudioRegisters.CapabilitiesStreamsMask);
        if (outputStreams == 0)
        {
            return ProbeResult.Declined("the controller has no output stream");
        }

        int streamIndex = inputStreams;
        ulong streamBase = HdAudioRegisters.StreamBase(streamIndex);
        if (streamBase + HdAudioRegisters.StreamDescriptorStride > bar0.Length)
        {
            return ProbeResult.Declined("BAR0 does not span the output stream's descriptor");
        }

        // 6. The command and response rings, then the codec walk over them.
        DmaBuffer corb = binding.AllocateDma(HdAudioCodec.CorbBytes, HdAudioCodec.RingAlignment);
        DmaBuffer rirb = binding.AllocateDma(HdAudioCodec.RirbBytes, HdAudioCodec.RingAlignment);
        HdAudioCodec codec = new(binding, registers, corb, rirb);
        if (!codec.Start())
        {
            return ProbeResult.Failed("the command ring pointers did not reset");
        }

        if (!codec.TryFindOutputPath(codecs, out HdAudioOutputPath path))
        {
            return ProbeResult.Declined("no codec offers a converter and an output pin");
        }

        // 7. The cyclic frame buffer and the descriptor list over it.
        DmaBuffer ring = binding.AllocateDma(HdAudioState.RingBytes, HdAudioState.RingAlignment);
        DmaBuffer bdl = binding.AllocateDma(HdAudioState.BdlBytes, HdAudioState.BdlAlignment);

        HdAudioState state = new(binding, registers, codec, ring, bdl, streamBase, path);
        binding.DriverState = state;

        if (!state.ResetStream())
        {
            return ProbeResult.Failed("the output stream did not leave reset");
        }

        state.ProgramStream();

        // 8. The default format, which also programs the converter and the
        //    pin, so a caller that never calls TrySetFormat still plays.
        if (!state.TrySetFormat(AudioFormat.Stereo16, HdAudioStreamFormat.DefaultSampleRate))
        {
            return ProbeResult.Failed("the controller did not accept 48 kHz stereo");
        }

        // 9. The line, when the platform routes it. The completion interrupt
        //    only tells the ring that room came free; the writer reads the
        //    engine's own position, so a controller whose line the platform
        //    cannot route still plays.
        bool hasLine = binding.Node.Interrupts.Count > LineInterruptIndex
            && binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptIndex], state.OnInterrupt, out _);
        if (hasLine)
        {
            // Only the stream's own cause is routed. The controller cause
            // covers the response ring, which this driver polls and whose
            // status bit it clears itself: delivered to the line it would be
            // an interrupt the handler does not acknowledge, and the line
            // would stay asserted.
            registers.Write32(
                HdAudioRegisters.InterruptControl,
                HdAudioRegisters.InterruptControlGlobalEnable | (1u << streamIndex));
        }

        // 10. The output, and the sink it reports completions through.
        AudioSink sink = binding.PublishAudio(state);
        state.AttachSink(sink);

        byte major = registers.Read8(HdAudioRegisters.VersionMajor);
        byte minor = registers.Read8(HdAudioRegisters.VersionMinor);
        string line = hasLine ? $"line {pci.InterruptLine}" : "no line";
        binding.Log(
            $"version {major}.{minor}, codec {path.Codec} converter {path.Converter} pin {path.Pin}, " +
            $"stream {streamIndex} ({inputStreams} in, {outputStreams} out), 48 kHz stereo 16-bit, {line}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Stops the stream and the rings before the kit frees the memory they
    /// point at. Thread context on the kit worker; nothing is written when
    /// the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not HdAudioState state || !reason.HardwarePresent)
        {
            return;
        }

        // The state holds the register window, so it turns the controller's
        // interrupts off, stops the stream and stops the codec rings, in that
        // order, before the kit frees the memory they point at.
        state.Quiesce();
    }

    // --- Private methods ---

    /// <summary>
    /// Takes the controller into reset and out again. The reset bit reads as
    /// the controller's run state: clear while it is held in reset, set once
    /// it is running.
    /// </summary>
    /// <param name="binding">The binding, for its delays.</param>
    /// <param name="registers">The controller's register window.</param>
    /// <returns>False when the controller did not follow either half of the reset.</returns>
    private static bool Reset(DeviceBinding binding, RegisterWindow registers)
    {
        registers.Write32(HdAudioRegisters.GlobalControl, 0);
        if (!PollReset(binding, registers, set: false))
        {
            return false;
        }

        registers.Write32(HdAudioRegisters.GlobalControl, HdAudioRegisters.GlobalControlReset);
        return PollReset(binding, registers, set: true);
    }

    /// <summary>Polls the controller's reset bit until it reaches the wanted state.</summary>
    /// <param name="binding">The binding, for its delays.</param>
    /// <param name="registers">The controller's register window.</param>
    /// <param name="set">The state to wait for: set means running.</param>
    private static bool PollReset(DeviceBinding binding, RegisterWindow registers, bool set)
    {
        for (int poll = 0; poll < ResetPollCount; poll++)
        {
            bool running = (registers.Read32(HdAudioRegisters.GlobalControl) & HdAudioRegisters.GlobalControlReset) != 0;
            if (running == set)
            {
                return true;
            }

            binding.Delay(ResetPollMicroseconds);
        }

        return false;
    }
}
