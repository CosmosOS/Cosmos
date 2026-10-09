// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Audio.HdAudio;

/// <summary>
/// The codec side of an HD Audio controller: the command ring (CORB) and
/// the response ring (RIRB) in DMA memory, the verb encoding over them, and
/// the walk of a codec's widget tree that finds a converter and a pin to
/// play out of. Commands are answered by polling the response write
/// pointer rather than by an interrupt, because every command here is sent
/// from the probe, in thread context, and a poll keeps the response path
/// out of the interrupt handler entirely.
/// </summary>
internal sealed class HdAudioCodec
{
    // --- Constants ---

    /// <summary>Entries in each ring; the size every controller supports.</summary>
    private const int RingEntries = 256;

    /// <summary>Bytes of one CORB entry: a command is 32 bits.</summary>
    private const int CorbEntryBytes = 4;

    /// <summary>Bytes of one RIRB entry: a response and its extended half.</summary>
    private const int RirbEntryBytes = 8;

    /// <summary>Bytes the CORB spans.</summary>
    internal const int CorbBytes = RingEntries * CorbEntryBytes;

    /// <summary>Bytes the RIRB spans.</summary>
    internal const int RirbBytes = RingEntries * RirbEntryBytes;

    /// <summary>Alignment both rings need.</summary>
    internal const int RingAlignment = 128;

    /// <summary>Polls of one response before the command is given up on.</summary>
    private const int ResponsePollCount = 1000;

    /// <summary>Microseconds between two polls of a response.</summary>
    private const uint ResponsePollMicroseconds = 10;

    /// <summary>Polls of a ring pointer reset.</summary>
    private const int ResetPollCount = 100;

    /// <summary>Microseconds between two polls of a ring pointer reset.</summary>
    private const uint ResetPollMicroseconds = 10;

    // --- Verbs ---

    /// <summary>Get a parameter of a node; the payload names which.</summary>
    private const ushort VerbGetParameter = 0xF00;

    /// <summary>Set the format of a converter. A four-bit verb: its payload is 16 bits wide.</summary>
    private const ushort VerbSetConverterFormat = 0x2;

    /// <summary>Set the amplifier gain and mute flags. A four-bit verb.</summary>
    private const ushort VerbSetAmplifierGain = 0x3;

    /// <summary>Set which stream and channel a converter takes its frames from.</summary>
    private const ushort VerbSetConverterStreamChannel = 0x706;

    /// <summary>Set what a pin complex does: output, input, headphone drive.</summary>
    private const ushort VerbSetPinWidgetControl = 0x707;

    /// <summary>Set the external amplifier power-down flags.</summary>
    private const ushort VerbSetExternalAmplifier = 0x70C;

    /// <summary>Set a node's power state.</summary>
    private const ushort VerbSetPowerState = 0x705;

    /// <summary>Read a node's connection list.</summary>
    private const ushort VerbGetConnectionList = 0xF02;

    /// <summary>Choose which entry of a connection list a node listens to.</summary>
    private const ushort VerbSetConnectionSelect = 0x701;

    // --- Parameters ---

    /// <summary>How many child nodes a node has, and where they start.</summary>
    private const byte ParameterSubordinateNodeCount = 0x04;

    /// <summary>What kind of function group a node is.</summary>
    private const byte ParameterFunctionGroupType = 0x05;

    /// <summary>What a widget is and what it can do.</summary>
    private const byte ParameterAudioWidgetCapabilities = 0x09;

    /// <summary>What a pin complex can be wired to.</summary>
    private const byte ParameterPinCapabilities = 0x0C;

    /// <summary>How long a node's connection list is.</summary>
    private const byte ParameterConnectionListLength = 0x0E;

    // --- Field shapes ---

    /// <summary>Function group type: an audio function group.</summary>
    private const byte FunctionGroupAudio = 0x01;

    /// <summary>Widget capabilities: shift of the widget type.</summary>
    private const int WidgetTypeShift = 20;

    /// <summary>Widget capabilities: mask of the widget type.</summary>
    private const uint WidgetTypeMask = 0x0F;

    /// <summary>Widget type: an audio output converter, a DAC.</summary>
    private const uint WidgetTypeAudioOutput = 0x0;

    /// <summary>Widget type: a pin complex, where a jack or a speaker hangs.</summary>
    private const uint WidgetTypePinComplex = 0x4;

