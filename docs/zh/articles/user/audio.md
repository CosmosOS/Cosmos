---
source_hash: f72a86ce81abff6fe262713b54335f180a566d31
---
<a id="audio"></a>

# 音频

本文将介绍 Cosmos Gen3 上的音频：如何找到声卡、播放音调、播放 WAV 文件以及生成你自己的声音。声音通过 **Intel High Definition Audio** 控制器输出，它是自 2004 年以来 PC 芯片组的音频标准，也是 QEMU 以 `intel-hda` 之名模拟的声卡。

如果你来自 Gen2，主要区别如下：

| | Gen2 | Gen3 |
|---|---|---|
| 音频 API | `Cosmos.System.Audio` | `Cosmos.Kernel.System.Audio` |
| 声卡驱动程序 | AC97 | 基于驱动工具包的 Intel HD Audio（x64 和 ARM64） |
| 初始化设置 | 一个带有 `Stream` 和 `Output` 的 `AudioManager`，然后调用 `Enable()` | 无需设置：驱动工具包在启动时发布输出，`AudioManager.Play(stream)` 通过它播放 |
| 混音 | 基于多个流的 `AudioMixer` | 每个输出同一时间只播放一个流 |
| `Console.Beep` | PC 扬声器 | 通过声卡播放的方波 |
| WAV 文件 | `MemoryAudioStream.FromWave` | `MemoryAudioStream.FromWave`，以及从卷中流式读取文件的 `WaveAudioStream` |
| 播放 | `Enable()` 运行后在后台播放 | 使用 `AudioManager.Play` 在调用线程上播放，或使用 `AudioManager.TryStartPlayback` 在独立线程上播放 |

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

<a id="enable-audio-in-your-kernel"></a>

## 在内核中启用音频

音频支持由一个功能开关控制。请确保内核的 `.csproj` 没有将其关闭（默认值为 `true`）：

```xml
<PropertyGroup>
  <CosmosEnableAudio>true</CosmosEnableAudio>
</PropertyGroup>
```

声卡是一个 PCI 功能，因此关闭 `CosmosEnablePCI`，或关闭会连带关闭 PCI 的 `CosmosEnableInterrupts`，同样会关闭音频。

启动时，声卡通过[驱动工具包](drivers.md)到达 `AudioManager`，就像网卡到达 `NetworkManager` 一样。`Cosmos.Kernel.Drivers` 中提供的 `HdAudioDriver` 会绑定所有类别为 `04.03.00`（即 HD Audio 控制器类别）的 PCI 功能，因此这一个驱动程序就能覆盖所有遵循该规范的控制器，而不是依赖一份设备 ID 列表。它在控制器后面的编解码器（codec）上找到一个转换器和一个输出引脚，然后发布一个名为 `hda` 的输出，管理器将其列为主输出。QEMU 默认不挂接任何声卡，因此需要让 `cosmos run` 添加一块，两种架构均可：

```console
$ cosmos run --audio intel-hda        # the ICH6 HD Audio controller, the one the test suite runs on
$ cosmos run --audio ich9-intel-hda   # the ICH9 one
```

HD Audio 控制器本身是一条总线，而不是一块完整的声卡：把样本转换为声音的编解码器位于它后面，因此 `cosmos run` 会在控制器旁边挂接 QEMU 的 `hda-duplex` 编解码器。声音通过 QEMU 的默认音频后端在你的主机上播放（PipeWire、PulseAudio、ALSA、CoreAudio 或 DirectSound，取决于主机）。

串口日志会显示驱动程序绑定该控制器的过程：

```
[Audio] primary: HdAudioDriver "hda"
[Drivers] pci:0000:00:02.0 HdAudioDriver published device "hda" (consumed)
[Drivers] pci:0000:00:02.0 HdAudioDriver: version 1.0, codec 0 converter 2 pin 3, stream 4 (4 in, 4 out), 48 kHz stereo 16-bit, line 11
```

最后一个字段表示控制器的中断线是否到达了驱动程序。上面显示的是 `line 11`，对应一台没有网卡的 x64 机器。在 ARM64 上它显示 `no line`，因为驱动工具包不路由任何 PCI 传统中断线；在 x64 上与默认的 e1000e 网卡并存时也是如此，因为该网卡占用了它与控制器共享的中断线。无论哪种情况，输出的播放效果都一样：播放器跟踪流引擎的位置，而不是等待中断。

下面的代码片段依赖以下 `using` 指令：

```csharp
using System.IO;
using Cosmos.Kernel.HAL.Devices.Audio;   // AudioFormat, for the sound you generate (COSMOS0003)
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.Timers;
```

