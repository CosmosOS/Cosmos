using System;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;


namespace Cosmos.Core_Plugs;

[Plug(Target = typeof(Math))]
internal class MathImpl
{
    [PlugMethod(Assembler = typeof(MathRoundASM))]
    public static double Round(double d) => throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(MathCosASM))]
    public static double Cos(double d) => throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(MathSinASM))]
    public static double Sin(double d) => throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(MathTanASM))]
    public static double Tan(double d) => throw new NotImplementedException();
}

internal class MathRoundASM : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.FPU.FloatLoad(EBP, true, 8, RegisterSize.Long64);
        XS.FPU.FloatRound();
        XS.Sub(ESP, 8);
        XS.FPU.FloatStoreAndPop(ESP, true, size: RegisterSize.Long64);
    }
}

internal class MathCosASM : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.FPU.FloatLoad(EBP, true, 8, RegisterSize.Long64);
        XS.FPU.FloatCosine();
        XS.Sub(ESP, 8);
        XS.FPU.FloatStoreAndPop(ESP, true, size: RegisterSize.Long64);
    }
}

internal class MathSinASM : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.FPU.FloatLoad(EBP, true, 8, RegisterSize.Long64);
        XS.FPU.FloatSine();
        XS.Sub(ESP, 8);
        XS.FPU.FloatStoreAndPop(ESP, true, size: RegisterSize.Long64);
    }
}

internal class MathTanASM : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.FPU.FloatLoad(EBP, true, 8, RegisterSize.Long64);
        XS.FPU.FloatTan();
        XS.Sub(ESP, 8);
        XS.FPU.FloatPop();
        XS.FPU.FloatStoreAndPop(ESP, true, size: RegisterSize.Long64);
    }
}
