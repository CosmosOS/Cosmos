//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;


//using Ben.Services.GC;

//namespace Ben.Services.MM
//{
//    /// <summary>
//    /// not thread safe !!
//    /// 
//    /// </summary>
//    internal sealed class MemoryPageTable 
//    {

//        MemoryDetails Memory;


//        //note for non HW paging i dont think its even needed but will leave here for VM implementation
        
//        UIntPtr m_baseAddress; // base of mem under management
        
//        [GCFixed]
//        private byte[] pageTableFixedMemory; //much nicer than using a static
//        UIntPtr pageTable; //ulong for each page 

//        private uint m_pageSize ; 
//        uint m_pages;

//        LinkedList<PageAllocation> m_availableMemoryRegions  ;  //small at front , big at back 

//        //TODO extend exisitng space support


//         internal MemoryPageTable( UIntPtr baseAddress, uint pages) : this (baseAddress, pages, 1 << 20 , new LinkedListWithNodeMemoryReuse<PageAllocation>(pages/2)) //1 M
//         {

//         }

//         internal MemoryPageTable(UIntPtr baseAddress, uint pages , uint pageSize)
//             : this(baseAddress, pages, pageSize, new LinkedListWithNodeMemoryReuse<PageAllocation>(pages / 2)) //1 M
//         {

//         }

//        internal MemoryPageTable(UIntPtr baseAddress, uint pages, uint pageSize , LinkedList<PageAllocation> list )
//        {
//            //TODO Guards.

//            m_baseAddress = baseAddress;
//            m_pages = pages;
//            m_pageSize = pageSize;
//            m_availableMemoryRegions = list; 

//            Init();
//        }

//        unsafe void Init()
//        {
//            InitPageTable();

//            PageAllocation allRegion = new PageAllocation(m_baseAddress, m_pages);
//            m_availableMemoryRegions.AddFirst(allRegion);



//        }

//        unsafe private void InitPageTable()
//        {
//            pageTableFixedMemory = new byte[m_pages * PageEntry.PAGE_ENTRY_SIZE];

//            fixed (void* pageTableBytePointer =  pageTableFixedMemory)  //fixed anyway but keeps compiler quite.
//            {
//                ulong* pageTablePointer = (ulong*)pageTableBytePointer;  // set to first byte ( we are not interested in the array ) 



//              //  ulong* pageTablePointer = (ulong*)m_baseAddress.ToPointer();  // set to first byte ( we are not interested in the array ) 

//                var emptyPage = PageEntry.EmptyPage;
//                ulong mark = emptyPage.GetPageTableEntry();
//                //mark page table 
//                for (int i = 0; i < m_pages; i++)
//                {
//                    *(pageTablePointer + i) = mark;
//                }

//                pageTable = new UIntPtr(pageTableBytePointer);  //TODO debug 

//                //UIntPtr baseAddress = new UIntPtr(memPtr);

//                //MemoryPageTable target = new MemoryPageTable(baseAddress, pages, pageSize); // TODO: Initialize to an appropriate value

//                //var details = target.ComputeDetails();

//                //Assert.AreEqual(pages, details.numPages);
//            }



         
//        }



//        internal UIntPtr AllocateMemory(ulong bytes,
//                                                ulong alignment,
//                                                PageType type,
//                                                PageSharing sharing,
//                                                 uint pid)
//        {

//            uint pages = (uint)(bytes / (ulong)m_pages + 1);
//            return AllocateMemory(pages, type, sharing, pid);
//        }

//        private UIntPtr AllocateMemory(uint pages, PageType type,
//                                                PageSharing sharing,
//                                                 uint pid)
//        {
//            PageAllocation candidate = FindSmallestMatchingNode(pages);


//            var result = candidate.Address;
            
//            //update page table
//            UpdatePageTable(type, sharing, pid, candidate.NumPages , result);


//            //update node list 
//            //remove node 
//            m_availableMemoryRegions.Remove(candidate);

//            //subtract memory 
//            candidate.NumPages = candidate.NumPages - pages;

//            if (candidate.NumPages > 0)
//                AddNode(candidate); //re add node 

//            return result;

//        }

//        internal unsafe PageEntry QueryPage(UIntPtr page)
//        {
//            return QueryPage(page, 0);        
//        }


//        internal unsafe PageEntry QueryPage(UIntPtr page , int pageOffset)
//        {
//            ulong* pageTablePointer = (ulong*)MemoryToPageTableAddress(page);

//            ulong pageEntry = *(pageTablePointer + pageOffset);

//            return new PageEntry(pageEntry);
//        }


//        internal unsafe void DeAllocateAllMemoryForProcess(Process process)
//        {
//            ulong* pageTablePointer = (ulong*)pageTable;

//            //sweep table for pid
//            for (int i = 0; i < m_pages; i++)
//            {
//                PageEntry entry = new PageEntry( *(pageTablePointer + i)); 
//                if (entry.pid == process.Pid) 
//                {
//                    var ptr = PageTableToMemoryAddress(i);
//                    DeAllocateMemory( new PageAllocation( ptr   ,1  ) , entry.pid);
//                }
//            }

//        }



//        internal void DeAllocateMemory(PageAllocation region ,uint pid)
//        {

//            // see if owned by process
//            for (int i = 0; i < region.NumPages; i++)
//            {
//                var entry = QueryPage(region.Address, i);

//                if (pid != entry.pid)
//                    throw new InvalidOperationException("Page does not belong to caller"); 
//            }


//            // update page table 
//            var defPage = PageEntry.EmptyPage;
//            UpdatePageTable(defPage.pageType, defPage.pageSharing, pid, region.NumPages, region.Address);


//            AddAndMergeNode(region);


//        }

