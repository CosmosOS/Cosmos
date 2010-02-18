using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM.Simple
{

    // applies policy ...  
    public class SimpleMemoryManager :IMemoryManager
    {
        // static stuff







        public IMemoryManager Instance
        {
            get
            {
                return null; // HACK 
            }
        }



        public void Init(MemoryRegion[] freeHoles)
        {
            MemoryManager.Init(freeHoles);          
        }

        public UIntPtr Allocate(uint amountOfBytes)
        {
            return MemoryManager.Allocate(amountOfBytes); 
        }

        public void Free(UIntPtr address, uint sizeCheck)
        {
            MemoryManager.Free(address, sizeCheck);
        }

        public IMemoryManagerDiagnostics DiagnosticProvider 
        {

            get { return new SimpleDiagnosticProvider(); }  
        }
    }
}
