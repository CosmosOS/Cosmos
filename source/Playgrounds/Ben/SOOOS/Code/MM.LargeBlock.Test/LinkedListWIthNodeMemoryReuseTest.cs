
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MM.LargeBlock;

namespace MM.LargeBlock.Test
{
    
    
    /// <summary>
    ///This is a test class for LinkedListWIthNodeMemoryReuseTest and is intended
    ///to contain all LinkedListWIthNodeMemoryReuseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinkedListWIthNodeMemoryReuseTest
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
        ///A test for Remove
        ///</summary>
        ///
        [TestMethod()]
      //  [ExpectedException(typeof(System.ArgumentNullException))]
        public void RemoveNullTest()
        {
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            string val = null; // TODO: Initialize to an appropriate value
            bool actual = target.Remove(val);
            Assert.AreEqual(actual , false);
        }



        [TestMethod()]
        public void RemoveUnknownTest()
        {
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            string val = null; // TODO: Initialize to an appropriate value
            bool actual = target.Remove("Unknown");
            Assert.AreEqual(actual, false);
        }



        [TestMethod()]
        public void RemoveTest()
        {
            string val = "Expected"; 
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            target.AddLast(val);
            bool actual = target.Remove(val);
            Assert.AreEqual(actual, true);
            Assert.AreEqual(0 , target.Count);
        }


        [TestMethod()]
        public void AddLastNullTest()
        {

            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            var actual = target.AddFirst((string) null);
            Assert.AreEqual(1 , target.Count);
        }

        [TestMethod()]
        public void AddAndFindTest()
        {
            string val = "Test"; 
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            var actual = target.AddFirst(val);

            var node = target.Find(val);
            Assert.IsNotNull(node); 
            Assert.AreEqual(node.Value, val); 
            Assert.AreEqual(1, target.Count);
        }


        [TestMethod()]
        public void AddFirstTest()
        {
            string val = "Test";
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            var actual = target.AddFirst(val);

            Assert.AreEqual(target.First.Value, val);
        }


        [TestMethod()]
        public void AddLastTest()
        {
            string val = "Test";
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            var actual = target.AddLast(val);
            Assert.AreEqual(target.Last.Value, val);


        }

        [TestMethod()]
        public void AddBeforeTest()
        {
            string val = "Test";
            LinkedListWithNodeMemoryReuse<string> target = new LinkedListWithNodeMemoryReuse<string>(); // TODO: Initialize to an appropriate value
            target.AddFirst("First");
            var actual = target.AddBefore( target.First , val);

            Assert.AreEqual(target.First.Value, val);
            Assert.AreEqual(target.First.Next.Value, "First");
            Assert.AreEqual(2, target.Count);

        }

      
        [TestMethod()]
        [DeploymentItem("Cosmos.Kernel.dll")]
        public void AllocateResevereTest()
        {
       
            LinkedListWithNodeMemoryReuse_Accessor<string> target = new LinkedListWithNodeMemoryReuse_Accessor<string>(100); // TODO: Initialize to an appropriate value
            //uint reservedCount = 100; 
            //target.AllocateMemoryForNodes(reservedCount);
            Assert.AreEqual(100 ,  target.reusedNodeList.Count);
        }


        [TestMethod()]
        [DeploymentItem("Cosmos.Kernel.dll")]
        public void RemoveNodeAndReuse()
        {

            LinkedListWithNodeMemoryReuse_Accessor<string> target = new LinkedListWithNodeMemoryReuse_Accessor<string>(); // TODO: Initialize to an appropriate value
            //uint reservedCount = 100; 

            int reuseCount = target.reusedNodeList.Count;
            string val = "test"; 
            target.AddFirst(val);
            Assert.AreEqual(reuseCount -1 , target.reusedNodeList.Count);
        }

        [TestMethod()]
        [DeploymentItem("Cosmos.Kernel.dll")]
        public void RemoveNodeAndReuse2()
        {

            LinkedListWithNodeMemoryReuse_Accessor<string> target = new LinkedListWithNodeMemoryReuse_Accessor<string>(); // TODO: Initialize to an appropriate value
            //uint reservedCount = 100; 

            int reuseCount = target.reusedNodeList.Count;
            string val = "test";
            target.AddFirst(new  LinkedListNode<string>(val));
            Assert.AreEqual(reuseCount , target.reusedNodeList.Count , "reuse count was changed when adding extrenal node");

            target.RemoveWithReuse(val);
           // Assert.AreEqual(0, target.Count); 
            Assert.AreEqual(reuseCount + 1, target.reusedNodeList.Count, "reuse count was not increased");

 

        }



        // remove and reuse test

        // fopr removes we have Remove node  which means it doesnt get reused it is removed we also have new method Remove with reuse

       


    }
}
