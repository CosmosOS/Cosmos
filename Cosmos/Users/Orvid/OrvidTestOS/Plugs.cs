using System;
using Cosmos.IL2CPU.Plugs;

namespace GuessKernel
{
    [Plug(Target = typeof(global::System.Environment))]
    class EnvironmentImpl
    {
        public static int TickCount
        {
            get
            {
                return GuessOS.Tick;
            }
        }
    }
}
