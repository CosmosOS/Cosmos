using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.Compiler
{
    public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x,
                           Type y)
        {
            return MethodInfoLabelGenerator.GetFullName(x).Equals(MethodInfoLabelGenerator.GetFullName(y));
        }

        public int GetHashCode(Type obj)
        {
            return MethodInfoLabelGenerator.GetFullName(obj).GetHashCode();
        }
    }
}