// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// What an audio output driver implements and hands to
/// <see cref="DeviceBinding.PublishAudio"/>. The kernel pushes frames in
/// with <see cref="Write"/> and the device drains them on its own clock,
/// the mirror of <see cref="INetworkInterface.Transmit"/>: the driver owns
/// the ring, the caller never blocks on it, and a short write is how the
/// device says it is full. The driver reports the room it frees through
/// <see cref="AudioSink.BufferCompleted"/>. Called by the ring in thread
/// context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IAudioOutput
{
    /// <summary>The name the driver gives the output, for the log and the ring: <c>hda</c>, <c>ac97</c>.</summary>
    string Name { get; }

    /// <summary>The format the device is programmed for.</summary>
    AudioFormat Format { get; }

    /// <summary>Frames per second the device is programmed for.</summary>
    uint SampleRate { get; }

    /// <summary>The formats the device accepts, in the driver's order.</summary>
    ReadOnlySpan<AudioFormat> SupportedFormats { get; }

    /// <summary>True while the device is drawing frames out of the ring.</summary>
    bool IsRunning { get; }

    /// <summary>Bytes <see cref="Write"/> would accept right now. Allocation-free; any context.</summary>
    int WritableBytes { get; }

    /// <summary>
    /// Reprograms the device. Only while stopped; a running device keeps
    /// what it has. Reports through <see cref="AudioSink.FormatChanged"/> on
    /// success.
    /// </summary>
    /// <param name="format">The frame layout wanted.</param>
    /// <param name="sampleRate">Frames per second wanted.</param>
    /// <returns>False when the device refuses the pair; nothing changed.</returns>
    bool TrySetFormat(AudioFormat format, uint sampleRate);

    /// <summary>Starts drawing frames out of the ring. Silence plays while the ring is empty.</summary>
    void Start();

    /// <summary>Stops the stream and drops whatever the ring still holds.</summary>
    void Stop();

    /// <summary>
    /// Copies as much of <paramref name="frames"/> into the ring as fits.
    /// Never blocks and never partially writes a frame.
    /// </summary>
    /// <param name="frames">Interleaved frames in <see cref="Format"/>.</param>
    /// <returns>Bytes taken, a whole number of frames, 0 when the ring is full.</returns>
    int Write(ReadOnlySpan<byte> frames);
}
