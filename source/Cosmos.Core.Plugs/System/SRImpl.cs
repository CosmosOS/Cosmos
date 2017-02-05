using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System
{
    [Plug(TargetName = "System.SR, System.Collections, Version=4.0.12.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", IsMicrosoftdotNETOnly = true)]
    public static class SRImpl
    {
        //public static string GetString(string aString)
        //{
        //    return aString;
        //}

        //public static string GetString(string aString, params object[] aArgs)
        //{
        //    return aString;
        //}

        public static string GetResourceString(string aResourceKey, string aDefaultString)
        {
            return aDefaultString;
        }
    }
}
