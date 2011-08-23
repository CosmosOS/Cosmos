using System;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Orvid.Graphics
{
    [Plug(Target = typeof(global::System.Math))]
    class MathImpl
    {
        //[Inline]
        public static double Sin(double m)
        {
            return 0;
            //new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
            //new CPUx86.x87.FloatSine { };
            //// reservate 8 byte for returntype double on stack
            //new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
            //// write double value to this reservation
            //new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
            //// after this is the result popped
            //return 0.0;
        }

        //[Inline]
        public static double Cos(double x)
        {
            return 0;
            //new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
            //new CPUx86.x87.FloatCosine { };
            //// reservate 8 byte for returntype double on stack
            //new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
            //// write double value to this reservation
            //new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
            //// after this is the result popped
            //return 0.0;
        }

        public static double Floor(double d)
        {
            String str = d.ToString();
            return (double)Int32.Parse(str.Substring(0, str.IndexOf('.')));
        }
    }
}
