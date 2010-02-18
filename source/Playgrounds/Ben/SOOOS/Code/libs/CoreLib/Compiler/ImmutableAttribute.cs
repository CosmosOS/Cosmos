using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    // Used by compiler and IPC  
    // SysAlloc fixes and allocates a Mutable object on the global heap .
    // Free releases them 

    // note we need to add runtime Immutables  or wrap then this includes things like Point, Font , String etc 
    

    /// <summary>
    /// Immutables are normally allocated on a global Shared Memory heap and ref counted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ImmutableAttribute : System.Attribute
    {
    
    }
}
