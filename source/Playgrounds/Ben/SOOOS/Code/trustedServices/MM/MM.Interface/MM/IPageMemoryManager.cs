using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM
{

    //Page based example 
    // we do not use pages anywhere.


    //note when starting a list of reserved regions is passed in along with a block 
    public interface IPageMemoryManager
    {
        void Init(uint amountOfPages, uint pageSize , UIntPtr baseAdddress, IEnumerable<MemoryRegion> reservedMemory); 
        
        MemoryRegion Allocate(uint amountOfPages) ;

        // must be under a region
        MemoryRegion AllocateDMA(uint amountOfPages, UIntPtr maxmem);

        // we check the size released against the original.
        void Free(MemoryRegion region);
    }


  
}
