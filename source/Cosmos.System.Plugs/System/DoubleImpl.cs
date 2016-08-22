using System;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof (double))]
    public static class DoubleImpl
    {
        public static string ToString(ref double aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
