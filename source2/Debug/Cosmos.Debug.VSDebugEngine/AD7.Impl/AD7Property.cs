using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.VSDebugEngine {
  // An implementation of IDebugProperty2
  // This interface represents a stack frame property, a program document property, or some other property. 
  // The property is usually the result of an expression evaluation. 
  //
  // The sample engine only supports locals and parameters for functions that have symbols loaded.
  class AD7Property : IDebugProperty2 {
    private DebugLocalInfo m_variableInformation;
    private AD7Process mProcess;
    private AD7StackFrame mStackFrame;
    private DebugInfo.Local_Argument_Info mDebugInfo;
    const uint xArrayLengthOffset = 8;
    const uint xArrayFirstElementOffset = 16;
    private const string NULL = "null";


    public AD7Property(DebugLocalInfo localInfo, AD7Process process, AD7StackFrame stackFrame) {
      m_variableInformation = localInfo;
      mProcess = process;
      mStackFrame = stackFrame;
      if (localInfo.IsLocal) {
        mDebugInfo = mStackFrame.mLocalInfos[m_variableInformation.Index];
      } else if (localInfo.IsArrayElement) {
        mDebugInfo = new DebugInfo.Local_Argument_Info() {
          Type = localInfo.ArrayElementType,
          Name = localInfo.Name,
          IsArrayElement = true,
          Offset = localInfo.ArrayElementLocation
        };
      } else {
        mDebugInfo = mStackFrame.mArgumentInfos[m_variableInformation.Index];
      }

    }

    public void ReadData<T>(ref DEBUG_PROPERTY_INFO propertyInfo, Func<byte[], int, T> ByteToTypeAction) {
      byte[] xData;
      if (m_variableInformation.IsArrayElement) {
        xData = mProcess.mDbgConnector.GetMemoryData((uint)mDebugInfo.Offset, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
        var xTypedIntValue = ByteToTypeAction(xData, 0);
        propertyInfo.bstrValue = String.Format("{0}", xTypedIntValue);
      } else {
        xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
        var xTypedIntValue = ByteToTypeAction(xData, 0);
        propertyInfo.bstrValue = String.Format("{0}", xTypedIntValue);
      }
    }

    public void ReadDataArray<T>(ref DEBUG_PROPERTY_INFO propertyInfo, string typeAsString) {
      byte[] xData;

      xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, 4);
      uint xArrayPointer = BitConverter.ToUInt32(xData, 0);
      if (xArrayPointer == 0) {
        propertyInfo.bstrValue = NULL;
      } else {
        xData = mProcess.mDbgConnector.GetMemoryData(xArrayPointer + xArrayLengthOffset, 4, 4);
        uint xDataLength = BitConverter.ToUInt32(xData, 0);
        bool xIsTooLong = xDataLength > 512;
        if (xIsTooLong) {
          xDataLength = 512;
        }
        if (xDataLength > 0) {
          if (this.m_variableInformation.Children.Count == 0) {
            for (int i = 0; i < xDataLength; i++) {
              DebugLocalInfo inf = new DebugLocalInfo();
              inf.IsArrayElement = true;
              inf.ArrayElementType = typeof(T).AssemblyQualifiedName;
              inf.ArrayElementLocation = (int)(xArrayPointer + xArrayFirstElementOffset + (System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)) * i));
              inf.Name = "[" + i.ToString() + "]";
              this.m_variableInformation.Children.Add(new AD7Property(inf, this.mProcess, this.mStackFrame));
            }
          }
        }
        propertyInfo.bstrValue = String.Format(typeAsString + "[{0}] at 0x{1} ", xDataLength, xArrayPointer.ToString("X"));
      }

    }

    // Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
    public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields) {
      DEBUG_PROPERTY_INFO propertyInfo = new DEBUG_PROPERTY_INFO();

      if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME)) {
        propertyInfo.bstrFullName = m_variableInformation.Name;
        propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
      }

      if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME)) {
        propertyInfo.bstrName = m_variableInformation.Name;
        propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
      }

      if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE)) {
        propertyInfo.bstrType = mDebugInfo.Type;
        propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
      }

      if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE)) {
        byte[] xData;

        #region String
        if (mDebugInfo.Type == typeof(string).AssemblyQualifiedName) {
          const uint xStringLengthOffset = 12;
          const uint xStringFirstCharOffset = 16;
          xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, 4);
          uint xStrPointer = BitConverter.ToUInt32(xData, 0);
          if (xStrPointer == 0) {
            propertyInfo.bstrValue = NULL;
          } else {
            xData = mProcess.mDbgConnector.GetMemoryData(xStrPointer + xStringLengthOffset, 4, 4);
            uint xStringLength = BitConverter.ToUInt32(xData, 0);
            propertyInfo.bstrValue = "String of length: " + xStringLength;
            if (xStringLength > 100) {
              propertyInfo.bstrValue = "For now, strings larger than 100 chars are not supported..";
            } else if (xStringLength == 0) {
              propertyInfo.bstrValue = "\"\"";
            } else {
              xData = mProcess.mDbgConnector.GetMemoryData(xStrPointer + xStringFirstCharOffset, xStringLength * 2, 2);
              propertyInfo.bstrValue = "\"" + Encoding.Unicode.GetString(xData) + "\"";
            }
          }
        }
