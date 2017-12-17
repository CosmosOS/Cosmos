using System;
using IL2CPU.API;

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