    /// <summary>Widget capabilities: the widget has an output amplifier.</summary>
    private const uint WidgetCapabilityOutputAmplifier = 1u << 2;

    /// <summary>Pin capabilities: the pin can drive an output.</summary>
    private const uint PinCapabilityOutput = 1u << 4;

    /// <summary>Pin widget control: enable the output path.</summary>
    private const byte PinControlOutputEnable = 1 << 6;

    /// <summary>Pin widget control: drive a headphone amplifier as well.</summary>
    private const byte PinControlHeadphoneEnable = 1 << 7;

    /// <summary>External amplifier: the EAPD pin, which many laptops need for their speakers.</summary>
    private const byte ExternalAmplifierEnable = 1 << 1;

    /// <summary>Power state D0, fully on.</summary>
    private const byte PowerStateD0 = 0x00;

    /// <summary>Subordinate node count: shift of the first child's node id.</summary>
    private const int SubordinateStartShift = 16;

    /// <summary>Subordinate node count: mask of a node id or a count.</summary>
    private const uint SubordinateMask = 0xFF;

    /// <summary>Amplifier payload: apply to the output amplifier.</summary>
    private const ushort AmplifierSetOutput = 1 << 15;

    /// <summary>Amplifier payload: apply to the left channel.</summary>
    private const ushort AmplifierSetLeft = 1 << 13;

    /// <summary>Amplifier payload: apply to the right channel.</summary>
    private const ushort AmplifierSetRight = 1 << 12;

    /// <summary>Amplifier payload: the gain field, set to its maximum here.</summary>
    private const ushort AmplifierGainMask = 0x7F;

    /// <summary>The highest codec address the state change register reports.</summary>
    private const int MaxCodecAddress = 15;

    /// <summary>The highest node id the walk visits, a guard against a codec that reports nonsense.</summary>
    private const int MaxNodeId = 255;

    // --- Private fields ---

    private readonly RegisterWindow _registers;
    private readonly DmaBuffer _corb;
    private readonly DmaBuffer _rirb;
    private readonly DeviceBinding _binding;

    /// <summary>Where the next command goes; the controller reads behind it.</summary>
    private ushort _corbWritePointer;

    /// <summary>The last response slot read; the controller writes ahead of it.</summary>
    private ushort _rirbReadPointer;

    // --- Construction ---

    /// <summary>Wraps the rings a probe allocated. Thread context.</summary>
    /// <param name="binding">The binding, for its delays.</param>
    /// <param name="registers">The controller's register window.</param>
    /// <param name="corb">The command ring, at least <see cref="CorbBytes"/> long.</param>
    /// <param name="rirb">The response ring, at least <see cref="RirbBytes"/> long.</param>
    internal HdAudioCodec(DeviceBinding binding, RegisterWindow registers, DmaBuffer corb, DmaBuffer rirb)
    {
        _binding = binding;
        _registers = registers;
        _corb = corb;
        _rirb = rirb;
    }

    // --- Ring lifecycle ---

    /// <summary>
    /// Points the controller at both rings, resets their pointers and
    /// starts the two DMA engines. Thread context, from the probe.
    /// </summary>
    /// <returns>False when a ring pointer did not reset, which means the controller is not answering.</returns>
    internal bool Start()
    {
        // The engines must be stopped before their base addresses move.
        _registers.Write8(HdAudioRegisters.CorbControl, 0);
        _registers.Write8(HdAudioRegisters.RirbControl, 0);

        _registers.Write8(HdAudioRegisters.CorbSize, HdAudioRegisters.RingSize256);
        _registers.Write8(HdAudioRegisters.RirbSize, HdAudioRegisters.RingSize256);

        _registers.Write32(HdAudioRegisters.CorbBaseLow, (uint)_corb.PhysicalAddress);
        _registers.Write32(HdAudioRegisters.CorbBaseHigh, (uint)(_corb.PhysicalAddress >> 32));
        _registers.Write32(HdAudioRegisters.RirbBaseLow, (uint)_rirb.PhysicalAddress);
        _registers.Write32(HdAudioRegisters.RirbBaseHigh, (uint)(_rirb.PhysicalAddress >> 32));

        if (!ResetCorbReadPointer())
        {
            return false;
        }

        _registers.Write16(HdAudioRegisters.CorbWritePointer, 0);
        _corbWritePointer = 0;

        // Resetting the response write pointer is a single write; the bit
        // clears itself, so there is nothing to poll for.
        _registers.Write16(HdAudioRegisters.RirbWritePointer, HdAudioRegisters.RirbWritePointerReset);
        _rirbReadPointer = 0;

        // One response per interrupt, though nothing here waits on one.
        _registers.Write16(HdAudioRegisters.RirbInterruptCount, 1);

        _registers.Write8(HdAudioRegisters.RirbStatus, HdAudioRegisters.RirbStatusResponseInterrupt);

        _registers.Write8(HdAudioRegisters.CorbControl, HdAudioRegisters.CorbControlRun);
        _registers.Write8(
            HdAudioRegisters.RirbControl,
            HdAudioRegisters.RirbControlRun | HdAudioRegisters.RirbControlResponseInterrupt);
        return true;
    }

