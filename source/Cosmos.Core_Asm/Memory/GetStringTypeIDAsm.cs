using Cosmos.IL2CPU.CIL;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm.Memory
{
    class GetStringTypeIDAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Push(ILOp.GetTypeIDLabel(typeof(string)));
        }
    }
}
