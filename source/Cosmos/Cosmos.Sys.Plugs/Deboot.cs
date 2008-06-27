using Indy.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target = typeof(Sys.Deboot))]
    public static class Deboot
    {
        [PlugMethod(MethodAssembler = typeof(Assemblers.Reboot))]
        public static void Reboot()
        {

        }

    }
}