    /// <summary>Stops both DMA engines, so the rings can be freed. Thread context, from the detach hook.</summary>
    internal void Stop()
    {
        _registers.Write8(HdAudioRegisters.CorbControl, 0);
        _registers.Write8(HdAudioRegisters.RirbControl, 0);
    }

    // --- Commands ---

    /// <summary>
    /// Sends a twelve-bit verb and waits for its response. Thread context.
    /// </summary>
    /// <param name="codec">The codec's address on the link.</param>
    /// <param name="node">The node inside the codec.</param>
    /// <param name="verb">The verb id.</param>
    /// <param name="payload">The verb's eight-bit payload.</param>
    /// <param name="response">What the codec answered.</param>
    /// <returns>False when no response arrived in time.</returns>
    internal bool TryCommand(byte codec, byte node, ushort verb, byte payload, out uint response)
    {
        uint command = ((uint)codec << 28) | ((uint)node << 20) | ((uint)verb << 8) | payload;
        return TrySend(command, out response);
    }

    /// <summary>
    /// Sends a four-bit verb, whose payload is sixteen bits wide. Thread
    /// context.
    /// </summary>
    /// <param name="codec">The codec's address on the link.</param>
    /// <param name="node">The node inside the codec.</param>
    /// <param name="verb">The verb id.</param>
    /// <param name="payload">The verb's sixteen-bit payload.</param>
    /// <param name="response">What the codec answered.</param>
    /// <returns>False when no response arrived in time.</returns>
    internal bool TryCommandWide(byte codec, byte node, ushort verb, ushort payload, out uint response)
    {
        uint command = ((uint)codec << 28) | ((uint)node << 20) | ((uint)verb << 16) | payload;
        return TrySend(command, out response);
    }

    /// <summary>Reads one parameter of a node. Thread context.</summary>
    /// <param name="codec">The codec's address.</param>
    /// <param name="node">The node.</param>
    /// <param name="parameter">Which parameter.</param>
    /// <param name="value">The parameter's value.</param>
    /// <returns>False when the codec did not answer.</returns>
    internal bool TryGetParameter(byte codec, byte node, byte parameter, out uint value)
    {
        return TryCommand(codec, node, VerbGetParameter, parameter, out value);
    }

    // --- Discovery ---

    /// <summary>
    /// Finds a converter and a pin that can play, on the first codec that
    /// offers both, and wires the two together. Thread context, from the
    /// probe.
    /// </summary>
    /// <param name="codecMask">The state change register's bits: which codec addresses answered.</param>
    /// <param name="path">The converter, the pin and the codec they live on.</param>
    /// <returns>False when no codec offers an output path.</returns>
    internal bool TryFindOutputPath(ushort codecMask, out HdAudioOutputPath path)
    {
        for (int address = 0; address <= MaxCodecAddress; address++)
        {
            if ((codecMask & (1 << address)) == 0)
            {
                continue;
            }

            if (TryFindOutputPathOn((byte)address, out path))
            {
                return true;
            }
        }

        path = default;
        return false;
    }

