using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(HashCode))]
    class HashCodeImpl
    {
        public static uint GenerateGlobalSeed()
        {
            return 0;
        }
    }
}