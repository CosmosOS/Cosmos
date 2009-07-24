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
      protected Type[] mILOpCodesLo = new Type[256];
      protected Type[] mILOpCodesHi = new Type[256];
      
      public ILReader(Type aAssemblerBaseOp) {
        // Profiler passes null
        if (aAssemblerBaseOp != null) {
          LoadILOpCodes(aAssemblerBaseOp);
        }
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

      protected void LoadILOpCodes(Type aAssemblerBaseOp) {
        foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
          if (xType.IsSubclassOf(aAssemblerBaseOp)) {
            var xAttrib = (OpCodeAttribute)xType.GetCustomAttributes(typeof(OpCodeAttribute), false)[0];
            var xOpCodeValue = (ushort)xAttrib.OpCode;
            if (xOpCodeValue <= 0xFF) {
              mILOpCodesLo[xOpCodeValue] = xType;
            } else {
              mILOpCodesHi[xOpCodeValue & 0xFF] = xType;
            }
          }
        }
      }
      
      public List<ILOpCode> ProcessMethod(MethodBase aMethod) {
        var xResult = new List<ILOpCode>();
        var xBody = aMethod.GetMethodBody();
        // Some methods return no body. Not sure why.. have to investigate
        if (xBody == null) {
          return null;
        }

        var xIL = xBody.GetILAsByteArray();
        int xPos = 0;

        //TODO: Move op info compeltely out of ILOp
        while (xPos < xIL.Length) {
          ILOpCode.Code xOpCodeVal;
          Type xILOpCodeType;
          OpCode xOpCode;
          int xOpCodeSize = 1; //TODO: Remove this after we have better logic
          if (xIL[xPos] == 0xFE) {
            xOpCodeVal = (ILOpCode.Code)(0xFE00 | xIL[xPos + 1]);
            xILOpCodeType = mILOpCodesHi[xIL[xPos + 1]];
            xOpCode = mOpCodesHi[xIL[xPos + 1]];
            xOpCodeSize = 2;
          } else {
            xOpCodeVal = (ILOpCode.Code)xIL[xPos];
            xILOpCodeType = mILOpCodesLo[xIL[xPos]];
            xOpCode = mOpCodesLo[xIL[xPos]];
          }

          //TODO: Need to look at OpCode operandtype and queue for these:
          // Call: QueueMethod(aReader.OperandValueMethod);
          // Callvirt: QueueMethod(aReader.OperandValueMethod);
          // Newobj: QueueMethod(aReader.OperandValueMethod);

          // TODO: Move all this parsing and moving forward logic into the ILOpCode instances. 
          // Default behaviour for most, but ones like switch should override.
          if (xOpCodeVal == ILOpCode.Code.Switch) {
            int xCount = ReadInt32(xIL, 1);
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
              xPos = xOpCodeSize + 4 + xCount * 4;
            }
          } else {
            // Get arguments before Shortcut expansion.
            //TODO: Are all shortcuts wo arguments? if so we can skip this step for shortcuts
            int xOperandSize = ILOpCode.GetOperandSize(xOpCodeVal);
            xPos = xPos + xOpCodeSize + xOperandSize;
          }

          // TODO: Optimize this. Can possibly fill slots in the mOpCodesHi
          // with the target op, or a translator fuction in the delegate
          var xOpCodeValFinal = ILOpCode.ExpandShortcut(xOpCodeVal);
          if (xOpCodeValFinal != xOpCodeVal) {
            if ((int)xOpCodeValFinal >= (int)0xFE00) {
              xILOpCodeType = mILOpCodesHi[((int)xOpCodeValFinal) & 0xFF];
              xOpCode = mOpCodesHi[(int)xOpCodeValFinal & 0xFF];
            } else {
              xILOpCodeType = mILOpCodesLo[(int)xOpCodeValFinal];
              xOpCode = mOpCodesLo[(int)xOpCodeValFinal];
            }
          }

          // Profiler has no ILOps
          //TODO: Find a way to have the profiler use this too without the overhead of a dummy class tree
          // Special LoadOps? Also remove if null in ctor for profiler in this class
          if (xILOpCodeType != null) {
            var xCtor = xILOpCodeType.GetConstructor(new Type[] { typeof(OpCode), typeof(Type) });
            // TODO: Change this back to the emit way of creating delegates which is faster
            // Has to be done at two levels now though, so its a lot tricker
            // TODO: Can probably eliminate ILOpCode now completely and use OpCode only + our enum
            // then can also change back to the emit code easily
            var xILOpCode = (ILOpCode)xCtor.Invoke(new object[] { OpCodes.Nop, xILOpCodeType });
            xResult.Add(xILOpCode);
          }
        }
         
        //  mOperand = null;
        //    mOperandValueStr = null;
        //    mOperandValueMethod = null;
        //    mOperandValueField = null;
        //    mOperandValueSingle = null;
        //    mOperandValueType = null;
        //    mOperandValueInt32 = null;
        //    OperandValueBranchLocations = null;
        //    mOperandValueDouble = null;
        //    if (xOperandSize > 0) {
        //      //TODO: Will we always use the Int32 result? Copying to array and then again seems wasteful
        //      // Probably better to make typed reads for each type
        //        mOperand = ReadOperand(xOperandSize);
        //        mOperandValueInt32 = GetInt32FromOperandByteArray(mOperand);
        //    } else {
        //        if (mOpCode != xOpCode) {
        //          long? xTempOperand = ILOp.GetShortcutOperand(xOpCode);
        //            if (xTempOperand != null) {
        //                mOperand = BitConverter.GetBytes(xTempOperand.Value);
        //            }
        //        }
        //        if (mOpCode == ILOpCode.Code.Switch) {
        //            int[] xBranchLocations1 = new int[ReadInt32()];
        //            for (int i = 0; i < xBranchLocations1.Length; i++) {
        //                xBranchLocations1[i] = ReadInt32();
        //            }
        //            uint[] xResult = new uint[xBranchLocations1.Length];
        //            for (int i = 0; i < xBranchLocations1.Length; i++) {
        //                if ((mPosition + xBranchLocations1[i]) < 0) {
        //                    xResult[i] = (uint)xBranchLocations1[i];
        //                } else {
        //                    xResult[i] = (uint)(mPosition + xBranchLocations1[i]);
        //                }
        //            }
        //            OperandValueBranchLocations = xResult;
        //        }
        //    }
        //    return true;
        //}

        return xResult;
      }

        private Int32 ReadInt32(byte[] aBytes, int aPos) {
          return (aBytes[aPos + 3] << 24 | aBytes[aPos + 2] << 16 | aBytes[aPos + 1] << 8 | aBytes[aPos]);
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

        //private static Int32 GetInt32FromOperandByteArray(byte[] aData) {
        //    Int32 xResult = 0;
        //    for (int i = 3; i >= 0; i--) {
        //        xResult = xResult << 8 | aData[i];
        //    }
        //    return xResult;
        //}


    }
}