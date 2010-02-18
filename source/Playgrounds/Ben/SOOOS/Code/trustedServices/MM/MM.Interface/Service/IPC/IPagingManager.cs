using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ben.Runtime.Util;



namespace Ben.Services.MM
{
    /// <summary>
    /// Paging manager may be replaced by paging , 32 bit and 64 bit memory managers
    /// As well as NUMA etc ..
    /// </summary>
    internal interface IPagingManager
    {
        MemoryUsage Details { get; }

        bool ReAllocatesMemory { get; }

        event EventHandler<EventArgs<MemoryPressure>> MemoryPressureChanged;

        UIntPtr RequestPages(MemoryAllocationRequest request);

        void FreePages(Process process);
        void FreePages(Process process, PageAllocation region);

        PageEntry QueryPage(UIntPtr page);


    }


    internal struct PageAllocation  
    {
        internal UIntPtr Address;
        internal uint NumPages;


        internal PageAllocation(UIntPtr address, uint numPages)
        {
            Address = address;
            NumPages = numPages;
        }




    }






}
