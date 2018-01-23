using System;
using System.Runtime.CompilerServices;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.CompilerServices
{
    [Plug(Target = typeof(RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {
        public static void cctor()
        {
        }

#pragma warning disable 108,114
        public static bool Equals(object o1, object o2)
#pragma warning restore 108,114
        {
            if (o1 == null
                && o2 == null)
            {
                return true;
            }
            if (o1 == null
                || o2 == null)
            {
                return false;
            }
            return object.Equals(o1, o2);
        }

        public static void ProbeForSufficientStack()
        {
            // no implementation yet, before threading not needed
        }

        public static int GetHashCode(object o)
        {
            throw new NotImplementedException();
        }

        // TODO: Implement this correctly
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return false;
        }
    }
}
