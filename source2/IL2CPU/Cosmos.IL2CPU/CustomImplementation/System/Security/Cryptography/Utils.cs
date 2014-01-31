using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System.Security.Cryptography
{
    [Plug(TargetName="System.Security.Cryptography.Utils", IsMicrosoftdotNETOnly = true)]
    public static class Utils
    {
        //public static int get_FipsAlgorithmPolicy() { return 0; }
    }
}
