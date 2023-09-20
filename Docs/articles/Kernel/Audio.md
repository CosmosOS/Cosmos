# The Cosmos Audio Infrastructure (CAI)
The Cosmos Audio Infrastructure allows for audio manipulation/conversion, audio I/O, and communication between audio devices. The CAI was designed with simplicity and versatility in mind.

A basic example of playing audio through an AC97-compatible audio card:
```cs
var mixer = new AudioMixer();
var audioStream = MemoryAudioStream.FromWave(sampleAudioBytes);
var driver = AC97.Initialize(bufferSize: 4096);
mixer.Streams.Add(audioStream);

var audioManager = new AudioManager()
{
    Stream = mixer,
    Output = driver
};
audioManager.Enable();
```

The sampleAudioBytes are the bytes of a ttf audio file. You can read it from [VFS](https://cosmosos.github.io/articles/Kernel/VFS.html) or using [ManifestResourceStream](https://cosmosos.github.io/articles/Kernel/ManifestResouceStream.html)

## Audio Streams
An `AudioStream` is an object that can provide sample data to audio buffers. By design, the base `AudioStream` class does not have any length or position properties, as audio streams may be infinite - for example, an output stream from a microphone, or an audio mixer. All seekable streams inherit from the class `SeekableAudioStream`, which provides functionality for accessing the position/length properties and allows methods to determine whether they accept infinite and finite streams, or only finite streams.

### Reading audio streams from memory
You can create seekable audio streams from byte arrays using the `MemoryAudioStream` class:
```cs
byte[] bytes = GetArrayOfAudioSamplesFromSomewhere();
var memAudioStream = new MemoryAudioStream(new SampleFormat(AudioBitDepth.Bits16, 2, true), 48000, bytes);
```

However, usually, you will have an audio file which contains a header containing information about the format of the audio samples it contains. The MemoryAudioStream class features support for the [Waveform Audio File Format (WAVE)](https://en.wikipedia.org/wiki/WAV), commonly used with the .WAV extension. To create an memory audio stream from a .WAV file, simply do:
```cs
byte[] bytes = GetWavFileFromSomewhere();
var wavAudioStream = MemoryAudioStream.FromWave(bytes); 
```
The method will parse the file and return a `MemoryAudioStream`. The sample format will be determined by using the .WAV header. Please keep in mind that this method only accepts uncompressed LPCM samples, which is the most common encoding used in .WAV files.

## Audio Mixing
The CAI includes an `AudioMixer` class out of the box. This class is an infinite `AudioStream` that mixes given streams together. Please keep in mind that mixing several audio streams together can result in [signal clipping](https://en.wikipedia.org/wiki/Clipping_(signal_processing)). In order to prevent clipping, it's recommended to either decrease the volume of the processed streams by using the `GainPostProcessor`, or implementing your own [audio limiter](https://en.wikipedia.org/wiki/Limiter).

## Audio Buffers
Audio buffers are commonly used in both hardware and software handling - for this reason, the `AudioBuffer` class exists to operate over an array of raw audio sample data.

### Audio Buffer R/W
Audio buffers can be easily written to or read from with the help of the `AudioBufferWriter` or `AudioBufferReader` classes, respectively. These classes automatically perform all bit-depth, channel, and sign conversions. Please keep in mind that conversion operations may be taxing on the CPU. It is recommended to use standard signed 16-bit PCM samples, but, if a conversion operation is necessary, it's recommended to perform them offline (as in, before feeding the unconverted streams into an audio mixer). The reason behind this is because processing the samples within a continously running audio driver will introduce audio crackle if the CPU cannot keep up with the conversion task.

## Audio Post-Processing
Audio streams can be processed before they write to an audio buffer by using the `PostProcessors` property on an `AudioStream` instance. Post-processing effects are simple to implement:

```cs
public class SilencePostProcessor : AudioPostProcessor {
    public override void Process(AudioBuffer buffer){
        Array.Clear(buffer.RawData);
    }
}
```

The above example implements an audio post-processor that turns any audio stream into silence. A more complex example can be seen in the `GainPostProcessor` class, included with the CAI.

## Interfacing with hardware
All hardware interfacing is abstracted behind the `AudioDriver` class. It's recommended to operate an audio driver using the `AudioManager` class. Implementations of the `AudioDriver` class usually do not have a public constructor, as they can handle only one instance of an audio card - if that is the case, they should feature a static `Initialize` method and a static `Instance` property.

For example, to initialize the AC97 driver:
```cs
var driver = AC97.Initialize(4096);
```

As you can see in the example above, the AC97 initialization method accepts an integer parameter - this is the buffer size the AC97 will use. A higher buffer size will result in a decreased amount of clicks and will usually decrease mixing overhead, however, it will increase latency. Some drivers, like the AC97 driver, include support for changing the buffer size while it is running - however, support for this is not guaranteed.

After initializing a driver, it's recommended to handle it using `AudioManager`:
```cs
var audioManager = new AudioManager()
{
    Stream = mixer,
    Output = driver
};

audioManager.Enable();
```
The audio manager accepts a `Stream` and an `Output` property - the `Stream` is the audio stream that the audio manager will read samples from, which will in turn be provided to the underlying `Output` audio driver. The audio manager abstracts all hardware handling - however, if you need more control over the devices, you can use the driver classes directly.

> **Note**<br>
- > When interfacing with audio devices, remember not to overload the system when supplying the audio samples. When mixing several streams of audio of different formats, for example, the system can get too overloaded, and this will result in audio crackle, or the system won't be able to respond to the audio device in time, resulting in the audio device stopping all output unexpectedly.
