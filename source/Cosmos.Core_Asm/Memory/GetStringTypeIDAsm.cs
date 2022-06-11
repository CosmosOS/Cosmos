using Cosmos.IL2CPU;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm.Memory;

internal class GetStringTypeIDAsm : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo) =>
        XS.Push(ILOp.GetTypeIDLabel(typeof(string)));
}
