// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Audio.HdAudio;

/// <summary>
/// The nodes of one codec that carry frames from the link to a jack: the
/// converter that takes a stream and the pin complex that drives it out,
/// with the function group they belong to and whether each carries an
/// output amplifier the driver has to unmute.
/// </summary>
internal readonly struct HdAudioOutputPath
{
    /// <summary>Records a path.</summary>
    /// <param name="codec">The codec's address on the link.</param>
    /// <param name="functionGroup">The audio function group holding both nodes.</param>
    /// <param name="converter">The audio output converter, a DAC.</param>
    /// <param name="pin">The pin complex the converter feeds.</param>
    /// <param name="converterHasAmplifier">Whether the converter has an output amplifier.</param>
    /// <param name="pinHasAmplifier">Whether the pin has an output amplifier.</param>
    internal HdAudioOutputPath(
        byte codec,
        byte functionGroup,
        byte converter,
        byte pin,
        bool converterHasAmplifier,
        bool pinHasAmplifier)
    {
        Codec = codec;
        FunctionGroup = functionGroup;
        Converter = converter;
        Pin = pin;
        ConverterHasAmplifier = converterHasAmplifier;
        PinHasAmplifier = pinHasAmplifier;
    }

    /// <summary>The codec's address on the link.</summary>
    internal byte Codec { get; }

    /// <summary>The audio function group both nodes belong to.</summary>
    internal byte FunctionGroup { get; }

    /// <summary>The audio output converter's node id.</summary>
    internal byte Converter { get; }

    /// <summary>The pin complex's node id.</summary>
    internal byte Pin { get; }

    /// <summary>Whether the converter carries an output amplifier.</summary>
    internal bool ConverterHasAmplifier { get; }

    /// <summary>Whether the pin carries an output amplifier.</summary>
    internal bool PinHasAmplifier { get; }
}
