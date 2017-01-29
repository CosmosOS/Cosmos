using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Runtime.CompilerServices
{
    [Plug(TargetName = "System.Runtime.CompilerServices.RuntimeHelpers, mscorlib", IsMicrosoftdotNETOnly = true)]
    public class RuntimeHelpers
    {
        public void ProbeForSufficientStack()
        {
            // no implementation yet, before threading not needed
        }
    }
}
