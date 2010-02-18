using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    // Trusted TCB code
    //Trysted code needs to be manually checked
    //Trusted code is NEVER linked by the Dynamic Loader unless it is marked with PublicAvailable

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface)]
    public class Trusted: System.Attribute
    {
       
    }


    // Trusted interfaces are a bit different they cannot be instantiated by non trusted code 
    [AttributeUsage(AttributeTargets.Interface)]
    public class TrustedInterface : System.Attribute
    {

    }
}
