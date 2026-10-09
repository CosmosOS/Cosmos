// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Threading;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Timers;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// Pushes an <see cref="AudioStream"/> into an <see cref="AudioDevice"/>
/// until the stream runs out. The device owns the ring and drains it on the
/// codec's clock; the player only keeps it fed, which is why it neither
/// allocates per buffer nor runs from an interrupt.
///
/// <para>Thread context: <see cref="Play"/> blocks for the length of the
/// audio, and claims the output for as long as it runs, so two players
/// cannot interleave frames into one ring.
/// <see cref="AudioPlayback.TryStart"/> runs it on a thread of its own for a
/// caller that must stay responsive; <see cref="RequestStop"/> ends it early
/// and is safe from any context.</para>
/// </summary>
public sealed class AudioPlayer
{
    /// <summary>Frames staged per pass. At 48 kHz stereo 16-bit that is about 21 ms of audio.</summary>
    private const int StagingFrames = 1024;

    /// <summary>Milliseconds waited when the device's ring is full. Shorter than the ring so it never runs dry.</summary>
    private const uint FullRingWaitMs = 5;

    /// <summary>
    /// Consecutive waits on a full ring before the player gives up. A device
    /// that stops draining would otherwise hold the caller for ever, and a
    /// second of no progress is far longer than any ring takes to turn over.
    /// </summary>
    private const int MaxFullRingWaits = 200;

    private readonly AudioDevice _device;

    /// <summary>The stream <see cref="Play"/> is feeding, for a status read from another thread.</summary>
    private AudioStream? _stream;

    private volatile bool _stopRequested;

    /// <summary>Creates a player over one output.</summary>
    /// <param name="device">The output to push frames into.</param>
    public AudioPlayer(AudioDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        _device = device;
    }

    /// <summary>The output this player pushes into.</summary>
    public AudioDevice Device => _device;

    /// <summary>
    /// The stream being played now, null while the player is idle. A
    /// <see cref="SeekableAudioStream"/> here answers how far playback has
    /// got. Any context.
    /// </summary>
    public AudioStream? Stream => Volatile.Read(ref _stream);

    /// <summary>True once <see cref="RequestStop"/> was called, which is for good.</summary>
    public bool StopRequested => _stopRequested;

    /// <summary>
    /// Asks the playback to end early. <see cref="Play"/> stops feeding, drops
    /// what the ring still holds and returns. The request is final: this
    /// player plays nothing more, which is what lets it be made before
    /// <see cref="Play"/> is even reached, as a stop asked for while the
    /// playback thread is still starting up is. Allocation-free and safe from
    /// any context, including an interrupt.
    /// </summary>
    public void RequestStop()
    {
        _stopRequested = true;
    }

    /// <summary>
    /// Takes the output now rather than when <see cref="Play"/> reaches it, so
    /// a caller that hands the playing to a thread knows at once whether it
    /// got the output, and so a stop asked for before that thread runs finds
    /// the player through <see cref="AudioDevice.Player"/>.
    /// </summary>
    /// <returns>False when another player holds the output.</returns>
    internal bool TryReserve()
    {
        return _device.TryClaim(this);
    }

    /// <summary>Gives a reservation back, for a playback that never started.</summary>
    internal void ReleaseReservation()
    {
        _device.Release(this);
    }

    /// <summary>
    /// Programs the device for the stream's format, starts it, feeds it
    /// until the stream is depleted or <see cref="RequestStop"/> is called,
    /// waits for the tail to play out and stops it again.
    /// </summary>
    /// <param name="stream">The frames to play.</param>
    /// <returns>
    /// False when the device is withdrawn, already claimed by another
    /// player, or refuses the stream's format, and when the player was
    /// already asked to stop; nothing was played.
    /// </returns>
    public bool Play(AudioStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // One ring, one writer: a second player would interleave its frames
        // with this one's and both would come out shredded. A player that
        // reserved the output already holds it and claims it again here.
        if (!_device.TryClaim(this))
        {
            return false;
        }

        try
        {
            // Both of these are reachable on a reserved player, whose claim
            // was taken before its thread ran: the output can be withdrawn,
            // and a stop asked for, in between. Refusing inside the claim is
            // what gives the output back; refusing before taking it would
            // strand a reservation and silence the device for good.
            if (_device.IsWithdrawn || _stopRequested)
            {
                return false;
            }

            AudioFormat format = stream.Format;
            if (!_device.TrySetFormat(format, stream.SampleRate))
            {
                return false;
            }

            // The ring is empty before the device starts, so this is its whole
            // capacity, which is how much silence the drain below pushes.
            int capacity = _device.WritableBytes;

            byte[] staging = new byte[StagingFrames * format.FrameSize];
            Volatile.Write(ref _stream, stream);
            _device.Start();

            bool fed;
            try
            {
                fed = Feed(stream, staging);
                Drain(staging, capacity);
            }
            finally
            {
                // Stop drops whatever the ring still holds: the frames a stop
                // request asked to abandon, or the silence a full drain left.
                _device.Stop();
                Volatile.Write(ref _stream, null);
            }

            return fed;
        }
        finally
        {
            _device.Release(this);
        }
    }

