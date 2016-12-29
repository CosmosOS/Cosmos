using System;

using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.Plugs.Assemblers.Math;

namespace Cosmos.Core.Plugs.System
{
    [Plug(typeof(Math))]
    public static class MathImpl
    {
        [PlugMethod(Assembler = typeof(MathCeilingOfDoubleAsm))]
        public static double Ceiling(double d)
        {
            return 0.0;
        }

        [PlugMethod(Assembler = typeof(MathFloorOfDoubleAsm))]
        public static double Floor(double d)
        {
            return 0.0;
        }
    }
}
