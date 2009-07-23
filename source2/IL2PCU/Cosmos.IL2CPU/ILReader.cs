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
      protected Func<ILOpCode>[] mOpCodesLo = new Func<ILOpCode>[256];
      protected Func<ILOpCode>[] mOpCodesHi = new Func<ILOpCode>[256];
      
      public ILReader(Type aAssemblerBaseOp) {
        LoadOpCodes(aAssemblerBaseOp);
      }

      protected void LoadOpCodes(Type aAssemblerBaseOp) {
        foreach (var xType in typeof(ILOpCode).Assembly.GetExportedTypes()) {
          if (xType.IsSubclassOf(typeof(ILOpCode))) {
            var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), false).FirstOrDefault() as OpCodeAttribute;
            var xTemp = new DynamicMethod("Create_" + xAttrib.OpCode + "_Obj", typeof(ILOpCode), new Type[0], true);
            var xGen = xTemp.GetILGenerator();
            var xCtor = xType.GetConstructor(new Type[0]);
            xGen.Emit(OpCodes.Newobj, xCtor);
            xGen.Emit(OpCodes.Ret);

            var xDeleg = (Func<ILOpCode>)xTemp.CreateDelegate(typeof(Func<ILOpCode>));
            var xOpCodeValue = (ushort)xAttrib.OpCode;
            if (xOpCodeValue <= 0xFF) {
              mOpCodesLo[xOpCodeValue] = xDeleg;
            } else {
              mOpCodesHi[xOpCodeValue & 0xFF] = xDeleg;
            }
          }
        }
      }
      
      public List<ILOpCode> ProcessMethod(MethodBase aMethod) {
        //TODO: remove
        mMethod = aMethod;
        mModule = aMethod.Module;
        
        var xResult = new List<ILOpCode>();
        var xBody = aMethod.GetMethodBody().GetILAsByteArray();
        int xPos = 0;

        //TODO: Move op info compeltely out of ILOp
        while (xPos < xBody.Length) {
          ILOpCode.Code xOpCodeVal;
          Func<ILOpCode> xILOpCodeCreate;
          int xOpCodeSize = 1; //TODO: Remove this after we have better logic
          if (xBody[xPos] == 0xFE) {
            xOpCodeVal = (ILOpCode.Code)(0xFE00 | xBody[xPos + 1]);
            xILOpCodeCreate = mOpCodesHi[xBody[xPos + 1]];
            xOpCodeSize = 2;
          } else {
            xOpCodeVal = (ILOpCode.Code)xBody[xPos];
            xILOpCodeCreate = mOpCodesLo[xBody[xPos]];
          }

          // Get arguments before Shortcut expansion.
          //TODO: Are all shortcuts wo arguments? if so we can skip this step for shortcuts
          int xOperandSize = ILOpCode.GetOperandSize(xOpCodeVal);
          xPos = xPos + xOpCodeSize + xOperandSize;

          // TODO: Optimize this. Can possibly fill slots in the mOpCodesHi
          // with the target op, or a translator fuction in the delegate
          var xOpCodeValFinal = ILOpCode.ExpandShortcut(xOpCodeVal);
          if (xOpCodeValFinal != xOpCodeVal) {
            if ((int)xOpCodeValFinal >= (int)0xFE00) {
              xILOpCodeCreate = mOpCodesHi[((int)xOpCodeValFinal) & 0xFF];
            } else {
              xILOpCodeCreate = mOpCodesLo[(int)xOpCodeValFinal];
            }
          }

          var xOpCode = xILOpCodeCreate();
          xResult.Add(xOpCode);

        }

          
        //  mOperand = null;
        //    mOperandValueStr = null;
        //    mOperandValueMethod = null;
        //    mOperandValueField = null;
        //    mOperandValueSingle = null;
        //    mOperandValueType = null;
        //    mOperandValueInt32 = null;
        //    //mOperandValueBranchPosition = null;
        //    OperandValueBranchLocations = null;
        //    mOperandValueDouble = null;
        //    mIsShortcut = mOpCode != xOpCode;
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

      private byte[] mBody;
      private MethodBase mMethod;
      private Module mModule;

      private ILOpCode.Code mOpCode;
        private byte[] mOperand;

        protected int mPosition = 0;

        public ILOpCode.Code OpCode {
            get {
                return mOpCode;
            }
        }

        public byte[] Operand {
            get {
                return mOperand;
            }
        }

        private string mOperandValueStr;
        public string OperandValueStr {
            get {
                if (mOperandValueStr == null) {
                    mOperandValueStr = mModule.ResolveString(OperandValueInt32);
                }
                return mOperandValueStr;
            }
        }

        private MethodBase mOperandValueMethod;
        public MethodBase OperandValueMethod {
            get {
                if (mOperandValueMethod == null) {
                  var xValue = OperandValueInt32;
                  if (((xValue & 0x6000000) == 0x6000000)
                    || ((xValue & 0x2b000000) == 0x2b000000)
                    || ((xValue & 0xa000000) == 0xA000000)) {
                        try {
                            Type[] xTypeGenArgs = null;
                            Type[] xMethodGenArgs = null;
                            if (mMethod.DeclaringType.IsGenericType) {
                                xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
                            }
                            if (mMethod.IsGenericMethod) {
                                xMethodGenArgs = mMethod.GetGenericArguments();
                            }
                          // http://msdn.microsoft.com/en-us/library/ms145421(VS.85).aspx
                          mOperandValueMethod = mModule.ResolveMethod(OperandValueInt32, xTypeGenArgs, xMethodGenArgs);
                        } catch { }
                    }
                }
                return mOperandValueMethod;
            }
        }

        private bool mIsShortcut;

        private FieldInfo mOperandValueField;
        public FieldInfo OperandValueField {
            get {
                if (mOperandValueField == null) {
                    try
                    {
                        Type[] xTypeGenArgs = null;
                        Type[] xMethodGenArgs = null;
                        if (mMethod.DeclaringType.IsGenericType)
                        {
                            xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
                        }
                        if (mMethod.IsGenericMethod)
                        {
                            xMethodGenArgs = mMethod.GetGenericArguments();
                        }
                        mOperandValueField = mModule.ResolveField(OperandValueInt32,
                                                                  xTypeGenArgs,
                                                                  xMethodGenArgs);
                    }
                    catch {
                    }
                }
                return mOperandValueField;
            }
        }

        private Type mOperandValueType;
        public Type OperandValueType {
            get {
                if (mOperandValueType == null) {
                    try{
                        Type[] xTypeGenArgs = null;
                        Type[] xMethodGenArgs = null;
                        if (mMethod.DeclaringType.IsGenericType) {
                            xTypeGenArgs = mMethod.DeclaringType.GetGenericArguments();
                        }
                        if (mMethod.IsGenericMethod) {
                            xMethodGenArgs = mMethod.GetGenericArguments();
                        }
                        mOperandValueType = mModule.ResolveType(OperandValueInt32,
                                                                xTypeGenArgs,
                                                                xMethodGenArgs);
                    }catch {
                    }
                }
                return mOperandValueType;
            }
        }

        public uint[] OperandValueBranchLocations {
            get;
            private set;
        }

        private int? mOperandValueInt32;
        public int OperandValueInt32 {
            get
            {
                if (mOperandValueInt32 == null)
                {
                    if(Operand == null)
                    {
                        return 0;
                    }
                    if (!mIsShortcut)
                    {
                        byte[] xData = new byte[4];
                        Array.Copy(Operand, xData, Math.Min(4, Operand.Length));
                        mOperandValueInt32 = BitConverter.ToInt32(xData, 0);
                    }
                    else
                    {
                        sbyte xShortValue = (sbyte) Operand[0];
                        mOperandValueInt32 = xShortValue;
                    }
                }
                return mOperandValueInt32.Value;
            }
        }

        private Single? mOperandValueSingle;
        public Single OperandValueSingle {
            get {
                if (mOperandValueSingle == null) {
                    mOperandValueSingle = BitConverter.ToSingle(Operand, 0);
                }
                return mOperandValueSingle.Value;
            }
        }

        private Double? mOperandValueDouble;
        public Double OperandValueDouble {
            get {
                if (mOperandValueDouble == null) {
                    mOperandValueDouble = BitConverter.ToDouble(Operand, 0);
                }
                return mOperandValueDouble.Value;
            }
        }

        protected byte ReadByte() {
          var xResult = mBody[mPosition];
          mPosition++;
          return xResult;
        }

      private Int64 ReadInt64() {
        long xResult = (mBody[mPosition + 7] << 56 | mBody[mPosition + 6] << 48 | mBody[mPosition + 5] << 40 | mBody[mPosition + 4] << 32
          | mBody[mPosition + 3] << 24 | mBody[mPosition + 2] << 16 | mBody[mPosition + 1] << 8 | mBody[mPosition]);
        mPosition = mPosition + 8;
        return xResult;
      }

      private byte[] mOperandBuff = new byte[8];
      //TODO: If we need further peformance, this function is one of the bigger users of time
      // We can load more data at a time, and use an index into larger buffers
      private byte[] ReadOperand(byte aOperandSize) {
        for (int i = 0; i < aOperandSize; i++) {
          //TODO: can do better than readbyte, can do locally
          mOperandBuff[i] = ReadByte();
        }
        return mOperandBuff;
      }

        private static Int32 GetInt32FromOperandByteArray(byte[] aData) {
            Int32 xResult = 0;
            for (int i = 3; i >= 0; i--) {
                xResult = xResult << 8 | aData[i];
            }
            return xResult;
        }

        private Int32 ReadInt32() {
            Int32 xResult = (mBody[mPosition + 3] << 24 | mBody[mPosition + 2] << 16 | mBody[mPosition + 1] << 8 | mBody[mPosition]);
            mPosition = mPosition + 4;
            return xResult;
        }

    }
}