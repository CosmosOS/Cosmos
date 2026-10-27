// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.Structures;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Structures;

[TestFixture]
[TestOf(typeof(RentedList<>))]
public class RentedListTest
{
    public class DefaultConstructor
    {
        [Test]
        public void CreatesEmptyList()
        {
            var list = new RentedList<int>();

            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(list.IsReadOnly, Is.False);
        }
    }

    public class Add
    {
        [Test]
        public void AddsItem_IncrementsCount()
        {
            var list = new RentedList<int>();

            list.Add(42);

            Assert.Multiple(() =>
            {
                Assert.That(list.Count, Is.EqualTo(1));
                Assert.That(list[0], Is.EqualTo(42));
            });
        }

        [Test]
        public void ExceedsInitialCapacity_GrowsAndKeepsItems()
        {
            var list = new RentedList<int>();

            for (int i = 0; i < 50; i++)
            {
                list.Add(i);
            }

            Assert.That(list.Count, Is.EqualTo(50));
            for (int i = 0; i < 50; i++)
            {
                Assert.That(list[i], Is.EqualTo(i));
            }

            list.Dispose();
        }
    }

    public class Clear
    {
        [Test]
        public void ResetsCount()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);

            list.Clear();

            Assert.That(list.Count, Is.EqualTo(0));
            list.Dispose();
        }
    }

    public class Contains
    {
        [Test]
        public void Present_ReturnsTrue()
        {
            var list = new RentedList<int>();
            list.Add(7);

            Assert.That(list.Contains(7), Is.True);
            list.Dispose();
        }

        [Test]
        public void Absent_ReturnsFalse()
        {
            var list = new RentedList<int>();

            Assert.That(list.Contains(7), Is.False);
            list.Dispose();
        }
    }

    public class IndexOf
    {
        [Test]
        public void Found_ReturnsIndex()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.That(list.IndexOf(2), Is.EqualTo(1));
            list.Dispose();
        }

        [Test]
        public void NotFound_ReturnsMinusOne()
        {
            var list = new RentedList<int>();
            list.Add(1);

            Assert.That(list.IndexOf(99), Is.EqualTo(-1));
            list.Dispose();
        }
    }

    public class Remove
    {
        [Test]
        public void Existing_ReturnsTrue_DecrementsCount()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);

            bool result = list.Remove(1);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(list.Count, Is.EqualTo(1));
                Assert.That(list[0], Is.EqualTo(2));
            });
            list.Dispose();
        }

        [Test]
        public void Missing_ReturnsFalse()
        {
            var list = new RentedList<int>();
            list.Add(1);

            Assert.That(list.Remove(99), Is.False);
            list.Dispose();
        }
    }

    public class RemoveAt
    {
        [Test]
        public void ValidIndex_Removes()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.RemoveAt(1);

            Assert.Multiple(() =>
            {
                Assert.That(list.Count, Is.EqualTo(2));
                Assert.That(list[0], Is.EqualTo(1));
                Assert.That(list[1], Is.EqualTo(3));
            });
            list.Dispose();
        }

        [Test]
        public void NegativeIndex_Throws()
        {
            var list = new RentedList<int>();
            list.Add(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
            list.Dispose();
        }
    }

    public class Insert
    {
        [Test]
        public void ValidIndex_Inserts()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(3);

            list.Insert(1, 2);

            Assert.Multiple(() =>
            {
                Assert.That(list.Count, Is.EqualTo(3));
                Assert.That(list[0], Is.EqualTo(1));
                Assert.That(list[1], Is.EqualTo(2));
                Assert.That(list[2], Is.EqualTo(3));
            });
            list.Dispose();
        }

        [Test]
        public void OutOfRange_Throws()
        {
            var list = new RentedList<int>();
            list.Add(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(5, 2));
            list.Dispose();
        }
    }

    public class Indexer
    {
        [Test]
        public void Get_Set_Works()
        {
            var list = new RentedList<int>();
            list.Add(1);

            list[0] = 9;

            Assert.That(list[0], Is.EqualTo(9));
            list.Dispose();
        }

        [Test]
        public void GetOutOfRange_Throws()
        {
            var list = new RentedList<int>();

            Assert.Throws<ArgumentOutOfRangeException>(() => { int _ = list[0]; });
            list.Dispose();
        }

        [Test]
        public void SetOutOfRange_Throws()
        {
            var list = new RentedList<int>();

            Assert.Throws<ArgumentOutOfRangeException>(() => list[0] = 1);
            list.Dispose();
        }
    }

    public class CopyTo
    {
        [Test]
        public void CopiesElements()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);

            int[] array = new int[2];
            list.CopyTo(array, 0);

            Assert.That(array, Is.EqualTo(new[] { 1, 2 }));
            list.Dispose();
        }
    }

    public class Enumeration
    {
        [Test]
        public void YieldsAllItems()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var result = new List<int>();
            foreach (int item in list)
            {
                result.Add(item);
            }

            Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
            list.Dispose();
        }
    }

    public class Dispose
    {
        [Test]
        public void DoesNotThrow()
        {
            var list = new RentedList<int>();
            list.Add(1);

            Assert.DoesNotThrow(() => list.Dispose());
        }
    }

    public class Allocation
    {
        [SetUp]
        public void SetUp()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Test]
        public void Add_WithinCapacity_AllocatesNothing()
        {
            var list = new RentedList<int>();

            long before = GC.GetAllocatedBytesForCurrentThread();
            list.Add(1);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
            list.Dispose();
        }

        [Test]
        public void Clear_AllocatesNothing()
        {
            var list = new RentedList<int>();
            list.Add(1);

            long before = GC.GetAllocatedBytesForCurrentThread();
            list.Clear();
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
            list.Dispose();
        }

        [Test]
        public void IndexerGet_AllocatesNothing()
        {
            var list = new RentedList<int>();
            list.Add(1);

            long before = GC.GetAllocatedBytesForCurrentThread();
            int _ = list[0];
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
            list.Dispose();
        }

        [Test]
        public void Enumeration_AllocatesNothing()
        {
            var list = new RentedList<int>();
            list.Add(1);
            list.Add(2);

            long before = GC.GetAllocatedBytesForCurrentThread();
            foreach (int _ in list) { }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
            list.Dispose();
        }
    }

    public class Performance
    {
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(100000000)]
        public void BulkAdd_MemoryUsage(int count)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // First run: should allocate backing arrays.
            var list1 = new RentedList<int>();
            long allocBefore1 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore1 = GC.GetTotalMemory(false);
            for (int i = 0; i < count; i++)
            {
                list1.Add(i);
            }
            long allocated1 = GC.GetAllocatedBytesForCurrentThread() - allocBefore1;
            long memAfter1 = GC.GetTotalMemory(false);

            Assert.That(list1.Count, Is.EqualTo(count));
            TestContext.WriteLine($"BulkAdd size={count}: run1 allocated={allocated1} bytes, memory delta={memAfter1 - memBefore1} bytes");

            list1.Dispose();

            // Second run: pool reused, should allocate nothing.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var list2 = new RentedList<int>();
            long allocBefore2 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore2 = GC.GetTotalMemory(false);
            for (int i = 0; i < count; i++)
            {
                list2.Add(i);
            }
            long allocated2 = GC.GetAllocatedBytesForCurrentThread() - allocBefore2;
            long memAfter2 = GC.GetTotalMemory(false);

            Assert.That(list2.Count, Is.EqualTo(count));
            TestContext.WriteLine($"BulkAdd size={count}: run2 allocated={allocated2} bytes, memory delta={memAfter2 - memBefore2} bytes");

            list2.Dispose();

            // Third: two lists of half the size.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int half = count / 2;
            long allocBefore3 = GC.GetAllocatedBytesForCurrentThread();
            long memBefore3 = GC.GetTotalMemory(false);

            var a = new RentedList<int>();
            var b = new RentedList<int>();
            for (int i = 0; i < half; i++)
            {
                a.Add(i);
                b.Add(i);
            }
            long allocated3 = GC.GetAllocatedBytesForCurrentThread() - allocBefore3;
            long memAfter3 = GC.GetTotalMemory(false);

            Assert.Multiple(() =>
            {
                Assert.That(a.Count, Is.EqualTo(half));
                Assert.That(b.Count, Is.EqualTo(half));
            });
            TestContext.WriteLine($"BulkAdd size={count}: two half-lists allocated={allocated3} bytes, memory delta={memAfter3 - memBefore3} bytes");

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Clear_IsFast()
        {
            const int count = 10000;
            var list = new RentedList<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }

            var stopwatch = Stopwatch.StartNew();
            list.Clear();
            stopwatch.Stop();

            Assert.Multiple(() =>
            {
                Assert.That(list.Count, Is.EqualTo(0));
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
            });

            list.Dispose();
        }

        [Test]
        public void Enumeration_IsFast()
        {
            const int count = 10000;
            var list = new RentedList<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }

            var stopwatch = Stopwatch.StartNew();
            long before = GC.GetAllocatedBytesForCurrentThread();
            long sum = 0;
            foreach (int item in list)
            {
                sum += item;
            }
            stopwatch.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.Multiple(() =>
            {
                Assert.That(sum, Is.EqualTo((long)count * (count - 1) / 2));
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
            });

            list.Dispose();
            list = new RentedList<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }

            stopwatch = Stopwatch.StartNew();
            before = GC.GetAllocatedBytesForCurrentThread();
            sum = 0;
            foreach (int item in list)
            {
                sum += item;
            }
            allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            stopwatch.Stop();

            Assert.Multiple(() =>
            {
                Assert.That(sum, Is.EqualTo((long)count * (count - 1) / 2));
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
                Assert.That(allocated, Is.EqualTo(0));
            });

            list.Dispose();
        }
    }
}
