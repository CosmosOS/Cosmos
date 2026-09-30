// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Threading;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// One published audio output as the ring sees it: the name the driver gave
/// it, which driver published it, where it sits in the device tree, and its
/// format read live from the driver. Created by
/// <see cref="KitAudioConsumer"/> when the kit publishes an output and kept
/// in <see cref="AudioManager"/>'s list until it is withdrawn. Once
/// withdrawn every call is a no-op and the format reads empty; the identity
/// members are copied at creation, so they stay readable.
/// </summary>
public sealed class AudioDevice
{
    private readonly PublishedDevice _published;
    private readonly IAudioOutput _output;

    /// <summary>1 while a player owns the output. Claimed with an int rather than a reference because only the integer compare-exchange is atomic on this runtime.</summary>
    private int _claimed;

    /// <summary>The player that won the claim, written after it and cleared before it is dropped.</summary>
    private AudioPlayer? _player;

    /// <summary>Wraps a published output. Thread context, from <see cref="KitAudioConsumer.OnPublished"/>.</summary>
    /// <param name="published">The published device, whose contract object is an <see cref="IAudioOutput"/>.</param>
    internal AudioDevice(PublishedDevice published)
    {
        _published = published;
        _output = (IAudioOutput)published.Device;
        Name = published.Name;
        DeviceBinding? binding = published.Binding;
        DriverName = binding is null ? "firmware" : binding.Driver.Name;
        NodePath = binding?.Node.Path;
    }

    /// <summary>The driver's name for the output: <c>hda</c>.</summary>
    public string Name { get; }

    /// <summary>The class name of the driver that published the output.</summary>
    public string DriverName { get; }

    /// <summary>The path of the device tree node the publishing driver bound.</summary>
    public string? NodePath { get; }

    /// <summary>True once the kit withdrew the output; every call below is then a no-op.</summary>
    public bool IsWithdrawn => _published.IsWithdrawn;

    /// <summary>The frame layout the device is programmed for; empty once withdrawn.</summary>
    public AudioFormat Format => _published.IsWithdrawn ? default : _output.Format;

    /// <summary>Frames per second the device is programmed for; 0 once withdrawn.</summary>
    public uint SampleRate => _published.IsWithdrawn ? 0 : _output.SampleRate;

    /// <summary>True while the device is drawing frames out of its ring.</summary>
    public bool IsRunning => !_published.IsWithdrawn && _output.IsRunning;

    /// <summary>Bytes <see cref="Write"/> would accept right now; 0 once withdrawn.</summary>
    public int WritableBytes => _published.IsWithdrawn ? 0 : _output.WritableBytes;

    /// <summary>
    /// The player feeding this output now, or null when it is idle. What a
    /// caller stops through <see cref="AudioPlayer.RequestStop"/> and reads
    /// progress from. Any context; the answer is a snapshot, and it reads
    /// null for the moment between a claim and the claiming player being
    /// recorded.
    /// </summary>
    public AudioPlayer? Player => Volatile.Read(ref _player);

    /// <summary>The published contract, for a player that drives the device directly.</summary>
    internal IAudioOutput Output => _output;

    /// <summary>The kit's record, for the consumer's identity comparisons.</summary>
    internal PublishedDevice Published => _published;

    /// <summary>Whether the device accepts this layout and rate.</summary>
    /// <param name="format">The frame layout wanted.</param>
    /// <param name="sampleRate">Frames per second wanted.</param>
    /// <returns>False when the device refuses the pair or is withdrawn; nothing changed.</returns>
    public bool TrySetFormat(AudioFormat format, uint sampleRate) =>
        !_published.IsWithdrawn && _output.TrySetFormat(format, sampleRate);

    /// <summary>Starts the stream. Silence plays while the ring is empty.</summary>
    public void Start()
    {
        if (!_published.IsWithdrawn)
        {
            _output.Start();
        }
    }

    /// <summary>Stops the stream and drops whatever the ring still holds.</summary>
    public void Stop()
    {
        if (!_published.IsWithdrawn)
        {
            _output.Stop();
        }
    }

    /// <summary>
    /// Takes the output for one player, so a second one cannot interleave
    /// frames into the same ring. Any context.
    /// </summary>
    /// <param name="player">The player asking for the output.</param>
    /// <returns>False when another player holds it; it keeps the output.</returns>
    internal bool TryClaim(AudioPlayer player)
    {
        // A player that reserved the output before handing the playing to a
        // thread claims it again when that thread reaches Play.
        if (ReferenceEquals(Volatile.Read(ref _player), player))
        {
            return true;
        }

        if (Interlocked.CompareExchange(ref _claimed, 1, 0) != 0)
        {
            return false;
        }

        Volatile.Write(ref _player, player);
        return true;
    }

    /// <summary>
    /// Gives the output back. Only the player that holds it may release it,
    /// so a late release from a player that already lost it cannot free a
    /// claim someone else has taken since.
    /// </summary>
    /// <param name="player">The player that claimed the output.</param>
    internal void Release(AudioPlayer player)
    {
        if (!ReferenceEquals(Volatile.Read(ref _player), player))
        {
            return;
        }

        Volatile.Write(ref _player, null);
        Volatile.Write(ref _claimed, 0);
    }

    /// <summary>Copies as much of <paramref name="frames"/> into the device's ring as fits.</summary>
    /// <param name="frames">Interleaved frames in <see cref="Format"/>.</param>
    /// <returns>Bytes taken, 0 when the ring is full or the device is withdrawn.</returns>
    public int Write(ReadOnlySpan<byte> frames) =>
        _published.IsWithdrawn ? 0 : _output.Write(frames);
}
