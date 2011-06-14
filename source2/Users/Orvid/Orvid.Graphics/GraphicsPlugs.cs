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
        #region Calculate Sine
        public class CalculateSinAsm : AssemblerMethod
        {
            public override void AssembleNew(object aAssembler, object aMethodInfo)
            {
                new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.EBP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
                new CPUx86.x87.FloatSine { };
                // reservate 8 byte for returntype double on stack
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                // write double value to this reservation
                new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
                // after this is the result popped
            }
        }
        [PlugMethod(Assembler = typeof(CalculateSinAsm))]
        public static double Sin(double m)
        {
            return 0.0;
        }
        #endregion

        #region Calculate Cosine
        public class CalculateCosAsm : AssemblerMethod
        {
            public override void AssembleNew(object aAssembler, object aMethodInfo)
            {
                new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.EBP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
                new CPUx86.x87.FloatCosine { };
                // reservate 8 byte for returntype double on stack
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                // write double value to this reservation
                new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
                // after this is the result popped
            }
        }
        [PlugMethod(Assembler = typeof(CalculateCosAsm))]
        public static double Cos(double x)
        {
            return 0.0;
        }
        #endregion

        public static double Floor(double d)
        {
            String str = d.ToString();
            return (double)Int32.Parse(str.Substring(0, str.IndexOf('.')));
        }

        #region double Sqrt(double d)
        public class CalculateSqrtAsm : AssemblerMethod
        {
            public override void AssembleNew(object aAssembler, object aMethodInfo)
            {
                new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.EBP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
                new CPUx86.x87.FloatSqrt { };
                // reservate 8 byte for returntype double on stack
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                // write double value to this reservation
                new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
                // after this is the result popped
            }
        }
        [PlugMethod(Assembler = typeof(CalculateSqrtAsm))]
        public static double Sqrt(double d) { return 0.0; }
        #endregion
    }
}
