using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class QueueTest
    {
        public static void Execute()
        {
            var xQueue = new Queue<int>();
            Assert.AreEqual(0, xQueue.Count, "After creating, Queue.Count != 0");
            xQueue.Enqueue(1);
            xQueue.Enqueue(2);
            xQueue.Enqueue(3);
            Assert.AreEqual(3, xQueue.Count, "After adding 3 items, Queue.Count != 3");
            Assert.AreEqual(1, xQueue.Peek(), "Peeking first item != 1");
            Assert.AreEqual(1, xQueue.Peek(), "Peeking first item for the second time != 1");

            var xPop = xQueue.Dequeue();
            Assert.AreEqual(1, xPop, "Popping first item != 1");
            xPop = xQueue.Dequeue();
            Assert.AreEqual(2, xPop, "Popping second item != 2");
            xPop = xQueue.Dequeue();
            Assert.AreEqual(3, xPop, "Popping third item != 3");
            Assert.AreEqual(0, xQueue.Count, "After popping all items, Queue.Count != 0");
        }
    }
}