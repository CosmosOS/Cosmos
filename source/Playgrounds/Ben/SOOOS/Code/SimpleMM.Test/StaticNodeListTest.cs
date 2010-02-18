using MM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SimpleMM.Test
{
    
    
    /// <summary>
    ///This is a test class for StaticNodeListTest and is intended
    ///to contain all StaticNodeListTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StaticNodeListTest
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
        ///A test for Init
        ///</summary>
        [TestMethod()]
        public unsafe void InitTest()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value
            
            Assert.AreEqual(0 , (int) target.NumberOfAvailableNodes);

            target.Init();

            Assert.IsTrue(target.NumberOfAvailableNodes > 0);
            Assert.IsTrue(new UIntPtr(target.head) != UIntPtr.Zero , "Head not initialized");

            Assert.AreEqual(CountNodesInChain(target.head ), target.NumberOfAvailableNodes , "Count does not match chain");

         }

        // TestMethod
        internal unsafe uint CountNodesInChain(HoleNode*  head )
        {

            uint count = 0;
            HoleNode* ptr = head;
            while (ptr != null)
            {
                count++;
                ptr = ptr->Next;

                if (count > 100000U)//detect loop
                    throw new InvalidOperationException("chain too long"); 

            }

            return count;



        }

        [TestMethod()]
        public void InitTestTwice()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value


            target.Init();
            target.Init();
        }


        [TestMethod()]
        public unsafe void InitTestTwiceWithRemove()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value


            target.Init();
            var ex = target.Remove();
            target.Init();
        }



        [TestMethod()]
        public unsafe void RemoveTest()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value

            target.Init();

            var count = target.NumberOfAvailableNodes;

            var nodePtr = target.Remove();
            Assert.IsTrue(new UIntPtr(nodePtr) != UIntPtr.Zero, "Node is null");
            Assert.IsTrue(nodePtr->Length == 0 , "Data not initialized" );
            Assert.IsTrue(nodePtr->MemAddress == UIntPtr.Zero, "Data not initialized");
            Assert.IsTrue(new UIntPtr(nodePtr->Next) == UIntPtr.Zero, "Data stil part of list");
            Assert.IsFalse(nodePtr == target.head, "Removed but is still head");  


            // available reduced ? 
            Assert.AreEqual( --count , target.NumberOfAvailableNodes , "Count not reduced");

            Assert.AreEqual(count , CountNodesInChain(target.head) , "Count does not match chain");


        }



        [TestMethod()]
        public unsafe void RemoveAllTest()
        {

            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value


            target.Init();



            var nodePtr = target.Remove();

            var firstNode = nodePtr;

            while (new UIntPtr(nodePtr) != UIntPtr.Zero)
            {
                nodePtr = target.Remove();
            }

            Assert.AreEqual(0U , target.NumberOfAvailableNodes, "Available not zero");

            try
            {
                // lets see what happens
                nodePtr = target.Remove();
                throw new InvalidOperationException("Should throw"); 
            }
            catch (InvalidOperationException) { }

            target.Add(firstNode);
        }


        [TestMethod()]
        public unsafe void AddTest()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value


            target.Init();

            var count =  target.NumberOfAvailableNodes;

            var nodePtr = target.Remove();
            Assert.IsTrue(new UIntPtr(nodePtr) != UIntPtr.Zero, "Node is null");

            count--;
            Assert.AreEqual(count, target.NumberOfAvailableNodes, "Count not reduced");
            Assert.AreEqual(CountNodesInChain(target.head), count, "Count does not match chain after remove");


            target.Add(nodePtr);
            count++;
            Assert.AreEqual(count, target.NumberOfAvailableNodes, "Count not same");
            Assert.AreEqual(count , CountNodesInChain(target.head) , "Count does not match chain after add");


        }


        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod()]
        public unsafe void AddPastEndTest()
        {
            StaticNodeList target = new StaticNodeList(); // TODO: Initialize to an appropriate value


            target.Init();


            var nodePtr = target.Remove();


            target.Add(nodePtr);
            target.Add(nodePtr);


        }

        // add beyond max test

    } //class StaticNodeList
}
