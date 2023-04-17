using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUSetESPValue : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(EAX, EBP,sourceDisplacement:8);
            XS.Set(ESP,EAX,destinationDisplacement:4);
        }
    }
}
