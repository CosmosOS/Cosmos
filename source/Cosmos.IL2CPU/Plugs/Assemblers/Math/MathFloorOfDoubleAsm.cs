using XSharp.Compiler;
using static XSharp.Compiler.XS.SSE4.Rounding;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.Plugs.Assemblers.Math
{
    public class MathFloorOfDoubleAsm : AssemblerMethod
    {
        private const int ValueDisplacement = 8;

        // double Floor(double d);          ebp + 8

        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.SSE2.MoveSD(XMM0, EBP, sourceDisplacement: 8);

            XS.SSE4.RoundSD(XMM1, XMM0, roundingMode: RoundingMode.RoundDown);

            XS.Sub(ESP, 8);
            XS.SSE2.MoveSD(ESP, XMM1, destinationIsIndirect: true);
        }
    }
}
