// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using Cosmos.Kernel.Drivers.Pci.Audio.HdAudio;
using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timers;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Audio;

/// <summary>
/// The audio stack end to end: the Intel HD Audio driver over the driver kit,
/// the output it publishes, the audio manager that lists it, and the player
/// and background playback that feed it. On the hda cell QEMU's intel-hda
/// plays into the <c>none</c> backend, which drains the stream at its rate;
/// on bare there is no controller, and the manager must say so.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total of TR.Run and TR.Skip calls in <see cref="BeforeRun"/>.</summary>
    private const ushort ExpectedTestCount = 22;

    private const string PciBusName = "pci";

    /// <summary>End of a PCI function node's description for an HD Audio controller: class 04, subclass 03, interface 00.</summary>
    private const string HdAudioClassSuffix = "class 04.03.00";

    private const string DriverName = "HdAudioDriver";

    /// <summary>The name every HD Audio output is published under.</summary>
    private const string OutputName = "hda";

    private const uint DefaultSampleRate = 48000;

    private const string NoControllerReason = "no HD Audio controller on this cell, needs the hda profile";

    /// <summary>Interval between two polls of a condition another thread or the device changes.</summary>
    private const uint PollIntervalMs = 5;

    /// <summary>
    /// Bound on any one wait for the device or the playback thread, far above
    /// what each takes. Measured on the <see cref="Stopwatch"/>, which runs on
    /// host time, never by counting sleeps, which a late tick stretches: a
    /// test makes at most three waits, and all of them together stay inside
    /// the engine's 10 s stall window.
    /// </summary>
    private const long WaitTimeoutMs = 2000;

    private const long MillisecondsPerSecond = 1000;

    /// <summary>How long a stream without a line runs while the test checks that no completion is reported: several periods.</summary>
    private const uint NoLineWindowMs = 150;

    /// <summary>Length of the stream the playback tests run to completion.</summary>
    private const int ShortStreamMs = 150;

    /// <summary>Length of the stream the stop test ends early; long enough that it cannot end on its own first.</summary>
    private const int LongStreamMs = 3000;

    /// <summary>Length of the tone the Console.Beep tests play.</summary>
    private const int BeepMs = 300;

    /// <summary>Length of <c>Console.Beep()</c>'s tone, the one Windows plays.</summary>
    private const int DefaultBeepMs = 200;

    /// <summary>
    /// Bound on a beep that has nothing to play on: it returns at once, so
    /// this is far below the tone it was asked for.
    /// </summary>
    private const long RefusedBeepBoundMs = 500;

    /// <summary>Length of the tone asked for while there is nothing to play it on, longer than <see cref="RefusedBeepBoundMs"/>.</summary>
    private const int RefusedBeepMs = 1000;

    /// <summary>Bytes written per call while the ring is filled.</summary>
    private const int FillChunkBytes = 4096;

    /// <summary>Bound on the writes that fill the ring, each <see cref="FillChunkBytes"/> long.</summary>
    private const int MaxFillWrites = 64;

    private const int ToneHz = 440;
    private const short ToneAmplitude = 4000;

    // Which cell this is, derived in BeforeRun from the device tree and never
    // from the profile name or from which driver bound: a driver that failed
    // to bind must fail the suite, not skip it.
    private static string? s_controllerPath;

    private static bool IsHdaCell => s_controllerPath is not null;

    protected override void BeforeRun()
    {
        Log.WriteString("[Audio Tests] Starting test suite\n");
        TR.Start("Audio Tests", expectedTests: ExpectedTestCount);

        s_controllerPath = FindControllerPath();
        Log.WriteString("[Audio Tests] cell: " + (IsHdaCell ? "hda at " + s_controllerPath : "bare") + "\n");

        // ==================== Manager ====================
        TR.Run("Feature_Enabled", TestFeatureEnabled);
        TR.Run("Manager_CountMatchesCell", TestManagerCountMatchesCell);
        TR.RunIf(!IsHdaCell, "Manager_PlayWithoutOutputRefused", TestPlayWithoutOutputRefused, "an output is published on this cell");

        // ==================== Driver ====================
        TR.RunIf(IsHdaCell, "Driver_BoundByHdAudioDriver", TestDriverBound, NoControllerReason);
        TR.RunIf(IsHdaCell, "Driver_OutputListedInDiagnostics", TestOutputListedInDiagnostics, NoControllerReason);

        // ==================== Output ====================
        TR.RunIf(IsHdaCell, "Output_IdentityMatchesNode", TestOutputIdentity, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_DefaultFormat", TestOutputDefaultFormat, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_TrySetFormat_AcceptsDerivableRates", TestTrySetFormatAcceptsRates, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_TrySetFormat_RefusesOtherLayouts", TestTrySetFormatRefusesLayouts, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_WritableWhileStopped", TestWritableWhileStopped, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_EngineDrainsRing", TestEngineDrainsRing, NoControllerReason);
        TR.RunIf(IsHdaCell, "Output_CompletionsFollowLine", TestCompletionsFollowLine, NoControllerReason);

        // ==================== Player ====================
        TR.RunIf(IsHdaCell, "Player_PlaysStreamToEnd", TestPlayerPlaysToEnd, NoControllerReason);
        TR.RunIf(IsHdaCell, "Player_RefusesUnsupportedFormat", TestPlayerRefusesFormat, NoControllerReason);
        TR.RunIf(IsHdaCell, "Playback_CompletesOnThread", TestPlaybackCompletes, NoControllerReason);
        TR.RunIf(IsHdaCell, "Playback_StopEndsEarly", TestPlaybackStop, NoControllerReason);
        TR.RunIf(IsHdaCell, "Wave_FromMemoryPlays", TestWaveFromMemoryPlays, NoControllerReason);

        // ==================== Console.Beep ====================
        TR.Run("Beep_RejectsOutOfRange", TestBeepRejectsOutOfRange);
        TR.RunIf(IsHdaCell, "Beep_PlaysForItsDuration", TestBeepPlaysForItsDuration, NoControllerReason);
        TR.RunIf(IsHdaCell, "Beep_DefaultPlaysWindowsTone", TestBeepDefault, NoControllerReason);
        TR.RunIf(IsHdaCell, "Beep_WhileOutputBusyReturnsAtOnce", TestBeepWhileBusy, NoControllerReason);
        TR.RunIf(!IsHdaCell, "Beep_WithoutOutputReturnsAtOnce", TestBeepWithoutOutput, "an output is published on this cell");

        Log.WriteString("[Audio Tests] All tests completed\n");
        TR.Finish();
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Sys.Power.Halt();
    }

    // ==================== Manager ====================

    // CosmosEnableAudio defaults on and the hda and bare cells both enable
    // PCI, which the switch follows off.
    private static void TestFeatureEnabled()
    {
        Assert.True(KernelFeatures.Audio, "the audio feature should be on");
        Assert.True(AudioManager.IsEnabled, "the audio manager should be compiled in");
    }

    // One output on the hda cell, the primary at index 0; none on bare, where
    // every index reads empty.
    private static void TestManagerCountMatchesCell()
    {
        if (!IsHdaCell)
        {
            Assert.Equal(0, AudioManager.Count, "bare should publish no output");
            Assert.Null(AudioManager.Primary, "bare should have no primary output");
            Assert.False(AudioManager.TryGet(0, out _), "index 0 should read empty on bare");
            return;
        }

        Assert.Equal(1, AudioManager.Count, "the hda cell should publish one output");
        AudioDevice? primary = AudioManager.Primary;
        Assert.NotNull(primary, "the hda cell should have a primary output");
        Assert.True(AudioManager.TryGet(0, out AudioDevice? first), "index 0 should hold the primary");
        Assert.True(ReferenceEquals(primary, first), "index 0 should be the primary");
        Assert.False(AudioManager.TryGet(1, out _), "index 1 should be past the end");
        Assert.False(AudioManager.TryGet(-1, out _), "a negative index should read empty");
    }

    // With no output both ways of playing refuse, and nothing is started.
    private static void TestPlayWithoutOutputRefused()
    {
        Assert.False(AudioManager.Play(Tone(ShortStreamMs, DefaultSampleRate)), "Play should refuse with no output");
        Assert.False(
            AudioManager.TryStartPlayback(Tone(ShortStreamMs, DefaultSampleRate), null, out AudioPlayback? playback),
            "TryStartPlayback should refuse with no output");
        Assert.Null(playback, "a refused playback should hand back nothing");
    }

    // ==================== Driver ====================

    // The controller's node is bound by HdAudioDriver, which published one
    // device and recorded no fault.
    private static void TestDriverBound()
    {
        Assert.True(TryGetControllerNode(out DeviceNodeInfo node), "the controller's node should still be in the tree");
        Assert.True(node.State == DeviceNodeState.Bound, "the controller should be bound");
        Assert.Equal(DriverName, node.DriverName ?? string.Empty, "the controller should be bound by " + DriverName);
        Assert.Equal(1, node.PublishedDeviceCount, "the driver should publish one output");
        Assert.Equal(0, node.FaultCount, "the driver should record no fault: " + (node.LastFault ?? string.Empty));
    }

    // The kit lists the output on the controller's node, consumed by the
    // audio manager and not withdrawn.
    private static void TestOutputListedInDiagnostics()
    {
        int index = FindOutputDeviceIndex();
        Assert.True(index >= 0, "DriverDiagnostics should list the output on the controller's node");
        if (index < 0 || !DriverDiagnostics.TryGetDevice(index, out PublishedDeviceInfo info))
        {
            return;
        }

        Assert.Equal(DriverName, info.DriverName ?? string.Empty, "the output should be published by " + DriverName);
        Assert.True(info.IsConsumed, "the audio manager should consume the output");
        Assert.False(info.IsWithdrawn, "the output should not be withdrawn");
    }

    // ==================== Output ====================

    // The ring's view of the output names the driver and the node it bound.
    private static void TestOutputIdentity()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        Assert.Equal(OutputName, device.Name, "the output's name");
        Assert.Equal(DriverName, device.DriverName, "the output's driver");
        Assert.Equal(s_controllerPath ?? string.Empty, device.NodePath ?? string.Empty, "the output's node");
        Assert.False(device.IsWithdrawn, "the output should not be withdrawn");
    }

    // A freshly bound controller is programmed for signed 16-bit stereo at
    // 48 kHz, the one layout it lists, and is idle.
    private static void TestOutputDefaultFormat()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        Assert.Equal(AudioFormat.Stereo16, device.Format, "the default format");
        Assert.Equal(DefaultSampleRate, device.SampleRate, "the default rate");
        Assert.False(device.IsRunning, "the output should be idle");
        Assert.Null(device.Player, "no player should hold the output");

        ReadOnlySpan<AudioFormat> supported = device.Output.SupportedFormats;
        Assert.Equal(1, supported.Length, "the driver lists one layout");
        if (supported.Length == 1)
        {
            Assert.Equal(AudioFormat.Stereo16, supported[0], "the listed layout");
        }
    }

    // Every rate the link derives from 48 kHz or 44.1 kHz is accepted for the
    // one layout, and the output reports the rate it was given.
    private static void TestTrySetFormatAcceptsRates()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        uint[] rates = [44100, 96000, 22050, 8000, 192000];
        for (int i = 0; i < rates.Length; i++)
        {
            Assert.True(device.TrySetFormat(AudioFormat.Stereo16, rates[i]), "the output should accept " + rates[i] + " Hz");
            Assert.Equal(rates[i], device.SampleRate, "the output should report " + rates[i] + " Hz");
        }

        Assert.True(device.TrySetFormat(AudioFormat.Stereo16, DefaultSampleRate), "the output should go back to 48 kHz");
        Assert.Equal(DefaultSampleRate, device.SampleRate, "the output should report 48 kHz again");
    }

    // A layout other than signed 16-bit stereo, or a rate the link cannot
    // derive, is refused, and the output keeps what it had.
    private static void TestTrySetFormatRefusesLayouts()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        AudioFormat[] refused =
        [
            new AudioFormat(AudioBitDepth.Bits16, 1, true),
            new AudioFormat(AudioBitDepth.Bits16, 2, false),
            new AudioFormat(AudioBitDepth.Bits8, 2, false),
            new AudioFormat(AudioBitDepth.Bits24, 2, true),
            new AudioFormat(AudioBitDepth.Bits32, 2, true),
        ];
        for (int i = 0; i < refused.Length; i++)
        {
            Assert.False(device.TrySetFormat(refused[i], DefaultSampleRate), "layout " + i + " should be refused");
        }

        Assert.False(device.TrySetFormat(AudioFormat.Stereo16, 12345), "a rate the link cannot derive should be refused");
        Assert.False(device.TrySetFormat(AudioFormat.Stereo16, 0), "a zero rate should be refused");
        Assert.Equal(AudioFormat.Stereo16, device.Format, "the format should be unchanged");
        Assert.Equal(DefaultSampleRate, device.SampleRate, "the rate should be unchanged");
    }

    // A stopped output offers room for whole frames: the ring less the guard.
    private static void TestWritableWhileStopped()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        int writable = device.WritableBytes;
        Assert.True(writable > 0, "a stopped output should offer room");
        Assert.Equal(0, writable % device.Format.FrameSize, "the room should be whole frames");
    }

    // Once started, the ring fills until a write is short, the format is
    // locked, and the room comes back as the stream engine reads: the
    // engine's own position, not an interrupt, is what frees it.
    private static void TestEngineDrainsRing()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        byte[] chunk = ToneBytes(FillChunkBytes / device.Format.FrameSize);
        device.Start();
        try
        {
            Assert.True(device.IsRunning, "the output should run once started");
            Assert.False(device.TrySetFormat(AudioFormat.Stereo16, 44100), "a running output should keep its format");

            bool filled = false;
            int written = 0;
            for (int i = 0; i < MaxFillWrites && !filled; i++)
            {
                int took = device.Write(chunk);
                Assert.Equal(0, took % device.Format.FrameSize, "a write should take whole frames");
                written += took;
                filled = took < chunk.Length;
            }

            Assert.True(filled, "a write should come back short once the ring is full");
            Assert.True(written > 0, "the ring should take frames");

            // The room grows only as the engine reads, until it laps the
            // writer a ring later, long after the first poll that sees it.
            int full = device.WritableBytes;
            bool drained = WaitUntil(() => device.WritableBytes > full);
            Assert.True(drained, "the engine should read the ring and free room");
        }
        finally
        {
            device.Stop();
        }

        Assert.False(device.IsRunning, "the output should be idle once stopped");
        Assert.Equal(DefaultSampleRate, device.SampleRate, "the rate should be unchanged");
    }

    // With the function's line connected, each period the engine finishes
    // raises the stream's completion interrupt and the driver reports it
    // through its sink. Without one, nothing is reported, and the stream
    // still runs: the writer reads the engine's position instead.
    private static void TestCompletionsFollowLine()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        if (device.Output is not HdAudioState state)
        {
            Assert.Fail("the primary output should be the HD Audio driver's");
            return;
        }

        Log.WriteString("[Audio Tests] completion interrupt: " + (state.HasLine ? "line connected" : "no line") + "\n");
        int before = AudioManager.Completions;
        device.Start();
        try
        {
            if (state.HasLine)
            {
                Assert.True(WaitUntil(() => AudioManager.Completions > before), "a running stream should report a completed period");
            }
            else
            {
                TimerManager.Wait(NoLineWindowMs);
                Assert.True(device.IsRunning, "the stream should run without a line");
                Assert.Equal(before, AudioManager.Completions, "no completion should be reported without a line");
            }
        }
        finally
        {
            device.Stop();
        }
    }

    // ==================== Player ====================

    // A player on the calling thread plays the stream to its end, then gives
    // the output back stopped.
    private static void TestPlayerPlaysToEnd()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        MemoryAudioStream stream = Tone(ShortStreamMs, DefaultSampleRate);
        AudioPlayer player = new(device);
        Assert.True(player.Play(stream), "the player should play the stream");
        Assert.True(stream.Depleted, "the stream should be read to its end");
        Assert.Equal(stream.Length, stream.Position, "the position should be at the end");
        Assert.Null(player.Stream, "the player should be idle afterwards");
        Assert.Null(device.Player, "the output should be released");
        Assert.False(device.IsRunning, "the output should be stopped");
    }

    // A stream in a layout the output refuses is not played, and the output
    // is released with its format unchanged.
    private static void TestPlayerRefusesFormat()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        AudioFormat mono = new(AudioBitDepth.Bits16, 1, true);
        MemoryAudioStream stream = new(mono, DefaultSampleRate, new byte[mono.FrameSize * 64]);
        Assert.False(new AudioPlayer(device).Play(stream), "the player should refuse a mono stream");
        Assert.Equal(0u, stream.Position, "nothing should be read from the stream");
        Assert.Null(device.Player, "the output should be released");
        Assert.False(device.IsRunning, "the output should not run");
        Assert.Equal(AudioFormat.Stereo16, device.Format, "the format should be unchanged");
    }

    // A background playback holds the output from the moment it starts, so a
    // second one is refused; it runs to Completed and releases what it owned.
    private static void TestPlaybackCompletes()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        MemoryAudioStream stream = Tone(ShortStreamMs, DefaultSampleRate);
        DisposeProbe owned = new();
        Assert.True(AudioManager.TryStartPlayback(stream, owned, out AudioPlayback? started), "the playback should start");
        if (started is not AudioPlayback playback)
        {
            return;
        }

        Assert.True(ReferenceEquals(device, playback.Device), "the playback should feed the primary output");
        Assert.False(
            AudioManager.TryStartPlayback(Tone(ShortStreamMs, DefaultSampleRate), null, out _),
            "a second playback should be refused while the first holds the output");

        Assert.True(WaitUntil(() => !playback.IsPlaying), "the playback should end");
        Assert.True(playback.State == AudioPlaybackState.Completed, "the playback should complete");
        Assert.True(stream.Depleted, "the stream should be read to its end");
        Assert.True(owned.Disposed, "the playback should release what it owned");
        Assert.True(WaitUntil(() => device.Player is null), "the output should be released");
        Assert.False(device.IsRunning, "the output should be stopped");
    }

    // Stopping a background playback ends it early as Stopped, with the
    // stream not read to its end and the output released.
    private static void TestPlaybackStop()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        MemoryAudioStream stream = Tone(LongStreamMs, DefaultSampleRate);
        Assert.True(AudioManager.TryStartPlayback(stream, null, out AudioPlayback? started), "the playback should start");
        if (started is not AudioPlayback playback)
        {
            return;
        }

        Assert.True(WaitUntil(() => device.IsRunning), "the playback thread should start the output");
        playback.Stop();

        Assert.True(WaitUntil(() => !playback.IsPlaying), "the playback should end once stopped");
        Assert.True(playback.State == AudioPlaybackState.Stopped, "the playback should end as Stopped");
        Assert.True(stream.Position < stream.Length, "the stream should not be read to its end");
        Assert.True(WaitUntil(() => device.Player is null), "the output should be released");
        Assert.False(device.IsRunning, "the output should be stopped");
    }

    // A WAV file in memory, with a LIST chunk between fmt and data, reads as
    // the layout and length it declares and plays through the output.
    private static void TestWaveFromMemoryPlays()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        int frames = (int)(DefaultSampleRate * ShortStreamMs / MillisecondsPerSecond);
        MemoryAudioStream stream = MemoryAudioStream.FromWave(Wave(ToneBytes(frames)));
        Assert.Equal(AudioFormat.Stereo16, stream.Format, "the file's layout");
        Assert.Equal(DefaultSampleRate, stream.SampleRate, "the file's rate");
        Assert.Equal((uint)frames, stream.Length, "the file's length in frames");
        Assert.True(new AudioPlayer(device).Play(stream), "the file should play");
        Assert.True(stream.Depleted, "the file should be read to its end");
    }

    // ==================== Console.Beep ====================

    // The plug keeps the Windows contract: a pitch from 37 to 32767 Hz and a
    // positive length, checked before anything is played.
    private static void TestBeepRejectsOutOfRange()
    {
        Assert.True(BeepThrowsOutOfRange(36, BeepMs), "36 Hz should be refused");
        Assert.True(BeepThrowsOutOfRange(32768, BeepMs), "32768 Hz should be refused");
        Assert.True(BeepThrowsOutOfRange(ToneHz, 0), "a zero length should be refused");
        Assert.True(BeepThrowsOutOfRange(ToneHz, -1), "a negative length should be refused");
    }

    // A beep holds the caller until its tone has played through the output,
    // then gives the output back stopped.
    private static void TestBeepPlaysForItsDuration()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        long start = Stopwatch.GetTimestamp();
        Console.Beep(ToneHz, BeepMs);
        long elapsed = ElapsedMs(start);

        Log.WriteString("[Audio Tests] Console.Beep(" + ToneHz + ", " + BeepMs + ") took " + elapsed + " ms\n");
        Assert.True(elapsed >= BeepMs / 2, "the beep should hold the caller while it plays");
        Assert.True(elapsed < WaitTimeoutMs, "the beep should return once played");
        Assert.Null(device.Player, "the output should be released");
        Assert.False(device.IsRunning, "the output should be stopped");
    }

    // The overload without arguments plays Windows' 800 Hz for 200 ms
    // through the same output, instead of writing a bell to a terminal.
    private static void TestBeepDefault()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        long start = Stopwatch.GetTimestamp();
        Console.Beep();
        long elapsed = ElapsedMs(start);

        Log.WriteString("[Audio Tests] Console.Beep() took " + elapsed + " ms\n");
        Assert.True(elapsed >= DefaultBeepMs / 2, "the beep should hold the caller while it plays");
        Assert.True(elapsed < WaitTimeoutMs, "the beep should return once played");
        Assert.Null(device.Player, "the output should be released");
    }

    // A beep does not wait for, or cut into, a playback that holds the
    // output: it returns at once and the playback carries on.
    private static void TestBeepWhileBusy()
    {
        if (RequirePrimary() is not AudioDevice device)
        {
            return;
        }

        Assert.True(AudioManager.TryStartPlayback(Tone(LongStreamMs, DefaultSampleRate), null, out AudioPlayback? started), "the playback should start");
        if (started is not AudioPlayback playback)
        {
            return;
        }

        try
        {
            Assert.True(WaitUntil(() => device.IsRunning), "the playback thread should start the output");

            long start = Stopwatch.GetTimestamp();
            Console.Beep(ToneHz, RefusedBeepMs);
            long elapsed = ElapsedMs(start);

            Assert.True(elapsed < RefusedBeepBoundMs, "a beep on a busy output should return at once");
            Assert.True(playback.IsPlaying, "the playback should carry on");
            Assert.True(ReferenceEquals(playback.Player, device.Player), "the playback should still hold the output");
        }
        finally
        {
            playback.Stop();
        }

        Assert.True(WaitUntil(() => device.Player is null), "the output should be released");
    }

    // With no output a beep plays nothing and returns at once, without
    // throwing.
    private static void TestBeepWithoutOutput()
    {
        long start = Stopwatch.GetTimestamp();
        Console.Beep(ToneHz, RefusedBeepMs);
        Console.Beep();
        long elapsed = ElapsedMs(start);

        Assert.True(elapsed < RefusedBeepBoundMs, "a beep with no output should return at once");
    }

    // One try/catch per method on purpose: true = the beep threw the
    // contract's ArgumentOutOfRangeException.
    private static bool BeepThrowsOutOfRange(int frequency, int duration)
    {
        try
        {
            Console.Beep(frequency, duration);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    // ==================== Helpers ====================

    /// <summary>The primary output, with a failed assertion when the manager lists none.</summary>
    private static AudioDevice? RequirePrimary()
    {
        AudioDevice? device = AudioManager.Primary;
        Assert.NotNull(device, "the hda cell should have a primary output");
        return device;
    }

    /// <summary>
    /// Finds the path of the HD Audio controller's PCI function node,
    /// whatever its state. Compared ordinally, as every string in kernel test
    /// code is.
    /// </summary>
    /// <returns>The node's path, or null when no controller is in the tree.</returns>
    private static string? FindControllerPath()
    {
        int count = DriverDiagnostics.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverDiagnostics.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == PciBusName
                && info.Description.EndsWith(HdAudioClassSuffix, StringComparison.Ordinal))
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>Reads the controller's node again, by the path <see cref="BeforeRun"/> found.</summary>
    /// <param name="node">The node.</param>
    /// <returns>False when the node left the tree.</returns>
    private static bool TryGetControllerNode(out DeviceNodeInfo node)
    {
        int count = DriverDiagnostics.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverDiagnostics.TryGetNode(i, out node) && node.Path == s_controllerPath)
            {
                return true;
            }
        }

        node = default;
        return false;
    }

    /// <summary>Finds the published device named <see cref="OutputName"/> on the controller's node.</summary>
    /// <returns>Its position in the published list, or -1.</returns>
    private static int FindOutputDeviceIndex()
    {
        int count = DriverDiagnostics.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverDiagnostics.TryGetDevice(i, out PublishedDeviceInfo info)
                && info.Name == OutputName
                && info.NodePath == s_controllerPath)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Polls <paramref name="condition"/> until it holds or
    /// <see cref="WaitTimeoutMs"/> of host time passes. The wait between
    /// polls lets the scheduler run the playback thread and the device
    /// advance.
    /// </summary>
    /// <param name="condition">What to wait for.</param>
    /// <returns>Whether it held before the bound.</returns>
    private static bool WaitUntil(Func<bool> condition)
    {
        long deadline = Stopwatch.GetTimestamp() + (Stopwatch.Frequency * WaitTimeoutMs / MillisecondsPerSecond);
        while (!condition())
        {
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            TimerManager.Wait(PollIntervalMs);
        }

        return true;
    }

    /// <summary>Milliseconds of host time since <paramref name="start"/>, a <see cref="Stopwatch"/> timestamp.</summary>
    private static long ElapsedMs(long start)
    {
        return (Stopwatch.GetTimestamp() - start) * MillisecondsPerSecond / Stopwatch.Frequency;
    }

    /// <summary>A signed 16-bit stereo square wave of the given length, in memory.</summary>
    /// <param name="milliseconds">How long it plays.</param>
    /// <param name="sampleRate">Frames per second.</param>
    private static MemoryAudioStream Tone(int milliseconds, uint sampleRate)
    {
        int frames = (int)(sampleRate * milliseconds / MillisecondsPerSecond);
        return new MemoryAudioStream(AudioFormat.Stereo16, sampleRate, ToneBytes(frames));
    }

    /// <summary>The frames of a signed 16-bit stereo square wave at <see cref="ToneHz"/> and 48 kHz.</summary>
    /// <param name="frames">How many frames.</param>
    private static byte[] ToneBytes(int frames)
    {
        int frameSize = AudioFormat.Stereo16.FrameSize;
        byte[] samples = new byte[frames * frameSize];
        int halfPeriod = (int)DefaultSampleRate / (ToneHz * 2);
        for (int frame = 0; frame < frames; frame++)
        {
            short value = (frame / halfPeriod) % 2 == 0 ? ToneAmplitude : (short)-ToneAmplitude;
            int offset = frame * frameSize;
            BitConverter.TryWriteBytes(samples.AsSpan(offset, 2), value);
            BitConverter.TryWriteBytes(samples.AsSpan(offset + 2, 2), value);
        }

        return samples;
    }

    /// <summary>
    /// Wraps signed 16-bit stereo frames at 48 kHz in a RIFF/WAVE file, with
    /// an odd-length LIST chunk between fmt and data, so the reader has to
    /// walk the chunk list and honour the pad byte to find the frames.
    /// </summary>
    /// <param name="frames">The interleaved frames.</param>
    private static byte[] Wave(byte[] frames)
    {
        const int FormatChunkBytes = 16;
        const int ListChunkBytes = 5;
        const int ListChunkPaddedBytes = 6;
        const int HeaderBytes = 12 + (8 + FormatChunkBytes) + (8 + ListChunkPaddedBytes) + 8;

        byte[] file = new byte[HeaderBytes + frames.Length];
        Span<byte> span = file;
        int offset = 0;

        WriteTag(span, ref offset, "RIFF");
        WriteUInt32(span, ref offset, (uint)(file.Length - 8));
        WriteTag(span, ref offset, "WAVE");

        WriteTag(span, ref offset, "fmt ");
        WriteUInt32(span, ref offset, FormatChunkBytes);
        WriteUInt16(span, ref offset, 1);
        WriteUInt16(span, ref offset, 2);
        WriteUInt32(span, ref offset, DefaultSampleRate);
        WriteUInt32(span, ref offset, DefaultSampleRate * 4);
        WriteUInt16(span, ref offset, 4);
        WriteUInt16(span, ref offset, 16);

        WriteTag(span, ref offset, "LIST");
        WriteUInt32(span, ref offset, ListChunkBytes);
        WriteTag(span, ref offset, "INFO");
        offset += ListChunkPaddedBytes - 4;

        WriteTag(span, ref offset, "data");
        WriteUInt32(span, ref offset, (uint)frames.Length);
        frames.CopyTo(span[offset..]);
        return file;
    }

    private static void WriteTag(Span<byte> span, ref int offset, string tag)
    {
        for (int i = 0; i < tag.Length; i++)
        {
            span[offset + i] = (byte)tag[i];
        }

        offset += tag.Length;
    }

    private static void WriteUInt32(Span<byte> span, ref int offset, uint value)
    {
        BitConverter.TryWriteBytes(span.Slice(offset, 4), value);
        offset += 4;
    }

    private static void WriteUInt16(Span<byte> span, ref int offset, ushort value)
    {
        BitConverter.TryWriteBytes(span.Slice(offset, 2), value);
        offset += 2;
    }

    /// <summary>Records that a playback released what it was handed.</summary>
    private sealed class DisposeProbe : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