    /// <summary>Reads the stream a staging buffer at a time and writes each one fully into the device.</summary>
    /// <param name="stream">The source of frames.</param>
    /// <param name="staging">The scratch buffer, reused for every pass.</param>
    /// <returns>False when the device stopped taking frames, which is a device that stopped playing.</returns>
    private bool Feed(AudioStream stream, byte[] staging)
    {
        while (!stream.Depleted && !_device.IsWithdrawn && !_stopRequested)
        {
            int read = stream.Read(staging);
            if (read <= 0)
            {
                break;
            }

            if (!WriteAll(staging.AsSpan(0, read)) && !_stopRequested && !_device.IsWithdrawn)
            {
                // The ring never drained: the device is not playing.
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Writes every byte, waiting out each full ring, since a device takes
    /// only what fits.
    /// </summary>
    /// <param name="bytes">What to write.</param>
    /// <returns>False when the playback was stopped, the device withdrawn, or the ring stopped draining.</returns>
    private bool WriteAll(ReadOnlySpan<byte> bytes)
    {
        int written = 0;
        int waits = 0;
        while (written < bytes.Length)
        {
            if (_device.IsWithdrawn || _stopRequested)
            {
                return false;
            }

            int took = _device.Write(bytes[written..]);
            if (took == 0)
            {
                if (++waits > MaxFullRingWaits)
                {
                    return false;
                }

                // The ring is full: the device is playing what it has.
                Pause(FullRingWaitMs);
                continue;
            }

            waits = 0;
            written += took;
        }

        return true;
    }

    /// <summary>
    /// Sees the tail of the stream through to the codec by writing one ring
    /// of silence after it.
    ///
    /// <para>A device's ring is cyclic and its engine never stops: left alone
    /// it replays whatever the ring holds. Waiting for the ring to read empty
    /// instead does not work, because the engine's position is reported modulo
    /// the ring, so the instant it has read exactly as far as the writer got
    /// is not something a poll can catch — a poll a few milliseconds late
    /// cannot tell an empty ring from a full one. Writing is what can be
    /// counted on: a device only accepts bytes into space its engine has
    /// already read, so once a whole ring of silence has gone in, every frame
    /// written before it has certainly reached the codec, and what the ring
    /// is playing by then is silence. Stopping there is inaudible.</para>
    /// </summary>
    /// <param name="staging">The scratch buffer, cleared here and used as the silence.</param>
    /// <param name="capacity">Bytes the empty ring accepts, measured before the device started.</param>
    private void Drain(byte[] staging, int capacity)
    {
        if (capacity <= 0)
        {
            return;
        }

        staging.AsSpan().Clear();

        int padded = 0;
        while (padded < capacity)
        {
            int take = capacity - padded < staging.Length ? capacity - padded : staging.Length;
            if (!WriteAll(staging.AsSpan(0, take)))
            {
                return;
            }

            padded += take;
        }
    }

    /// <summary>
    /// Waits out one pass. A thread the scheduler owns gives the CPU up, so
    /// the rest of the kernel runs while the codec plays what it has; the
    /// boot thread, which is the scheduler's idle thread, and a kernel built
    /// without the scheduler wait on the timer device instead, since putting
    /// the idle thread to sleep would take the scheduler's fallback off its
    /// own run queue.
    /// </summary>
    /// <param name="milliseconds">How long to wait.</param>
    private static void Pause(uint milliseconds)
    {
        SchedulerThread? current = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;

        if (current is not null && (current.Flags & SchedulerThreadFlags.IdleThread) == 0)
        {
            SchedulerManager.Sleep(milliseconds);
            return;
        }

        TimerManager.Wait(milliseconds);
    }
}