<a id="the-audio-output"></a>

## 音频输出

`AudioManager` 持有驱动工具包发布的输出。在播放任何内容之前，先检查是否存在输出：

```csharp
if (!AudioManager.IsEnabled)
{
    Console.WriteLine("Audio is compiled out (CosmosEnableAudio=false)");
    return;
}

AudioDevice? output = AudioManager.Primary;
if (output is null)
{
    Console.WriteLine("No audio output: attach one with cosmos run --audio intel-hda");
    return;
}

Console.WriteLine("Output:  " + output.Name + " (" + output.DriverName + ")");
Console.WriteLine("Node:    " + output.NodePath);
Console.WriteLine("Format:  " + output.SampleRate + " Hz, " + output.Format.Channels + " ch, "
    + (output.Format.ChannelSize * 8) + "-bit");
```

<!-- screenshot: 控制台显示 "Output: hda (HdAudioDriver)"、pci 节点路径以及 "Format: 48000 Hz, 2 ch, 16-bit" -->
![音频输出](images/audio-output.png)

`AudioDevice` 会报告它当前编程所用的格式（在有内容以其他采样率播放之前，为 48 kHz 有符号 16 位立体声）、其流引擎是否 `IsRunning`，以及当前为它提供数据的 `Player`，空闲时为 `null`。一旦驱动工具包撤回该输出，`IsWithdrawn` 就会变为 true，对它的所有调用都不再起作用。

有多块声卡时，可以枚举它们。列表按节点路径排序，主输出排在最前，因此主输出不取决于哪个驱动程序最先完成探测：

```csharp
for (int i = 0; i < AudioManager.Count; i++)
{
    if (AudioManager.TryGet(i, out AudioDevice? device))
    {
        Console.WriteLine($"[{i}] {device.Name}  {device.NodePath}");
    }
}
```

`AudioManager.Play` 和 `AudioManager.TryStartPlayback` 始终使用主输出。要在其他输出上播放，请把该输出交给 `AudioPlayer` 或 `AudioPlayback.TryStart`（两者见下文）。

<a id="playing-a-tone"></a>

## 播放音调

发出声音最快的方式既不需要文件，也不需要任何设置：`Console.Beep`，内核通过 plug 将它接到主输出上。

```csharp
/* The Windows default: 800 Hz for 200 ms */
Console.Beep();

/* A C major scale, a quarter of a second per note */
int[] notes = { 262, 294, 330, 349, 392, 440, 494, 523 };
foreach (int note in notes)
{
    Console.Beep(note, 250);
}
```

音调是幅度为满量程四分之一的方波，调用在播放结束后才返回，与 Windows 上一致。它也会拒绝 Windows 所拒绝的参数：低于 37 Hz 或高于 32767 Hz 的音高，或小于等于零的时长，都会抛出 `ArgumentOutOfRangeException`。当没有可供播放的输出时（没有发布输出、音频被编译排除，或另一个播放正占用着输出），它会立即返回，不播放任何声音。

.NET 将 `Console.Beep(int, int)` 标记为仅限 Windows，因此该调用会引发 `CA1416` 平台警告。内核仍然实现了它；你可以在调用处抑制该警告，或在整个项目中抑制：

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);CA1416</NoWarn>
</PropertyGroup>
```

<a id="playing-a-wav-file"></a>

## 播放 WAV 文件

`MemoryAudioStream.FromWave` 读取已在内存中的 `.wav` 文件，`AudioManager.Play` 通过主输出播放它，并在最后一帧播放完毕后返回：

```csharp
/* chime.wav is a 16-bit stereo PCM file on the FAT partition mounted at /mnt */
MemoryAudioStream chime = MemoryAudioStream.FromWave(File.ReadAllBytes("/mnt/chime.wav"));

