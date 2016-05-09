using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Collections.Generic
{
    [Plug(TargetName = "System.Collections.Generic.RandomizedStringEqualityComparer")]
    public static class RandomizedStringEqualityComparerImpl
    {
        public static int GetHashCode(object aThis, string obj)
        {
            return (aThis.GetHashCode() * obj.GetHashCode()) & 0xF777;
        }
    }
}
