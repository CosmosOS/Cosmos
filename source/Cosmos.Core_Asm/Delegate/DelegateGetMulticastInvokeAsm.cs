using Cosmos.IL2CPU;
using IL2CPU.Reflection;
using static Cosmos.IL2CPU.TypeRefHelper;

using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    public class DelegateGetMulticastInvokeAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            var xAssembler = aAssembler;
            var xMethodInfo = (MethodInfo)aMethodInfo;
            var xDelegate = TypeOf(BclType.Delegate);
            var xMethod = xDelegate.GetMethod("GetInvokeMethod");
            XS.Push(ILOp.GetLabel(xMethod));
        }
    }
}
