using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    //is linked by the Dynamic Loader
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ExternalEntry: System.Attribute
    {
       
    }
}
