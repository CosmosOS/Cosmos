// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Audio;

[TestFixture]
public class MemoryAudioStreamTest
{
    public class Constructor : MemoryAudioStreamTest
    {
        [Test]
        public void WhenTheFormatIsEmpty_Throws()
        {
            Assert.Throws<ArgumentException>(() => _ = new MemoryAudioStream(default, 48000, new byte[8]));
        }

        [Test]
        public void WhenTheDataEndsInAPartialFrame_CountsWholeFrames()
        {
            MemoryAudioStream stream = new(AudioFormat.Stereo16, 48000, new byte[10]);

            Assert.That(stream.Length, Is.EqualTo(2u));
            Assert.That(stream.Position, Is.EqualTo(0u));
            Assert.That(stream.Depleted, Is.False);
        }
    }

    public class FromWave : MemoryAudioStreamTest
    {
        [Test]
        public void WhenTheFileIsPcm_ReadsBackTheFramesOnly()
        {
            byte[] frames = WaveFileBuilder.Counting(16);
            MemoryAudioStream stream = MemoryAudioStream.FromWave(WaveFileBuilder.Stereo16(frames));

            byte[] read = new byte[32];
            int count = stream.Read(read);

            Assert.That(stream.Format, Is.EqualTo(AudioFormat.Stereo16));
            Assert.That(stream.SampleRate, Is.EqualTo(48000u));
            Assert.That(stream.Length, Is.EqualTo(4u));
            Assert.That(read[..count], Is.EqualTo(frames));
        }

        [Test]
        public void WhenTheFileIsNotWave_Throws()
        {
            Assert.Throws<ArgumentException>(() => MemoryAudioStream.FromWave(new byte[64]));
        }
    }

    public class Read : MemoryAudioStreamTest
    {
        [Test]
        public void WhenTheDestinationHoldsAPartialFrame_ReadsWholeFramesOnly()
        {
            MemoryAudioStream stream = new(AudioFormat.Stereo16, 48000, WaveFileBuilder.Counting(16));

            int count = stream.Read(new byte[6]);

            Assert.That(count, Is.EqualTo(4));
            Assert.That(stream.Position, Is.EqualTo(1u));
        }

        [Test]
        public void WhenTheStreamRunsOut_ReturnsWhatIsLeftThenZero()
        {
            MemoryAudioStream stream = new(AudioFormat.Stereo16, 48000, WaveFileBuilder.Counting(12));

            Assert.That(stream.Read(new byte[8]), Is.EqualTo(8));
            Assert.That(stream.Read(new byte[8]), Is.EqualTo(4));
            Assert.That(stream.Depleted, Is.True);
            Assert.That(stream.Read(new byte[8]), Is.EqualTo(0));
        }
    }

    public class Rewind : MemoryAudioStreamTest
    {
        [Test]
        public void WhenAFrameWasRead_ReadsFromTheFirstFrameAgain()
        {
            byte[] frames = WaveFileBuilder.Counting(8);
            MemoryAudioStream stream = new(AudioFormat.Stereo16, 48000, frames);
            stream.Read(new byte[8]);

            stream.Rewind();
            byte[] read = new byte[8];
            int count = stream.Read(read);

            Assert.That(count, Is.EqualTo(8));
            Assert.That(read, Is.EqualTo(frames));
        }
    }
}
