using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ben.Runtime.Util;

namespace Ben.Services.MM
{
    /// <summary>
    /// 64 bit address uni processor multiple threads ( single thread CPUs could use a more efficient locking system
    /// 
    /// Allocates pages but does not deallocate used for non paged pool bytes like IO and kernel memory.
    /// 
    /// </summary>
    internal class SimplePageManager : IPagingManager
    {
        private event EventHandler<EventArgs<MemoryPressure>> memoryPressureChanged;

        MemoryPageTable pageTable;
        Mutex pageTableMutex;
        uint m_pageSize = 1 << 20;


        internal SimplePageManager(uint pageSize, UIntPtr baseAddress, ulong managedMemory)
        {
            m_pageSize = pageSize;
            uint numPages = (uint)(managedMemory / (ulong)m_pageSize);
            if (managedMemory % m_pageSize != 0)
                numPages++;

            pageTableMutex = new Mutex(true, "SimplePageManager" + baseAddress.ToString());

            pageTable = new MemoryPageTable(baseAddress, numPages, m_pageSize);
            pageTableMutex.ReleaseMutex();
        }


        bool IPagingManager.ReAllocatesMemory
        {
            get { return false; }
        }



     
        protected void OnMemoryPressureChanged(object sender, EventArgs<MemoryPressure> args)
        {
            if (memoryPressureChanged != null)
                memoryPressureChanged(sender, args);
        }


        /// <summary>
        /// Must be called from a lock
        /// </summary>
        /// <returns></returns>
        private MemoryPressure ComputePressure()
        {

            var details = pageTable.ComputeDetails();

            if (details.amountFree > 0.95)
                return MemoryPressure.None;


            if (details.amountFree > 0.7)
                return MemoryPressure.Low;



            if (details.amountFree > 0.4)
                return MemoryPressure.Medium;

            if (details.amountFree > 0.1)
                return MemoryPressure.High;

            return MemoryPressure.Critical;

        }



        #region IPagingManager Members


        PageEntry IPagingManager.QueryPage(UIntPtr page)
        {
            try
            {
                pageTableMutex.WaitOne(TimeSpan.FromSeconds(1));

                return pageTable.QueryPage(page);
            }
            finally
            {
                pageTableMutex.ReleaseMutex();
            }
        }




        UIntPtr IPagingManager.RequestPages(MemoryAllocationRequest request)
        {
           

            try
            {
                var pressure = ComputePressure();

                pageTableMutex.WaitOne(TimeSpan.FromSeconds(1));

                var result =  pageTable.AllocateMemory(request.NumBytes, request.Allignment, request.PageType, request.Sharing, request.Sender.Pid);

                 var newpres = ComputePressure();
                 if (pressure != newpres)
                     OnMemoryPressureChanged(this, new EventArgs<MemoryPressure>(newpres)); 

                return result; 
            }
            finally
            {
                pageTableMutex.ReleaseMutex();
            }

           

        }

        void IPagingManager.FreePages(Ben.Kernel.Dispatch.Process process)
        {
            throw new InvalidOperationException("SimplePageManager Does not support deallocating memory"); 
        }

        void IPagingManager.FreePages(Ben.Kernel.Dispatch.Process process, PageAllocation region)
        {
            throw new InvalidOperationException("SimplePageManager Does not support deallocating memory"); 
        }




        event EventHandler<EventArgs<MemoryPressure>> IPagingManager.MemoryPressureChanged
        {
            add
            {
                memoryPressureChanged += value;
            }

            remove
            {
                memoryPressureChanged -= value;
            }
        }


       


        MemoryUsage IPagingManager.Details
        {
            get 
            { 
                      try
                      {
                          pageTableMutex.WaitOne(TimeSpan.FromSeconds(1));
                          MemoryUsage usage;
                          usage.details = pageTable.ComputeDetails();
                          usage.pressure = ComputePressure();
                          usage.TotalMemory = this.m_pageSize * PageEntry.PAGE_ENTRY_SIZE;
                          usage.PagedMemory = 0;
                          usage.NonPagedMemory = Convert.ToUInt64(usage.details.memoryManaged * (1 - usage.details.amountFree));  

                          return usage; 
                      }
                      finally
                      {
                          pageTableMutex.ReleaseMutex();
                      }

            
            }
        }



        ///// <summary>
        ///// DO NOT call this from a critical section
        ///// </summary>
        //MemoryDetails IPagingManager.Details
        //{
        //    get
        //    {


        //        try
        //        {
        //            pageTableMutex.WaitOne(TimeSpan.FromSeconds(1));

        //            return pageTable.ComputeDetails();
        //        }
        //        finally
        //        {
        //            pageTableMutex.ReleaseMutex();
        //        }





        //    }
        //}


        #endregion
    }
}
