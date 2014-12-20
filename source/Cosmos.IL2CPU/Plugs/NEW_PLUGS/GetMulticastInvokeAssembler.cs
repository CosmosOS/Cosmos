using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.IL2CPU.X86.IL;
using Cosmos.Assembler;
using System.Reflection;
using MethodInfo = Cosmos.IL2CPU.MethodInfo;

using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU;

namespace Indy.IL2CPU.X86.Plugs.NEW_PLUGS
{
    public class GetMulticastInvokeAssembler: AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            var xAsm = (Assembler)aAssembler;
            var xMethodInfo = (MethodInfo)aMethodInfo;
            var xDelegate = typeof(Delegate);
            var xMethod = xDelegate.GetMethod("GetInvokeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New(ILOp.GetMethodLabel(xMethod)) };
        }
    }
}
