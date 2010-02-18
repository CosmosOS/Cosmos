using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    //Is ref counted
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct)]
    public class RefCounted: System.Attribute
    {
       
    }
}