//        private unsafe  void UpdatePageTable(PageType type, PageSharing sharing, uint pid, uint numPages, UIntPtr baseMemoryAddress)
//        {

//            PageEntry entry = new PageEntry();
//            entry.pid = pid;
//            entry.pageSharing = sharing;
//            entry.pageType = type;


//            for (int pageCount = 0; pageCount < numPages; pageCount++)
//            {

//                //  var pageAddress = result + pageCount 
//                ulong* pageTableAddress = (ulong*)MemoryToPageTableAddress(baseMemoryAddress);
//                //MarkPage
//                *pageTableAddress = entry.GetPageTableEntry();

//            }

//        }

//      //  void FreePages(Process process);

//        //TODO non 0 allignement
//        internal unsafe bool IsAdjacent(PageAllocation region1 , PageAllocation region2)
//        {
//            byte* region1Addr = (byte*)region1.Address;
//            byte* region2Addr = (byte*)region2.Address;

//            if (region1Addr + 1 + region1.NumPages* m_pageSize == region2Addr
//                || region2Addr + 1 + region2.NumPages * m_pageSize == region1Addr)
//                return true;

//            return false;
//        }




//        private unsafe  UIntPtr MemoryToPageTableAddress(UIntPtr physical)
//        {
//            //TODO range checking !
//            byte* pageTablePointer = (byte*)pageTable;
//            byte* physicalPtr = (byte*) physical;

//            long offSet = physicalPtr - (byte *) m_baseAddress;

//            var pageNumber = offSet / m_pageSize;

           
//            byte* addr = pageTablePointer + PageEntry.PAGE_ENTRY_SIZE * pageNumber;

//            return new UIntPtr( addr ); 
//           // offSet.
//        }



//        unsafe private UIntPtr PageTableToMemoryAddress(int page)
//        {
//            var ptr = (byte*)m_baseAddress + page * m_pageSize;
//            return new UIntPtr(ptr);
//        }

//        private PageAllocation FindSmallestMatchingNode(uint pages)
//        {


//            foreach (var node in m_availableMemoryRegions)
//            {
//                if (node.NumPages >= pages)
//                {
//                    return node;
//                }
//            }


//            throw new InsufficientMemoryException("not enough memory available , largest size is " + LargestMemoryBlock().ToString() + " requested " + pages * m_pageSize);

//        }

 


//        //this does not check for adjoining regions
//        private void AddNode(PageAllocation region)
//        {
            
//            var node = m_availableMemoryRegions.First;                

//            while (node != null)
//            {

//                if (node.Value.NumPages >= region.NumPages)
//                {
//                    m_availableMemoryRegions.AddBefore(node, region);
//                    return;
//                }

//                node = node.Next;
//            }

//            m_availableMemoryRegions.AddLast(region);
//        }

//        /// <summary>
//        /// find adjacent regions and see if we can join and if join repeat 
//        /// </summary>
//        /// <param name="region"></param>
//        private void AddAndMergeNode(PageAllocation region)
//        {
//            if ( TryMerge(region) == false) // no merge need to add
//                AddNode(region); 
//        }

//        private bool TryMerge(PageAllocation region)
//        {
//            bool result = true;
//            bool regionMerged = false;

//            while (result == true)
//            {
//                PageAllocation combinedRegion;

//                result = ParseForAdjacent(region, out combinedRegion);
//                if (result == true)
//                {
//                    regionMerged = true;
//                    region = combinedRegion;
//                }
//            }
//            return regionMerged;
//        }

//        unsafe private bool ParseForAdjacent(PageAllocation region , out PageAllocation  combinedRegion)
//        {
//            var node = m_availableMemoryRegions.First;

//            while (node != null)
//            {

//                if (IsAdjacent(region, node.Value))
//                {

//                    combinedRegion = region;
//                    combinedRegion.NumPages = node.Value.NumPages + region.NumPages;

//                    if (region.Address.ToPointer() > node.Value.Address.ToPointer()) //TODO check! 
//                        combinedRegion.Address = node.Value.Address;


//                    //remove node and readd Combined
//                    m_availableMemoryRegions.Remove(node);

//                    AddNode(combinedRegion);

//                    return true; 
//                }

//                node = node.Next;
//            }

//            combinedRegion = new PageAllocation();
//            return false;
//        }

//        #region Instrumentation
//        internal MemoryDetails ComputeDetails()
//        {
//            MemoryDetails details = new MemoryDetails();

//            details.largestSegment = LargestMemoryBlock();
//            details.numPages = m_pages;
//            details.memoryManaged = m_pages * m_pageSize;
//            details.fragmentation = GetFragmentation();
//            details.amountFree = GetAmountFree();
//            details.pageSize = m_pageSize; 

//            return details; 
//        }

//        private float GetFragmentation()
//        {
//            uint freeMemPages = 0;
//            uint fragPages = 0 ;

//            int totalCount = m_availableMemoryRegions.Count(); 

//            foreach (var pageRegion in m_availableMemoryRegions)
//            {
//                freeMemPages += pageRegion.NumPages;
//                fragPages +=  pageRegion.NumPages / (uint)  totalCount;
//                totalCount--;

//            }
//            return ( 1.0f - (float) (fragPages/freeMemPages)) ; 
//        }



//        private float GetAmountFree()
//        {
//            ulong freeMemPages = 0;

//            foreach (var pageRegion in m_availableMemoryRegions)
//                freeMemPages += pageRegion.NumPages;

//            return freeMemPages / m_pages; 
//        }


//        private ulong LargestMemoryBlock()
//        {
//            return (ulong)m_availableMemoryRegions.Last.Value.NumPages * (ulong)m_pageSize;
//        }
//        #endregion

//    }


  
//}