#warning TODO: String[]
        #endregion

          // Byte
        else if (mDebugInfo.Type == typeof(byte).AssemblyQualifiedName) {
          ReadData<byte>(ref propertyInfo, new Func<byte[], int, byte>(delegate(byte[] barr, int ind) { return barr[ind]; }));
        } else if (mDebugInfo.Type == typeof(byte[]).AssemblyQualifiedName) {
          ReadDataArray<byte>(ref propertyInfo, "byte");
        }

          // SByte
          else if (mDebugInfo.Type == typeof(sbyte).AssemblyQualifiedName) {
          ReadData<sbyte>(ref propertyInfo, new Func<byte[], int, sbyte>(delegate(byte[] barr, int ind) { return unchecked((sbyte)barr[ind]); }));
        } else if (mDebugInfo.Type == typeof(sbyte[]).AssemblyQualifiedName) {
          ReadDataArray<sbyte>(ref propertyInfo, "sbyte");
        }

        #region Char
 else if (mDebugInfo.Type == typeof(char).AssemblyQualifiedName) {
          xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, 2);
          var xTypedCharValue = BitConverter.ToChar(xData, 0);
          propertyInfo.bstrValue = String.Format("{0} '{1}'", (ushort)xTypedCharValue, xTypedCharValue);
        } else if (mDebugInfo.Type == typeof(char[]).AssemblyQualifiedName) {
          xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, 4);
          uint xArrayPointer = BitConverter.ToUInt32(xData, 0);
          if (xArrayPointer == 0) {
            propertyInfo.bstrValue = NULL;
          } else {
            xData = mProcess.mDbgConnector.GetMemoryData(xArrayPointer + xArrayLengthOffset, 4, 4);
            uint xDataLength = BitConverter.ToUInt32(xData, 0);
            bool xIsTooLong = xDataLength > 512;
            var xSB = new StringBuilder();
            xSB.AppendFormat("Char[{0}] at 0x{1} {{ ", xDataLength, xArrayPointer.ToString("X"));
            if (xIsTooLong) {
              xDataLength = 512;
            }
            if (xDataLength > 0) {
              xData = mProcess.mDbgConnector.GetMemoryData(xArrayPointer + xArrayFirstElementOffset, xDataLength * 2);
              bool first = true;
              for (int i = 0; (i / 2) < xDataLength; i += 2) {
                if (!first)
                  xSB.Append(", ");
                char c = BitConverter.ToChar(xData, i);
                xSB.Append('\'');
                if (c == '\0') {
                  xSB.Append("\\0");
                } else {
                  xSB.Append(c);
                }
                xSB.Append('\'');

                first = false;
              }
            }
            if (xIsTooLong) {
              xSB.Append(", ..");
            }

            xSB.Append(" }");
            propertyInfo.bstrValue = xSB.ToString();
          }
        }
        #endregion

          // Short
          else if (mDebugInfo.Type == typeof(short).AssemblyQualifiedName) {
          ReadData<short>(ref propertyInfo, new Func<byte[], int, short>(BitConverter.ToInt16));
        } else if (mDebugInfo.Type == typeof(short[]).AssemblyQualifiedName) {
          ReadDataArray<short>(ref propertyInfo, "short");
        }

          // UShort
          else if (mDebugInfo.Type == typeof(ushort).AssemblyQualifiedName) {
          ReadData<ushort>(ref propertyInfo, new Func<byte[], int, ushort>(BitConverter.ToUInt16));
        } else if (mDebugInfo.Type == typeof(ushort[]).AssemblyQualifiedName) {
          ReadDataArray<ushort>(ref propertyInfo, "ushort");
        }

          // Int32
          else if (mDebugInfo.Type == typeof(int).AssemblyQualifiedName) {
          ReadData<int>(ref propertyInfo, new Func<byte[], int, int>(BitConverter.ToInt32));
        } else if (mDebugInfo.Type == typeof(int[]).AssemblyQualifiedName) {
          ReadDataArray<int>(ref propertyInfo, "int");
        }

          // UInt32
          else if (mDebugInfo.Type == typeof(uint).AssemblyQualifiedName) {
          ReadData<uint>(ref propertyInfo, new Func<byte[], int, uint>(BitConverter.ToUInt32));
        } else if (mDebugInfo.Type == typeof(uint[]).AssemblyQualifiedName) {
          ReadDataArray<uint>(ref propertyInfo, "uint");
        }

          // Long
          else if (mDebugInfo.Type == typeof(long).AssemblyQualifiedName) {
          ReadData<long>(ref propertyInfo, new Func<byte[], int, long>(BitConverter.ToInt64));
        } else if (mDebugInfo.Type == typeof(long[]).AssemblyQualifiedName) {
          ReadDataArray<long>(ref propertyInfo, "long");
        }

          // ULong
          else if (mDebugInfo.Type == typeof(ulong).AssemblyQualifiedName) {
          ReadData<ulong>(ref propertyInfo, new Func<byte[], int, ulong>(BitConverter.ToUInt64));
        } else if (mDebugInfo.Type == typeof(ulong[]).AssemblyQualifiedName) {
          ReadDataArray<ulong>(ref propertyInfo, "ulong");
        }

          // Float
          else if (mDebugInfo.Type == typeof(float).AssemblyQualifiedName) {
          ReadData<float>(ref propertyInfo, new Func<byte[], int, float>(BitConverter.ToSingle));
        } else if (mDebugInfo.Type == typeof(float[]).AssemblyQualifiedName) {
          ReadDataArray<float>(ref propertyInfo, "float");
        }

          // Double
          else if (mDebugInfo.Type == typeof(double).AssemblyQualifiedName) {
          ReadData<double>(ref propertyInfo, new Func<byte[], int, double>(BitConverter.ToDouble));
        } else if (mDebugInfo.Type == typeof(double[]).AssemblyQualifiedName) {
          ReadDataArray<double>(ref propertyInfo, "double");
        }

          // Bool
          else if (mDebugInfo.Type == typeof(bool).AssemblyQualifiedName) {
          ReadData<bool>(ref propertyInfo, new Func<byte[], int, bool>(BitConverter.ToBoolean));
        } else if (mDebugInfo.Type == typeof(bool[]).AssemblyQualifiedName) {
          ReadDataArray<bool>(ref propertyInfo, "bool");
        } else {
          xData = mProcess.mDbgConnector.GetStackData(mDebugInfo.Offset, 4);
          var xPointer = BitConverter.ToUInt32(xData, 0);
          if (xPointer == 0) {
            propertyInfo.bstrValue = NULL;
          } else {
            var mp = mProcess.mDebugInfoDb.GetFieldMap(mDebugInfo.Type);
            foreach (string str in mp.FieldNames) {
              var fInf = mProcess.mDebugInfoDb.GetFieldInfo(str);

              var inf = new DebugLocalInfo();
              inf.IsArrayElement = true;
              inf.ArrayElementType = fInf.Type;
              inf.ArrayElementLocation = (int)(xPointer + fInf.Offset + 12);
              inf.Name = GetFieldName(fInf);
              this.m_variableInformation.Children.Add(new AD7Property(inf, this.mProcess, this.mStackFrame));
            }
            propertyInfo.bstrValue = String.Format("{0} (0x{1})", xPointer, xPointer.ToString("X").ToUpper());
          }
        }
        propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
      }

      if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB)) {
        // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
        propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

        if (this.m_variableInformation.Children.Count > 0) {
          propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
        }
      }

      propertyInfo.pProperty = (IDebugProperty2)this;
      propertyInfo.dwFields |= (enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP);
      // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
      // then set the pProperty field so the debugger can call back when the children are enumerated.
      //if (((dwFields & (uint)enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0) 
      //|| (this.m_variableInformation.child != null))
      //{
      //    propertyInfo.pProperty = (IDebugProperty2)this;
      //    propertyInfo.dwFields |= (enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP);
      //}

      return propertyInfo;
    }

    private static string GetFieldName(DebugInfo.Field_Info fInf) {
      string s = fInf.Name;
      int i = s.LastIndexOf('.');
      if (i > 0) {
        s = s.Substring(i + 1, s.Length - i - 1);
        return s;
      } else {
        return s;
      }
    }

    #region IDebugProperty2 Members

    // Enumerates the children of a property. This provides support for dereferencing pointers, displaying members of an array, or fields of a class or struct.
    // The sample debugger only supports pointer dereferencing as children. This means there is only ever one child.
    public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref System.Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
      ppEnum = null;

      if (this.m_variableInformation.Children.Count > 0) {
        List<DEBUG_PROPERTY_INFO> infs = new List<DEBUG_PROPERTY_INFO>();
        foreach (AD7Property dp in m_variableInformation.Children) {
          infs.Add(dp.ConstructDebugPropertyInfo(dwFields));
        }
        ppEnum = new AD7PropertyEnum(infs.ToArray());
        return VSConstants.S_OK;
      }
      //if (this.m_variableInformation.child != null)
      //{
      //    DEBUG_PROPERTY_INFO[] properties = new DEBUG_PROPERTY_INFO[1];
      //    properties[0] = (new AD7Property(this.m_variableInformation.child)).ConstructDebugPropertyInfo(dwFields);
      //    ppEnum = new AD7PropertyEnum(properties);
      //    return VSConstants.S_OK;
      //}

      return VSConstants.S_FALSE;
    }

    // Returns the property that describes the most-derived property of a property
    // This is called to support object oriented languages. It allows the debug engine to return an IDebugProperty2 for the most-derived 
    // object in a hierarchy. This engine does not support this.
    public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost) {
      throw new Exception("The method or operation is not implemented.");
    }

    // This method exists for the purpose of retrieving information that does not lend itself to being retrieved by calling the IDebugProperty2::GetPropertyInfo 
    // method. This includes information about custom viewers, managed type slots and other information.
    // The sample engine does not support this.
    public int GetExtendedInfo(ref System.Guid guidExtendedInfo, out object pExtendedInfo) {
      throw new Exception("The method or operation is not implemented.");
    }

    // Returns the memory bytes for a property value.
    public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
      throw new Exception("The method or operation is not implemented.");
    }

    // Returns the memory context for a property value.
    public int GetMemoryContext(out IDebugMemoryContext2 ppMemory) {
      throw new Exception("The method or operation is not implemented.");
    }

    // Returns the parent of a property.
    // The sample engine does not support obtaining the parent of properties.
    public int GetParent(out IDebugProperty2 ppParent) {
      throw new Exception("The method or operation is not implemented.");
    }

    // Fills in a DEBUG_PROPERTY_INFO structure that describes a property.
    public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
      pPropertyInfo[0] = new DEBUG_PROPERTY_INFO();
      rgpArgs = null;
      pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
      return VSConstants.S_OK;
    }

    //  Return an IDebugReference2 for this property. An IDebugReference2 can be thought of as a type and an address.
    public int GetReference(out IDebugReference2 ppReference) {
      throw new Exception("The method or operation is not implemented.");
    }

    // Returns the size, in bytes, of the property value.
    public int GetSize(out uint pdwSize) {
      throw new Exception("The method or operation is not implemented.");
    }

    // The debugger will call this when the user tries to edit the property's values
    // the sample has set the read-only flag on its properties, so this should not be called.
    public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) {
      throw new Exception("The method or operation is not implemented.");
    }

    // The debugger will call this when the user tries to edit the property's values in one of the debugger windows.
    // the sample has set the read-only flag on its properties, so this should not be called.
    public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion

  }
}
