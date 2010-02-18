using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM.Simple
{
    public class SimpleDiagnosticProvider : IMemoryManagerDiagnostics
    {
        public unsafe MemoryRegion[] GetFreeList()
        {
            List<MemoryRegion> list = new List<MemoryRegion>();
  
            HoleNode* ptr = MemoryManager.Head;
            while (ptr != null)
            {

                list.Add(new MemoryRegion(ptr->MemAddress, ptr->Length)); 
                ptr = ptr->Next;             
            }
            return list.ToArray();
        }

        private MMDetails GetDetails()
        {
            MMDetails details = new MMDetails();
            details.BytesManaged = MemoryManager.BytesManaged;

          //  var availableBytes = 
            details.BytesFree = MemoryManager.BytesManaged - MemoryManager.BytesAllocated;
            return details; 
        }

        public MMDetails Details
        {
            get { return GetDetails(); }
        }
    }
}
