// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Structures;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Structures;

[TestFixture]
[TestOf(typeof(RingBuffer<>))]
public class RingBufferTest
{
    public class Constructor
    {
        [Test]
        public void SetsCapacity()
        {
            var rb = new RingBuffer<int>(4);
            Assert.That(rb.Count, Is.EqualTo(0));
        }
    }

    public class PushPop
    {
        [Test]
        public void PopReturnsInFifoOrder()
        {
            var rb = new RingBuffer<int>(4);
            rb.Push(1);
            rb.Push(2);
            rb.Push(3);

            Assert.Multiple(() =>
            {
                Assert.That(rb.Pop(out int a), Is.True);
                Assert.That(a, Is.EqualTo(1));
                Assert.That(rb.Pop(out int b), Is.True);
                Assert.That(b, Is.EqualTo(2));
            });
        }

        [Test]
        public void PopEmpty_ReturnsFalse()
        {
            var rb = new RingBuffer<int>(4);
            Assert.That(rb.Pop(out int _), Is.False);
        }
    }

    public class Peek
    {
        [Test]
        public void ReturnsOldestWithoutRemoving()
        {
            var rb = new RingBuffer<int>(4);
            rb.Push(7);
            rb.Push(9);

            Assert.Multiple(() =>
            {
                Assert.That(rb.Peek(out int p), Is.True);
                Assert.That(p, Is.EqualTo(7));
                Assert.That(rb.Count, Is.EqualTo(2));
            });
        }
    }

    public class FullBuffer
    {
        [Test]
        public void OverwritesOldest()
        {
            var rb = new RingBuffer<int>(2);
            rb.Push(1);
            rb.Push(2);
            rb.Push(3);

            Assert.Multiple(() =>
            {
                Assert.That(rb.Count, Is.EqualTo(2));
                Assert.That(rb.Pop(out int a), Is.True);
                Assert.That(a, Is.EqualTo(3));
                Assert.That(rb.Pop(out int b), Is.True);
                Assert.That(b, Is.EqualTo(2));
            });
        }
    }

    public class WrapAround
    {
        [Test]
        public void WrapsCorrectly()
        {
            var rb = new RingBuffer<int>(3);
            rb.Push(1);
            rb.Push(2);
            rb.Pop(out _);
            rb.Push(3);
            rb.Push(4);

            Assert.Multiple(() =>
            {
                Assert.That(rb.Count, Is.EqualTo(3));
                Assert.That(rb.Pop(out int a), Is.True);
                Assert.That(a, Is.EqualTo(2));
                Assert.That(rb.Pop(out int b), Is.True);
                Assert.That(b, Is.EqualTo(3));
                Assert.That(rb.Pop(out int c), Is.True);
                Assert.That(c, Is.EqualTo(4));
            });
        }
    }

    public class Clear
    {
        [Test]
        public void ResetsCount()
        {
            var rb = new RingBuffer<int>(4);
            rb.Push(1);
            rb.Push(2);
            rb.Clear();

            Assert.Multiple(() =>
            {
                Assert.That(rb.Count, Is.EqualTo(0));
                Assert.That(rb.Pop(out int _), Is.False);
            });
        }
    }

    public class Enumeration
    {
        [Test]
        public void DrainsAllItems()
        {
            var rb = new RingBuffer<int>(4);
            rb.Push(1);
            rb.Push(2);
            rb.Push(3);

            var collected = new List<int>();
            foreach (int v in rb)
            {
                collected.Add(v);
            }

            Assert.Multiple(() =>
            {
                Assert.That(collected, Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(rb.Count, Is.EqualTo(0));
            });
        }
    }
}
