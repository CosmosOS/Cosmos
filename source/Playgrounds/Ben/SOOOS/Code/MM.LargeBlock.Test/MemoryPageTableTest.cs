//using Cosmos.Kernel.MM;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using Cosmos.Kernel.Dispatch;
//using System.Collections.Generic;

//namespace Test.Cosmos.Kernel
//{
    
    
//    /// <summary>
//    ///This is a test class for MemoryPageTableTest and is intended
//    ///to contain all MemoryPageTableTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class MemoryPageTableTest
//    {


//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion


//        /// <summary>
//        ///A test for UpdatePageTable
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void UpdatePageTableTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            PageType type = new PageType(); // TODO: Initialize to an appropriate value
//            PageSharing sharing = new PageSharing(); // TODO: Initialize to an appropriate value
//            uint pid = 0; // TODO: Initialize to an appropriate value
//            uint numPages = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseMemoryAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            target.UpdatePageTable(type, sharing, pid, numPages, baseMemoryAddress);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for TryMerge
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void TryMergeTest()
//        {
//            // Private Accessor for TryMerge is not found. Please rebuild the containing project or run the Publicize.exe manually.
//            Assert.Inconclusive("Private Accessor for TryMerge is not found. Please rebuild the containing project" +
//                    " or run the Publicize.exe manually.");
//        }

//        /// <summary>
//        ///A test for QueryPage
//        ///</summary>
//        [TestMethod()]
//        public void QueryPageTest1()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            UIntPtr page = new UIntPtr(); // TODO: Initialize to an appropriate value
//            int pageOffset = 0; // TODO: Initialize to an appropriate value
//            PageEntry expected = new PageEntry(); // TODO: Initialize to an appropriate value
//            PageEntry actual;
//            actual = target.QueryPage(page, pageOffset);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for QueryPage
//        ///</summary>
//        [TestMethod()]
//        public void QueryPageTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            UIntPtr page = new UIntPtr(); // TODO: Initialize to an appropriate value
//            PageEntry expected = new PageEntry(); // TODO: Initialize to an appropriate value
//            PageEntry actual;
//            actual = target.QueryPage(page);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for ParseForAdjacent
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void ParseForAdjacentTest()
//        {
//            // Private Accessor for ParseForAdjacent is not found. Please rebuild the containing project or run the Publicize.exe manually.
//            Assert.Inconclusive("Private Accessor for ParseForAdjacent is not found. Please rebuild the containing" +
//                    " project or run the Publicize.exe manually.");
//        }

//        /// <summary>
//        ///A test for PageTableToMemoryAddress
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void PageTableToMemoryAddressTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            int page = 0; // TODO: Initialize to an appropriate value
//            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr actual;
//            actual = target.PageTableToMemoryAddress(page);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for MemoryToPageTableAddress
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void MemoryToPageTableAddressTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            UIntPtr physical = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr actual;
//            actual = target.MemoryToPageTableAddress(physical);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for LargestMemoryBlock
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void LargestMemoryBlockTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            ulong expected = 0; // TODO: Initialize to an appropriate value
//            ulong actual;
//            actual = target.LargestMemoryBlock();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for IsAdjacent
//        ///</summary>
//        [TestMethod()]
//        public void IsAdjacentTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            PageRegion region1 = new PageRegion(); // TODO: Initialize to an appropriate value
//            PageRegion region2 = new PageRegion(); // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.IsAdjacent(region1, region2);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for InitPageTable
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void InitPageTableTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            target.InitPageTable();
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for Init
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void InitTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            target.Init();
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for GetFragmentation
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void GetFragmentationTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            float expected = 0F; // TODO: Initialize to an appropriate value
//            float actual;
//            actual = target.GetFragmentation();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetAmountFree
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void GetAmountFreeTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            float expected = 0F; // TODO: Initialize to an appropriate value
//            float actual;
//            actual = target.GetAmountFree();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for FindSmallestMatchingNode
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void FindSmallestMatchingNodeTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            PageRegion_Accessor expected = null; // TODO: Initialize to an appropriate value
//            PageRegion_Accessor actual;
//            actual = target.FindSmallestMatchingNode(pages);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for DeAllocateMemory
//        ///</summary>
//        [TestMethod()]
//        public void DeAllocateMemoryTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            PageRegion region = new PageRegion(); // TODO: Initialize to an appropriate value
//            uint pid = 0; // TODO: Initialize to an appropriate value
//            target.DeAllocateMemory(region, pid);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

    
//        /// <summary>
//        ///A test for MemoryPageTable Constructor
//        ///</summary>
//        [TestMethod()]
//        public void MemoryPageTableConstructorTest1()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }

