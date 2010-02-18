//using Cosmos.Kernel.MM;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using Cosmos.Kernel.Dispatch;
//using Cosmos.Kernel;

//namespace Test.Cosmos.Kernel
//{
    
    
//    /// <summary>
//    ///This is a test class for SoftwarePageManagerTest and is intended
//    ///to contain all SoftwarePageManagerTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class SoftwarePageManagerTest
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
//        ///A test for Cosmos.Kernel.MM.IPagingManager.ReAllocatesMemory
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void ReAllocatesMemoryTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.ReAllocatesMemory;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Cosmos.Kernel.MM.IPagingManager.Details
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void DetailsTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            MemoryUsage actual;
//            actual = target.Details;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for OnMemoryPressureChanged
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void OnMemoryPressureChangedTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            SoftwarePageManager_Accessor target = new SoftwarePageManager_Accessor(param0); // TODO: Initialize to an appropriate value
//            object sender = null; // TODO: Initialize to an appropriate value
//            EventArgs<MemoryPressure> args = null; // TODO: Initialize to an appropriate value
//            target.OnMemoryPressureChanged(sender, args);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for Cosmos.Kernel.MM.IPagingManager.RequestPages
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void RequestPagesTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            MemoryAllocationRequest request = null; // TODO: Initialize to an appropriate value
//            UIntPtr expected = new UIntPtr(); // TODO: Initialize to an appropriate value
//            UIntPtr actual;
//            actual = target.RequestPages(request);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Cosmos.Kernel.MM.IPagingManager.QueryPage
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void QueryPageTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            UIntPtr page = new UIntPtr(); // TODO: Initialize to an appropriate value
//            PageEntry expected = new PageEntry(); // TODO: Initialize to an appropriate value
//            PageEntry actual;
//            actual = target.QueryPage(page);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Cosmos.Kernel.MM.IPagingManager.FreePages
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void FreePagesTest1()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            Process process = null; // TODO: Initialize to an appropriate value
//            PageRegion region = new PageRegion(); // TODO: Initialize to an appropriate value
//            target.FreePages(process, region);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for Cosmos.Kernel.MM.IPagingManager.FreePages
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void FreePagesTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value
//            IPagingManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory); // TODO: Initialize to an appropriate value
//            Process process = null; // TODO: Initialize to an appropriate value
//            target.FreePages(process);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for ComputePressure
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("Cosmos.Kernel.dll")]
//        public void ComputePressureTest()
//        {
//            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
//            SoftwarePageManager_Accessor target = new SoftwarePageManager_Accessor(param0); // TODO: Initialize to an appropriate value
//            MemoryPressure expected = new MemoryPressure(); // TODO: Initialize to an appropriate value
//            MemoryPressure actual;
//            actual = target.ComputePressure();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for SoftwarePageManager Constructor
//        ///</summary>
//        [TestMethod()]
//        public void SoftwarePageManagerConstructorTest()
//        {
//            uint pageSize = 0; // TODO: Initialize to an appropriate value
//            UIntPtr baseAddress = new UIntPtr(); // TODO: Initialize to an appropriate value
//            ulong managedMemory = 0; // TODO: Initialize to an appropriate value



//            SoftwarePageManager target = new SoftwarePageManager(pageSize, baseAddress, managedMemory);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }
//    }
//}
