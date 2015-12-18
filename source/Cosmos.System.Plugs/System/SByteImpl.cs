using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(sbyte))]
    public static class SByteImpl
    {
        public static string ToString(ref sbyte aThis)
        {
            return ((int)(aThis)).ToString();
        }
    }
}
