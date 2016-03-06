using System;
using System.Reflection;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Constrained)]
    public class Constrained : ILOp
    {
        public Constrained(Assembler.Assembler aAsmblr) : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
#warning TODO: This needs to either be implemented, or needs to throw a NotImplementedException!
            // todo: Implement correct Constrained support
            //throw new NotImplementedException("Constrained used in '" + aMethod.MethodBase.GetFullName() + "'");

            var xOpType = aOpCode as OpType;
            DoExecute(Assembler, aMethod, aOpCode, xOpType, DebugEnabled);
        }

        private void DoExecute(Assembler.Assembler assembler, MethodInfo aMethod, ILOpCode aOpCode, OpType aTargetType, bool debugEnabled)
        {
            // If thisType is a reference type (as opposed to a value type) then
            //     ptr is dereferenced and passed as the ‘this’ pointer to the callvirt of method
            // If thisType is a value type and thisType implements method then
            //     ptr is passed unmodified as the ‘this’ pointer to a call of method implemented by thisType
            // If thisType is a value type and thisType does not implement method then
            //     ptr is dereferenced, boxed, and passed as the ‘this’ pointer to the callvirt of method

            new Comment(assembler, $"Type = {aTargetType.Value}");
            if (aTargetType.Value.BaseType == typeof (ValueType))
            {
                
            }
            else if (aTargetType.Value.BaseType == typeof (object))
            {
                throw new NotImplementedException($"Constrained not implemented for {aTargetType.Value}");
            }
        }
    }
}