// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// A square wave at one frequency for a set time, in signed 16-bit stereo at
/// 48 kHz, the layout and rate every output takes. The frames are worked out
/// as they are read, so a long tone costs no memory. What the
/// <c>Console.Beep</c> plug plays.
/// </summary>
internal sealed class ToneAudioStream : AudioStream
{
    /// <summary>Frames per second the tone is generated at.</summary>
    internal const uint ToneSampleRate = 48000;

    /// <summary>Amplitude of the wave, a quarter of full scale so it is not harsh.</summary>
    internal const short Amplitude = 8192;

    private const long MillisecondsPerSecond = 1000;

    private readonly int _frequency;
    private long _position;

    /// <summary>Frames the tone lasts.</summary>
    internal long FrameCount { get; }

    /// <inheritdoc/>
    public override AudioFormat Format => AudioFormat.Stereo16;

    /// <inheritdoc/>
    public override uint SampleRate => ToneSampleRate;

    /// <inheritdoc/>
    public override bool Depleted => _position >= FrameCount;

    /// <summary>Creates a tone.</summary>
    /// <param name="frequency">The pitch in Hz.</param>
    /// <param name="milliseconds">How long the tone lasts.</param>
    /// <exception cref="ArgumentOutOfRangeException">Either value is zero or negative.</exception>
    internal ToneAudioStream(int frequency, int milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(milliseconds);

        _frequency = frequency;
        FrameCount = milliseconds * (long)ToneSampleRate / MillisecondsPerSecond;
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> destination)
    {
        int frameSize = AudioFormat.Stereo16.FrameSize;
        long left = FrameCount - _position;
        long wanted = destination.Length / frameSize;
        int frames = (int)(wanted < left ? wanted : left);

        for (int i = 0; i < frames; i++)
        {
            short sample = IsHigh(_position + i) ? Amplitude : (short)-Amplitude;
            Span<byte> frame = destination.Slice(i * frameSize, frameSize);
            BitConverter.TryWriteBytes(frame, sample);
            BitConverter.TryWriteBytes(frame[sizeof(short)..], sample);
        }

        _position += frames;
        return frames * frameSize;
    }

    /// <summary>
    /// Whether a frame falls in the high half of its period. The half periods
    /// are counted from the first frame rather than stepped a whole number of
    /// frames at a time, so a frequency that does not divide the rate keeps its
    /// pitch. A second holds an even number of half periods, so the wave
    /// repeats every second and the frame is reduced to its place in the
    /// second first, which keeps the product in range for any frequency.
    /// </summary>
    /// <param name="frame">The frame, counted from the start of the tone.</param>
    private bool IsHigh(long frame)
    {
        long inSecond = frame % ToneSampleRate;
        return inSecond * 2 * _frequency / ToneSampleRate % 2 == 0;
    }
}
