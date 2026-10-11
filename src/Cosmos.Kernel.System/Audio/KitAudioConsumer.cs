// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// The audio manager's consumer of the driver kit: every output the kit
/// publishes becomes an <see cref="AudioDevice"/> in the manager's list and
/// leaves it when withdrawn. The list is kept in node-path order, so the
/// primary output is stable whichever driver probes first. Installed by
/// <see cref="AudioManager.Initialize"/>, before the driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run in
/// thread context on the kit worker, never concurrently, which is what
/// makes the copy-on-write array safe without a lock;
/// <see cref="Primary"/> and <see cref="Count"/> are volatile reads of the
/// current array. <see cref="OnBufferCompleted"/> runs in the sink caller's
/// context, an interrupt included, and only bumps a counter the player
/// polls, so it allocates nothing and takes no lock.
/// </para>
/// </summary>
internal sealed class KitAudioConsumer : AudioConsumer
{
    private const string Prefix = "[Audio] ";

    /// <summary>The outputs in primary order; replaced on every change, never changed in place.</summary>
    private AudioDevice[] _devices = [];

    /// <summary>Buffer completions seen since boot, for a player waiting on room.</summary>
    private int _completions;

    /// <summary>How many outputs are published now. Any context.</summary>
    internal int Count => Volatile.Read(ref _devices).Length;

    /// <summary>The primary output, or null when none is published. Any context.</summary>
    internal AudioDevice? Primary
    {
        get
        {
            AudioDevice[] devices = Volatile.Read(ref _devices);
            return devices.Length == 0 ? null : devices[0];
        }
    }

    /// <summary>Buffer completions reported so far; a player waits for this to move. Any context.</summary>
    internal int Completions => Volatile.Read(ref _completions);

    /// <summary>The output at <paramref name="index"/> in primary order. Any context.</summary>
    /// <param name="index">Position, from 0 to <see cref="Count"/> exclusive.</param>
    /// <param name="device">The output at that position.</param>
    /// <returns>False when the index is out of range.</returns>
    internal bool TryGet(int index, [NotNullWhen(true)] out AudioDevice? device)
    {
        AudioDevice[] devices = Volatile.Read(ref _devices);
        if (index < 0 || index >= devices.Length)
        {
            device = null;
            return false;
        }

        device = devices[index];
        return true;
    }

    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device)
    {
        AudioDevice[] current = _devices;
        AudioDevice[] devices = new AudioDevice[current.Length + 1];
        Array.Copy(current, devices, current.Length);
        devices[current.Length] = new AudioDevice(device);
        Sort(devices);
        Volatile.Write(ref _devices, devices);

        AudioDevice published = devices[0];
        Serial.WriteString($"{Prefix}primary: {published.DriverName} \"{published.Name}\"\n");
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        AudioDevice[] current = _devices;
        AudioDevice? found = Find(current, device);
        if (found is null)
        {
            return;
        }

        AudioDevice[] devices = new AudioDevice[current.Length - 1];
        int kept = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (!ReferenceEquals(current[i], found))
            {
                devices[kept++] = current[i];
            }
        }

        Volatile.Write(ref _devices, devices);
        Serial.WriteString($"{Prefix}withdrawn: \"{found.Name}\"\n");
    }

    /// <inheritdoc/>
    public override void OnBufferCompleted(PublishedDevice device)
    {
        Interlocked.Increment(ref _completions);
    }

    /// <inheritdoc/>
    public override void OnFormatChanged(PublishedDevice device)
    {
        AudioDevice? found = Find(_devices, device);
        if (found is null)
        {
            return;
        }

        Serial.WriteString(Prefix);
        Serial.WriteString("format changed: \"");
        Serial.WriteString(found.Name);
        Serial.WriteString("\" now ");
        Serial.WriteNumber(found.SampleRate);
        Serial.WriteString(" Hz, ");
        Serial.WriteNumber(found.Format.Channels);
        Serial.WriteString(" ch, ");
        Serial.WriteNumber((uint)found.Format.ChannelSize * 8);
        Serial.WriteString("-bit\n");
    }

    /// <summary>A stable insertion sort by node path: equal entries keep their publication order.</summary>
    private static void Sort(AudioDevice[] devices)
    {
        for (int i = 1; i < devices.Length; i++)
        {
            AudioDevice device = devices[i];
            int j = i - 1;
            while (j >= 0 && string.CompareOrdinal(devices[j].NodePath, device.NodePath) > 0)
            {
                devices[j + 1] = devices[j];
                j--;
            }

            devices[j + 1] = device;
        }
    }

    /// <summary>The output of a published device, by reference: a scan of a copy-on-write array, allocation-free. Any context.</summary>
    private static AudioDevice? Find(AudioDevice[] devices, PublishedDevice device)
    {
        for (int i = 0; i < devices.Length; i++)
        {
            if (ReferenceEquals(devices[i].Published, device))
            {
                return devices[i];
            }
        }

        return null;
    }
}