if (!AudioManager.Play(chime))
{
    Console.WriteLine("No output, or the output refused the file's format");
}
```

对于不是未压缩 PCM RIFF/WAVE 的文件，`FromWave` 会抛出 `ArgumentException`。读取器会遍历文件的块（chunk）列表，因此在样本之前带有 `LIST` 或 `fact` 块的文件也能像其他文件一样读取。当没有发布输出，或输出拒绝流的格式时，`Play` 返回 `false`。

最后一种情况很重要，因为读取器能理解的格式比声卡能播放的更多：

| | WAV 读取器接受 | HD Audio 输出播放 |
|---|---|---|
| 编码 | PCM 和 `WAVE_FORMAT_EXTENSIBLE` | 有符号 PCM |
| 每样本位数 | 8、16、24、32 | 16 |
| 声道数 | 1 到 255 | 2 |
| 采样率 | 任意 | 8、9.6、11.025、12、16、22.05、24、32、44.1、48、88.2、96、144、176.4 和 192 kHz |

途中不会进行任何转换或重采样：单声道、8 位或 24 位的文件会被拒绝，而不是以错误的速度或音高播放。请先在主机上完成转换再复制过去，例如使用 [FFmpeg](https://ffmpeg.org/)，它还能解码 MP3、OGG 和 FLAC：

```console
$ ffmpeg -i song.mp3 -ac 2 -ar 48000 -c:a pcm_s16le song.wav
```

声音只是一个文件，因此最简单的分发方式是把它放到[文件系统](filesystem.md)一文中的 FAT 磁盘上，并使用 FAT 兼容的文件名。运行[网络](network.md#ftp-server)一文中 FTP 服务器的内核，也可以在运行期间从主机接收声音文件：`curl -T song.wav ftp://localhost:2121/`。

<a id="playing-in-the-background"></a>

## 后台播放

`FromWave` 把整个文件保存在内存中，`Play` 会在音频的整个时长内占住调用者，这适合短促的提示音。对于一首歌曲，请使用 `WaveAudioStream` 从卷中流式读取文件，每次读取一个块，占用固定大小的内存，并使用 `AudioManager.TryStartPlayback` 在独立线程上播放，这样内核在播放期间可以继续运行：

```csharp
if (!VfsManager.TryOpenFile("/mnt/song.wav", out IVfsFileHandle? file))
{
    Console.WriteLine("File not found");
    return;
}

if (!WaveAudioStream.TryOpen(file, out WaveAudioStream? song))
{
    Console.WriteLine("Not a PCM .wav file");
    file.Dispose();
    return;
}

uint seconds = song.Length / song.SampleRate;
Console.WriteLine("Playing " + seconds + " s of audio");

/* The playback thread reads the file as it plays, and disposes of it when it is done */
if (!AudioManager.TryStartPlayback(song, file, out AudioPlayback? playback))
{
    Console.WriteLine("No output, the output is busy, or no scheduler to run the playback on");
    file.Dispose();
    return;
}
```

第二个参数是流读取数据的来源：播放线程结束后，播放会将其释放，因此文件句柄随播放一起移交，调用者无需再管它。当 `TryStartPlayback` 返回 `false` 时，没有启动任何播放，句柄仍需由调用者关闭。对于不需要释放任何资源的流，传入 `null`。

`WaveAudioStream` 是一个 `SeekableAudioStream`，因此它以帧为单位报告其 `Length` 和 `Position`，调用 `Rewind()` 或设置新的 `Position` 可以移动它。`AudioPlayback` 报告播放进行到了哪里，`Stop()` 可以提前结束播放：

```csharp
while (playback.IsPlaying)
{
    Console.WriteLine((song.Position / song.SampleRate) + " s of " + seconds + " s");

    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
    {
        /* Returns at once; the state turns Stopped shortly after */
        playback.Stop();
    }

    TimerManager.Wait(500);
}

if (playback.State == AudioPlaybackState.Completed)
{
    Console.WriteLine("Played to the end");
}
```

| `State` | 含义 |
|---|---|
| `Playing` | 线程仍在向输出提供数据 |
| `Completed` | 流已播放到末尾 |
| `Stopped` | `Stop()` 提前结束了播放；输出中尚存的数据被丢弃 |
| `Failed` | 输出拒绝了流的格式、被撤回或不再接收帧，或者读取流时抛出了异常；抛出的异常会以 `[Audio] playback failed: ...` 的形式写入串口日志 |

关于后台播放，需要了解三点：

- **它需要调度器。** 没有 `CosmosEnableScheduler` 就没有第二个线程可用于播放，`TryStartPlayback` 会返回 `false`。此时可改用 `AudioManager.Play(stream)` 在调用线程上播放同一个流。
- **每个输出同时只有一个播放。** 从 `TryStartPlayback` 返回 `true` 的那一刻起直到播放结束，该播放都占用着输出，因此在此期间第二个 `TryStartPlayback`、`Play` 或 `Console.Beep` 都会被拒绝。不持有 `AudioPlayback` 的代码可以通过 `AudioManager.Primary?.Player?.RequestStop()` 停止正在播放的内容。
- **播放期间不要动这个卷。** 播放线程在内核其余部分运行的同时读取文件，而文件系统层自身没有任何锁。在 `IsPlaying` 变为 false 之前向同一个卷写入，就是自找麻烦。

`Position` 统计的是从流中读取的帧数，它比你实际听到的内容超前，超前的部分就是已在声卡缓冲区中排队的帧，在 48 kHz 下大约为十分之一秒。

