using MM.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MM;

namespace SimpleMM.Test
{
    
    
    /// <summary>
    ///This is a test class for MemoryManagerTest and is intended
    ///to contain all MemoryManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MemoryManagerTest
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


        //TODO make test instance based


        [TestMethod()]
        public unsafe void InitTestOneRegion()
        {
            ulong bufSize = 100000;

            //allocating actual memory not needed as its not used
            byte[] testBlock = new byte[bufSize];
            fixed (void* memPtr = testBlock)
            {
                UIntPtr baseAddress = new UIntPtr(memPtr);

                MemoryManager.Init(baseAddress, bufSize); // TODO: Initialize to an appropriate value

                Assert.IsTrue(MemoryManager.BytesManaged > bufSize / 10 * 9, "Bytes managed should be at least 90% buffer"); 

                var regions = new SimpleDiagnosticProvider().GetFreeList();

                Assert.IsNotNull(regions);
                CollectionAssert.AllItemsAreNotNull(regions);
                Assert.AreNotEqual(regions.Length, 0 , "Must not be empty");
             //   var details = target.;

                //Assert.AreEqual(pages, details.numPages);
            }

            //UIntPtr baseAddress = new UIntPtr(1000000); //random address

            //MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize); // TODO: Initialize to an appropriate value


            //var details = target.ComputeDetails();

            //Assert.AreEqual(pages, details.numPages);
            //Assert.AreEqual(pageSize, details.pageSize);
            //Assert.AreEqual(pageSize * pages, details.memoryManaged);
            //Assert.AreEqual(pageSize * pages, details.largestSegment);




        }




       // MemoryRegion[] GetFreeList(); 

        /// <summary>
        ///A test for Allocate
        ///</summary>
        [TestMethod()]
        public void AllocateTest()
        {
            uint amountOfBytes = 0; // TODO: Initialize to an appropriate value
            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
            UIntPtr actual;
            actual = MemoryManager.Allocate(amountOfBytes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Free
        ///</summary>
        [TestMethod()]
        public void FreeTest()
        {
            UIntPtr address = new UIntPtr(); // TODO: Initialize to an appropriate value
            uint sizeCheck = 0; // TODO: Initialize to an appropriate value
            MemoryManager.Free(address, sizeCheck);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
