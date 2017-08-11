using System;
using Cosmos.Common;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(float))]
    public static class SingleImpl
    { 
        public static string ToString(ref float aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
