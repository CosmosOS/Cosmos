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

      //protected void LoadILOpCodes(Type aAssemblerBaseOp) {
      //  foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
      //    if (xType.IsSubclassOf(aAssemblerBaseOp)) {
      //      var xAttrib = (OpCodeAttribute)xType.GetCustomAttributes(typeof(OpCodeAttribute), false)[0];
      //      var xOpCodeValue = (ushort)xAttrib.OpCode;
      //      if (xOpCodeValue <= 0xFF) {
      //        mILOpCodesLo[xOpCodeValue] = xType;
      //      } else {
      //        mILOpCodesHi[xOpCodeValue & 0xFF] = xType;
      //      }
      //    }
      //  }
      //}
      
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
          int xOpCodeSize = 1; //TODO: Remove this after we have better logic
          if (xIL[xPos] == 0xFE) {
            xOpCodeVal = (ILOpCode.Code)(0xFE00 | xIL[xPos + 1]);
            xOpCode = mOpCodesHi[xIL[xPos + 1]];
            xOpCodeSize = 2;
          } else {
            xOpCodeVal = (ILOpCode.Code)xIL[xPos];
            xOpCode = mOpCodesLo[xIL[xPos]];
          }

          //TODO: Need to look at OpCode operandtype and queue for these:
          // Call: QueueMethod(aReader.OperandValueMethod);
          // Callvirt: QueueMethod(aReader.OperandValueMethod);
          // Newobj: QueueMethod(aReader.OperandValueMethod);

          // Get arguments before Shortcut expansion.
          //TODO: Are all shortcuts wo arguments? if so we can skip this step for shortcuts

          int xOperandSize;
          ILOpCode xILOpCode = null;
          switch (xOpCode.OperandType) {
            // The operand is a 32-bit integer branch target.
            case OperandType.InlineBrTarget:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit metadata token.
            case OperandType.InlineField:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit integer.
            case OperandType.InlineI:
              xOperandSize = 4;
              xILOpCode = new ILOpCodes.InlineI(xOpCodeVal, ReadInt32(xIL, 1));
              break;
            // The operand is a 64-bit integer.
            case OperandType.InlineI8:
              xOperandSize = 8;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit metadata token.
            case OperandType.InlineMethod:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // No operand.
            case OperandType.InlineNone:
              xOperandSize = 0;
              xILOpCode = new ILOpCodes.InlineNone(xOpCodeVal);
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 64-bit IEEE floating point number.
            case OperandType.InlineR:
              xOperandSize = 8;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit metadata signature token.
            case OperandType.InlineSig:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit metadata string token.
            case OperandType.InlineString:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;

            case OperandType.InlineSwitch: {
                int xCount = (int)ReadInt32(xIL, 1);
                int[] xBranchLocations = new int[xCount];
                uint[] xBranchValues = new uint[xCount];
                for (int i = 0; i < xCount; i++) {
                  xBranchLocations[i] = xIL[i + 5];
                  //xBranchValues[i] = 
                  //                if ((mPosition + xBranchLocations1[i]) < 0) {
                  //                    xResult[i] = (uint)xBranchLocations1[i];
                  //                } else {
                  //                    xResult[i] = (uint)(mPosition + xBranchLocations1[i]);
                  //                }
                }
                xOperandSize = 4 + xCount * 4;
                xILOpCode = new ILOpCode(xOpCodeVal);
                break;
              }

            // The operand is a FieldRef, MethodRef, or TypeRef token.
            case OperandType.InlineTok:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit metadata token.
            case OperandType.InlineType:
              xOperandSize = 4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // OperandType.OperandType.OperandType.The operand is 16-bit integer containing the ordinal of a local variable or an argument.
            case OperandType.InlineVar:
              xOperandSize =  2;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is an 8-bit integer branch target.
            case OperandType.ShortInlineBrTarget:
              xOperandSize = 1;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is an 8-bit integer.
            case OperandType.ShortInlineI:
              xOperandSize = 1;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is a 32-bit IEEE floating point number.
            case OperandType.ShortInlineR:
              xOperandSize =  4;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            // The operand is an 8-bit integer containing the ordinal of a local variable or an argumenta.
            case OperandType.ShortInlineVar:
              xOperandSize =  1;
              xILOpCode = new ILOpCode(xOpCodeVal);
              break;
            default:
              throw new Exception("Unknown OperandType");
          }
          xPos = xPos + xOpCodeSize + xOperandSize;

          // TODO: Optimize this. Can possibly fill slots in the mOpCodesHi
          // with the target op, or a translator fuction in the delegate
          var xOpCodeValFinal = ILOpCode.ExpandShortcut(xOpCodeVal);
          if (xOpCodeValFinal != xOpCodeVal) {
            if ((int)xOpCodeValFinal >= (int)0xFE00) {
              xOpCode = mOpCodesHi[(int)xOpCodeValFinal & 0xFF];
            } else {
              xOpCode = mOpCodesLo[(int)xOpCodeValFinal];
            }
          }

          xResult.Add(xILOpCode);
        }
        return xResult;
      }

      private UInt32 ReadInt32(byte[] aBytes, int aPos) {
        return (UInt32)(aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
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