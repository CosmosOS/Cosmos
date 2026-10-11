// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Audio;

[TestFixture]
public class ToneAudioStreamTest
{
    private const int FrameSize = 4;

    /// <summary>The left channel's sample of each frame in <paramref name="bytes"/>, after checking the right one matches it.</summary>
    private static short[] Samples(byte[] bytes)
    {
        short[] samples = new short[bytes.Length / FrameSize];
        for (int i = 0; i < samples.Length; i++)
        {
            short left = BitConverter.ToInt16(bytes, i * FrameSize);
            short right = BitConverter.ToInt16(bytes, (i * FrameSize) + sizeof(short));
            Assert.That(right, Is.EqualTo(left), $"frame {i} should carry one sample on both channels");
            samples[i] = left;
        }

        return samples;
    }

    public class Constructor : ToneAudioStreamTest
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void WhenTheFrequencyIsNotPositive_Throws(int frequency)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new ToneAudioStream(frequency, 100));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void WhenTheDurationIsNotPositive_Throws(int milliseconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new ToneAudioStream(440, milliseconds));
        }

        [TestCase(1, ExpectedResult = 48L)]
        [TestCase(200, ExpectedResult = 9600L)]
        [TestCase(1000, ExpectedResult = 48000L)]
        [TestCase(int.MaxValue, ExpectedResult = 103_079_215_056L)]
        public long WhenTheDurationIsGiven_LastsThatManyFramesAt48kHz(int milliseconds)
        {
            return new ToneAudioStream(440, milliseconds).FrameCount;
        }

        [Test]
        public void WhenCreated_IsSigned16BitStereoAt48kHz()
        {
            ToneAudioStream stream = new(440, 100);

            Assert.That(stream.Format, Is.EqualTo(AudioFormat.Stereo16));
            Assert.That(stream.SampleRate, Is.EqualTo(ToneAudioStream.ToneSampleRate));
            Assert.That(stream.Depleted, Is.False);
        }
    }

    public class Read : ToneAudioStreamTest
    {
        [Test]
        public void WhenTheFrequencyDividesTheRate_FlipsEveryHalfPeriod()
        {
            // 1 kHz at 48 kHz: 48 frames a period, 24 high then 24 low.
            ToneAudioStream stream = new(1000, 10);
            byte[] bytes = new byte[96 * FrameSize];

            stream.Read(bytes);
            short[] samples = Samples(bytes);

            for (int i = 0; i < samples.Length; i++)
            {
                short expected = (i / 24) % 2 == 0 ? ToneAudioStream.Amplitude : (short)-ToneAudioStream.Amplitude;
                Assert.That(samples[i], Is.EqualTo(expected), $"frame {i}");
            }
        }

        [TestCase(440)]
        [TestCase(800)]
        [TestCase(1234)]
        public void WhenASecondIsRead_ChangesLevelTwiceAPeriod(int frequency)
        {
            ToneAudioStream stream = new(frequency, 1000);
            byte[] bytes = new byte[(int)ToneAudioStream.ToneSampleRate * FrameSize];

            stream.Read(bytes);
            short[] samples = Samples(bytes);

            int changes = 0;
            for (int i = 1; i < samples.Length; i++)
            {
                if (samples[i] != samples[i - 1])
                {
                    changes++;
                }
            }

            // Twice a period for a second, less the edge the second's first
            // frame starts on.
            Assert.That(changes, Is.EqualTo((2 * frequency) - 1));
        }

        [Test]
        public void WhenReadInPieces_ContinuesTheWave()
        {
            byte[] whole = new byte[480 * FrameSize];
            new ToneAudioStream(440, 10).Read(whole);

            ToneAudioStream pieces = new(440, 10);
            byte[] joined = new byte[whole.Length];
            int offset = 0;
            while (offset < joined.Length)
            {
                int length = Math.Min(7 * FrameSize, joined.Length - offset);
                offset += pieces.Read(joined.AsSpan(offset, length));
            }

            Assert.That(joined, Is.EqualTo(whole));
        }

        [Test]
        public void WhenTheDestinationHoldsAPartialFrame_ReadsWholeFramesOnly()
        {
            ToneAudioStream stream = new(440, 10);

            Assert.That(stream.Read(new byte[FrameSize + 2]), Is.EqualTo(FrameSize));
        }

        [Test]
        public void WhenTheToneRunsOut_ReturnsWhatIsLeftThenZero()
        {
            // 1 ms is 48 frames, 192 bytes.
            ToneAudioStream stream = new(440, 1);

            Assert.That(stream.Read(new byte[128]), Is.EqualTo(128));
            Assert.That(stream.Read(new byte[128]), Is.EqualTo(64));
            Assert.That(stream.Depleted, Is.True);
            Assert.That(stream.Read(new byte[128]), Is.EqualTo(0));
        }
    }
}
