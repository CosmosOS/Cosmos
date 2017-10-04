using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Security.Cryptography
{
    [Plug(TargetName = "System.Security.Cryptography.Utils", IsMicrosoftdotNETOnly = true)]
    public static class UtilsImpl
    {
        //public static int get_FipsAlgorithmPolicy() { return 0; }
    }
}
