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

    /// 
    /// </summary>
    internal class HardwarePageManager : IPagingManager
    {
        private event EventHandler<EventArgs<MemoryPressure>> memoryPressureChanged;

        //HardwareMM pageTable;
        Mutex pageTableMutex;
        uint m_pageSizeTier1 = 1 << 12;  //TODO get from hardware
        uint m_pageSizeTier2 = 1 << 20;


        internal HardwarePageManager( UIntPtr baseAddress, ulong managedMemory)
        {

            pageTableMutex = new Mutex(true, "SimplePageManager" + baseAddress.ToString());

          //  pageTable = new MemoryPageTable(baseAddress, numPages, m_pageSize);
            pageTableMutex.ReleaseMutex();
        }


        bool IPagingManager.ReAllocatesMemory
        {
            get { return true; }
        }



     
        protected void OnMemoryPressureChanged(object sender, EventArgs<MemoryPressure> args)
        {
            if (memoryPressureChanged != null)
                memoryPressureChanged(sender, args);
        }




        #region IPagingManager Members

        MemoryUsage IPagingManager.Details
        {
            get { throw new NotImplementedException(); }
        }

        event EventHandler<EventArgs<MemoryPressure>> IPagingManager.MemoryPressureChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        UIntPtr IPagingManager.RequestPages(MemoryAllocationRequest request)
        {
            throw new NotImplementedException();
        }

        void IPagingManager.FreePages(Ben.Kernel.Dispatch.Process process)
        {
            throw new NotImplementedException();
        }

        void IPagingManager.FreePages(Ben.Kernel.Dispatch.Process process, PageAllocation region)
        {
            throw new NotImplementedException();
        }

        PageEntry IPagingManager.QueryPage(UIntPtr page)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