    /// <summary>
    /// Programs the path for playback: both nodes to D0, the converter onto
    /// the given stream, the pin driving its output with its amplifiers
    /// unmuted. Thread context.
    /// </summary>
    /// <param name="path">The path <see cref="TryFindOutputPath"/> found.</param>
    /// <param name="streamNumber">The stream tag the converter takes frames from.</param>
    /// <param name="format">The encoded stream format.</param>
    internal void ProgramOutputPath(in HdAudioOutputPath path, byte streamNumber, ushort format)
    {
        byte codec = path.Codec;

        TryCommand(codec, path.FunctionGroup, VerbSetPowerState, PowerStateD0, out _);
        TryCommand(codec, path.Converter, VerbSetPowerState, PowerStateD0, out _);
        TryCommand(codec, path.Pin, VerbSetPowerState, PowerStateD0, out _);

        // The converter takes channel 0 of the given stream.
        TryCommand(codec, path.Converter, VerbSetConverterStreamChannel, (byte)(streamNumber << 4), out _);
        TryCommandWide(codec, path.Converter, VerbSetConverterFormat, format, out _);

        // The pin drives its output, and its headphone amplifier too: on a
        // codec whose only jack is a headphone socket that is the one that
        // makes a sound, and on a speaker pin the bit is ignored.
        TryCommand(codec, path.Pin, VerbSetPinWidgetControl, PinControlOutputEnable | PinControlHeadphoneEnable, out _);
        TryCommand(codec, path.Pin, VerbSetExternalAmplifier, ExternalAmplifierEnable, out _);

        Unmute(codec, path.Converter, path.ConverterHasAmplifier);
        Unmute(codec, path.Pin, path.PinHasAmplifier);
    }

    // --- Private methods ---

    /// <summary>Writes a command into the ring and polls the response ring for its answer.</summary>
    private bool TrySend(uint command, out uint response)
    {
        Span<uint> commands = MemoryMarshal.Cast<byte, uint>(_corb.Span);
        Span<ulong> responses = MemoryMarshal.Cast<byte, ulong>(_rirb.Span);

        ushort slot = (ushort)((_corbWritePointer + 1) % RingEntries);
        commands[slot] = command;
        DmaBuffer.WriteBarrier();

        _corbWritePointer = slot;
        _registers.Write16(HdAudioRegisters.CorbWritePointer, slot);

        ushort expected = (ushort)((_rirbReadPointer + 1) % RingEntries);
        for (int poll = 0; poll < ResponsePollCount; poll++)
        {
            ushort written = (ushort)(_registers.Read16(HdAudioRegisters.RirbWritePointer) % RingEntries);
            if (written != _rirbReadPointer)
            {
                DmaBuffer.ReadBarrier();

                // The low half of the entry is the response; the high half
                // says which codec sent it, which nothing here needs.
                response = (uint)responses[expected];
                _rirbReadPointer = expected;

                // Acknowledging the response resets the controller's count of
                // outstanding ones. Without this the ring stops after
                // RINTCNT responses and every later command waits forever,
                // because the count is only cleared here.
                _registers.Write8(HdAudioRegisters.RirbStatus, HdAudioRegisters.RirbStatusResponseInterrupt);
                return true;
            }

            _binding.Delay(ResponsePollMicroseconds);
        }

        response = 0;
        return false;
    }

    /// <summary>Asserts the CORB read pointer reset, waits for it to read back, then clears it and waits again.</summary>
    private bool ResetCorbReadPointer()
    {
        _registers.Write16(HdAudioRegisters.CorbReadPointer, HdAudioRegisters.CorbReadPointerReset);
        if (!PollCorbReadPointer(set: true))
        {
            return false;
        }

        _registers.Write16(HdAudioRegisters.CorbReadPointer, 0);
        return PollCorbReadPointer(set: false);
    }

    /// <summary>Polls the CORB read pointer until its reset bit reaches the wanted state.</summary>
    private bool PollCorbReadPointer(bool set)
    {
        for (int poll = 0; poll < ResetPollCount; poll++)
        {
            bool asserted = (_registers.Read16(HdAudioRegisters.CorbReadPointer) & HdAudioRegisters.CorbReadPointerReset) != 0;
            if (asserted == set)
            {
                return true;
            }

            _binding.Delay(ResetPollMicroseconds);
        }

        return false;
    }

