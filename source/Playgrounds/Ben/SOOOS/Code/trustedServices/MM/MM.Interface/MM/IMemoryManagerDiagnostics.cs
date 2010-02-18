using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM
{

    // basic performance from algorithm use of pages is not up to it. 
    public interface IMemoryManagerDiagnostics
    {

        MemoryRegion[] GetFreeList(); 
        MMDetails Details { get; }

       // bool ReAllocatesMemory { get; }

       


    }
}
