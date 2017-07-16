using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target = typeof(Sys.Deboot))]
    public static class Deboot
    {
        [PlugMethod(Assembler = typeof(Assemblers.Reboot))]
        public static void Reboot()
        {

        }
        [PlugMethod(Assembler = typeof(Assemblers.ShutDown))]
        public static void ShutDown()
        {

        }
    }
}
