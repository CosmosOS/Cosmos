using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MonoCecilToEcmaCil1
{
    public static class CecilExtensions
    {
        public static bool IsSubclassOf(this TypeDefinition aThis, TypeDefinition aThat)
        {
            if (aThis.Equals(aThat))
            {
                return true;
            }
            if (aThis.BaseType != null)
            {
                return aThis.BaseType.Resolve().IsSubclassOf(aThat);
            }
            return false;
        }
    }
}
