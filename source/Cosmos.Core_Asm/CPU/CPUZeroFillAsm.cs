using XSharp.Assembler;
using XSharp;
using x86 = XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUZeroFillAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearDirectionFlag();
            XS.Set(RDI, RBP, sourceDisplacement: 0xC); //address
            XS.Set(RCX, RBP, sourceDisplacement: 0x8); //length
            //XS.Set(EAX, 0x1000);
            //XS.Compare(EAX, ECX);
            //XS.Jump(x86.ConditionalTestEnum.GreaterThan, ".AfterSizeCheck");
            //XS.Exchange(BX, BX);
            //XS.Label(".AfterSizeCheck");
            // set EAX to value of fill (zero)
            XS.Xor(RAX, RAX);
            XS.ShiftRight(RCX, 1);
            XS.Jump(x86.ConditionalTestEnum.NotBelow, ".step2");
            XS.StoreByteInString();
            XS.Label(".step2");
            XS.ShiftRight(RCX, 1);
            XS.Jump(x86.ConditionalTestEnum.NotBelow, ".step3");
            XS.StoreWordInString();
            XS.Label(".step3");
            new x86.Stos { Size = 32, Prefixes = x86.InstructionPrefixes.Repeat };
        }
    }
}
