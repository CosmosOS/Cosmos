using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
  
    /// <summary>
    /// Used to port existing up it allows statics to exist accross multiple threads. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |AttributeTargets.Method)]
    public class StaticComptability : System.Attribute
    {
       
    }
}
