// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.FileSystem;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// The audio commands: what the driver kit published, a tone through
/// <c>Console.Beep</c>, and a <c>.wav</c> played off a mounted volume. The
/// file is streamed rather than loaded, so a file of any size plays in a
/// fixed amount of memory, and playback runs on a thread of its own, so the
/// shell stays usable while it plays: <c>audio</c> shows how far it has got
/// and <c>stop</c> ends it.
/// </summary>
internal static class AudioCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Audio";

    /// <summary>Label column width of the device listing.</summary>
    private const int LabelWidth = 14;

    /// <summary>Pitch (Hz) the beep command plays when none is given: what <c>Console.Beep()</c> plays.</summary>
    private const int BeepHz = 800;

    /// <summary>Length (ms) the beep command plays when none is given: what <c>Console.Beep()</c> plays.</summary>
    private const int BeepMs = 200;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "audio",
                Usage = "audio",
                Description = "List the audio outputs the driver kit published",
                Execute = static (context, args) => ListDevices(),
            },
            new ShellCommand
            {
                Name = "play",
                Usage = "play <path>",
                Description = "Play a PCM .wav file from a mounted volume, in the background",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => PlayFile(context, args[0]),
            },
            new ShellCommand
            {
                Name = "beep",
                Usage = "beep [hz] [ms]",
                Description = "Play a tone through Console.Beep (default 800 Hz for 200 ms, no file needed)",
                MaxArgs = 2,
                Execute = static (context, args) =>
                {
                    int frequency = BeepHz;
                    int duration = BeepMs;
                    if ((args.Count >= 1 && !args.TryGetInt(0, out frequency))
                        || (args.Count >= 2 && !args.TryGetInt(1, out duration)))
                    {
                        args.PrintUsage();
                        return;
                    }

                    Beep(frequency, duration);
                },
            },
            new ShellCommand
            {
                Name = "stop",
                Usage = "stop",
                Description = "Stop whatever is playing on the audio outputs",
                Execute = static (context, args) => StopPlayback(),
            });
    }

    /// <summary>Prints every published output, what it is programmed for and what it is playing.</summary>
    private static void ListDevices()
    {
        if (!AudioManager.IsEnabled)
        {
            Terminal.Warning("Audio support is compiled out (CosmosEnableAudio=false).");
            return;
        }

        int count = AudioManager.Count;
        if (count == 0)
        {
            Terminal.Warning("No audio output published.");
            Terminal.Hint("QEMU needs an HD Audio controller and a codec:");
            Terminal.Info("  -device intel-hda -device hda-duplex");
            return;
        }

        Terminal.Header($"Audio outputs ({count})");
        for (int i = 0; i < count; i++)
        {
            if (!AudioManager.TryGet(i, out AudioDevice? device))
            {
                continue;
            }

            AudioFormat format = device.Format;
            Terminal.InfoLine("name", device.Name, LabelWidth);
            Terminal.InfoLine("driver", device.DriverName, LabelWidth);
            Terminal.InfoLine("node", device.NodePath ?? "(none)", LabelWidth);
            Terminal.InfoLine("format", $"{device.SampleRate} Hz, {format.Channels} ch, {format.ChannelSize * 8}-bit", LabelWidth);
            Terminal.InfoLine("ring free", $"{device.WritableBytes} bytes", LabelWidth);
            Terminal.InfoLine("running", device.IsRunning ? "yes" : "no", LabelWidth);
            Terminal.InfoLine("playing", DescribePlayback(device), LabelWidth);
            Console.WriteLine();
        }
    }

    /// <summary>How far the player on an output has got, or that it is idle.</summary>
    private static string DescribePlayback(AudioDevice device)
    {
        AudioPlayer? player = device.Player;
        AudioStream? stream = player?.Stream;
        if (stream is null)
        {
            return "(idle)";
        }

        if (stream is not SeekableAudioStream seekable || seekable.Length == 0)
        {
            return "yes";
        }

        uint rate = stream.SampleRate == 0 ? 1 : stream.SampleRate;
        return $"{seekable.Position / rate}s of {seekable.Length / rate}s";
    }

    /// <summary>Opens a <c>.wav</c> on a mounted volume and streams it to the primary output.</summary>
    private static void PlayFile(ShellContext context, string path)
    {
        AudioDevice? device = RequireIdleDevice();
        if (device is null)
        {
            return;
        }

        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);
        if (!VfsManager.TryOpenFile(fullPath, out IVfsFileHandle? file))
        {
            Terminal.Error($"File not found: {fullPath}");
            return;
        }

        bool handedOver = false;
        try
        {
            if (!WaveAudioStream.TryOpen(file, out WaveAudioStream? wave))
            {
                Terminal.Error($"Not a PCM .wav file this reader understands: {fullPath}");
                return;
            }

            WaveHeader header = wave.Header;
            uint seconds = header.SampleRate == 0 ? 0 : wave.Length / header.SampleRate;
            Terminal.Info(
                $"{fullPath}: {header.SampleRate} Hz, {header.Format.Channels} ch, " +
                $"{header.Format.ChannelSize * 8}-bit, {wave.Length} frames (~{seconds}s)");

            if (header.Format != AudioFormat.Stereo16)
            {
                Terminal.Warning($"The output only plays signed 16-bit stereo; this file is {header.Format.Channels} ch.");
                return;
            }

            // The playback reads the file for as long as it runs, so the
            // handle goes with it: it is closed when the thread is done,
            // not when this command returns.
            handedOver = Start(device, wave, file);
            if (handedOver)
            {
                Terminal.Hint("The volume is read as it plays; leave it alone until 'audio' says idle.");
            }
        }
        finally
        {
            if (!handedOver)
            {
                file.Dispose();
            }
        }
    }

    /// <summary>
    /// Plays a tone through <c>Console.Beep</c>, which the kernel plugs onto
    /// the primary output, so the driver can be proven with no volume mounted.
    /// The call holds the shell until the tone has played.
    /// </summary>
    /// <param name="frequency">The pitch in Hz.</param>
    /// <param name="duration">The length in milliseconds.</param>
    private static void Beep(int frequency, int duration)
    {
        AudioDevice? device = RequireIdleDevice();
        if (device is null)
        {
            return;
        }

        Terminal.Info($"A {frequency} Hz tone for {duration} ms on \"{device.Name}\"...");
        try
        {
            // .NET implements this overload on Windows only (CA1416); the
            // kernel's Console plug implements it here.
#pragma warning disable CA1416
            Console.Beep(frequency, duration);
#pragma warning restore CA1416
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Terminal.Error(ex.Message);
            return;
        }

        Terminal.Success("Done.");
    }

    /// <summary>Asks whatever is playing to stop, on every published output.</summary>
    private static void StopPlayback()
    {
        int stopped = 0;
        int count = AudioManager.Count;
        for (int i = 0; i < count; i++)
        {
            if (!AudioManager.TryGet(i, out AudioDevice? device))
            {
                continue;
            }

            AudioPlayer? player = device.Player;
            if (player is null)
            {
                continue;
            }

            player.RequestStop();
            stopped++;
        }

        if (stopped == 0)
        {
            Terminal.Info("Nothing is playing.");
            return;
        }

        Terminal.Success($"Stopped {stopped} playback(s).");
    }

    /// <summary>
    /// Hands the stream to a playback thread, or plays it on this thread when
    /// there is none to put it on, which is a kernel built without the
    /// scheduler.
    /// </summary>
    /// <param name="device">The output to play through.</param>
    /// <param name="stream">The frames to play.</param>
    /// <param name="owned">What the stream reads from, released by whoever ends up owning it.</param>
    /// <returns>True when a thread took the stream, and with it <paramref name="owned"/>.</returns>
    private static bool Start(AudioDevice device, AudioStream stream, IDisposable? owned)
    {
        if (AudioManager.TryStartPlayback(stream, owned, out _))
        {
            Terminal.Success($"Playing in the background on \"{device.Name}\".");
            Terminal.Hint("'audio' shows how far it has got, 'stop' ends it.");
            return true;
        }

        Terminal.Warning("No playback thread available; playing here, which holds the shell.");
        if (!new AudioPlayer(device).Play(stream))
        {
            Terminal.Error($"The output refused {stream.SampleRate} Hz.");
            return false;
        }

        Terminal.Success("Done.");
        return false;
    }

    /// <summary>The primary output, once it is published and not already playing.</summary>
    private static AudioDevice? RequireIdleDevice()
    {
        AudioDevice? device = AudioManager.Primary;
        if (device is null)
        {
            Terminal.Error("No audio output published. Run 'audio' for what QEMU needs.");
            return null;
        }

        if (device.Player is not null)
        {
            Terminal.Error($"\"{device.Name}\" is already playing. Run 'stop' first.");
            return null;
        }

        return device;
    }
}
