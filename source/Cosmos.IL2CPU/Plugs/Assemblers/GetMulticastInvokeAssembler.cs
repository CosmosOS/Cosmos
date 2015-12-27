using System;
using System.Reflection;

using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.Plugs.Assemblers
{
    public class GetMulticastInvokeAssembler: AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAsm = (Assembler.Assembler)aAssembler;
            var xMethodInfo = (MethodInfo)aMethodInfo;
            var xDelegate = typeof(Delegate);
            var xMethod = xDelegate.GetMethod("GetInvokeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New(ILOp.GetMethodLabel(xMethod)) };
        }
    }
}
