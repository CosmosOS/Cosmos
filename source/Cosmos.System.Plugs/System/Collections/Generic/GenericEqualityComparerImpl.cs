using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Collections.Generic
{
    [Plug(TargetName = "System.Collections.Generic.GenericEqualityComparer<T>")]
    public static class GenericEqualityComparerImpl<T>
    {
        public static int GetHashCode(object aThis)
        {
            return 0;
        }
    }
}
