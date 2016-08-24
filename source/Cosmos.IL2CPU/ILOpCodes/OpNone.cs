using System;
using System.Linq;
using System.Reflection;

using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpNone : ILOpCode {

    public OpNone(Code aOpCode, int aPos, int aNextPos, ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler)
    {
    }

    public override int GetNumberOfStackPops(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Pop:
          return 1;
        case Code.Nop:
          return 0;
        case Code.Ret:
          var methodInfo = aMethod as SysReflection.MethodInfo;
          if (methodInfo != null && methodInfo.ReturnType != typeof (void))
          {
            return 1;
          }
          else
          {
            return 0;
          }
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
        case Code.Add_Ovf:
        case Code.Mul:
        case Code.Mul_Ovf:
        case Code.Mul_Ovf_Un:
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
        case Code.Not:
          return 1;
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
        case Code.Localloc:
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
          return 0;
        case Code.Endfinally:
          return 0;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    public override int GetNumberOfStackPushes(MethodBase aMethod)
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
        case Code.Add_Ovf:
        case Code.Mul:
        case Code.Mul_Ovf:
        case Code.Mul_Ovf_Un:
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
        case Code.Not:
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
        case Code.Localloc:
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
          return 1;
        case Code.Ldnull:
          return 1;
        case Code.Dup:
          return 2;
        case Code.Volatile:
          return 0;
        case Code.Endfinally:
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
        case Code.Ldind_U1:
          StackPushTypes[0] = typeof (byte);
          return;

        case Code.Ldind_U2:
          StackPushTypes[0] = typeof (ushort);
          return;

        case Code.Ldind_U4:
          StackPushTypes[0] = typeof (UInt32);
          return;

        case Code.Ldind_R4:
          StackPushTypes[0] = typeof (Single);
          return;

        case Code.Ldind_R8:
          StackPushTypes[0] = typeof (Double);
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
          return;

        case Code.Ldind_I1:
          StackPushTypes[0] = typeof(sbyte);
          return;

        case Code.Ldind_I2:
          StackPushTypes[0] = typeof(short);
          return;

        case Code.Ldind_I4:
          StackPushTypes[0] = typeof(Int32);
          return;

        case Code.Ldind_I8:
          StackPushTypes[0] = typeof(long);
          return;

        case Code.Stelem_I4:
          StackPopTypes[0] = typeof(int);
          return;

        case Code.Stelem_I8:
          StackPopTypes[0] = typeof(long);
          return;
        case Code.Conv_R_Un:
          StackPushTypes[0] = typeof (Double);
          return;
      }
    }

    protected override void DoInterpretStackTypes(ref bool aSituationChanged)
    {
      base.DoInterpretStackTypes(ref aSituationChanged);
      switch (OpCode)
      {
        case Code.Add:
        case Code.Mul:
        case Code.Mul_Ovf:
        case Code.Mul_Ovf_Un:
        case Code.Div:
        case Code.Div_Un:
        case Code.Sub:
        case Code.Rem:
        case Code.Rem_Un:
        case Code.Xor:
        case Code.And:
        case Code.Or:
          if (StackPushTypes[0] != null)
          {
            return;
          }
          if (!StackPopTypes.Contains(null))
          {
            // PopTypes set, but PushType not yet, so fill it.

            if (StackPopTypes[0] == typeof(bool) && StackPopTypes[1] == typeof(bool))
            {
                StackPushTypes[0] = typeof(bool);
                aSituationChanged = true;
                return;
            }

            if ((StackPopTypes[0] == typeof(bool) && StackPopTypes[1] == typeof(Int32)) ||
              (StackPopTypes[0] == typeof(Int32) && StackPopTypes[1] == typeof(bool)))
            {
              StackPushTypes[0] = typeof(Int32);
              aSituationChanged = true;
              return;
            }

            if ((StackPopTypes[0] == typeof (IntPtr) && StackPopTypes[1] == typeof (uint*))
              || (StackPopTypes[0] == typeof (uint*) && StackPopTypes[1] == typeof (IntPtr)))
            {
              StackPushTypes[0] = typeof (uint*);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(UIntPtr) && StackPopTypes[1] == typeof(uint*))
              || (StackPopTypes[0] == typeof(uint*) && StackPopTypes[1] == typeof(UIntPtr)))
            {
              StackPushTypes[0] = typeof(uint*);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(byte*))
              || (StackPopTypes[0] == typeof(byte*) && StackPopTypes[1] == typeof(uint)))
            {
              StackPushTypes[0] = typeof(byte*);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(byte*))
              || (StackPopTypes[0] == typeof(byte*) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(byte*);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(IntPtr) && StackPopTypes[1] == typeof(byte*))
              || (StackPopTypes[0] == typeof(byte*) && StackPopTypes[1] == typeof(IntPtr)))
            {
              StackPushTypes[0] = typeof(byte*);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(IntPtr) && StackPopTypes[1] == typeof(uint))
              || (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(IntPtr)))
            {
              StackPushTypes[0] = typeof(UIntPtr);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(UIntPtr))
              || (StackPopTypes[0] == typeof(UIntPtr) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(UIntPtr);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(IntPtr))
              || (StackPopTypes[0] == typeof(IntPtr) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(IntPtr);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(uint))
              || (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(short) && StackPopTypes[1] == typeof(ushort))
              || (StackPopTypes[0] == typeof(ushort) && StackPopTypes[1] == typeof(short)))
            {
              StackPushTypes[0] = typeof(short);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(byte))
             || (StackPopTypes[0] == typeof(byte) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(short))
             || (StackPopTypes[0] == typeof(short) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(long) && StackPopTypes[1] == typeof(ulong))
             || (StackPopTypes[0] == typeof(ulong) && StackPopTypes[1] == typeof(long)))
            {
              StackPushTypes[0] = typeof(long);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(ushort))
             || (StackPopTypes[0] == typeof(ushort) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(byte) && StackPopTypes[1] == typeof(uint))
             || (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(byte)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(ushort) && StackPopTypes[1] == typeof(uint))
            || (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(ushort)))
            {
              StackPushTypes[0] = typeof(uint);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(char))
             || (StackPopTypes[0] == typeof(char) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(IntPtr) && StackPopTypes[1] == typeof(UIntPtr))
             || (StackPopTypes[0] == typeof(UIntPtr) && StackPopTypes[1] == typeof(IntPtr)))
            {
              StackPushTypes[0] = typeof(UIntPtr);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(IntPtr) && StackPopTypes[1] == typeof(IntPtr))
            {
              StackPushTypes[0] = typeof(IntPtr);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(uint))
            {
              StackPushTypes[0] = typeof(uint);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(uint) && StackPopTypes[1] == typeof(char))
            {
              StackPushTypes[0] = typeof(uint);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(byte) && StackPopTypes[1] == typeof(byte))
            {
              StackPushTypes[0] = typeof(byte);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(int))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(bool))
             || (StackPopTypes[0] == typeof(bool) && StackPopTypes[1] == typeof(int)))

            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(ushort) && StackPopTypes[1] == typeof(ushort))
            {
              StackPushTypes[0] = typeof(ushort);
              aSituationChanged = true;
              return;
            }
            //Changed
            if (StackPopTypes[0] == typeof(short) && StackPopTypes[1] == typeof(short))
            {
              StackPushTypes[0] = typeof(short);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(long) && StackPopTypes[1] == typeof(long))
            {
              StackPushTypes[0] = typeof(long);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(ulong) && StackPopTypes[1] == typeof(ulong))
            {
              StackPushTypes[0] = typeof(ulong);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(Double) && StackPopTypes[1] == typeof(Double))
            {
              StackPushTypes[0] = typeof(Double);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(Single) && StackPopTypes[1] == typeof(Single))
            {
              StackPushTypes[0] = typeof(Single);
              aSituationChanged = true;
              return;
            }

            if ((StackPopTypes[0] == typeof(int) && StackPopTypes[1] == typeof(sbyte))
             || (StackPopTypes[0] == typeof(sbyte) && StackPopTypes[1] == typeof(int)))
            {
              StackPushTypes[0] = typeof(int);
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == StackPopTypes[1] && StackPopTypes[0].IsPointer)
            {
              StackPushTypes[0] = StackPopTypes[0];
              aSituationChanged = true;
              return;
            }
            if (StackPopTypes[0] == typeof(int) &&
                StackPopTypes[1].IsPointer)
            {
              StackPushTypes[0] = StackPopTypes[1];
              aSituationChanged = true;
              return;
            }
            if ((StackPopTypes[0] == typeof(IntPtr) || StackPopTypes[0] == typeof(UIntPtr)) &&
                StackPopTypes[1].IsPointer)
            {
              StackPushTypes[0] = StackPopTypes[1];
              aSituationChanged = true;
              return;
            }
            if (OpCode == Code.Add &&
                ((StackPopTypes[0] == typeof(IntPtr) && (StackPopTypes[1].IsPointer || StackPopTypes[1].IsByRef))
                 || ((StackPopTypes[0].IsPointer || StackPopTypes[0].IsByRef) && StackPopTypes[1] == typeof(IntPtr))))
            {
              if (StackPopTypes[0] == typeof(IntPtr))
              {
                StackPushTypes[0] = StackPopTypes[1];
              }
              else
              {
                StackPushTypes[0] = StackPopTypes[0];
              }
              aSituationChanged = true;
              return;
            }

            throw new NotImplementedException(string.Format("{0} on types '{1}' and '{2}' not yet implemented!", OpCode, StackPopTypes[0], StackPopTypes[1]));
          }
          break;
        case Code.Localloc:
          StackPushTypes[0] = typeof(void*);
          aSituationChanged = true;
          return;
        case Code.Stelem_I2:
          var xTypeValue = StackPopTypes[0];
          if (xTypeValue == null)
          {
            return;
          }

          if (xTypeValue == typeof (byte)
            || xTypeValue == typeof (char)
            || xTypeValue == typeof(short)
            || xTypeValue == typeof(ushort)
            || xTypeValue == typeof(int))
          {
            return;
          }
          throw new NotImplementedException(String.Format("Stelem_I2 storing type '{0}' is not implemented!", xTypeValue));
        case Code.Shl:
        case Code.Shr:
        case Code.Shr_Un:
          xTypeValue = StackPopTypes[1];
          var xTypeShift = StackPopTypes[0];
          if (xTypeValue == null || xTypeShift == null)
          {
            return;
          }
          if (xTypeValue == typeof (int) && xTypeShift == typeof (int))
          {
            StackPushTypes[0] = typeof (int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(byte) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(long) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(long);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(IntPtr) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(int) && xTypeShift == typeof(IntPtr))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(ushort) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(char) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(uint) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(long) && xTypeShift == typeof(IntPtr))
          {
            StackPushTypes[0] = typeof(long);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(IntPtr) && xTypeShift == typeof(IntPtr))
          {
            StackPushTypes[0] = typeof(IntPtr);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(IntPtr) && xTypeShift == typeof(IntPtr))
          {
            StackPushTypes[0] = typeof(IntPtr);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(ulong) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(ulong);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(sbyte) && xTypeShift == typeof(int))
          {
            StackPushTypes[0] = typeof(int);
            aSituationChanged = true;
            return;
          }
          if (xTypeValue == typeof(short) && xTypeShift == typeof(int))
          {
             StackPushTypes[0] = typeof(int);
             aSituationChanged = true;
             return;
          }
          throw new NotImplementedException(String.Format("{0} with types {1} and {2} is not implemented!", OpCode, xTypeValue.FullName, xTypeShift.FullName));
        case Code.Ldelem_Ref:
          if (StackPushTypes[0] != null)
          {
            return;
          }
          var xTypeArray = StackPopTypes[1];
          if (xTypeArray == null)
          {
            return;
          }
          if (!xTypeArray.IsArray)
          {
            throw new Exception("Ldelem Array type is not an array (Actual = " + xTypeArray.FullName + ")");
          }
          StackPushTypes[0] = xTypeArray.GetElementType();
          aSituationChanged = true;
          break;
        case Code.Not:
        case Code.Neg:
          if (StackPushTypes[0] != null)
          {
            return;
          }
          if (StackPopTypes[0] != null)
          {
            StackPushTypes[0] = StackPopTypes[0];
            aSituationChanged = true;
            return;
          }
          break;
        case Code.Dup:
          if (StackPushTypes[0] != null && StackPushTypes[1] != null)
          {
            return;
          }
          if (StackPopTypes[0] != null)
          {
            StackPushTypes[0] = StackPopTypes[0];
            StackPushTypes[1] = StackPopTypes[0];
            aSituationChanged = true;
            return;
          }
          return;
        case Code.Stind_I1:
          if (StackPopTypes[1] == null || StackPopTypes[0] == null)
          {
            return;
          }
          if (!IsIntegralType(StackPopTypes[0]))
          {
            throw new Exception("Wrong value type: " + StackPopTypes[0].FullName);
          }
          if (!IsPointer(StackPopTypes[1]))
          {
            throw new Exception("Wrong Pointer type: " + StackPopTypes[1].FullName);
          }
          break;
        case Code.Stind_I2:
          if (StackPopTypes[1] == null || StackPopTypes[0] == null)
          {
            return;
          }
          if (!IsIntegralType(StackPopTypes[0]))
          {
            throw new Exception("Wrong value type: " + StackPopTypes[0].FullName);
          }
          if (!IsPointer(StackPopTypes[1]))
          {
            throw new Exception("Wrong Pointer type: " + StackPopTypes[1].FullName);
          }
          break;
        case Code.Stind_I4:
          if (StackPopTypes[1] == null || StackPopTypes[0] == null)
          {
            return;
          }
          if (!IsIntegralType(StackPopTypes[0]))
          {
            throw new Exception("Wrong value type: " + StackPopTypes[0].FullName);
          }
          if (!IsPointer(StackPopTypes[1]))
          {
            throw new Exception("Wrong Pointer type: " + StackPopTypes[1].FullName);
          }
          break;
        case Code.Stind_I8:
          if (StackPopTypes[1] == null || StackPopTypes[0] == null)
          {
            return;
          }
          if (!IsIntegralType(StackPopTypes[0]))
          {
            throw new Exception("Wrong value type: " + StackPopTypes[0].FullName);
          }
          if (!IsPointer(StackPopTypes[1]))
          {
            throw new Exception("Wrong Pointer type: " + StackPopTypes[1].FullName);
          }
          break;
        case Code.Stind_I:
          if (StackPopTypes[1] == null || StackPopTypes[0] == null)
          {
            return;
          }
          if (!IsIntegralTypeOrPointer(StackPopTypes[0]))
          {
            throw new Exception("Wrong value type: " + StackPopTypes[0].FullName);
          }
          if (!IsPointer(StackPopTypes[1]))
          {
            throw new Exception("Wrong Pointer type: " + StackPopTypes[1].FullName);
          }
          break;
        case Code.Ldind_Ref:
          if (StackPushTypes[0] != null)
          {
            return;
          }
          if (StackPopTypes[0] == null)
          {
            return;
          }
          if (!StackPopTypes[0].IsByRef)
          {
            throw new Exception("Invalid ref type: " + StackPopTypes[0].FullName);
          }
          StackPushTypes[0] = StackPopTypes[0].GetElementType();
          aSituationChanged = true;
          break;
      }
    }
  }
}
