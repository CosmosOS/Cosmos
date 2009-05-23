using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Compiler
{
    public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x,
                           Type y)
        {
            return x.FullName.Equals(y.FullName);
        }

        public int GetHashCode(Type obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}