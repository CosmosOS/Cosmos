using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm;

public class CPUZeroFillAsm : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.ClearDirectionFlag();
        XS.Set(EDI, EBP, sourceDisplacement: 0xC); //address
        XS.Set(ECX, EBP, sourceDisplacement: 0x8); //length
        //XS.Set(EAX, 0x1000);
        //XS.Compare(EAX, ECX);
        //XS.Jump(x86.ConditionalTestEnum.GreaterThan, ".AfterSizeCheck");
        //XS.Exchange(BX, BX);
        //XS.Label(".AfterSizeCheck");
        // set EAX to value of fill (zero)
        XS.Xor(EAX, EAX);
        XS.ShiftRight(ECX, 1);
        XS.Jump(ConditionalTestEnum.NotBelow, ".step2");
        XS.StoreByteInString();
        XS.Label(".step2");
        XS.ShiftRight(ECX, 1);
        XS.Jump(ConditionalTestEnum.NotBelow, ".step3");
        XS.StoreWordInString();
        XS.Label(".step3");
        new Stos { Size = 32, Prefixes = InstructionPrefixes.Repeat };
    }
}
