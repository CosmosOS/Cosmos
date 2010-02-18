using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    // Used by compiler and IPC  
    //ensures the heap is not called.
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class |AttributeTargets.Method | AttributeTargets.Property)]
    public class NoHeapAllocation: System.Attribute
    {
       
    }
}
