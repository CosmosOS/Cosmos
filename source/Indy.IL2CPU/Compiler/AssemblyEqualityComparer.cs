using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Compiler
{
    public class AssemblyEqualityComparer : IEqualityComparer<Assembly>
    {
        public bool Equals(Assembly x,
                           Assembly y)
        {
            return x.GetName().FullName.Equals(y.GetName().FullName);
        }

        public int GetHashCode(Assembly obj)
        {
            return obj.GetName().FullName.GetHashCode();
        }
    }
}
