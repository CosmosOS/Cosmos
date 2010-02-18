using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM
{

    //Nothing fancy here this is just the algorithm. Allow easy swap.
    // DO NOT CHANGE THIS add new interfaces such as IVMMemoryManager or IPageMemoryManager 

    //note when starting a list of reserved regions is passed in along with a block 
    
    
    public interface IMemoryManager
    {

        //Note Init must be static hence this is a wrapper around a static class which allows testing run time changes etc. 
        void Init(MemoryRegion[] freeHoles); 
        

        UIntPtr Allocate(uint amountOfBytes) ;

        // must be under a region
        // deprecated the policy wrapper can  use a sub MM for under 16Meg.
        //UIntPtr AllocateDMA(uint amountOfBytes , UIntPtr maxmem);

        // we check the size released against the original.
        void Free(UIntPtr address , uint sizeCheck);

        IMemoryManagerDiagnostics DiagnosticProvider { get; }
    }


  
}
