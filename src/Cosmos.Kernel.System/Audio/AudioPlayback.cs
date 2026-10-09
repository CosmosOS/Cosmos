// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// An <see cref="AudioPlayer"/> running on a thread of its own, so the caller
/// gets on with its work while the audio plays. The thread wakes a few times
/// per ring to top the device up and sleeps in between, which is why a kernel
/// keeps its console, its timers and its drivers while a file plays.
///
/// <para>Needs the scheduler running: without it there is no second thread to
/// put this on and <see cref="TryStart"/> says so, leaving the caller to play
/// the stream inline through <see cref="AudioPlayer.Play"/>.</para>
///
/// <para>The stream is read from the playback thread from here on. A stream
/// over a filesystem is therefore read while the rest of the kernel runs,
/// and the filesystem layer has no locking of its own: a caller that streams
/// off a volume should leave that volume alone until the playback ends.</para>
/// </summary>
public sealed class AudioPlayback
{
    private readonly AudioPlayer _player;
    private readonly AudioStream _stream;

    /// <summary>What the stream reads from, released once the thread is done with it.</summary>
    private readonly IDisposable? _owned;

    private volatile AudioPlaybackState _state;

    private AudioPlayback(AudioDevice device, AudioStream stream, IDisposable? owned)
    {
        _player = new AudioPlayer(device);
        _stream = stream;
        _owned = owned;
        _state = AudioPlaybackState.Playing;
    }

    /// <summary>The output being fed.</summary>
    public AudioDevice Device => _player.Device;

    /// <summary>The player the thread runs, which also carries how far it has got.</summary>
    public AudioPlayer Player => _player;

    /// <summary>The stream being played.</summary>
    public AudioStream Stream => _stream;

    /// <summary>Where the playback has got to. Any context.</summary>
    public AudioPlaybackState State => _state;

    /// <summary>True while the thread is still feeding the output. Any context.</summary>
    public bool IsPlaying => _state == AudioPlaybackState.Playing;

    /// <summary>
    /// Ends the playback early. The thread stops feeding, the output drops
    /// what it still holds, and <see cref="State"/> becomes
    /// <see cref="AudioPlaybackState.Stopped"/> shortly after. Returns at
    /// once rather than waiting for that; allocation-free and safe from any
    /// context.
    /// </summary>
    public void Stop()
    {
        _player.RequestStop();
    }

    /// <summary>
    /// Starts a thread that plays <paramref name="stream"/> through
    /// <paramref name="device"/> and returns as soon as it is running.
    /// </summary>
    /// <param name="device">The output to play through.</param>
    /// <param name="stream">The frames to play, read from the new thread from here on.</param>
    /// <param name="owned">
    /// What the stream reads from, disposed once the thread is done with it,
    /// so a caller can hand over a file handle and forget it. Null when the
    /// stream needs nothing released, and left untouched when this returns
    /// false.
    /// </param>
    /// <param name="playback">The playback, for stopping it and following it.</param>
    /// <returns>
    /// False when the output is withdrawn or already playing, or when there
    /// is no scheduler to run the thread on; nothing was started and nothing
    /// was disposed. True means the output is this playback's from here until
    /// it ends.
    /// </returns>
    public static bool TryStart(
        AudioDevice device,
        AudioStream stream,
        IDisposable? owned,
        [NotNullWhen(true)] out AudioPlayback? playback)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(stream);

        playback = null;

        if (device.IsWithdrawn)
        {
            return false;
        }

        if (!CosmosFeatures.SchedulerEnabled || !SchedulerManager.IsRunning)
        {
            return false;
        }

        AudioPlayback started = new(device, stream, owned);

        // The output is taken here rather than on the new thread, so that from
        // the moment this returns true the playback can be stopped and a
        // second one is refused. Otherwise both would race the thread's own
        // first instruction.
        if (!started._player.TryReserve())
        {
            return false;
        }

        // The thread only runs once the scheduler switches to it, and a timer
        // that never ticks would never let it: KernelThread gives up after a
        // few quanta and makes sure the thread never runs later.
        if (!KernelThread.TryStart(started.Run))
        {
            started._player.ReleaseReservation();
            return false;
        }

        playback = started;
        return true;
    }

    /// <summary>
    /// The thread's body. Nothing above a kernel thread catches, so neither
    /// the playback nor the release of what it owned may let an exception
    /// escape: a file that fails to read mid-song would take the kernel down
    /// with it.
    /// </summary>
    private void Run()
    {
        AudioPlaybackState state = AudioPlaybackState.Failed;

        try
        {
            bool played = _player.Play(_stream);
            state = _player.StopRequested
                ? AudioPlaybackState.Stopped
                : played ? AudioPlaybackState.Completed : AudioPlaybackState.Failed;
        }
        catch (Exception ex)
        {
            Report("playback failed: ", ex);
        }

        try
        {
            _owned?.Dispose();
        }
        catch (Exception ex)
        {
            Report("releasing the source failed: ", ex);
        }

        _state = state;
    }

    /// <summary>Puts a thread's failure on the log, the only place it can be reported from here.</summary>
    /// <param name="what">What was being done.</param>
    /// <param name="ex">What went wrong.</param>
    private static void Report(string what, Exception ex)
    {
        Serial.WriteString("[Audio] ");
        Serial.WriteString(what);
        Serial.WriteString(ex.Message);
        Serial.WriteString("\n");
    }
}
