using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Cosmos.IL2CPU {
    public class ILReader {
      // We split this into two arrays since we have to read
      // a byte at a time anways. In the future if we need to 
      // back to a unifed array, instead of 64k entries 
      // we can change it to a signed int, and then add x0200 to the value.
      // This will reduce array size down to 768 entries.
      protected OpCode[] mOpCodesLo = new OpCode[256];
      protected OpCode[] mOpCodesHi = new OpCode[256];
      
      public ILReader() {
        LoadOpCodes();
      }

      protected void LoadOpCodes() {
        foreach (var xField in typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)) {
          var xOpCode = (OpCode)xField.GetValue(null);
          var xValue = (ushort)xOpCode.Value;
          if (xValue <= 0xFF) {
            mOpCodesLo[xValue] = xOpCode;
          } else {
            mOpCodesHi[xValue & 0xFF] = xOpCode;
          }
        }
      }

      public List<ILOpCode> ProcessMethod(MethodBase aMethod) {
        var xResult = new List<ILOpCode>();
        var xBody = aMethod.GetMethodBody();

        // Some methods return no body. Not sure why.. have to investigate
        // They arent abstracts or icalls...
        if (xBody == null) {
          return null;
        }

        var xIL = xBody.GetILAsByteArray();
        int xPos = 0;
        while (xPos < xIL.Length) {
          ILOpCode.Code xOpCodeVal;
          Type xILOpCodeType;
          OpCode xOpCode;
          if (xIL[xPos] == 0xFE) {
            xOpCodeVal = (ILOpCode.Code)(0xFE00 | xIL[xPos + 1]);
            xOpCode = mOpCodesHi[xIL[xPos + 1]];
            xPos = xPos + 2;
          } else {
            xOpCodeVal = (ILOpCode.Code)xIL[xPos];
            xOpCode = mOpCodesLo[xIL[xPos]];
            xPos++;
          }

          //TODO: Need to look at OpCode operandtype and queue for these:
          // probably dont need to look by op, but can do by operand instead.
          // Call: QueueMethod(aReader.OperandValueMethod);
          // Callvirt: QueueMethod(aReader.OperandValueMethod);
          // Newobj: QueueMethod(aReader.OperandValueMethod);

          ILOpCode xILOpCode = null;
          switch (xOpCode.OperandType) {
            // The operand is a 32-bit integer branch target.
            case OperandType.InlineBrTarget:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // The operand is a 32-bit metadata token.
            case OperandType.InlineField:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // The operand is a 32-bit integer.
            case OperandType.InlineI:
              xILOpCode = new ILOpCodes.InlineI(xOpCodeVal, ReadUInt32(xIL, xPos));
              xPos = xPos + 4;
              break;

            // The operand is a 64-bit integer.
            case OperandType.InlineI8:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 8;
              break;

            // The operand is a 32-bit metadata token.
            case OperandType.InlineMethod:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // No operand.
            case OperandType.InlineNone:
              xILOpCode = new ILOpCodes.InlineNone(xOpCodeVal);
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;

            // 64-bit IEEE floating point number.
            case OperandType.InlineR:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 8;
              break;

            // 32-bit metadata signature token.
            case OperandType.InlineSig:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // 32-bit metadata string token.
            case OperandType.InlineString:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            case OperandType.InlineSwitch: {
                int xCount = (int)ReadUInt32(xIL, xPos);
                int[] xBranchLocations = new int[xCount];
                uint[] xBranchValues = new uint[xCount];
                for (int i = 0; i < xCount; i++) {
                  xBranchLocations[i] = xIL[xPos + i + 5];
                  //xBranchValues[i] = 
                  //                if ((mPosition + xBranchLocations1[i]) < 0) {
                  //                    xResult[i] = (uint)xBranchLocations1[i];
                  //                } else {
                  //                    xResult[i] = (uint)(mPosition + xBranchLocations1[i]);
                  //                }
                }
                xILOpCode = new ILOpCode(xOpCodeVal);
                xPos = xPos + 4 + xCount * 4;
                break;
              }

            // The operand is a FieldRef, MethodRef, or TypeRef token.
            case OperandType.InlineTok:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // 32-bit metadata token.
            case OperandType.InlineType:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // 16-bit integer containing the ordinal of a local variable or an argument.
            case OperandType.InlineVar:
              xILOpCode = new ILOpCodes.InlineVar(xOpCodeVal, ReadUInt16(xIL, xPos));
              xPos = xPos + 2;
              break;

            // 8-bit integer branch target.
            case OperandType.ShortInlineBrTarget:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 1;
              break;

            // The operand is an 8-bit integer.
            case OperandType.ShortInlineI:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 1;
              break;

            // 32-bit IEEE floating point number.
            case OperandType.ShortInlineR:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            // 8-bit integer containing the ordinal of a local variable or an argument.
            case OperandType.ShortInlineVar:
              xILOpCode = new ILOpCode(xOpCodeVal);
              xPos = xPos + 4;
              break;

            default:
              throw new Exception("Unknown OperandType");
          }

          #region Expand shortcuts
          // This region expands shortcut ops into full ops
          // This elminates the amount of code required in the assemblers
          // by allowing them to ignore the shortcuts
          switch (xOpCodeVal) {
            case ILOpCode.Code.Beq_S:
              //xILOpCode = new ILOpCodes.xxx(ILOpCode.Code.Beq, xILOpCode.value);
              break;

            case ILOpCode.Code.Bge_S:
              //return Code.Bge;
              break;

            case ILOpCode.Code.Bge_Un_S:
              //return Code.Bge_Un;
              break;

            case ILOpCode.Code.Bgt_S:
              //return Code.Bgt;
              break;

            case ILOpCode.Code.Bgt_Un_S:
              //return Code.Bgt_Un;
              break;

            case ILOpCode.Code.Ble_S:
              //return Code.Ble;
              break;

            case ILOpCode.Code.Ble_Un_S:
              //return Code.Ble_Un;
              break;

            case ILOpCode.Code.Blt_S:
              //return Code.Blt;
              break;

            case ILOpCode.Code.Blt_Un_S:
              //return Code.Blt_Un;
              break;

            case ILOpCode.Code.Bne_Un_S:
              //return Code.Bne_Un;
              break;

            case ILOpCode.Code.Br_S:
              //return Code.Br;
              break;

            case ILOpCode.Code.Brfalse_S:
              //return Code.Brfalse;
              break;

            case ILOpCode.Code.Brtrue_S:
              //return Code.Brtrue;
              break;

            case ILOpCode.Code.Ldarg_0:
              xILOpCode = new ILOpCodes.InlineVar(ILOpCode.Code.Ldarg, 0);
              break;

            case ILOpCode.Code.Ldarg_1:
              xILOpCode = new ILOpCodes.InlineVar(ILOpCode.Code.Ldarg, 1);
              break;

            case ILOpCode.Code.Ldarg_2:
              xILOpCode = new ILOpCodes.InlineVar(ILOpCode.Code.Ldarg, 2);
              break;

            case ILOpCode.Code.Ldarg_3:
              xILOpCode = new ILOpCodes.InlineVar(ILOpCode.Code.Ldarg, 3);
              break;

            case ILOpCode.Code.Ldarg_S:
              //return Code.Ldarg;
              break;

            case ILOpCode.Code.Ldarga_S:
              //return Code.Ldarga;
              break;

            case ILOpCode.Code.Ldc_I4_0:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_1:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_2:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_3:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_4:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_5:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_6:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_7:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_8:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_M1:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldc_I4_S:
              //return Code.Ldc_I4;
              break;

            case ILOpCode.Code.Ldloc_0:
              //return Code.Ldloc;
              break;

            case ILOpCode.Code.Ldloc_1:
              //return Code.Ldloc;
              break;

            case ILOpCode.Code.Ldloc_2:
              //return Code.Ldloc;
              break;

            case ILOpCode.Code.Ldloc_3:
              //return Code.Ldloc;
              break;

            case ILOpCode.Code.Ldloc_S:
              //return Code.Ldloc;
              break;

            case ILOpCode.Code.Ldloca_S:
              //return Code.Ldloca;
              break;

            case ILOpCode.Code.Leave_S:
              //return Code.Leave;
              break;

            case ILOpCode.Code.Starg_S:
              //return Code.Starg;
              break;

            case ILOpCode.Code.Stloc_0:
              //return Code.Stloc;
              break;

            case ILOpCode.Code.Stloc_1:
              //return Code.Stloc;
              break;

            case ILOpCode.Code.Stloc_2:
              //return Code.Stloc;
              break;

            case ILOpCode.Code.Stloc_3:
              //return Code.Stloc;
              break;

            case ILOpCode.Code.Stloc_S:
              //return Code.Stloc;
              break;

          }
#endregion


          xResult.Add(xILOpCode);
        }
        return xResult;
      }

      private UInt32 ReadUInt32(byte[] aBytes, int aPos) {
        return (UInt32)(aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
      }

      private UInt16 ReadUInt16(byte[] aBytes, int aPos) {
        return (UInt16)(aBytes[aPos + 1] << 8 | aBytes[aPos]);
      }

      //mOperandValueStr = mModule.ResolveString(OperandValueInt32);

        //public MethodBase OperandValueMethod {
        //    get {
        //        if (mOperandValueMethod == null) {
        //          var xValue = OperandValueInt32;
        //          if (((xValue & 0x6000000) == 0x6000000)
        //            || ((xValue & 0x2b000000) == 0x2b000000)
        //            || ((xValue & 0xa000000) == 0xA000000)) {
        //                try {
        //                    Type[] xTypeGenArgs = null;
        //                    Type[] xMethodGenArgs = null;
        //                    if (mMethod.DeclaringType.IsGenericType) {
        //                        xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
        //                    }
        //                    if (mMethod.IsGenericMethod) {
        //                        xMethodGenArgs = mMethod.GetGenericArguments();
        //                    }
        //                  // http://msdn.microsoft.com/en-us/library/ms145421(VS.85).aspx
        //                  mOperandValueMethod = mModule.ResolveMethod(OperandValueInt32, xTypeGenArgs, xMethodGenArgs);
        //                } catch { }
        //            }
        //        }
        //        return mOperandValueMethod;
        //    }
        //}

        //public FieldInfo OperandValueField {
        //    get {
        //        if (mOperandValueField == null) {
        //            try
        //            {
        //                Type[] xTypeGenArgs = null;
        //                Type[] xMethodGenArgs = null;
        //                if (mMethod.DeclaringType.IsGenericType)
        //                {
        //                    xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
        //                }
        //                if (mMethod.IsGenericMethod)
        //                {
        //                    xMethodGenArgs = mMethod.GetGenericArguments();
        //                }
        //                mOperandValueField = mModule.ResolveField(OperandValueInt32,
        //                                                          xTypeGenArgs,
        //                                                          xMethodGenArgs);
        //            }
        //            catch {
        //            }
        //        }
        //        return mOperandValueField;
        //    }
        //}

        //public Type OperandValueType {
        //    get {
        //        if (mOperandValueType == null) {
        //            try{
        //                Type[] xTypeGenArgs = null;
        //                Type[] xMethodGenArgs = null;
        //                if (mMethod.DeclaringType.IsGenericType) {
        //                    xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
        //                }
        //                if (mMethod.IsGenericMethod) {
        //                    xMethodGenArgs = mMethod.GetGenericArguments();
        //                }
        //                mOperandValueType = mModule.ResolveType(OperandValueInt32,
        //                                                        xTypeGenArgs,
        //                                                        xMethodGenArgs);
        //            }catch {
        //            }
        //        }
        //        return mOperandValueType;
        //    }
        //}

        //public int OperandValueInt32 {
        //    get
        //    {
        //        if (mOperandValueInt32 == null)
        //        {
        //            if(Operand == null)
        //            {
        //                return 0;
        //            }
        //            if (!mIsShortcut)
        //            {
        //                byte[] xData = new byte[4];
        //                Array.Copy(Operand, xData, Math.Min(4, Operand.Length));
        //                mOperandValueInt32 = BitConverter.ToInt32(xData, 0);
        //            }
        //            else
        //            {
        //                sbyte xShortValue = (sbyte) Operand[0];
        //                mOperandValueInt32 = xShortValue;
        //            }
        //        }
        //        return mOperandValueInt32.Value;
        //    }
        //}

        //private Single? mOperandValueSingle;
        //public Single OperandValueSingle {
        //    get {
        //        if (mOperandValueSingle == null) {
        //            mOperandValueSingle = BitConverter.ToSingle(Operand, 0);
        //        }
        //        return mOperandValueSingle.Value;
        //    }
        //}

        //private Double? mOperandValueDouble;
        //public Double OperandValueDouble {
        //    get {
        //        if (mOperandValueDouble == null) {
        //            mOperandValueDouble = BitConverter.ToDouble(Operand, 0);
        //        }
        //        return mOperandValueDouble.Value;
        //    }
        //}


      //private Int64 ReadInt64() {
      //  long xResult = (mBody[mPosition + 7] << 56 | mBody[mPosition + 6] << 48 | mBody[mPosition + 5] << 40 | mBody[mPosition + 4] << 32
      //    | mBody[mPosition + 3] << 24 | mBody[mPosition + 2] << 16 | mBody[mPosition + 1] << 8 | mBody[mPosition]);
      //  mPosition = mPosition + 8;
      //  return xResult;
      //}

    }
}