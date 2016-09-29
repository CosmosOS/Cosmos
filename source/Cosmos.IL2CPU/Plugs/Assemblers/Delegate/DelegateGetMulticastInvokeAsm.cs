using System;
using System.Reflection;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.Plugs.Assemblers
{
    public class DelegateGetMulticastInvokeAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAssembler = aAssembler;
            var xMethodInfo = (MethodInfo) aMethodInfo;
            var xDelegate = typeof(global::System.Delegate);
            var xMethod = xDelegate.GetMethod("GetInvokeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            XS.Push(ILOp.GetMethodLabel(xMethod));
        }
    }
}
