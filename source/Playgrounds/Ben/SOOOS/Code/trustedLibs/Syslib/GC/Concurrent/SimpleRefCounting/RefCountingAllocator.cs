using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.Locking;

namespace CoreLib.GC.Concurrent.SimpleRefCounting
{



    public class RefCountingAllocator :IAllocator
    {
   


        public RefCountingAllocator()
        {

        }


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
         public static void IncRefCount(UIntPtr objectAddress)       
         {
            // yes we need to null check 
             if ( objectAddress != UIntPtr.Zero)
             {
                 UIntPtr refLocation = UIntPtr.Add(objectAddress, ObjectHeader.RefCountOffset);
                 Interlocked.Increment(objectAddress); 
            
             }
          
         }        
        
        [Inline] 
        public unsafe static void DecRefCount(UIntPtr objectAddress)        
        {          
            if (objectAddress != UIntPtr.Zero)
            {
                UIntPtr refLocation = UIntPtr.Add(objectAddress, ObjectHeader.RefCountOffset);
                Interlocked.Decrement(objectAddress);


                uint* ptr = (uint*)objectAddress;

                if (*ptr == 0)
                    RemoveObject(objectAddress); 
                //var value = (int*)refLocation.ToPointer();   //TODO test 

                //if ( value == 0 ) 
                // check if zero. 

            }
        }


        public unsafe static void RemoveObject(UIntPtr objectAddress)
        {
            //TODO
        }


    }

}
