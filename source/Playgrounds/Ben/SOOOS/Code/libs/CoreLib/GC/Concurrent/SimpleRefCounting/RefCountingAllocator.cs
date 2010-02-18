using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.GC.Concurrent.SimpleRefCounting
{
    public class RefCountingAllocator :IAllocator
    {
        public ICollector Collector
        {
            get { throw new NotImplementedException(); }
        }

        [Inline]
        public UIntPtr Allocate(uint size)
        {
            throw new NotImplementedException();
        }

        [Inline]
         public unsafe static void IncRefCount(uint aObject)       
         {            
             //throw new NotImplementedException();        
         }        
        
        [Inline] 
        public unsafe static void DecRefCount(uint aObject)        
        {            //throw new NotImplementedException();        
        }
    }
}
