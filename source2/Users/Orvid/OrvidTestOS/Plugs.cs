using System;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPUAll = Cosmos.Compiler.Assembler;

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
