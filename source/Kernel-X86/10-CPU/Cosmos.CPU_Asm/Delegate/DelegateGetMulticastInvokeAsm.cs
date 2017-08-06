using System.Reflection;
using Cosmos.Assembler;
using Cosmos.IL2CPU;
using XSharp.Common;

namespace Cosmos.CPU_Asm {
    public class DelegateGetMulticastInvokeAsm : AssemblerMethod {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
            var xAssembler = aAssembler;
            var xMethodInfo = (MethodInfo)aMethodInfo;
            var xDelegate = typeof(global::System.Delegate);
            var xMethod = xDelegate.GetMethod("GetInvokeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            XS.Push(ILOp.GetLabel(xMethod));
        }
    }
}
