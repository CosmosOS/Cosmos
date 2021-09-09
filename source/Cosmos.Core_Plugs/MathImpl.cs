using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;


namespace Cosmos.Core_Plugs
{
    [Plug(Target = typeof(Math))]
    class MathImpl
    {
        [PlugMethod(Assembler = typeof(MathRoundASM))]
        public static double Round(double d)
        {
            throw new NotImplementedException();
            //return ((Floor(d) % 2 == 0) ? Floor(d) : Ceiling(d));
        }
    }

    class MathRoundASM : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.FPU.FloatLoad(EBP, destinationIsIndirect: true, displacement: 8, size: RegisterSize.Long64);
            XS.FPU.FloatRound();
            XS.Sub(ESP, 8);
            XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
        }
    }
}
