using System;
using System.IO;
using System.Reflection;

namespace Cosmos.IL2CPU {
    public class ILReader {
        private byte[] mBody;
        private MethodBase mMethod;
        private Module mModule;

        public ILReader(MethodBase aMethod):this(aMethod, aMethod.GetMethodBody()) {
        }

        public ILReader(MethodBase aMethod, MethodBody aBody) {
          mMethod = aMethod;
          mModule = mMethod.Module;
          mBody = aBody.GetILAsByteArray();
        }

        private ILOp.Code mOpCode;
        private byte[] mOperand;

        protected int mPosition = 0;

        public ILOp.Code OpCode {
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

        public bool Read() {
          // End of stream
          if (mPosition == mBody.Length) {
            return false;
          }
          
          // Get OpCode
          ILOp.Code xOpCode;
          if (mBody[mPosition] == 0xFE) {
            xOpCode = (ILOp.Code)(mBody[mPosition] << 8 | mBody[mPosition + 1]);
            // TODO: Eliminate this and use indexing below for data, and increment all at once
            mPosition = mPosition + 2;
          } else {
            xOpCode = (ILOp.Code)mBody[mPosition];
            mPosition++;
          }

          byte xOperandSize = ILOp.GetOperandSize(xOpCode);
          mOpCode = ILOp.ExpandShortcut(xOpCode);
          
          mOperand = null;
            mOperandValueStr = null;
            mOperandValueMethod = null;
            mOperandValueField = null;
            mOperandValueSingle = null;
            mOperandValueType = null;
            mOperandValueInt32 = null;
            //mOperandValueBranchPosition = null;
            OperandValueBranchLocations = null;
            mOperandValueDouble = null;
            mIsShortcut = mOpCode != xOpCode;
            if (xOperandSize > 0) {
              //TODO: Will we always use the Int32 result? Copying to array and then again seems wasteful
              // Probably better to make typed reads for each type
                mOperand = ReadOperand(xOperandSize);
                mOperandValueInt32 = GetInt32FromOperandByteArray(mOperand);
            } else {
                if (mOpCode != xOpCode) {
                  long? xTempOperand = ILOp.GetShortcutOperand(xOpCode);
                    if (xTempOperand != null) {
                        mOperand = BitConverter.GetBytes(xTempOperand.Value);
                    }
                }
                if (mOpCode == ILOp.Code.Switch) {
                    int[] xBranchLocations1 = new int[ReadInt32()];
                    for (int i = 0; i < xBranchLocations1.Length; i++) {
                        xBranchLocations1[i] = ReadInt32();
                    }
                    uint[] xResult = new uint[xBranchLocations1.Length];
                    for (int i = 0; i < xBranchLocations1.Length; i++) {
                        if ((mPosition + xBranchLocations1[i]) < 0) {
                            xResult[i] = (uint)xBranchLocations1[i];
                        } else {
                            xResult[i] = (uint)(mPosition + xBranchLocations1[i]);
                        }
                    }
                    OperandValueBranchLocations = xResult;
                }
            }
            return true;
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