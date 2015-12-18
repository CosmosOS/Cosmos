using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Security.Cryptography
{
    [Plug(TargetName = "System.Security.Cryptography.Utils", IsMicrosoftdotNETOnly = true)]
    public static class Utils
    {
        //public static int get_FipsAlgorithmPolicy() { return 0; }
    }
}