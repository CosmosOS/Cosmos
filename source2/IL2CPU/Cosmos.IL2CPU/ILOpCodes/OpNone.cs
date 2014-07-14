using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpNone : ILOpCode {

    public OpNone(Code aOpCode, int aPos, int aNextPos, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler)
    {
    }

    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Pop:
            return 1;
          case Code.Nop:
            return 0;
          case Code.Ret:
            return 1;
          case Code.Conv_I:
          case Code.Conv_I1:
          case Code.Conv_I2:
          case Code.Conv_I4:
          case Code.Conv_I8:
          case Code.Conv_U:
          case Code.Conv_U1:
          case Code.Conv_U2:
          case Code.Conv_U4:
          case Code.Conv_U8:
          case Code.Conv_R4:
          case Code.Conv_R8:
          case Code.Conv_R_Un:
          case Code.Conv_Ovf_I:
          case Code.Conv_Ovf_I1:
          case Code.Conv_Ovf_I1_Un:
          case Code.Conv_Ovf_I2:
          case Code.Conv_Ovf_I2_Un:
          case Code.Conv_Ovf_I4:
          case Code.Conv_Ovf_I4_Un:
          case Code.Conv_Ovf_I8:
          case Code.Conv_Ovf_I8_Un:
          case Code.Conv_Ovf_I_Un:
          case Code.Conv_Ovf_U:
          case Code.Conv_Ovf_U1:
            case Code.Conv_Ovf_U1_Un:
          case Code.Conv_Ovf_U2:
          case Code.Conv_Ovf_U2_Un:
          case Code.Conv_Ovf_U4:
          case Code.Conv_Ovf_U4_Un:
          case Code.Conv_Ovf_U8:
          case Code.Conv_Ovf_U8_Un:
          case Code.Conv_Ovf_U_Un:
            return 1;
          case Code.Add:
          case Code.Mul:
          case Code.Div:
          case Code.Div_Un:
          case Code.Sub:
          case Code.Rem:
          case Code.Rem_Un:
          case Code.Xor:
            return 2;
          case Code.Ldind_I:
          case Code.Ldind_I1:
          case Code.Ldind_I2:
          case Code.Ldind_I4:
          case Code.Ldind_I8:
          case Code.Ldind_U1:
          case Code.Ldind_U2:
          case Code.Ldind_U4:
          case Code.Ldind_R4:
          case Code.Ldind_R8:
          case Code.Ldind_Ref:
            return 1;
          case Code.Stind_I:
          case Code.Stind_I1:
          case Code.Stind_I2:
          case Code.Stind_I4:
          case Code.Stind_I8:
          case Code.Stind_Ref:
            return 2;
          case Code.Clt:
            return 2;
          case Code.Clt_Un:
            return 2;
          case Code.Cgt:
            return 2;
          case Code.Cgt_Un:
            return 2;
          case Code.Ceq:
            return 2;
          case Code.Throw:
            return 1;
          case Code.Or:
          case Code.And:
            return 2;
          case Code.Stelem_Ref:
          case Code.Stelem_I:
          case Code.Stelem_I1:
          case Code.Stelem_I2:
          case Code.Stelem_I4:
          case Code.Stelem_I8:
            return 3;
          case Code.Shr:
          case Code.Shr_Un:
          case Code.Shl:
            return 2;
          case Code.Neg:
            return 1;
          case Code.Ldlen:
            return 1;
          case Code.Ldelem:
          case Code.Ldelem_Ref:
          case Code.Ldelem_I:
          case Code.Ldelem_I1:
          case Code.Ldelem_I2:
          case Code.Ldelem_I4:
          case Code.Ldelem_I8:
          case Code.Ldelem_U1:
          case Code.Ldelem_U2:
          case Code.Ldelem_U4:
            return 2;
          case Code.Ldnull:
            return 0;
          case Code.Dup:
            return 1;
          case Code.Volatile:
            return 1;
          case Code.Endfinally:
            return 0;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }

    public override int NumberOfStackPushes
    {
      get
      {
        switch (OpCode)
        {
          case Code.Pop:
            return 0;
          case Code.Ret:
            return 0;
          case Code.Nop:
            return 0;
          case Code.Conv_I:
          case Code.Conv_I1:
          case Code.Conv_I2:
          case Code.Conv_I4:
          case Code.Conv_I8:
          case Code.Conv_U:
          case Code.Conv_U1:
          case Code.Conv_U2:
          case Code.Conv_U4:
          case Code.Conv_U8:
          case Code.Conv_R4:
          case Code.Conv_R8:
          case Code.Conv_R_Un:
          case Code.Conv_Ovf_I:
          case Code.Conv_Ovf_I1:
          case Code.Conv_Ovf_I1_Un:
          case Code.Conv_Ovf_I2:
          case Code.Conv_Ovf_I2_Un:
          case Code.Conv_Ovf_I4:
          case Code.Conv_Ovf_I4_Un:
          case Code.Conv_Ovf_I8:
          case Code.Conv_Ovf_I8_Un:
          case Code.Conv_Ovf_I_Un:
          case Code.Conv_Ovf_U:
          case Code.Conv_Ovf_U1:
          case Code.Conv_Ovf_U1_Un:
          case Code.Conv_Ovf_U2:
          case Code.Conv_Ovf_U2_Un:
          case Code.Conv_Ovf_U4:
          case Code.Conv_Ovf_U4_Un:
          case Code.Conv_Ovf_U8:
          case Code.Conv_Ovf_U8_Un:
          case Code.Conv_Ovf_U_Un:
            return 1;
          case Code.Add:
          case Code.Mul:
          case Code.Div:
          case Code.Div_Un:
          case Code.Sub:
          case Code.Rem:
          case Code.Rem_Un:
          case Code.Xor:
            return 1;
          case Code.Ldind_I:
          case Code.Ldind_I1:
          case Code.Ldind_I2:
          case Code.Ldind_I4:
          case Code.Ldind_I8:
          case Code.Ldind_U1:
          case Code.Ldind_U2:
          case Code.Ldind_U4:
          case Code.Ldind_R4:
          case Code.Ldind_R8:
          case Code.Ldind_Ref:
            return 1;
          case Code.Stind_I:
          case Code.Stind_I1:
          case Code.Stind_I2:
          case Code.Stind_I4:
          case Code.Stind_I8:
          case Code.Stind_Ref:
            return 0;
          case Code.Clt:
            return 1;
          case Code.Clt_Un:
            return 1;
          case Code.Cgt:
            return 1;
          case Code.Cgt_Un:
            return 1;
          case Code.Ceq:
            return 1;
          case Code.Throw:
            return 0;
          case Code.Or:
          case Code.And:
            return 1;
          case Code.Stelem_I:
          case Code.Stelem_I1:
          case Code.Stelem_I2:
          case Code.Stelem_I4:
          case Code.Stelem_I8:
          case Code.Stelem_Ref:
            return 0;
            case Code.Shr:
          case Code.Shr_Un:
          case Code.Shl:
            return 1;
          case Code.Neg:
            return 1;
          case Code.Ldlen:
            return 1;
          case Code.Ldelem:
          case Code.Ldelem_Ref:
          case Code.Ldelem_I:
          case Code.Ldelem_I1:
          case Code.Ldelem_I2:
          case Code.Ldelem_I4:
          case Code.Ldelem_I8:
          case Code.Ldelem_U1:
          case Code.Ldelem_U2:
          case Code.Ldelem_U4:
            return 2;
          case Code.Ldnull:
            return 1;
          case Code.Dup:
            return 2;
          case Code.Volatile:
            return 1;
          case Code.Endfinally:
            return 0;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }

    protected override void DoInitStackAnalysis()
    {
      base.DoInitStackAnalysis();

      switch (OpCode)
      {
        case Code.Stind_I:
          StackPopTypes[0] = typeof (IntPtr);
          StackPopTypes[1] = typeof (IntPtr);
          return;

        case Code.Stind_I1:
          StackPopTypes[0] = typeof (sbyte);
          StackPopTypes[1] = typeof (IntPtr);
          return;

        case Code.Stind_I2:
          StackPopTypes[0] = typeof (short);
          StackPopTypes[1] = typeof (IntPtr);
          return;

        case Code.Stind_I4:
          StackPopTypes[0] = typeof (int);
          StackPopTypes[1] = typeof (IntPtr);
          return;

        case Code.Stind_I8:
          StackPopTypes[0] = typeof (long);
          StackPopTypes[1] = typeof (IntPtr);
          return;

        case Code.Ldind_U1:
          StackPushTypes[0] = typeof (byte);
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Ldind_U2:
          StackPushTypes[0] = typeof (ushort);
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Ldind_U4:
          StackPushTypes[0] = typeof (UInt32);
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Ldind_R4:
          StackPushTypes[0] = typeof (Single);
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Ldind_R8:
          StackPushTypes[0] = typeof (Double);
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Conv_I:
          StackPushTypes[0] = typeof (IntPtr);
          break;

        case Code.Conv_I1:
          StackPushTypes[0] = typeof (sbyte);
          break;

        case Code.Conv_I2:
          StackPushTypes[0] = typeof (short);
          break;

        case Code.Conv_I4:
          StackPushTypes[0] = typeof (int);
          break;

        case Code.Conv_I8:
          StackPushTypes[0] = typeof (long);
          break;

        case Code.Conv_U:
          StackPushTypes[0] = typeof (UIntPtr);
          break;

        case Code.Conv_U1:
          StackPushTypes[0] = typeof (byte);
          break;

        case Code.Conv_U2:
          StackPushTypes[0] = typeof (ushort);
          break;

        case Code.Conv_U4:
          StackPushTypes[0] = typeof (uint);
          break;

        case Code.Conv_U8:
          StackPushTypes[0] = typeof (ulong);
          break;

        case Code.Conv_R4:
          StackPushTypes[0] = typeof(Single);
          break;

        case Code.Conv_R8:
          StackPushTypes[0] = typeof(Double);
          break;
        case Code.Conv_Ovf_I:
          StackPushTypes[0] = typeof(IntPtr);
          break;
        case Code.Conv_Ovf_I1:
          StackPushTypes[0] = typeof(sbyte);
          break;
        case Code.Conv_Ovf_I1_Un:
          StackPushTypes[0] = typeof(sbyte);
          break;
        case Code.Conv_Ovf_I2:
          StackPushTypes[0] = typeof(short);
          break;
        case Code.Conv_Ovf_I2_Un:
          StackPushTypes[0] = typeof(short);
          break;
        case Code.Conv_Ovf_I4:
          StackPushTypes[0] = typeof(int);
          break;
        case Code.Conv_Ovf_I4_Un:
          StackPushTypes[0] = typeof(int);
          break;
        case Code.Conv_Ovf_I8:
          StackPushTypes[0] = typeof(long);
          break;
        case Code.Conv_Ovf_I8_Un:
          StackPushTypes[0] = typeof(long);
          break;
        case Code.Conv_Ovf_I_Un:
          StackPushTypes[0] = typeof(IntPtr);
          break;
        case Code.Conv_Ovf_U:
          StackPushTypes[0] = typeof(UIntPtr);
          break;
        case Code.Conv_Ovf_U1:
          StackPushTypes[0] = typeof(byte);
          break;
        case Code.Conv_Ovf_U1_Un:
          StackPushTypes[0] = typeof(byte);
          break;
        case Code.Conv_Ovf_U2:
          StackPushTypes[0] = typeof(ushort);
          break;
        case Code.Conv_Ovf_U2_Un:
          StackPushTypes[0] = typeof(ushort);
          break;
        case Code.Conv_Ovf_U4:
          StackPushTypes[0] = typeof(uint);
          break;
        case Code.Conv_Ovf_U4_Un:
          StackPushTypes[0] = typeof(uint);
          break;
        case Code.Conv_Ovf_U8:
          StackPushTypes[0] = typeof(ulong);
          break;
        case Code.Conv_Ovf_U8_Un:
          StackPushTypes[0] = typeof(ulong);
          break;
        case Code.Conv_Ovf_U_Un:
          StackPushTypes[0] = typeof(UIntPtr);
          break;

        case Code.Clt:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Clt_Un:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Cgt:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Cgt_Un:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Ceq:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Throw:
          StackPopTypes[0] = typeof (object);
          return;
        case Code.Ldlen:
          StackPushTypes[0] = typeof (UIntPtr);
          return;

        case Code.Ldelem_I:
          StackPushTypes[0] = typeof (IntPtr);
          return;
        case Code.Ldelem_I1:
          StackPushTypes[0] = typeof (sbyte);
          return;
        case Code.Ldelem_I2:
          StackPushTypes[0] = typeof (short);
          return;
        case Code.Ldelem_I4:
          StackPushTypes[0] = typeof (int);
          return;
        case Code.Ldelem_I8:
          StackPushTypes[0] = typeof (long);
          return;
        case Code.Ldelem_U1:
          StackPushTypes[0] = typeof (byte);
          return;
        case Code.Ldelem_U2:
          StackPushTypes[0] = typeof (ushort);
          return;
        case Code.Ldelem_U4:
          StackPushTypes[0] = typeof (uint);
          return;
        case Code.Ldnull:
          StackPushTypes[0] = typeof (NullRef);
          return;

        case Code.Ldind_I:
          StackPushTypes[0] = typeof(IntPtr);
          StackPopTypes[0] = typeof(IntPtr);
          return;

        case Code.Ldind_I1:
          StackPushTypes[0] = typeof(sbyte);
          StackPopTypes[0] = typeof(IntPtr);
          return;

        case Code.Ldind_I2:
          StackPushTypes[0] = typeof(short);
          StackPopTypes[0] = typeof(IntPtr);
          return;

        case Code.Ldind_I4:
          StackPushTypes[0] = typeof(Int32);
          StackPopTypes[0] = typeof(IntPtr);
          return;

        case Code.Ldind_I8:
          StackPushTypes[0] = typeof(long);
          StackPopTypes[0] = typeof(IntPtr);
          return;

        case Code.Stelem_I:
          StackPopTypes[0] = typeof (IntPtr);
          return;

        case Code.Stelem_I1:
          StackPopTypes[0] = typeof(sbyte);
          return;

        case Code.Stelem_I2:
          StackPopTypes[0] = typeof(short);
          return;

        case Code.Stelem_I4:
          StackPopTypes[0] = typeof(int);
          return;

        case Code.Stelem_I8:
          StackPopTypes[0] = typeof(long);
          return;
      }
    }
  }
}