<a id="generating-sound"></a>

## 生成声音

你自己计算的样本也以同样的方式播放。流的格式是一个 `AudioFormat`：每声道位数、每帧声道数以及样本是否有符号，其中 `AudioFormat.Stereo16` 是 HD Audio 输出所播放的格式。一帧按声道交错存放每个声道的一个样本，先左后右，每个样本是一个小端序的有符号 16 位值。

`AudioFormat` 和 `AudioBitDepth` 属于驱动工具包的实验性扩展点，因此在内核项目确认接受缺少兼容性承诺这一点之前，引用它们会产生带有 `COSMOS0003` 诊断的构建错误（[公共 API 跟踪](../dev/public-api.md)）。像上面 `output.Format.Channels` 那样读取格式则无需任何操作：

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
</PropertyGroup>
```

对于短促的声音，填充一个缓冲区并将其包装进 `MemoryAudioStream`：

```csharp
const uint SampleRate = 48000;
AudioFormat format = AudioFormat.Stereo16;

/* One second of a 440 Hz sine wave, at a quarter of full scale */
int frames = (int)SampleRate;
byte[] data = new byte[frames * format.FrameSize];

for (int i = 0; i < frames; i++)
{
    short sample = (short)(Math.Sin(2 * Math.PI * 440 * i / SampleRate) * 8192);

    Span<byte> frame = data.AsSpan(i * format.FrameSize, format.FrameSize);
    BitConverter.TryWriteBytes(frame, sample);       // left
    BitConverter.TryWriteBytes(frame[2..], sample);  // right
}

AudioManager.Play(new MemoryAudioStream(format, SampleRate, data));
```

较长或无限持续的声音最好在读取时即时计算，这样不占用内存：从 `AudioStream` 派生，并填充播放器传给 `Read` 的缓冲区。基类有意不带长度，这样流就可以一直播放下去，就像合成器、你自己写的混音器或游戏配乐那样：

```csharp
/* A sine wave worked out as it is read: it costs no memory and never runs out */
internal sealed class SineStream : AudioStream
{
    private const uint Rate = 48000;

    private readonly double _step;
    private double _phase;

    public SineStream(double frequency)
    {
        _step = 2 * Math.PI * frequency / Rate;
    }

    public override AudioFormat Format => AudioFormat.Stereo16;

    public override uint SampleRate => Rate;

    public override bool Depleted => false;

    public override int Read(Span<byte> destination)
    {
        int frameSize = Format.FrameSize;
        int frames = destination.Length / frameSize;

        for (int i = 0; i < frames; i++)
        {
            short sample = (short)(Math.Sin(_phase) * 8192);

            Span<byte> frame = destination.Slice(i * frameSize, frameSize);
            BitConverter.TryWriteBytes(frame, sample);
            BitConverter.TryWriteBytes(frame[2..], sample);

            _phase += _step;
            if (_phase >= 2 * Math.PI)
            {
                _phase -= 2 * Math.PI;
            }
        }

        return frames * frameSize;
    }
}
```

`Read` 返回它写入的字节数，始终是整数个帧；一旦 `Depleted` 变为 `true` 或 `Read` 返回 `0`，播放器就停止读取。永不耗尽的流必须被主动停止，因此请在后台播放它：

```csharp
if (AudioManager.TryStartPlayback(new SineStream(440), null, out AudioPlayback? playback))
{
    TimerManager.Wait(2000);
    playback.Stop();
}
```

<a id="writing-frames-yourself"></a>

## 自己写入帧

播放器只是 `AudioDevice` 之上的便利封装，内核也可以直接驱动 `AudioDevice`，例如在一个自行混合音效的游戏循环中。设备把帧接收到一个环形缓冲区中，由其流引擎按编解码器的时钟消耗：`Write` 复制能放下的部分并返回接收的字节数，缓冲区已满时返回 `0`，并且从不阻塞。

```csharp
AudioDevice? output = AudioManager.Primary;
if (output is null || output.Player is not null || !output.TrySetFormat(AudioFormat.Stereo16, 48000))
{
    return;
}

SineStream source = new(440);
byte[] chunk = new byte[4096];
output.Start();

/* 500 chunks of 1024 frames: about ten seconds */
for (int pass = 0; pass < 500; pass++)
{
    int length = source.Read(chunk);
    int written = 0;
    while (written < length)
    {
        int took = output.Write(chunk.AsSpan(written, length - written));
        if (took == 0)
        {
            /* The buffer is full: the codec is playing what it has */
            TimerManager.Wait(5);
            continue;
        }

        written += took;
    }
}