//        /// <summary>
//        ///A test for MemoryPageTable Constructor
//        ///</summary>
//        [TestMethod()]
//        public void MemoryPageTableConstructorTest()
//        {

           



//            uint pages = 12;
//            uint pageSize = 1 << 20;

//            //allocating actual memory not needed as its not used
//            //byte[] testBlock = new byte[pages * pageSize];
//            //fixed (void* memPtr = testBlock)
//            //{
//            //    UIntPtr baseAddress = new UIntPtr(memPtr);

//            //    MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize); // TODO: Initialize to an appropriate value

//            //    var details = target.ComputeDetails();

//            //    Assert.AreEqual(pages, details.numPages);
//            //}

//            UIntPtr baseAddress = new UIntPtr(1000000); //random address

//                MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize); // TODO: Initialize to an appropriate value


//                var details = target.ComputeDetails();
             
//                Assert.AreEqual(pages, details.numPages);
//                Assert.AreEqual(pageSize, details.pageSize);
//                Assert.AreEqual(pageSize* pages, details.memoryManaged);
//                Assert.AreEqual(pageSize * pages , details.largestSegment);

                
//            //details.largestSegment = LargestMemoryBlock();

//            //details.memoryManaged = m_pages * PageEntry.PAGE_ENTRY_SIZE;
//            //details.fragmentation = GetFragmentation();
//            //details.amountFree = GetAmountFree();
//            //details.pageSize = m_pageSize; 
//        }

//        /// <summary>
//        ///A test for AddAndMergeNode
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void AddAndMergeNodeTest()
//        {
//            // Private Accessor for AddAndMergeNode is not found. Please rebuild the containing project or run the Publicize.exe manually.
//            Assert.Inconclusive("Private Accessor for AddAndMergeNode is not found. Please rebuild the containing " +
//                    "project or run the Publicize.exe manually.");
//        }

//        /// <summary>
//        ///A test for AddNode
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void AddNodeTest()
//        {
//            // Private Accessor for AddNode is not found. Please rebuild the containing project or run the Publicize.exe manually.
//            Assert.Inconclusive("Private Accessor for AddNode is not found. Please rebuild the containing project " +
//                    "or run the Publicize.exe manually.");
//        }

//        /// <summary>
//        ///A test for AllocateMemory
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void AllocateMemoryTest1()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable_Accessor target = new MemoryPageTable_Accessor(param0); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            PageType type = new PageType(); // TODO: Initialize to an appropriate value
//            PageSharing sharing = new PageSharing(); // TODO: Initialize to an appropriate value
//            uint pid = 0; // TODO: Initialize to an appropriate value
//            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr actual;
//            actual = target.AllocateMemory(pages, type, sharing, pid);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for MemoryPageTable Constructor
//        ///</summary>
//        [TestMethod()]
//        public void MemoryPageTableConstructorTest2()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            LinkedList<PageRegion> list = null; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize, list);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }




//        /// <summary>
//        ///A test for AllocateMemory
//        ///</summary>
//        [TestMethod()]
//        public void AllocateMemoryTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(1<<20); 
//            uint pages = 12; 
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages , 1<<20); // TODO: Initialize to an appropriate value
//            ulong bytes = 0; // TODO: Initialize to an appropriate value
//            ulong alignment = 0; // TODO: Initialize to an appropriate value
//            PageType type = new PageType(); // TODO: Initialize to an appropriate value
//            PageSharing sharing = new PageSharing(); // TODO: Initialize to an appropriate value
//            uint pid = 0; // TODO: Initialize to an appropriate value
//            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr actual;
//            actual = target.AllocateMemory(bytes, alignment, type, sharing, pid);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }



//        /// <summary>
//        ///A test for ComputeDetails
//        ///</summary>
//        [TestMethod()]
//        public void ComputeDetailsTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            MemoryDetails expected = new MemoryDetails(); // TODO: Initialize to an appropriate value
//            MemoryDetails actual;
//            actual = target.ComputeDetails();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for DeAllocateAllMemoryForProcess
//        ///</summary>
//        [TestMethod()]
//        public void DeAllocateAllMemoryForProcessTest()
//        {
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            uint pages = 0; // TODO: Initialize to an appropriate value
//            MemoryPageTable target = new MemoryPageTable(baseAddress, pages); // TODO: Initialize to an appropriate value
//            Process process = null; // TODO: Initialize to an appropriate value
//            target.DeAllocateAllMemoryForProcess(process);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }
//    }
//}
