using System;
using Cosmos.IL2CPU.X86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Beq)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bgt)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ble)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Blt)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bne_Un)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge_Un)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bgt_Un)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ble_Un)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Blt_Un)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Brfalse)]
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Brtrue)]
    public class Branch : ILOp
    {

        public Branch(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xIsSingleCompare = true;
            switch (aOpCode.OpCode)
            {
                case ILOpCode.Code.Beq:
                case ILOpCode.Code.Bge:
                case ILOpCode.Code.Bgt:
                case ILOpCode.Code.Bge_Un:
                case ILOpCode.Code.Bgt_Un:
                case ILOpCode.Code.Ble:
                case ILOpCode.Code.Ble_Un:
                case ILOpCode.Code.Bne_Un:
                case ILOpCode.Code.Blt:
                case ILOpCode.Code.Blt_Un:
                    xIsSingleCompare = false;
                    break;
            }

            var xStackContent = aOpCode.StackPopTypes[0];
            var xStackContentSize = SizeOfType(xStackContent);

            if (xStackContentSize > 8)
            {
                throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: StackSize > 8 not supported");
            }

            CPU.ConditionalTestEnum xTestOp;
            // all conditions are inverted here?
            switch (aOpCode.OpCode)
            {
                case ILOpCode.Code.Beq:
                    xTestOp = CPU.ConditionalTestEnum.Zero;
                    break;
                case ILOpCode.Code.Bge:
                    xTestOp = CPU.ConditionalTestEnum.GreaterThanOrEqualTo;
                    break;
                case ILOpCode.Code.Bgt:
                    xTestOp = CPU.ConditionalTestEnum.GreaterThan;
                    break;
                case ILOpCode.Code.Ble:
                    xTestOp = CPU.ConditionalTestEnum.LessThanOrEqualTo;
                    break;
                case ILOpCode.Code.Blt:
                    xTestOp = CPU.ConditionalTestEnum.LessThan;
                    break;
                case ILOpCode.Code.Bne_Un:
                    xTestOp = CPU.ConditionalTestEnum.NotEqual;
                    break;
                case ILOpCode.Code.Bge_Un:
                    xTestOp = CPU.ConditionalTestEnum.AboveOrEqual;
                    break;
                case ILOpCode.Code.Bgt_Un:
                    xTestOp = CPU.ConditionalTestEnum.Above;
                    break;
                case ILOpCode.Code.Ble_Un:
                    xTestOp = CPU.ConditionalTestEnum.BelowOrEqual;
                    break;
                case ILOpCode.Code.Blt_Un:
                    xTestOp = CPU.ConditionalTestEnum.Below;
                    break;
                case ILOpCode.Code.Brfalse:
                    xTestOp = CPU.ConditionalTestEnum.Zero;
                    break;
                case ILOpCode.Code.Brtrue:
                    xTestOp = CPU.ConditionalTestEnum.NotZero;
                    break;
                default:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Unknown OpCode for conditional branch.");
            }
            if (!xIsSingleCompare)
            {
                if (xStackContentSize <= 4)
                {
                    //if (xStackContent.IsFloat)
                    //{
                    //    throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Comparison of floats (System.Single) is not yet supported!");
                    //}
                    //else
                    //{
                    new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                    new CPU.Pop { DestinationReg = CPU.Registers.EBX };
                    new CPU.Compare { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EAX };
                    new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                    //}
                }
                else
                {
                    //if (xStackContent.IsFloat)
                    //{
                    //    throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Comparison of doubles (System.Double) is not yet supported!");
                    //}
                    //else
                    //{
                    var xNoJump = GetLabel(aMethod, aOpCode) + "__NoBranch";

                    // value 2  EBX:EAX
                    new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                    new CPU.Pop { DestinationReg = CPU.Registers.EBX };
                    // value 1  EDX:ECX
                    new CPU.Pop { DestinationReg = CPU.Registers.ECX };
                    new CPU.Pop { DestinationReg = CPU.Registers.EDX };
                    switch (xTestOp)
                    {
                        case ConditionalTestEnum.Zero: // Equal
                        case ConditionalTestEnum.NotEqual: // NotZero
                            new CPU.Xor { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.ECX };
                            new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.Xor { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EDX };
                            new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            break;
                        case ConditionalTestEnum.GreaterThanOrEqualTo:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.LessThan, DestinationLabel = xNoJump };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.GreaterThan, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Below, DestinationLabel = xNoJump };
                            break;
                        case ConditionalTestEnum.GreaterThan:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.LessThan, DestinationLabel = xNoJump };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.GreaterThan, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.BelowOrEqual, DestinationLabel = xNoJump };
                            break;
                        case ConditionalTestEnum.LessThanOrEqualTo:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.LessThan, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.GreaterThan, DestinationLabel = xNoJump };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.BelowOrEqual, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            break;
                        case ConditionalTestEnum.LessThan:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.LessThan, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.GreaterThan, DestinationLabel = xNoJump };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Below, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            break;
                        // from here all unsigned
                        case ConditionalTestEnum.AboveOrEqual:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Above, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Below, DestinationLabel = xNoJump };
                            break;
                        case ConditionalTestEnum.Above:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Above, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.BelowOrEqual, DestinationLabel = xNoJump };
                            break;
                        case ConditionalTestEnum.BelowOrEqual:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Above, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Below, DestinationLabel = xNoJump };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Above, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            break;
                        case ConditionalTestEnum.Below:
                            new CPU.Compare { DestinationReg = CPU.Registers.EDX, SourceReg = CPU.Registers.EBX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Above, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Below, DestinationLabel = xNoJump };
                            new CPU.Compare { DestinationReg = CPU.Registers.ECX, SourceReg = CPU.Registers.EAX };
                            new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.AboveOrEqual, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                            break;
                        default:
                            throw new Exception("Unknown OpCode for conditional branch in 64-bit.");
                    }
                    new Label(xNoJump);
                    //}
                }
            }
            else
            {
                //if (xStackContent.IsFloat)
                //{
                //    throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Simple comparison of floating point numbers is not yet supported!");
                //}
                //else
                //{
                // todo: improve code clarity
                if (xStackContentSize > 4)
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Simple branches are not yet supported on operands > 4 bytes!");
                }
                new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                if (xTestOp == ConditionalTestEnum.Zero)
                {
                    new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceValue = 0 };
                    new CPU.ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                }
                else if (xTestOp == ConditionalTestEnum.NotZero)
                {
                    new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceValue = 0 };
                    new CPU.ConditionalJump { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
                }
                else
                {
                    throw new NotSupportedException("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Situation not supported yet! (In the Simple Comparison)");
                }
            }
            //}
        }
    }
}