output.Stop();
```

设备要求你遵守以下规则：

- `TrySetFormat` 只在设备停止时有效；正在运行的设备会保留现有格式并返回 `false`。
- 持续提供数据。`Start()` 之后缓冲区为空时会播放静音，但缓冲区是环形的，因此长时间落后的写入方会听到旧的帧再次循环播放。
- `Stop()` 会丢弃缓冲区中尚存的数据。要听到声音的尾音，请先在其后写入一个缓冲区大小的静音（在 `TrySetFormat` 之后立即读取 `WritableBytes` 即可得知其大小），播放器正是这样做的。
- 直接写入不会占用输出。开始之前请检查 `Player` 是否为 `null`；在你写入期间到来的 `Play` 或 `Console.Beep` 会被拒绝，因为正在运行的设备会拒绝它们发起的格式更改。

<a id="current-limitations"></a>

## 当前限制

- Intel HD Audio 是唯一的声卡驱动程序：没有 AC97、Sound Blaster、virtio-sound 或 USB 音频，也没有 PC 扬声器。没有 HD Audio 控制器时，`Console.Beep` 会立即返回，不播放任何声音。
- 仅支持播放。不支持录音：编解码器的输入，包括 QEMU `hda-duplex` 的线路输入，均未使用。
- 只有一种格式：有符号 16 位立体声，采样率为上表所列。其他布局或其他采样率的流会被拒绝，而不是被转换或重采样。
- 没有混音器：一个输出同一时间只播放一个流，在其结束之前，第二个播放、`Play` 或 `Console.Beep` 都会被拒绝。
- 没有音量控制。驱动程序将编解码器的放大器打开到最大增益；要降低音量，请缩放你的样本。
- 驱动程序在第一个同时具备两者的编解码器上，通过第一个转换器和第一个能够输出的引脚播放。在有多个插孔的机器上，这可能不是你正在收听的那个；而计算机的 HDMI 或 DisplayPort 音频如果是一个独立的 HD Audio 控制器，会作为单独的输出发布，并且可能排在前面。
- WAV 是内核唯一能读取的文件格式；没有 MP3、OGG 或 FLAC 解码器。

<a id="how-it-works"></a>

## 工作原理

启动时，库初始化器会在驱动程序阶段运行之前安装音频管理器的[驱动工具包](drivers.md)消费者，因此管理器能看到驱动程序发布的每一个输出。`HdAudioDriver` 绑定控制器的 PCI 功能，将其复位，建立用于与编解码器通信的命令环和响应环，并遍历每个编解码器的部件（widget），寻找一个输出转换器和一个能够驱动输出的引脚。它将引脚连接到转换器，打开两者的放大器，开启许多笔记本电脑的扬声器所需的外部放大器，在一个划分为四个 4 KiB 周期的 16 KiB 环形 DMA 缓冲区上对第一个输出流编程，并以 `hda` 之名发布该输出（任何驱动程序都可以发布输出：[发布设备](drivers.md#publishing-a-device)）。

`AudioPlayer` 负责播放，无论启动播放的是 `AudioManager.Play`、后台的 `AudioPlayback`，还是 `Console.Beep` 的 plug（它播放一段生成的方波）。它占用输出，按流的格式和采样率对其编程，启动流引擎，并每次读取 1024 帧到暂存缓冲区，在有空间腾出时将每一批写入设备的缓冲区。设备根据流引擎自身的位置计算可用空间，这正是播放器不需要中断的原因；设备还让写入方前方紧邻的那个周期保持静音，因此写入方短暂落后而发生欠载（underrun）时，播放的是静音。完成中断（在中断线已连接的情况下）只是通知管理器有空间腾出。缓冲区已满时，播放器等待 5 ms：在调度器管理的线程上通过调度器休眠，否则通过定时器等待；如果整整一秒内设备都没有接收任何数据，它就放弃。在流的末尾，它会写入一个缓冲区的静音，使最后的帧在引擎停止之前到达编解码器。

```
Console.Beep / AudioManager.Play / AudioManager.TryStartPlayback
        │
AudioStream ── WaveAudioStream (a volume), MemoryAudioStream, your own   (Cosmos.Kernel.System.Audio)
        │  read 1024 frames at a time
AudioPlayer ── claims the output, writes what fits, waits 5 ms when full
        │
AudioManager ── AudioDevice (primary first)
        │
driver kit: IAudioOutput "hda"                                           (HdAudioDriver, Cosmos.Kernel.Drivers)
        │  16 KiB cyclic DMA buffer, drained on the codec's clock
HD Audio controller → codec converter → pin → speakers (or QEMU's audio backend)
```
