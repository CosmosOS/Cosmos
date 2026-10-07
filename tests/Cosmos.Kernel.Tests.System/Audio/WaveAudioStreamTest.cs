// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Audio;

[TestFixture]
public class WaveAudioStreamTest
{
    private static WaveAudioStream Open(byte[] file, out MemoryFileHandle handle)
    {
        handle = new MemoryFileHandle(file);
        if (!WaveAudioStream.TryOpen(handle, out WaveAudioStream? stream))
        {
            throw new AssertionException("TryOpen should accept the file");
        }

        return stream;
    }

    public class TryOpen : WaveAudioStreamTest
    {
        [Test]
        public void WhenTheFileIsPcm_LeavesTheHandleOnTheFirstFrame()
        {
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(WaveFileBuilder.Counting(16)), out MemoryFileHandle handle);

            Assert.That(stream.Format, Is.EqualTo(AudioFormat.Stereo16));
            Assert.That(stream.SampleRate, Is.EqualTo(48000u));
            Assert.That(stream.Length, Is.EqualTo(4u));
            Assert.That(stream.Position, Is.EqualTo(0u));
            Assert.That(handle.Position, Is.EqualTo(WaveFileBuilder.PlainDataOffset));
        }

        [Test]
        public void WhenAChunkPrecedesData_FindsTheFramesPastIt()
        {
            byte[] frames = WaveFileBuilder.Counting(8);
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.PcmTag, 2, 48000, 16)
                .Chunk("LIST", [1, 2, 3])
                .Data(frames)
                .ToArray();
            WaveAudioStream stream = Open(file, out _);

            byte[] read = new byte[8];

            Assert.That(stream.Read(read), Is.EqualTo(8));
            Assert.That(read, Is.EqualTo(frames));
        }

        // The size comes from the handle's stat, not from a buffer: the
        // stream never holds the file.
        [Test]
        public void WhenDataRunsPastTheFile_ClampsToTheFileSize()
        {
            byte[] file = new WaveFileBuilder()
                .Format(WaveFileBuilder.PcmTag, 2, 48000, 16)
                .Data(WaveFileBuilder.Counting(14), declaredLength: 4096)
                .ToArray();
            WaveAudioStream stream = Open(file, out _);

            Assert.That(stream.Length, Is.EqualTo(3u));
        }

        [Test]
        public void WhenTheFileIsNotWave_ReturnsFalseAndNoStream()
        {
            MemoryFileHandle handle = new(new byte[64]);

            Assert.That(WaveAudioStream.TryOpen(handle, out WaveAudioStream? stream), Is.False);
            Assert.That(stream, Is.Null);
            Assert.That(handle.Disposed, Is.False);
        }

        [Test]
        public void WhenTheFormatHasNoLayout_ReturnsFalse()
        {
            byte[] file = new WaveFileBuilder().Format(WaveFileBuilder.PcmTag, 2, 48000, 12).Data(new byte[12]).ToArray();

            Assert.That(WaveAudioStream.TryOpen(new MemoryFileHandle(file), out _), Is.False);
        }
    }

    public class Read : WaveAudioStreamTest
    {
        [Test]
        public void WhenReadToTheEnd_ReturnsTheFramesInOrderThenZero()
        {
            byte[] frames = WaveFileBuilder.Counting(24);
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(frames), out _);

            using MemoryStream read = new();
            byte[] buffer = new byte[8];
            int count;
            while ((count = stream.Read(buffer)) > 0)
            {
                read.Write(buffer, 0, count);
            }

            Assert.That(read.ToArray(), Is.EqualTo(frames));
            Assert.That(stream.Depleted, Is.True);
            Assert.That(stream.Position, Is.EqualTo(stream.Length));
        }

        [Test]
        public void WhenTheDestinationHoldsAPartialFrame_ReadsWholeFramesOnly()
        {
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(WaveFileBuilder.Counting(16)), out _);

            Assert.That(stream.Read(new byte[7]), Is.EqualTo(4));
            Assert.That(stream.Position, Is.EqualTo(1u));
        }
    }

    public class Position : WaveAudioStreamTest
    {
        [Test]
        public void WhenSet_TheNextReadStartsAtThatFrame()
        {
            byte[] frames = WaveFileBuilder.Counting(16);
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(frames), out _);

            stream.Position = 2;
            byte[] read = new byte[4];
            stream.Read(read);

            Assert.That(stream.Position, Is.EqualTo(3u));
            Assert.That(read, Is.EqualTo(frames[8..12]));
        }

        [Test]
        public void WhenSetPastTheEnd_ClampsToTheLength()
        {
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(WaveFileBuilder.Counting(16)), out _);

            stream.Position = 100;

            Assert.That(stream.Position, Is.EqualTo(stream.Length));
            Assert.That(stream.Depleted, Is.True);
        }
    }

    public class Rewind : WaveAudioStreamTest
    {
        [Test]
        public void WhenAFrameWasRead_ReadsFromTheFirstFrameAgain()
        {
            byte[] frames = WaveFileBuilder.Counting(8);
            WaveAudioStream stream = Open(WaveFileBuilder.Stereo16(frames), out _);
            stream.Read(new byte[8]);

            stream.Rewind();
            byte[] read = new byte[8];
            stream.Read(read);

            Assert.That(read, Is.EqualTo(frames));
        }
    }
}
