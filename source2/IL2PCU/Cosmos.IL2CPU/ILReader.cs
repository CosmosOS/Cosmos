using System;
using System.IO;
using System.Reflection;

namespace Cosmos.IL2CPU {
    public class ILReader : IDisposable {
        private Stream mStream;
        private MethodBase mMethod;
        private Module mModule;
        private bool mDisposeStream;

        public ILReader(MethodBase aMethod):this(aMethod, aMethod.GetMethodBody()) {
        }

        public ILReader(MethodBase aMethod, MethodBody aBody) {
          mMethod = aMethod;
          mModule = mMethod.Module;
          //TODO: Why do we convert a small array of bytes in memory to a memory stream only to convert back to individual bytes?
          // Instead lets work on the array itself
          mStream = new MemoryStream(aBody.GetILAsByteArray());
          //TODO: Why do we suppress finalize here?
          GC.SuppressFinalize(mStream);
          mDisposeStream = true;
        }

        public ILReader(MethodBase aMethod, Stream aStream)
        {
            mMethod = aMethod;
            mModule = mMethod.Module;
            mStream = aStream;
            mDisposeStream = false;
        }

        public void Dispose() {
            if (mDisposeStream) {
              mStream.Dispose();
              //TODO: See comment above about supress finalize
              GC.ReRegisterForFinalize(mStream);
            }
            mStream = null;
            mMethod = null;
            mModule = null;
        }

        private ILOp.Code mOpCode;
        private byte[] mOperand;

        public uint Position {
            get;
            private set;
        }

        public uint NextPosition {
            get {
                return (uint)mStream.Position;
            }
        }

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

        private uint? mOperandValueBranchPosition;
        private bool mIsShortcut;
        public uint OperandValueBranchPosition {
            get {
                if (mOperandValueBranchPosition == null) {
                    //sbyte xTemp = (sbyte)mOperand;
                    //if (xTemp == mOperand) {
                    //    mOperandValueBranchPosition = NextPosition + xTemp;
                    //} else {
                    //						if (mStream.Length < (NextPosition + mOperand + 1)) {
                    //							mOperandValueBranchPosition = (uint)mOperand;
                    //						} else {
                    //							mOperandValueBranchPosition = (uint)(NextPosition + mOperand);
                    //						}
                    if (mIsShortcut) {
                      mOperandValueBranchPosition = (uint?)(NextPosition + (sbyte)OperandValueInt32);
                    } else {
                      mOperandValueBranchPosition = (uint?)(NextPosition + OperandValueInt32);
                    }
                    //}
                }
                return mOperandValueBranchPosition.Value;
            }
        }

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

        public bool Read() {
          Position = NextPosition;
          // End of stream
          if (mStream.Position == mStream.Length) {
            return false;
          }
          
          // Get OpCode
          byte xOpCodeByte1 = (byte)mStream.ReadByte();
          ILOp.Code xOpCode;
          if (xOpCodeByte1 == 0xFE) {
            xOpCode = (ILOp.Code)(xOpCodeByte1 << 8 | (byte)mStream.ReadByte());
          } else {
            xOpCode = (ILOp.Code)xOpCodeByte1;
          }

          byte xOperandSize = ILOp.GetOperandSize(xOpCode);
            mOperand = null;
            mOperandValueStr = null;
            mOperandValueMethod = null;
            mOperandValueField = null;
            mOperandValueSingle = null;
            mOperandValueType = null;
            mOperandValueInt32 = null;
            mOperandValueBranchPosition = null;
            OperandValueBranchLocations = null;
            mOperandValueDouble = null;
            mOpCode = ILOp.ExpandShortcut(xOpCode);
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
                        if ((NextPosition + xBranchLocations1[i]) < 0) {
                            xResult[i] = (uint)xBranchLocations1[i];
                        } else {
                            xResult[i] = (uint)(NextPosition + xBranchLocations1[i]);
                        }
                    }
                    OperandValueBranchLocations = xResult;
                }
            }
            return true;
        }

        private Int64 ReadInt64() {
            long xResult = 0;
            byte xOperandSize = 8;
            byte[] xBytes = new byte[xOperandSize];
            while (xOperandSize > 0) {
                int xByteValueInt = mStream.ReadByte();
                if (xByteValueInt == -1) {
                    break;
                }
                xBytes[xOperandSize - 1] = (byte)xByteValueInt;
                xOperandSize--;
            }
            for (int i = 0; i < xBytes.Length; i++) {
                xResult = xResult << 8 | xBytes[i];
            }
            return xResult;
        }

      private byte[] mOperandBuff = new byte[8];
      //TODO: If we need further peformance, this function is one of the bigger users of time
      // We can load more data at a time, and use an index into larger buffers
      private byte[] ReadOperand(byte aOperandSize) {
        mStream.Read(mOperandBuff, 0, aOperandSize);
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
            return GetInt32FromOperandByteArray(ReadOperand(4));
        }

    }
}