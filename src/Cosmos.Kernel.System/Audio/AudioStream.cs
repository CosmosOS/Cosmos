// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// A source of interleaved audio frames. The base class carries no length
/// and no position on purpose: a stream may be endless, as a mixer or a
/// capture device is. <see cref="SeekableAudioStream"/> adds both for the
/// finite ones.
/// </summary>
public abstract class AudioStream
{
    /// <summary>The layout of the frames <see cref="Read"/> writes.</summary>
    public abstract AudioFormat Format { get; }

    /// <summary>Frames per second the samples were recorded at.</summary>
    public abstract uint SampleRate { get; }

    /// <summary>True once the stream has no further frames to give.</summary>
    public abstract bool Depleted { get; }

    /// <summary>
    /// Fills <paramref name="destination"/> with the next frames and
    /// advances by what it wrote. A short read means the stream ran out;
    /// the caller pads the rest with silence.
    /// </summary>
    /// <param name="destination">Where to write, in <see cref="Format"/>.</param>
    /// <returns>Bytes written, a whole number of frames.</returns>
    public abstract int Read(Span<byte> destination);
}
