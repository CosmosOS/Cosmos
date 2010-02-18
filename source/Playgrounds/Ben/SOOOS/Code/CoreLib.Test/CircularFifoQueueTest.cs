using CoreLib.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CoreLib.ProcessModel;

namespace CoreLib.Test
{


    /// <summary>
    ///This is a test class for CircularFifoQueueTest and is intended
    ///to contain all CircularFifoQueueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CircularFifoQueueTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CircularFifoQueue`1 Constructor
        ///</summary>
        public void CircularFifoQueueConstructorTestHelper<T>()
        {
            uint capacity = 1;
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity);
            Assert.IsTrue(target.IsEmpty);
            Assert.IsFalse(target.IsFull);

        }

        [TestMethod()]
        public void CircularFifoQueueConstructorTest()
        {
            CircularFifoQueueConstructorTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Dequeue
        ///</summary>
        public void DequeueTestHelper<T>()
        {
            uint capacity = 0; // TODO: Initialize to an appropriate value
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity); // TODO: Initialize to an appropriate value
            T item = default(T); // TODO: Initialize to an appropriate value
            T itemExpected = default(T); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Dequeue(ref item);
            Assert.AreEqual(itemExpected, item);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void DequeueTest()
        {
            DequeueTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Enqueue
        ///</summary>
        public void EnqueueTestHelper<T>()
        {
            uint capacity = 0; // TODO: Initialize to an appropriate value
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity); // TODO: Initialize to an appropriate value
            T item = default(T); // TODO: Initialize to an appropriate value
            T itemExpected = default(T); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Enqueue(ref item);
            Assert.AreEqual(itemExpected, item);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void EnqueueTest()
        {
            EnqueueTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Increment
        ///</summary>
        public void IncrementTestHelper<T>()
        {
            uint capacity = 0; // TODO: Initialize to an appropriate value
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity); // TODO: Initialize to an appropriate value
            uint idx_ = 0; // TODO: Initialize to an appropriate value
            uint expected = 0; // TODO: Initialize to an appropriate value
            uint actual;
            actual = target.Increment(idx_);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void IncrementTest()
        {
            IncrementTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        public void IsEmptyTestHelper<T>()
        {
            uint capacity = 0; // TODO: Initialize to an appropriate value
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsEmpty;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void IsEmptyTest()
        {
            IsEmptyTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for IsFull
        ///</summary>
        public void IsFullTestHelper<T>()
        {
            uint capacity = 0; // TODO: Initialize to an appropriate value
            CircularFifoQueue<T> target = new CircularFifoQueue<T>(capacity); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsFull;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void IsFullTest()
        {
            IsFullTestHelper<GenericParameterHelper>();
        }





        [TestMethod()]
        public void FullTest()
        {
            uint capacity = 4;
            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(capacity);
            Assert.IsTrue(target.IsEmpty);

            target.Enqueue(ref capacity);
            target.Enqueue(ref capacity);



            var result = target.Enqueue(ref capacity);

            Assert.IsTrue(result);

            result = target.Enqueue(ref capacity);

            Assert.IsTrue(result);

            result = target.Enqueue(ref capacity);

            Assert.IsFalse(result);


            target.Dequeue(ref capacity);
            target.Dequeue(ref capacity);
            target.Dequeue(ref capacity);

            Assert.IsFalse(target.IsEmpty);


            target.Dequeue(ref capacity);

            Assert.IsTrue(target.IsEmpty);

            //actual = target.Enqueue(ref item);
            //Assert.AreEqual(itemExpected, item);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");

        }


        [TestMethod()]
        public void FullTestCapacity1()
        {
            uint capacity = 1;
            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(capacity);
            Assert.IsTrue(target.IsEmpty);

            var result = target.Enqueue(ref capacity);


            Assert.IsTrue(target.IsFull);

            Assert.IsTrue(result);
            result = target.Dequeue(ref capacity);

            Assert.IsTrue(result);


            Assert.IsTrue(target.IsEmpty);
        }



        [TestMethod()]
        public void FullTestCapacity1FillTwice()
        {
            uint capacity = 1;
            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(capacity);
            Assert.IsTrue(target.IsEmpty);

            var result = target.Enqueue(ref capacity);


            Assert.IsTrue(target.IsFull);

            Assert.IsTrue(result);
            result = target.Dequeue(ref capacity);

            Assert.IsTrue(result);


            result = target.Enqueue(ref capacity);


            Assert.IsTrue(target.IsFull);

            Assert.IsTrue(result);
            result = target.Dequeue(ref capacity);

            Assert.IsTrue(result);


            Assert.IsTrue(target.IsEmpty);
        }


        [TestMethod()]
        public void Test5()
        {
            Assert.IsTrue(LoopTest(5));
        }

        [TestMethod()]
        public void LoopTest()
        {
            for (uint i = 1; i < 100; i++)
                Assert.IsTrue(LoopTest(i));
        }

        public bool LoopTest(uint capacity)
        {

            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(100);
            Assert.IsTrue(target.IsEmpty);

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            uint outval = 0;

            for (int i = 0; i < capacity; i++)
            {
                target.Enqueue(ref  capacity);
            }


            int count = 0;
            while (target.IsEmpty == false)
            {
                if (target.Dequeue(ref outval))
                    count++;
            }
            //child.Join();
            return count == capacity;
        }


        [TestMethod()]
        public void ThreadTest1()
        {
            Assert.IsTrue(ThreadTest(1));
        }

        [TestMethod()]
        public void ThreadTest3()
        {
            Assert.IsTrue(ThreadTest(3));
        }

        [TestMethod()]
        public void ThreadedTest()
        {
            for (uint i = 1; i < 100; i++)
                Assert.IsTrue(ThreadTest(i));
        }


        public bool ThreadTest(uint capacity)
        {

            CircularFifoQueue<uint> target = new CircularFifoQueue<uint>(100);
            Assert.IsTrue(target.IsEmpty);

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            int count = 0;
            uint inval = 3;

            var child = new System.Threading.Thread(
                o =>
                {
                    var queue = (o as CircularFifoQueue<uint>);


                    while (target.IsEmpty == false)
                    {
                        uint outval = 0;
                        if (target.Dequeue(ref outval))
                            count++;
                    }


                });
            child.Start(target);

            for (int i = 0; i < capacity; i++)
            {
                target.Enqueue(ref  inval);
            }
            child.Join();

            Assert.IsTrue( target.IsEmpty);
            return count == capacity;
        }


    }
}
