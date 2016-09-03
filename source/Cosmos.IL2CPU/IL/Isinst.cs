using CPU = Cosmos.Assembler.x86;
using System.Reflection;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Tests whether an object reference (type O) is an instance of a particular class.
    /// </summary>
    [OpCode(ILOpCode.Code.Isinst)]
    public class Isinst : ILOp
    {
        public Isinst(Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            OpType xType = (OpType)aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);
            string xCurrentMethodLabel = GetLabel(aMethod, aOpCode);
            string xReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            string xNextPositionLabel = GetLabel(aMethod, aOpCode.NextPosition);

            XS.Set(EAX, ESP, sourceIsIndirect: true, sourceDisplacement: 4);
            XS.Compare(EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Zero, xReturnNullLabel);
            XS.Push(EAX, isIndirect: true);
            XS.Push(xTypeID, isIndirect: true);
            Call.DoExecute(Assembler, aMethod, VTablesImplRefs.IsInstanceRef, aOpCode, xCurrentMethodLabel, xCurrentMethodLabel + "_After_IsInstance_Call", DebugEnabled);
            XS.Label(xCurrentMethodLabel + "_After_IsInstance_Call");
            XS.Pop(EAX);
            XS.Compare(EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Equal, xReturnNullLabel);
            XS.Jump(xNextPositionLabel);
            XS.Label(xReturnNullLabel);
            XS.Add(ESP, 8);
            XS.Push(0);
            XS.Push(0);
        }
    }
}
