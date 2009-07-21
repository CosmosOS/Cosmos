using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Dispatch;


namespace Cosmos.Kernel.MM
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
        void FreePages(Process process, PageRegion region);

        PageEntry QueryPage(UIntPtr page);


    }


    internal struct PageRegion
    {
        internal UIntPtr Address;
        internal uint NumPages;


        internal PageRegion(UIntPtr address, uint numPages)
        {
            Address = address;
            NumPages = numPages;
        }




    }




    public enum MemoryPressure : byte { None, Low, Medium, High, Critical }
    public enum PageType : byte { Unknown , Kernel, IO, Paged }
    public enum PageSharing : byte { Unknown, Private, SharedRead }
}
