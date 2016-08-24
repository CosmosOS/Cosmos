using System;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
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
