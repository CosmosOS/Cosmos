using CoreLib.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CoreLib.ProcessModel;

namespace CoreLib.Test
{


    /// <summary>
    ///This is a test class for CircularFifoQueueWithWaitWithWaitTest and is intended
    ///to contain all CircularFifoQueueWithWaitWithWaitTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CircularFifoQueueWithWaitWithWaitTest
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
        ///A test for CircularFifoQueueWithWait`1 Constructor
        ///</summary>
        public void CircularFifoQueueWithWaitConstructorTestHelper<T>()
        {
            uint capacity = 2;
            CircularFifoQueueWithWait<T> target = new CircularFifoQueueWithWait<T>(capacity);
         

        }

        [TestMethod()]
        public void CircularFifoQueueWithWaitConstructorTest()
        {
            CircularFifoQueueWithWaitConstructorTestHelper<GenericParameterHelper>();
        }





        [TestMethod()]
        public void FullTest()
        {
            uint capacity = 4;
            CircularFifoQueueWithWait<uint> target = new CircularFifoQueueWithWait<uint>(capacity);

            target.Enqueue(ref capacity);
            target.Enqueue(ref capacity);
            target.Enqueue(ref capacity);
            target.Enqueue(ref capacity);

            target.Dequeue(ref capacity);
            target.Dequeue(ref capacity);
            target.Dequeue(ref capacity);
            target.Dequeue(ref capacity);

        }


        [TestMethod()]
        public void FullTestCapacity1()
        {
            uint capacity = 1;
            CircularFifoQueueWithWait<uint> target = new CircularFifoQueueWithWait<uint>(capacity);
      

            target.Enqueue(ref capacity);

            target.Dequeue(ref capacity);


        }



        [TestMethod()]
        public void FullTestCapacity1FillTwice()
        {
            uint capacity = 1;
            CircularFifoQueueWithWait<uint> target = new CircularFifoQueueWithWait<uint>(capacity);
            target.Enqueue(ref capacity);

            target.Dequeue(ref capacity);

            target.Enqueue(ref capacity);

            target.Dequeue(ref capacity);
        }


        [TestMethod()]
        public void Test5()
        {
            LoopTest(5);
        }

        [TestMethod()]
        public void LoopTest()
        {
            for (uint i = 1; i < 100; i++)
                LoopTest(i);
        }

        public void LoopTest(uint capacity)
        {

            CircularFifoQueueWithWait<uint> target = new CircularFifoQueueWithWait<uint>(100);
          

            // target.Enqueue(ref capacity);
            // target.Enqueue(ref capacity);

            uint outval = 0;

            for (int i = 0; i < capacity; i++)
            {
                target.Enqueue(ref  capacity);
            }


            int count = 0;
            for (int i = 0; i < capacity; i++)
            {
                target.Dequeue(ref outval); // blocking
                count++;
            }
            //child.Join();
        }


        //[TestMethod()]
        //public void ThreadTest1()
        //{
        //    Assert.IsTrue(ThreadTest(1));
        //}

        //[TestMethod()]
        //public void ThreadTest3()
        //{
        //    Assert.IsTrue(ThreadTest(3));
        //}

        //[TestMethod()]
        //public void ThreadedTest()
        //{
        //    for (uint i = 1; i < 100; i++)
        //        Assert.IsTrue(ThreadTest(i));
        //}


        //public bool ThreadTest(uint capacity)
        //{

        //    CircularFifoQueueWithWait<uint> target = new CircularFifoQueueWithWait<uint>(100);
        //    Assert.IsTrue(target.IsEmpty());

        //    // target.Enqueue(ref capacity);
        //    // target.Enqueue(ref capacity);

        //    int count = 0;
        //    uint inval = 3;

        //    var child = new System.Threading.Thread(
        //        o =>
        //        {
        //            var queue = (o as CircularFifoQueueWithWait<uint>);


        //            while (target.IsEmpty() == false)
        //            {
        //                uint outval = 0;
        //                if (target.Dequeue(ref outval))
        //                    count++;
        //            }


        //        });
        //    child.Start(target);

        //    for (int i = 0; i < capacity; i++)
        //    {
        //        target.Enqueue(ref  inval);
        //    }
        //    child.Join();

        //    Assert.IsTrue( target.IsEmpty());
        //    return count == capacity;
        //}


    }
}
