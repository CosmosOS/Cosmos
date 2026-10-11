// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Audio;

/// <summary>Where a background <see cref="AudioPlayback"/> has got to.</summary>
public enum AudioPlaybackState : byte
{
    /// <summary>The thread is still feeding the output.</summary>
    Playing,

    /// <summary>The stream was played to its end and the output drained it.</summary>
    Completed,

    /// <summary>
    /// <see cref="AudioPlayback.Stop"/> ended it early, so what the ring
    /// still held was dropped rather than played.
    /// </summary>
    Stopped,

    /// <summary>
    /// The output refused the stream's format, was withdrawn or busy, or
    /// reading the stream threw. Nothing, or only part of the stream, played.
    /// </summary>
    Failed,
}
