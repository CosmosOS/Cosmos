using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Runtime.CompilerServices
{
    [Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {
        public static void cctor()
        {
        }

        public static bool Equals(object o1, object o2)
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
    }
}
