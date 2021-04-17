using System;
using System.Runtime.CompilerServices;
using Cosmos.Debug.Kernel;
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

        public static new bool Equals(object aO1, object aO2)
        {
            if (aO1 == null && aO2 == null)
            {
                return true;
            }
            if (aO1 == null || aO2 == null)
            {
                return false;
            }
            Debugger.DoBochsBreak();
            return aO1 == aO2; //we cant use object.Equals since it just calls this
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

        public static bool TryEnsureSufficientExecutionStack()
        {
            throw new NotImplementedException();
        }
    }
}
