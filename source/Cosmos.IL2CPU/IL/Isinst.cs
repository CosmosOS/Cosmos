using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Tests whether an object reference (type O) is an instance of a particular class.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Isinst)]
    public class Isinst : ILOp
    {
        public Isinst(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            XS.Exchange(XSRegisters.BX, XSRegisters.BX);

            OpType xType = (OpType)aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);
            string xCurrentMethodLabel = GetLabel(aMethod, aOpCode);
            string xReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            string xNextPositionLabel = GetLabel(aMethod, aOpCode.NextPosition);

            XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 4);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Zero, xReturnNullLabel);
            XS.Push(XSRegisters.EAX, isIndirect: true);
            XS.Push(xTypeID, isIndirect: true);
            MethodBase xMethodIsInstance = VTablesImplRefs.IsInstanceRef;
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, xCurrentMethodLabel, xCurrentMethodLabel + "_After_IsInstance_Call", DebugEnabled);
            XS.Label(xCurrentMethodLabel + "_After_IsInstance_Call");
            XS.Pop(XSRegisters.EAX);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Equal, xReturnNullLabel);
            XS.Jump(xNextPositionLabel);
            XS.Label(xReturnNullLabel);
            XS.Add(XSRegisters.ESP, 4);
            XS.Push(0);
        }
    }
}
