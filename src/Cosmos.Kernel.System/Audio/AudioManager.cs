// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// The audio outputs the kit published, and which one is primary. Every
/// output a kit driver publishes is listed by the manager's
/// <see cref="KitAudioConsumer"/> from the kit worker and dropped when it is
/// withdrawn. <see cref="Play"/> is the short way to put a stream through
/// the primary output. Reads answer honestly before <see cref="Initialize"/>
/// ran and when audio is compiled out: 0, null, false.
/// </summary>
public static class AudioManager
{
    private static KitAudioConsumer? s_consumer;

    /// <summary>Whether audio support is compiled into this kernel.</summary>
    public static bool IsEnabled => CosmosFeatures.AudioEnabled;

    /// <summary>How many outputs are published now. Any context.</summary>
    public static int Count => s_consumer?.Count ?? 0;

    /// <summary>The primary output, or null when none is published. Any context.</summary>
    public static AudioDevice? Primary => s_consumer?.Primary;

    /// <summary>Buffer completions the primary output reported so far; a player waits for this to move. Any context.</summary>
    internal static int Completions => s_consumer?.Completions ?? 0;

    /// <summary>
    /// Installs the consumer. Called once during boot from the library
    /// initializer, before the driver stage runs, so the consumer sees every
    /// output a kit driver publishes.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_consumer is not null)
        {
            return;
        }

        s_consumer = new KitAudioConsumer();
        DeviceRegistry.SetConsumer(DeviceKind.Audio, s_consumer);
    }

    /// <summary>The output at <paramref name="index"/> in the manager's list (primary first). Any context.</summary>
    /// <param name="index">Position, from 0 to <see cref="Count"/> exclusive.</param>
    /// <param name="device">The output at that position.</param>
    /// <returns>False when the index is out of range, which before <see cref="Initialize"/> ran is every index.</returns>
    public static bool TryGet(int index, [NotNullWhen(true)] out AudioDevice? device)
    {
        if (s_consumer is null)
        {
            device = null;
            return false;
        }

        return s_consumer.TryGet(index, out device);
    }

    /// <summary>
    /// Plays a stream through the primary output and returns once the
    /// stream is depleted and the device has drained it. Thread context:
    /// the call blocks for the length of the audio, which
    /// <see cref="TryStartPlayback"/> does not.
    /// </summary>
    /// <param name="stream">The frames to play.</param>
    /// <returns>False when no output is published, or the primary refuses the stream's format.</returns>
    public static bool Play(AudioStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        AudioDevice? device = Primary;
        if (device is null)
        {
            return false;
        }

        return new AudioPlayer(device).Play(stream);
    }

    /// <summary>
    /// Starts a thread that plays a stream through the primary output and
    /// returns as soon as it is running, so the caller stays responsive
    /// while the audio plays. <see cref="AudioPlayback.TryStart"/> carries
    /// the rules, including what <paramref name="owned"/> is for.
    /// </summary>
    /// <param name="stream">The frames to play, read from the new thread from here on.</param>
    /// <param name="owned">What the stream reads from, disposed once the thread is done with it, or null.</param>
    /// <param name="playback">The playback, for stopping it and following it.</param>
    /// <returns>
    /// False when no output is published, the primary is withdrawn or already
    /// playing, or there is no scheduler to run the thread on; nothing was
    /// started. <see cref="Play"/> plays the stream on the calling thread
    /// instead.
    /// </returns>
    public static bool TryStartPlayback(AudioStream stream, IDisposable? owned, [NotNullWhen(true)] out AudioPlayback? playback)
    {
        ArgumentNullException.ThrowIfNull(stream);

        playback = null;
        AudioDevice? device = Primary;
        return device is not null && AudioPlayback.TryStart(device, stream, owned, out playback);
    }

    /// <summary>Throws when audio support is compiled out. Guards the one action here, <see cref="Initialize"/>; the reads answer honestly instead.</summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Audio support is disabled. Set CosmosEnableAudio=true in your csproj to enable it.");
        }
    }
}
