using System;
using System.Reflection;
using Cosmos.IL2CPU;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm;

public class DelegateGetMulticastInvokeAsm : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        var xAssembler = aAssembler;
        var xMethodInfo = (MethodInfo)aMethodInfo;
        var xDelegate = typeof(Delegate);
        var xMethod = xDelegate.GetMethod("GetInvokeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
        XS.Push(ILOp.GetLabel(xMethod));
    }
}
