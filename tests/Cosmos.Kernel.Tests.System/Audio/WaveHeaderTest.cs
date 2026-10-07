// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Audio;

[TestFixture]
public class WaveHeaderTest
{
    public class TryParse : WaveHeaderTest
    {
        [Test]
        public void WhenPcmStereo16_ReadsLayoutRateAndFrames()
        {
            byte[] file = WaveFileBuilder.Stereo16(WaveFileBuilder.Counting(16));

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.Format, Is.EqualTo(AudioFormat.Stereo16));
            Assert.That(header.SampleRate, Is.EqualTo(48000u));
            Assert.That(header.DataOffset, Is.EqualTo(WaveFileBuilder.PlainDataOffset));
            Assert.That(header.DataLength, Is.EqualTo(16));
            Assert.That(header.FrameCount, Is.EqualTo(4u));
        }

        // A LIST chunk of odd length sits before the frames, as tagging
        // software writes one, and RIFF pads it to an even length.
        [Test]
        public void WhenAnOddChunkPrecedesData_WalksPastItAndItsPadByte()
        {
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.PcmTag, 2, 48000, 16)
                .Chunk("LIST", [1, 2, 3, 4, 5])
                .Data(WaveFileBuilder.Counting(8))
                .ToArray();

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.DataOffset, Is.EqualTo(WaveFileBuilder.PlainDataOffset + WaveFileBuilder.ChunkHeaderBytes + 6));
            Assert.That(header.DataLength, Is.EqualTo(8));
        }

        // A recording cut short leaves a data length past the end of the file.
        [Test]
        public void WhenDataRunsPastTheFile_ClampsToWhatIsThere()
        {
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.PcmTag, 2, 48000, 16)
                .Data(WaveFileBuilder.Counting(12), declaredLength: 1000)
                .ToArray();

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.DataLength, Is.EqualTo(12));
            Assert.That(header.FrameCount, Is.EqualTo(3u));
        }

        [Test]
        public void WhenTheLastFrameIsPartial_TrimsIt()
        {
            byte[] file = WaveFileBuilder.Stereo16(WaveFileBuilder.Counting(10));

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.DataLength, Is.EqualTo(8));
            Assert.That(header.FrameCount, Is.EqualTo(2u));
        }

        [TestCase((ushort)8, AudioBitDepth.Bits8, false)]
        [TestCase((ushort)16, AudioBitDepth.Bits16, true)]
        [TestCase((ushort)24, AudioBitDepth.Bits24, true)]
        [TestCase((ushort)32, AudioBitDepth.Bits32, true)]
        public void WhenBitsPerSampleHasALayout_OnlyEightBitIsUnsigned(ushort bits, AudioBitDepth depth, bool signed)
        {
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.PcmTag, 1, 22050, bits)
                .Data(new byte[bits / 8 * 4])
                .ToArray();

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.Format, Is.EqualTo(new AudioFormat(depth, 1, signed)));
            Assert.That(header.SampleRate, Is.EqualTo(22050u));
            Assert.That(header.FrameCount, Is.EqualTo(4u));
        }

        [Test]
        public void WhenTheTagIsExtensible_ReadsItAsPcm()
        {
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.ExtensibleTag, 2, 48000, 16)
                .Data(new byte[8])
                .ToArray();

            Assert.That(WaveHeader.TryParse(file, out WaveHeader header), Is.True);
            Assert.That(header.Format, Is.EqualTo(AudioFormat.Stereo16));
        }

        // MPEG layer 3 and IEEE float inside a RIFF wrapper: not LPCM, which
        // is all the reader decodes.
        [TestCase((ushort)0x0055)]
        [TestCase((ushort)0x0003)]
        public void WhenTheTagIsNotPcm_Refuses(ushort tag)
        {
            byte[] file = new WaveFileBuilder().Format(tag, 2, 48000, 16).Data(new byte[8]).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [TestCase((ushort)0)]
        [TestCase((ushort)12)]
        [TestCase((ushort)20)]
        public void WhenBitsPerSampleHasNoLayout_Refuses(ushort bits)
        {
            byte[] file = new WaveFileBuilder().Format(WaveFileBuilder.PcmTag, 2, 48000, bits).Data(new byte[8]).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [Test]
        public void WhenChannelsIsZero_Refuses()
        {
            byte[] file = new WaveFileBuilder().Format(WaveFileBuilder.PcmTag, 0, 48000, 16).Data(new byte[8]).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [Test]
        public void WhenThereIsNoDataChunk_Refuses()
        {
            byte[] file = new WaveFileBuilder().Format(WaveFileBuilder.PcmTag, 2, 48000, 16).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [Test]
        public void WhenThereIsNoFormatChunk_Refuses()
        {
            byte[] file = new WaveFileBuilder().Data(new byte[8]).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [Test]
        public void WhenTheFormatChunkIsShort_Refuses()
        {
            byte[] file = new WaveFileBuilder().Chunk("fmt ", [1, 0, 2, 0]).Data(new byte[8]).ToArray();

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [Test]
        public void WhenTheFormIsNotWave_Refuses()
        {
            byte[] file = WaveFileBuilder.Stereo16(new byte[8]);
            file[8] = (byte)'A';

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }

        [TestCase(0)]
        [TestCase(11)]
        public void WhenShorterThanTheRiffHeader_Refuses(int length)
        {
            byte[] file = WaveFileBuilder.Stereo16(new byte[8])[..length];

            Assert.That(WaveHeader.TryParse(file, out _), Is.False);
        }
    }

    public class FrameCount : WaveHeaderTest
    {
        [Test]
        public void WhenTheHeaderIsDefault_ReturnsZero()
        {
            Assert.That(default(WaveHeader).FrameCount, Is.EqualTo(0u));
        }
    }
}
