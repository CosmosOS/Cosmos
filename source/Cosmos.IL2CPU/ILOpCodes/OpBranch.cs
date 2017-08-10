
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;


namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpBranch : ILOpCode {
    public readonly int Value;

    public OpBranch(Code aOpCode, int aPos, int aNextPos, int aValue, _ExceptionRegionInfo aCurrentExceptionRegion)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionRegion) {
      Value = aValue;
    }

    public override int GetNumberOfStackPops(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Leave:
        case Code.Br:
          return 0;
        case Code.Brtrue:
          return 1;
        case Code.Brfalse:
          return 1;
        case Code.Beq:
        case Code.Ble:
        case Code.Ble_Un:
        case Code.Bne_Un:
        case Code.Bge:
        case Code.Bge_Un:
        case Code.Bgt:
        case Code.Bgt_Un:
        case Code.Blt:
        case Code.Blt_Un:
          return 2;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    public override int GetNumberOfStackPushes(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Leave:
        case Code.Br:
          return 0;
        case Code.Brtrue:
          return 0;
        case Code.Brfalse:
          return 0;
        case Code.Beq:
        case Code.Ble:
        case Code.Ble_Un:
        case Code.Bne_Un:
        case Code.Bge:
        case Code.Bge_Un:
        case Code.Bgt:
        case Code.Bgt_Un:
        case Code.Blt:
        case Code.Blt_Un:
          return 0;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    protected override void DoInitStackAnalysis(MethodBase aMethod)
    {
      base.DoInitStackAnalysis(aMethod);

      switch (OpCode)
      {
        default:
          break;
      }
    }

    protected override void DoInterpretStackTypes(ref bool aSituationChanged)
    {
      base.DoInterpretStackTypes(ref aSituationChanged);
      // this method is supposed to deduct push types from pop types. Branch ops don't push, but we want to do checks here,
      // to help verify other code is right
      switch (OpCode)
      {
        case Code.Brtrue:
        case Code.Brfalse:
          // check pop types according to ECMA 335
          var xPopType = StackPopTypes[0];
          if (xPopType == null)
          {
            return;
          }
          if (ILOp.IsIntegralType(xPopType))
          {
            return;
          }
          if (xPopType.GetTypeInfo().IsClass)
          {
            return;
          }
          if (xPopType.GetTypeInfo().IsInterface)
          {
            return;
          }
          // ECMA apparently sees a boolean on the stack as a native int. We push as boolean, so acccept that as well
          if (xPopType == typeof (bool))
          {
            return;
          }

          throw new Exception("Invalid type in PopTypes! (Type = '" + xPopType.AssemblyQualifiedName + "')");
        case Code.Br:
        case Code.Leave:
          return;
        case Code.Blt:
        case Code.Ble:
        case Code.Beq:
        case Code.Bge:
        case Code.Bgt:
        case Code.Bge_Un:
        case Code.Blt_Un:
        case Code.Ble_Un:
        case Code.Bne_Un:
        case Code.Bgt_Un:
          var xValue1 = StackPopTypes[0];
          var xValue2 = StackPopTypes[1];
          if (xValue1 == null || xValue2 == null)
          {
            return;
          }
          if (ILOp.IsIntegralTypeOrPointer(xValue1) && ILOp.IsIntegralTypeOrPointer(xValue2))
          {
            return;
          }
          if (xValue1 == typeof(Single) && xValue2 == typeof(Single))
          {
            return;
          }
          if (xValue1 == typeof(Double) && xValue2 == typeof(Double))
          {
            return;
          }
          if (xValue1 == typeof(IntPtr) && xValue2 == typeof(IntPtr))
          {
            return;
          }
          if ((xValue1 == typeof(int) && xValue2 == typeof(IntPtr))
            ||(xValue1 == typeof(IntPtr) && xValue2 == typeof(int)))
          {
            return;
          }
          if ((xValue1 == typeof(UIntPtr) && xValue2 == typeof(byte*))
              || (xValue1 == typeof(byte*) && xValue2 == typeof(UIntPtr)))
          {
            return;
          }

          if ((xValue1 == typeof(long) && xValue2 == typeof(ulong))
            || (xValue1 == typeof(ulong) && xValue2 == typeof(long)))
          {
            return;
          }
          if (xValue1.GetTypeInfo().IsClass &&
              xValue2.GetTypeInfo().IsClass)
          {
            return;
          }

          throw new Exception(String.Format("Comparing types '{0}' and '{1}' not supported!", xValue1.AssemblyQualifiedName, xValue2.AssemblyQualifiedName));
        default:
          throw new NotImplementedException("Checks for opcode " + OpCode + " not implemented!");
      }
    }

    protected override void DoInterpretNextInstructionStackTypes(IDictionary<int, ILOpCode> aOpCodes, Stack<Type> aStack, ref bool aSituationChanged, int aMaxRecursionDepth)
    {
      switch (OpCode)
      {
        case Code.Brtrue:
        case Code.Brfalse:
        case Code.Blt:
        case Code.Blt_Un:
        case Code.Ble:
        case Code.Ble_Un:
        case Code.Bgt:
        case Code.Bgt_Un:
        case Code.Bge:
        case Code.Bge_Un:
        case Code.Beq:
        case Code.Bne_Un:
        case Code.Br:
          InterpretInstructionIfNotYetProcessed(Value, aOpCodes, new Stack<Type>(aStack.Reverse()), ref aSituationChanged, aMaxRecursionDepth);
          base.DoInterpretNextInstructionStackTypesIfNotYetProcessed(aOpCodes, new Stack<Type>(aStack.Reverse()), ref aSituationChanged, aMaxRecursionDepth);
          break;
        case Code.Leave:
          InterpretInstructionIfNotYetProcessed(Value, aOpCodes, new Stack<Type>(aStack.Reverse()), ref aSituationChanged, aMaxRecursionDepth);
          base.DoInterpretNextInstructionStackTypesIfNotYetProcessed(aOpCodes, new Stack<Type>(aStack.Reverse()), ref aSituationChanged, aMaxRecursionDepth);

          break;
        default:
          throw new NotImplementedException("OpCode " + OpCode);
      }
    }
  }
}
