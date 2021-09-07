using System;
using System.Text;
using IL2CPU.API;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Orvid.Graphics
{
    [Plug(Target = typeof(global::System.Math))]
    class MathImpl
    {
        public static double Floor(double d)
        {
            // Should subtract 0.5 then round.
            String str = d.ToString();
            return (double)Int32.Parse(str.Substring(0, str.IndexOf('.')));
        }
    }
}
