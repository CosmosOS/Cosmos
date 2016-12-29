using System;

using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Beq)]
  [OpCode(ILOpCode.Code.Bge)]
  [OpCode(ILOpCode.Code.Bgt)]
  [OpCode(ILOpCode.Code.Ble)]
  [OpCode(ILOpCode.Code.Blt)]
  [OpCode(ILOpCode.Code.Bne_Un)]
  [OpCode(ILOpCode.Code.Bge_Un)]
  [OpCode(ILOpCode.Code.Bgt_Un)]
  [OpCode(ILOpCode.Code.Ble_Un)]
  [OpCode(ILOpCode.Code.Blt_Un)]
  [OpCode(ILOpCode.Code.Brfalse)]
  [OpCode(ILOpCode.Code.Brtrue)]
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
      var xSize = SizeOfType(xStackContent);
      var xIsFloat = TypeIsFloat(xStackContent);

      if (!xIsSingleCompare)
      {
        var xStackContent2 = aOpCode.StackPopTypes[1];
        xSize = Math.Max(xSize, SizeOfType(xStackContent2));
        xIsFloat = xIsFloat || TypeIsFloat(xStackContent2);
      }

      if (xSize > 8)
      {
        throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: StackSize > 8 not supported");
      }

      ConditionalTestEnum xTestOp;
      // all conditions are inverted here?
      switch (aOpCode.OpCode)
      {
        case ILOpCode.Code.Beq:
          xTestOp = ConditionalTestEnum.Zero;
          break;
        case ILOpCode.Code.Bge:
          xTestOp = ConditionalTestEnum.GreaterThanOrEqualTo;
          break;
        case ILOpCode.Code.Bgt:
          xTestOp = ConditionalTestEnum.GreaterThan;
          break;
        case ILOpCode.Code.Ble:
          xTestOp = ConditionalTestEnum.LessThanOrEqualTo;
          break;
        case ILOpCode.Code.Blt:
          xTestOp = ConditionalTestEnum.LessThan;
          break;
        case ILOpCode.Code.Bne_Un:
          xTestOp = ConditionalTestEnum.NotEqual;
          break;
        case ILOpCode.Code.Bge_Un:
          xTestOp = ConditionalTestEnum.AboveOrEqual;
          break;
        case ILOpCode.Code.Bgt_Un:
          xTestOp = ConditionalTestEnum.Above;
          break;
        case ILOpCode.Code.Ble_Un:
          xTestOp = ConditionalTestEnum.BelowOrEqual;
          break;
        case ILOpCode.Code.Blt_Un:
          xTestOp = ConditionalTestEnum.Below;
          break;
        case ILOpCode.Code.Brfalse:
          xTestOp = ConditionalTestEnum.Zero;
          break;
        case ILOpCode.Code.Brtrue:
          xTestOp = ConditionalTestEnum.NotZero;
          break;
        default:
          throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Unknown OpCode for conditional branch.");
      }
      if (!xIsSingleCompare)
      {
        if (xSize <= 4)
        {
          if (xIsFloat) //float
          {
            XS.SSE.MoveSS(XMM0, ESP, true);
            XS.Add(ESP, 4);
            XS.SSE.MoveSS(XMM1, ESP, true);
            XS.Add(ESP, 4);

            switch (xTestOp)
            {
              case ConditionalTestEnum.Equal:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.Equal);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.NotEqual:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.NotEqual);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThan:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.LessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThanOrEqualTo:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.LessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThan:
                XS.SSE.MoveSS(XMM2, XMM1);
                XS.SSE.CompareSS(XMM2, XMM0, ComparePseudoOpcodes.Ordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.NotLessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThanOrEqualTo:
                XS.SSE.MoveSS(XMM2, XMM1);
                XS.SSE.CompareSS(XMM2, XMM0, ComparePseudoOpcodes.Ordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.NotLessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              // from here all unordered
              case ConditionalTestEnum.BelowOrEqual:
                XS.SSE.MoveSS(XMM2, XMM1);
                XS.SSE.CompareSS(XMM2, XMM0, ComparePseudoOpcodes.Unordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.LessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.Or(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Below:
                XS.SSE.MoveSS(XMM2, XMM1);
                XS.SSE.CompareSS(XMM2, XMM0, ComparePseudoOpcodes.Unordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.LessThan);
                XS.MoveD(EBX, XMM1);
                XS.Or(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.AboveOrEqual:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.NotLessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Above:
                XS.SSE.CompareSS(XMM1, XMM0, ComparePseudoOpcodes.NotLessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              default:
                throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Comparison of floats (System.Single) is not yet supported!");
            }
          }
          else //int and uint
          {
            XS.Pop(EAX);
            XS.Pop(EBX);
            XS.Compare(EBX, EAX);
            XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
          }
        }
        else if (xSize <= 8)
        {
          if (xIsFloat) //double
          {
            XS.SSE2.MoveSD(XMM0, ESP, true);
            XS.Add(ESP, 8);
            XS.SSE2.MoveSD(XMM1, ESP, true);
            XS.Add(ESP, 8);

            switch (xTestOp)
            {
              case ConditionalTestEnum.Equal:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.Equal);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.NotEqual:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.NotEqual);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThan:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.LessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThanOrEqualTo:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.LessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThan:
                XS.SSE2.MoveSD(XMM2, XMM1);
                XS.SSE2.CompareSD(XMM2, XMM0, ComparePseudoOpcodes.Ordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.NotLessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThanOrEqualTo:
                XS.SSE2.MoveSD(XMM2, XMM1);
                XS.SSE2.CompareSD(XMM2, XMM0, ComparePseudoOpcodes.Ordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.NotLessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              // from here all unordered
              case ConditionalTestEnum.BelowOrEqual:
                XS.SSE2.MoveSD(XMM2, XMM1);
                XS.SSE2.CompareSD(XMM2, XMM0, ComparePseudoOpcodes.Unordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.LessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.Or(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Below:
                XS.SSE2.MoveSD(XMM2, XMM1);
                XS.SSE2.CompareSD(XMM2, XMM0, ComparePseudoOpcodes.Unordered);
                XS.MoveD(EAX, XMM2);
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.LessThan);
                XS.MoveD(EBX, XMM1);
                XS.Or(EBX, EAX);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.AboveOrEqual:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.NotLessThan);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Above:
                XS.SSE2.CompareSD(XMM1, XMM0, ComparePseudoOpcodes.NotLessThanOrEqualTo);
                XS.MoveD(EBX, XMM1);
                XS.And(EBX, 1);
                XS.Jump(ConditionalTestEnum.NotZero, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              default:
                throw new Exception("Unknown OpCode for conditional branch in double.");
            }
          }
          else //long and ulong
          {
            var xNoJump = GetLabel(aMethod, aOpCode) + "__NoBranch";

            // value 2  EBX:EAX
            XS.Pop(EAX);
            XS.Pop(EBX);
            // value 1  EDX:ECX
            XS.Pop(ECX);
            XS.Pop(EDX);

            switch (xTestOp)
            {
              case ConditionalTestEnum.Equal: // Zero
                XS.Xor(EBX, EDX);
                XS.Xor(EAX, ECX);
                XS.Or(EAX, EBX);
                XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.NotEqual: // NotZero
                XS.Xor(EAX, ECX);
                XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Xor(EBX, EDX);
                XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThanOrEqualTo:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.LessThan, xNoJump);
                XS.Jump(ConditionalTestEnum.GreaterThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.GreaterThanOrEqualTo, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.GreaterThan:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.LessThan, xNoJump);
                XS.Jump(ConditionalTestEnum.GreaterThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.GreaterThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThanOrEqualTo:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.GreaterThan, xNoJump);
                XS.Jump(ConditionalTestEnum.LessThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.LessThanOrEqualTo, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.LessThan:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.GreaterThan, xNoJump);
                XS.Jump(ConditionalTestEnum.LessThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.LessThan, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              // from here all unsigned
              case ConditionalTestEnum.AboveOrEqual:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.Below, xNoJump);
                XS.Jump(ConditionalTestEnum.Above, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.AboveOrEqual, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Above:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.Below, xNoJump);
                XS.Jump(ConditionalTestEnum.Above, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.Above, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.BelowOrEqual:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.Above, xNoJump);
                XS.Jump(ConditionalTestEnum.Below, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.BelowOrEqual, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              case ConditionalTestEnum.Below:
                XS.Compare(EDX, EBX);
                XS.Jump(ConditionalTestEnum.Above, xNoJump);
                XS.Jump(ConditionalTestEnum.Below, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                XS.Compare(ECX, EAX);
                XS.Jump(ConditionalTestEnum.BelowOrEqual, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                break;
              default:
                throw new Exception("Unknown OpCode for conditional branch in 64-bit.");
            }

            XS.Label(xNoJump);
          }
        }
      }
      else
      {
        if (xIsFloat)
        {
          throw new Exception("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Simple comparison of floating point numbers is not yet supported!");
        }
        else
        {
          // todo: improve code clarity
          if (xSize <= 4)
          {
            XS.Pop(EAX);
            if (xTestOp == ConditionalTestEnum.Zero)
            {
              XS.Compare(EAX, 0);
              XS.Jump(ConditionalTestEnum.Equal, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
            }
            else if (xTestOp == ConditionalTestEnum.NotZero)
            {
              XS.Compare(EAX, 0);
              XS.Jump(ConditionalTestEnum.NotEqual, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
            }
            else
            {
              throw new NotSupportedException("Cosmos.IL2CPU.x86->IL->Branch.cs->Error: Situation not supported yet! (In the Simple Comparison)");
            }
          }
          else if (xSize <= 8)
          {
            if (TypeIsReferenceType(xStackContent))
            {
              XS.Add(ESP, 4);
              XS.Pop(EAX);
            }
            else
            {
              XS.Pop(EAX);
              XS.Pop(EBX);
            }

            switch (xTestOp)
            {
              case ConditionalTestEnum.Zero: // Equal
              case ConditionalTestEnum.NotZero: // NotEqual
                if (TypeIsReferenceType(xStackContent))
                {
                  XS.Xor(EAX, 0);
                  XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                }
                else
                {
                  XS.Xor(EAX, 0);
                  XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                  XS.Xor(EBX, 0);
                  XS.Jump(xTestOp, AppAssembler.TmpBranchLabel(aMethod, aOpCode));
                }
                break;

              default:
                throw new NotImplementedException("Cosmos.IL2CPU.X86.IL.Branch: Simple branch " + aOpCode.OpCode + " not implemented for operand ");
            }
          }
        }
      }
    }
  }
}
