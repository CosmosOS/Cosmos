// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Structures;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Structures;

[TestFixture]
[TestOf(typeof(BufferBuilder<>))]
public class BufferBuilderTest
{
    public class DefaultConstructor
    {
        [Test]
        public void CreatesEmptyBuilder()
        {
            using var builder = new BufferBuilder<int>();

            // No chunks added; Build yields empty memory.
            using var built = builder.Build();
            Assert.That(built.Memory.Length, Is.EqualTo(0));
        }
    }

    public class NewChunk
    {
        [Test]
        public void AppendsToEnd_IncrementsImplicitCount()
        {
            using var builder = new BufferBuilder<int>();
            var chunk = builder.NewChunk(4);

            Assert.That(chunk.Data().Length, Is.EqualTo(4));

            using var built = builder.Build();
            Assert.That(built.Memory.Length, Is.EqualTo(4));
        }

        [Test]
        public void InsertAtBeginning_Prepends()
        {
            using var builder = new BufferBuilder<int>();
            builder.NewChunk(2); // [2]
            builder.NewChunk(0, 3); // [3, 2]

            using var built = builder.Build();
            Assert.That(built.Memory.Length, Is.EqualTo(5));
        }
    }

    public class RemoveIndex
    {
        [Test]
        public void RemovesChunk_DecrementsTotalSize()
        {
            using var builder = new BufferBuilder<int>();
            builder.NewChunk(2);
            builder.NewChunk(2);

            builder.RemoveIndex(0);

            using var built = builder.Build();
            Assert.That(built.Memory.Length, Is.EqualTo(2));
        }
    }

    public class Build
    {
        [Test]
        public void CombinesChunksInOrder()
        {
            using var builder = new BufferBuilder<byte>();
            var c1 = builder.NewChunk(2);
            var c2 = builder.NewChunk(2);

            c1.Data().Span[0] = 1;
            c1.Data().Span[1] = 2;
            c2.Data().Span[0] = 3;
            c2.Data().Span[1] = 4;

            using var built = builder.Build();
            Assert.That(built.Memory.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void EmptyBuilder_BuildsEmpty()
        {
            using var builder = new BufferBuilder<int>();
            using var built = builder.Build();
            Assert.That(built.Memory.Length, Is.EqualTo(0));
        }
    }

    public class Dispose
    {
        [Test]
        public void DoesNotThrow()
        {
            var builder = new BufferBuilder<int>();
            builder.NewChunk(2);
            Assert.DoesNotThrow(() => builder.Dispose());
        }
    }

    [TestFixture]
    public class Allocation
    {
        private static void Prime(int count)
        {
            using var b = new BufferBuilder<int>();
            for (int i = 0; i < count; i++)
            {
                b.NewChunk(4);
            }
            using var m = b.Build();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [TestCase(10)]
        [TestCase(100)]
        public void BulkNewChunk_MemoryUsage(int count)
        {
            Prime(count);

            // First run: allocates backing arrays.
            var list1 = new BufferBuilder<int>();
            long allocBefore1 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore1 = GC.GetTotalMemory(false);
            for (int i = 0; i < count; i++)
            {
                list1.NewChunk(4);
            }
            long allocated1 = GC.GetAllocatedBytesForCurrentThread() - allocBefore1;
            long memAfter1 = GC.GetTotalMemory(false);

            using (var built1 = list1.Build()) { Assert.That(built1.Memory.Length, Is.EqualTo(count * 4)); }

            TestContext.WriteLine($"BulkNewChunk size={count}: run1 allocated={allocated1} bytes, memory delta={memAfter1 - memBefore1} bytes");

            list1.Dispose();

            // Second run: pool reused, should allocate nothing.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var list2 = new BufferBuilder<int>();
            long allocBefore2 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore2 = GC.GetTotalMemory(false);
            for (int i = 0; i < count; i++)
            {
                list2.NewChunk(4);
            }
            long allocated2 = GC.GetAllocatedBytesForCurrentThread() - allocBefore2;
            long memAfter2 = GC.GetTotalMemory(false);

            using (var built2 = list2.Build()) { Assert.That(built2.Memory.Length, Is.EqualTo(count * 4)); }

            TestContext.WriteLine($"BulkNewChunk size={count}: run2 allocated={allocated2} bytes, memory delta={memAfter2 - memBefore2} bytes");
            Assert.That(allocated2, Is.EqualTo(0));

            list2.Dispose();

            // Third: two builders of half the size.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int half = count / 2;
            long allocBefore3 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore3 = GC.GetTotalMemory(false);

            var a = new BufferBuilder<int>();
            var b = new BufferBuilder<int>();
            for (int i = 0; i < half; i++)
            {
                a.NewChunk(4);
                b.NewChunk(4);
            }
            long allocated3 = GC.GetAllocatedBytesForCurrentThread() - allocBefore3;
            long memAfter3 = GC.GetTotalMemory(false);

            using (var ba = a.Build()) { Assert.That(ba.Memory.Length, Is.EqualTo(half * 4)); }
            using (var bb = b.Build()) { Assert.That(bb.Memory.Length, Is.EqualTo(half * 4)); }

            TestContext.WriteLine($"BulkNewChunk size={count}: two half-builders allocated={allocated3} bytes, memory delta={memAfter3 - memBefore3} bytes");
        }

        [Test]
        public void Build_SecondUse_AllocatesNothing()
        {
            Prime(4);

            using var builder = new BufferBuilder<int>();
            builder.NewChunk(4);

            long before = GC.GetAllocatedBytesForCurrentThread();
            using var built = builder.Build();
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
        }

        [Test]
        public void Resize_SecondUse_AllocatesNothing()
        {
            Prime(4);

            using var builder = new BufferBuilder<int>();
            var chunk = builder.NewChunk(2);

            long before = GC.GetAllocatedBytesForCurrentThread();
            chunk.Resize(4);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
        }
    }
}

[TestFixture]
[TestOf(typeof(BufferChunk<>))]
public class BufferChunkTest
{
    public class Resize
    {
        [Test]
        public void Grow_KeepsExistingData()
        {
            using var builder = new BufferBuilder<int>();
            var chunk = builder.NewChunk(2);
            chunk.Data().Span[0] = 7;
            chunk.Data().Span[1] = 9;

            chunk.Resize(4);

            Assert.Multiple(() =>
            {
                Assert.That(chunk.Data().Length, Is.EqualTo(4));
                Assert.That(chunk.Data().Span[0], Is.EqualTo(7));
                Assert.That(chunk.Data().Span[1], Is.EqualTo(9));
            });
        }

        [Test]
        public void Shrink_UpdatesLength()
        {
            using var builder = new BufferBuilder<int>();
            var chunk = builder.NewChunk(4);
            chunk.Resize(2);

            Assert.That(chunk.Data().Length, Is.EqualTo(2));
        }
    }

    public class Data
    {
        [Test]
        public void ReturnsRequestedLength()
        {
            using var builder = new BufferBuilder<int>();
            var chunk = builder.NewChunk(3);
            Assert.That(chunk.Data().Length, Is.EqualTo(3));
        }
    }
}