    /// <summary>Walks one codec's function groups for an audio one carrying both a converter and an output pin.</summary>
    private bool TryFindOutputPathOn(byte codec, out HdAudioOutputPath path)
    {
        path = default;

        if (!TryGetParameter(codec, 0, ParameterSubordinateNodeCount, out uint root))
        {
            return false;
        }

        int firstGroup = (int)((root >> SubordinateStartShift) & SubordinateMask);
        int groupCount = (int)(root & SubordinateMask);

        for (int i = 0; i < groupCount; i++)
        {
            int group = firstGroup + i;
            if (group > MaxNodeId)
            {
                break;
            }

            if (!TryGetParameter(codec, (byte)group, ParameterFunctionGroupType, out uint type))
            {
                continue;
            }

            if ((type & SubordinateMask) != FunctionGroupAudio)
            {
                continue;
            }

            if (TryFindOutputPathInGroup(codec, (byte)group, out path))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Walks the widgets of one audio function group for a converter and a pin that can play.</summary>
    private bool TryFindOutputPathInGroup(byte codec, byte group, out HdAudioOutputPath path)
    {
        path = default;

        if (!TryGetParameter(codec, group, ParameterSubordinateNodeCount, out uint widgets))
        {
            return false;
        }

        int firstWidget = (int)((widgets >> SubordinateStartShift) & SubordinateMask);
        int widgetCount = (int)(widgets & SubordinateMask);

        byte converter = 0;
        bool converterAmplifier = false;
        bool haveConverter = false;

        byte pin = 0;
        bool pinAmplifier = false;
        bool havePin = false;

        for (int i = 0; i < widgetCount; i++)
        {
            int node = firstWidget + i;
            if (node > MaxNodeId)
            {
                break;
            }

            if (!TryGetParameter(codec, (byte)node, ParameterAudioWidgetCapabilities, out uint capabilities))
            {
                continue;
            }

            uint widgetType = (capabilities >> WidgetTypeShift) & WidgetTypeMask;
            bool amplifier = (capabilities & WidgetCapabilityOutputAmplifier) != 0;

            if (!haveConverter && widgetType == WidgetTypeAudioOutput)
            {
                converter = (byte)node;
                converterAmplifier = amplifier;
                haveConverter = true;
                continue;
            }

            if (!havePin && widgetType == WidgetTypePinComplex && CanOutput(codec, (byte)node))
            {
                pin = (byte)node;
                pinAmplifier = amplifier;
                havePin = true;
            }
        }

        if (!haveConverter || !havePin)
        {
            return false;
        }

        SelectConverter(codec, pin, converter);
        path = new HdAudioOutputPath(codec, group, converter, pin, converterAmplifier, pinAmplifier);
        return true;
    }

    /// <summary>Whether a pin complex says it can drive an output.</summary>
    private bool CanOutput(byte codec, byte pin)
    {
        return TryGetParameter(codec, pin, ParameterPinCapabilities, out uint capabilities)
            && (capabilities & PinCapabilityOutput) != 0;
    }

    /// <summary>
    /// Points a pin at the converter, when the pin has a connection list to
    /// choose from. A pin with one input is already wired to it and takes no
    /// select; a pin whose list does not name the converter is left as it
    /// is, because the first entry is the codec's own default.
    /// </summary>
    private void SelectConverter(byte codec, byte pin, byte converter)
    {
        if (!TryGetParameter(codec, pin, ParameterConnectionListLength, out uint length))
        {
            return;
        }

        int entries = (int)(length & 0x7F);
        if (entries <= 1)
        {
            return;
        }

        // The list is read four entries at a time, each entry a byte.
        for (int index = 0; index < entries; index += 4)
        {
            if (!TryCommand(codec, pin, VerbGetConnectionList, (byte)index, out uint list))
            {
                return;
            }

            for (int slot = 0; slot < 4 && index + slot < entries; slot++)
            {
                byte entry = (byte)(list >> (slot * 8));
                if (entry == converter)
                {
                    TryCommand(codec, pin, VerbSetConnectionSelect, (byte)(index + slot), out _);
                    return;
                }
            }
        }
    }

    /// <summary>Opens a node's output amplifier to full gain on both channels, when it has one.</summary>
    private void Unmute(byte codec, byte node, bool hasAmplifier)
    {
        if (!hasAmplifier)
        {
            return;
        }

        ushort payload = AmplifierSetOutput | AmplifierSetLeft | AmplifierSetRight | AmplifierGainMask;
        TryCommandWide(codec, node, VerbSetAmplifierGain, payload, out _);
    }
}
