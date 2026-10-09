// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Audio;

/// <summary>A finite audio stream whose position can be read and moved.</summary>
public abstract class SeekableAudioStream : AudioStream
{
    /// <summary>The position in frames, from 0 to <see cref="Length"/>.</summary>
    public abstract uint Position { get; set; }

    /// <summary>The length of the stream in frames.</summary>
    public abstract uint Length { get; }

    /// <summary>Moves back to the first frame.</summary>
    public void Rewind()
    {
        Position = 0;
    }
}
