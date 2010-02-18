using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    // Used by compiler and IPC  
    // Compiler will check that all members are either struct or registered immutables
    [AttributeUsage(AttributeTargets.Struct)]
    public class IPCStruct: System.Attribute
    {
       
    }
}
