using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{

    /// <summary>
    /// We use this so we make a call the struct as an interface it is not boxed 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IPCStructCall: System.Attribute
    {
       
    }
}
