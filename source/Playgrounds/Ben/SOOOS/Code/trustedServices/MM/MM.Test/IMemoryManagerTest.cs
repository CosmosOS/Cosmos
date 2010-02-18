using MM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MM.Test
{
    // multiple tests here woth onot 


    //eg   SimpleTest : IMemoryMangerTest

    [TestClass()]
    public class SimpleMMTest : IMemoryManagerTest
    {


        public SimpleMMTest() : base ( new Simple.SimpleMemoryManager()) 
        {

        }
    }
    
    /// <summary>
    ///This is a test class for IMemoryManagerTest and is intended
    ///to contain all IMemoryManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IMemoryManagerTest
    {
        IMemoryManager algorithm ; // default mm

        public IMemoryManagerTest() : this( new Simple.SimpleMemoryManager())
        {

        }


        public IMemoryManagerTest(IMemoryManager algorithm)
        {
            this.algorithm = algorithm;

        }

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


        internal virtual IMemoryManager CreateIMemoryManager()
        {
            return algorithm;
        }

        //CreateIMemoryManager

        // [TestMethod()]
        //public unsafe void InitTestOneRegion()
        //{
        //    ulong bufSize = 100000;

        //    //allocating actual memory not needed as its not used
        //    byte[] testBlock = new byte[bufSize];
        //    fixed (void* memPtr = testBlock)
        //    {
        //        UIntPtr baseAddress = new UIntPtr(memPtr);

        //        MemoryManager.Init(baseAddress, bufSize); // TODO: Initialize to an appropriate value

        //        Assert.IsTrue(MemoryManager.BytesManaged > bufSize / 10 * 9, "Bytes managed should be at least 90% buffer"); 

        //        var regions = MemoryManager.GetFreeList();

        //        Assert.IsNotNull(regions);
        //        CollectionAssert.AllItemsAreNotNull(regions);
        //        Assert.AreNotEqual(regions.Length, 0 , "Must not be empty");
        //     //   var details = target.;

        //        //Assert.AreEqual(pages, details.numPages);
        //    }


        [TestMethod()]
        public unsafe void InitTestOneRegion()
        {
         
                var target =  CreateIMemoryManager();
                var testRegions = GetDummyRegions();
                var memGranted = MemoryInRegions(testRegions);     
            
                target.Init( testRegions   );

                Assert.IsTrue(target.DiagnosticProvider.Details.BytesManaged == memGranted , "memory managed is less");            
                Assert.IsTrue(target.DiagnosticProvider.Details.BytesFree >= memGranted / 10 * 9, "Bytes managed should be at least 90% buffer"); 

                var regions = target.DiagnosticProvider.GetFreeList();

                Assert.IsNotNull(regions);
                CollectionAssert.AllItemsAreNotNull(regions);
                Assert.AreEqual(testRegions.Length , regions.Length , "Must not be empty");

                Assert.AreEqual(memGranted , MemoryInRegions(regions), "memory in use does not match granted");
             //   var details = target.;

                //Assert.AreEqual(pages, details.numPages);
            }


        [TestMethod()]
        public unsafe void AllocateTest()
        {
            uint allocateBlockSize = 1024; 
            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();
            
            target.Init(testRegions);
       
            var bytesFree = target.DiagnosticProvider.Details.BytesFree; 
       
            // allocator 
            var result = target.Allocate(allocateBlockSize);

            Assert.IsNotNull(result);
            
            Assert.AreEqual(bytesFree-allocateBlockSize, target.DiagnosticProvider.Details.BytesFree , "Memory available not reduced by block");

            var memGranted = MemoryInRegions(target.DiagnosticProvider.GetFreeList());
            Assert.AreEqual(target.DiagnosticProvider.Details.BytesFree, memGranted, "Memory available in blocks doesnt match count");
        }

        [TestMethod()]
        public unsafe void AllocateReleaseTest()
        {

            uint allocateBlockSize = 1024;
            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();

            target.Init(testRegions);

            var initialBytesFree = target.DiagnosticProvider.Details.BytesFree;

            // allocator 
            var result = target.Allocate(allocateBlockSize);
            target.Free(result, allocateBlockSize); 

            Assert.AreEqual(initialBytesFree , target.DiagnosticProvider.Details.BytesFree, "Memory not starting ");

            var memGranted = MemoryInRegions(target.DiagnosticProvider.GetFreeList());
            Assert.AreEqual(target.DiagnosticProvider.Details.BytesFree, memGranted, "Memory available in blocks doesnt match count");

            var regions = target.DiagnosticProvider.GetFreeList();

            Assert.IsNotNull(regions);
            CollectionAssert.AllItemsAreNotNull(regions);
            Assert.AreEqual(testRegions.Length, regions.Length, "Must not be empty");


        }


        [TestMethod()]
        public unsafe void AllocateExactTest()
        {

         
            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();

            target.Init(testRegions);

            var initialBytesFree = target.DiagnosticProvider.Details.BytesFree;

            // allocator 
            var result = target.Allocate(testRegions[0].Size);
            Assert.AreNotEqual(UIntPtr.Zero, result);  
        }

        [TestMethod()]
        public unsafe void AllocateToManyTest()
        {


            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();

            target.Init(testRegions);

            var initialBytesFree = target.DiagnosticProvider.Details.BytesFree;

            // allocator 
            var result = target.Allocate(testRegions[0].Size * 2);

            Assert.AreEqual(UIntPtr.Zero , result); 
        }

        [TestMethod()]
        public unsafe void MultAllocateReleaseTest()
        {

            uint allocateBlockSize = 1024;
            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();

            target.Init(testRegions);

            var initialBytesFree = target.DiagnosticProvider.Details.BytesFree;

            // allocator 
            var result = target.Allocate(allocateBlockSize);
            var result2 = target.Allocate(allocateBlockSize*2);
            var result3 = target.Allocate(allocateBlockSize/2);
            var result4 = target.Allocate(allocateBlockSize/4);
            target.Free(result, allocateBlockSize);
            target.Free(result2, allocateBlockSize*2);
            target.Free(result3, allocateBlockSize/2);
            target.Free(result4, allocateBlockSize/4);

            var regions = target.DiagnosticProvider.GetFreeList();

            Assert.IsNotNull(regions);
            CollectionAssert.AllItemsAreNotNull(regions);
            Assert.AreEqual(testRegions.Length, regions.Length, "Must not be empty");

             

        }

        //TODO alocatopr specific move to SImple tests
        [TestMethod()]
        public unsafe void MergeTest()
        {

          
            var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();

            target.Init(testRegions);

            var initialBytesFree = target.DiagnosticProvider.Details.BytesFree;

            // allocator 
            var result = target.Allocate(128);
            var result2 = target.Allocate(128);
            var result3 = target.Allocate(128);
            var result4 = target.Allocate(128);
            var result5 = target.Allocate(128);
            var result6 = target.Allocate(128);


            var regions = target.DiagnosticProvider.GetFreeList();
            Assert.AreEqual(1, regions.Length, "Not fragmented");


            target.Free(result2,  128);
            target.Free(result4, 128);
            target.Free(result6, 128);
            // should be fragmented


            regions = target.DiagnosticProvider.GetFreeList();
            Assert.AreEqual(3, regions.Length, "Not fragmented");  //2 , 3, 6  and remain block


            target.Free(result3, 128);
          
         //   target.Free(result4, allocateBlockSize / 4);
            // 2nd and 3rd should merge 


            regions = target.DiagnosticProvider.GetFreeList();
            Assert.AreEqual(2, regions.Length, "Not fragmented");  //2-4, 6 and remain block

            
            target.Free(result, 128);
            target.Free(result5, 128);

            // all should merge 

            regions = target.DiagnosticProvider.GetFreeList();
      
            Assert.AreEqual(1, regions.Length, "Not fragmented");

        }



        [TestMethod()]
        public unsafe void FragTest()
        {

                     var target = CreateIMemoryManager();
            var testRegions = GetDummyRegions();
            target.Init(testRegions);


            List<MemoryRegion> created = new List<MemoryRegion>();
            var rand = new Random(12);

 
            for (int i = 0 ; i < 1000 ; i++)
            {
                var roll = rand.NextDouble();
                if (roll > 0.2 || i < 30)
                {
                    uint size = (uint)rand.Next(10, Math.Max( 11 ,(int)  target.DiagnosticProvider.Details.BytesFree/3));
                    var result = target.Allocate(size);
                    if ( result != UIntPtr.Zero) 
                        created.Add(new MemoryRegion(result, size)); 
                }
                else
                {
                    if ( created.Count == 0 ) //empty
                        continue;

                    int index = (int)rand.Next(10, Math.Max(10, created.Count - 1  ));
      
                    var toRemove = created[0]; created.RemoveAt(0); 
                    target.Free(toRemove.Address , toRemove.Size); 
                }

                   var deb_regions = target.DiagnosticProvider.GetFreeList();
                   Debug.WriteLine("free " + deb_regions.Length + " mem :" + target.DiagnosticProvider.Details.BytesFree.ToString() + "outstanding" + created.Count );

            }

            // clean up
            while (created.Count != 0)
            {
                var toRemove = created[0]; created.RemoveAt(0);
                    target.Free(toRemove.Address , toRemove.Size);
                    var deb_regions = target.DiagnosticProvider.GetFreeList();
                    Debug.WriteLine("clean" + deb_regions.Length + " mem :" + target.DiagnosticProvider.Details.BytesFree.ToString() + "outstanding" + created.Count);

            }


            //see if one region 
            var regions = target.DiagnosticProvider.GetFreeList();

            Assert.IsNotNull(regions);
            CollectionAssert.AllItemsAreNotNull(regions);
            Assert.AreEqual(testRegions.Length, regions.Length, "Must not be empty");


        }


            private MemoryRegion[] GetDummyRegions()
            {
                var size = 5000U; // UInt32.MaxValue;
                ulong addr = 1 << 50;
             	   UIntPtr baseAddress = new UIntPtr(addr); // out of bounce pointers but since we dont use it shouldnt matter and we can check that

                return new  MemoryRegion[] { new MemoryRegion(baseAddress , size)};

            }

            private ulong MemoryInRegions(MemoryRegion[] regions)
            {
                ulong result = 0;
                foreach (var region in regions)
                {
                    result += region.Size;
                }

                return result;

            }





        ///// <summary>
        /////A test for Allocate
        /////</summary>
        //[TestMethod()]
        //public void AllocateTest()
        //{
        //    IMemoryManager target = CreateIMemoryManager(); // TODO: Initialize to an appropriate value
        //    uint amountOfBytes = 0; // TODO: Initialize to an appropriate value
        //    MemoryRegion expected = new MemoryRegion(); // TODO: Initialize to an appropriate value
        //    MemoryRegion actual;
        //    actual = target.Allocate(amountOfBytes);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for AllocateDMA
        /////</summary>
        //[TestMethod()]
        //public void AllocateDMATest()
        //{
        //    IMemoryManager target = CreateIMemoryManager(); // TODO: Initialize to an appropriate value
        //    uint amountOfBytes = 0; // TODO: Initialize to an appropriate value
        //    UIntPtr maxmem = new UIntPtr(); // TODO: Initialize to an appropriate value
        //    MemoryRegion expected = new MemoryRegion(); // TODO: Initialize to an appropriate value
        //    MemoryRegion actual;
        //    actual = target.AllocateDMA(amountOfBytes, maxmem);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Free
        /////</summary>
        //[TestMethod()]
        //public void FreeTest()
        //{
        //    IMemoryManager target = CreateIMemoryManager(); // TODO: Initialize to an appropriate value
        //    MemoryRegion region = new MemoryRegion(); // TODO: Initialize to an appropriate value
        //    target.Free(region);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Init
        /////</summary>
        //[TestMethod()]
        //public void InitTest()
        //{
        //    IMemoryManager target = CreateIMemoryManager(); // TODO: Initialize to an appropriate value
        //    uint amountOfBytes = 0; // TODO: Initialize to an appropriate value
        //    UIntPtr baseAdddress = new UIntPtr(); // TODO: Initialize to an appropriate value
        //    IEnumerable<MemoryRegion> reservedMemory = null; // TODO: Initialize to an appropriate value
        //    target.Init(amountOfBytes, baseAdddress, reservedMemory);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}
    }
}
