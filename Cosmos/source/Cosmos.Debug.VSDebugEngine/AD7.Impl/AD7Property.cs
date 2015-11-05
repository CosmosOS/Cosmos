using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Cosmos.Debug.Common;
using Dapper;
using SQLinq.Dapper;
using SQLinq;

namespace Cosmos.Debug.VSDebugEngine
{
    // An implementation of IDebugProperty2
    // This interface represents a stack frame property, a program document property, or some other property.
    // The property is usually the result of an expression evaluation.
    //
    // The sample engine only supports locals and parameters for functions that have symbols loaded.
    class AD7Property : IDebugProperty2
    {
        private DebugLocalInfo m_variableInformation;
        private AD7Process mProcess;
        private AD7StackFrame mStackFrame;
        private LOCAL_ARGUMENT_INFO mDebugInfo;
        const uint mArrayLengthOffset = 8;
        const uint mArrayFirstElementOffset = 16;
        private const string NULL = "null";

        protected Int32 OFFSET
        {
            get
            {
                return mDebugInfo.OFFSET;
            }
        }

        public AD7Property(DebugLocalInfo localInfo, AD7Process process, AD7StackFrame stackFrame)
        {
            m_variableInformation = localInfo;
            mProcess = process;
            mStackFrame = stackFrame;
            if (localInfo.IsLocal)
            {
                mDebugInfo = mStackFrame.mLocalInfos[m_variableInformation.Index];
            }
            else if (localInfo.IsReference)
            {
                mDebugInfo = new LOCAL_ARGUMENT_INFO()
                {
                    TYPENAME = localInfo.Type,
                    NAME = localInfo.Name,
                    OFFSET = localInfo.Offset
                };
            }
            else
            {
                mDebugInfo = mStackFrame.mArgumentInfos[m_variableInformation.Index];
            }

        }

        public void ReadData<T>(ref DEBUG_PROPERTY_INFO propertyInfo, Func<byte[], int, T> ByteToTypeAction)
        {
            byte[] xData;
            if (m_variableInformation.IsReference)
            {
                xData = mProcess.mDbgConnector.GetMemoryData(m_variableInformation.Pointer, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
                if (xData == null)
                {
                    propertyInfo.bstrValue = String.Format("Error! Memory data received was null!");
                    return;
                }

                var xTypedIntValue = ByteToTypeAction(xData, 0);
                propertyInfo.bstrValue = String.Format("{0}", xTypedIntValue);
            }
            else
            {
                xData = mProcess.mDbgConnector.GetStackData(OFFSET, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
                if (xData == null)
                {
                    propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
                    return;
                }

                var xTypedIntValue = ByteToTypeAction(xData, 0);
                propertyInfo.bstrValue = String.Format("{0}", xTypedIntValue);
            }
        }

        public void ReadDataArray<T>(ref DEBUG_PROPERTY_INFO propertyInfo, string typeAsString)
        {
            byte[] xData;

            xData = mProcess.mDbgConnector.GetStackData(OFFSET, 4);
            if (xData == null)
            {
                propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
            }
            else
            {
                uint xPointer = BitConverter.ToUInt32(xData, 0);
                if (xPointer == 0)
                {
                    propertyInfo.bstrValue = NULL;
                }
                else
                {
                    xData = mProcess.mDbgConnector.GetMemoryData(xPointer + mArrayLengthOffset, 4, 4);
                    if (xData == null)
                    {
                        propertyInfo.bstrValue = String.Format("Error! Memory data received was null!");
                    }
                    else
                    {
                        uint xDataLength = BitConverter.ToUInt32(xData, 0);
                        bool xIsTooLong = xDataLength > 512;
                        if (xIsTooLong)
                        {
                            xDataLength = 512;
                        }
                        if (xDataLength > 0)
                        {
                            if (this.m_variableInformation.Children.Count == 0)
                            {
                                for (int i = 0; i < xDataLength; i++)
                                {
                                    DebugLocalInfo inf = new DebugLocalInfo();
                                    inf.IsReference = true;
                                    inf.Type = typeof(T).AssemblyQualifiedName;
                                    inf.Offset = (int)(mArrayFirstElementOffset + (System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)) * i));
                                    inf.Pointer = (uint)(xPointer + mArrayFirstElementOffset + (System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)) * i));
                                    inf.Name = "[" + i.ToString() + "]";
                                    this.m_variableInformation.Children.Add(new AD7Property(inf, this.mProcess, this.mStackFrame));
                                }
                            }
                        }
                        propertyInfo.bstrValue = String.Format(typeAsString + "[{0}] at 0x{1} ", xDataLength, xPointer.ToString("X"));
                    }
                }
            }


        }

        // Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
        public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
        {
            DEBUG_PROPERTY_INFO propertyInfo = new DEBUG_PROPERTY_INFO();

            try
            {
                if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME))
                {
                    propertyInfo.bstrFullName = m_variableInformation.Name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
                }

                if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME))
                {
                    propertyInfo.bstrName = m_variableInformation.Name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
                }

                if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE))
                {
                    propertyInfo.bstrType = mDebugInfo.TYPENAME;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                }

                if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE))
                {
                    byte[] xData;

                    #region string
                    if (mDebugInfo.TYPENAME == typeof(string).AssemblyQualifiedName)
                    {
                        const uint xStringLengthOffset = 12;
                        const uint xStringFirstCharOffset = 16;
                        xData = mProcess.mDbgConnector.GetStackData(OFFSET, 4);
                        if (xData == null)
                        {
                            propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
                        }
                        else
                        {
                            uint xStrPointer = BitConverter.ToUInt32(xData, 0);
                            if (xStrPointer == 0)
                            {
                                propertyInfo.bstrValue = NULL;
                            }
                            else
                            {
                                xData = mProcess.mDbgConnector.GetMemoryData(xStrPointer + xStringLengthOffset, 4, 4);
                                if (xData == null)
                                {
                                    propertyInfo.bstrValue = String.Format("Error! Memory data received was null!");
                                }
                                else
                                {
                                    uint xStringLength = BitConverter.ToUInt32(xData, 0);
                                    propertyInfo.bstrValue = "String of length: " + xStringLength;
                                    if (xStringLength > 100)
                                    {
                                        propertyInfo.bstrValue = "For now, strings larger than 100 chars are not supported..";
                                    }
                                    else if (xStringLength == 0)
                                    {
                                        propertyInfo.bstrValue = "\"\"";
                                    }
                                    else
                                    {
                                        xData = mProcess.mDbgConnector.GetMemoryData(xStrPointer + xStringFirstCharOffset, xStringLength * 2, 2);
                                        if (xData == null)
                                        {
                                            propertyInfo.bstrValue = String.Format("Error! Memory data received was null!");
                                        }
                                        else
                                        {
                                            propertyInfo.bstrValue = "\"" + Encoding.Unicode.GetString(xData) + "\"";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

#warning TODO: String[]

                    #region byte
                    // Byte
                    else if (mDebugInfo.TYPENAME == typeof(byte).AssemblyQualifiedName)
                    {
                        ReadData<byte>(ref propertyInfo, new Func<byte[], int, byte>(delegate(byte[] barr, int ind) { return barr[ind]; }));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(byte[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<byte>(ref propertyInfo, "byte");
                    }
                    #endregion

                    #region sbyte
                    // SByte
                    else if (mDebugInfo.TYPENAME == typeof(sbyte).AssemblyQualifiedName)
                    {
                        ReadData<sbyte>(ref propertyInfo, new Func<byte[], int, sbyte>(delegate(byte[] barr, int ind) { return unchecked((sbyte)barr[ind]); }));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(sbyte[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<sbyte>(ref propertyInfo, "sbyte");
                    }
                    #endregion

                    #region char
                    else if (mDebugInfo.TYPENAME == typeof(char).AssemblyQualifiedName)
                    {
                        xData = mProcess.mDbgConnector.GetStackData(OFFSET, 2);
                        if (xData == null)
                        {
                            propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
                        }
                        else
                        {
                            var xTypedCharValue = BitConverter.ToChar(xData, 0);
                            propertyInfo.bstrValue = String.Format("{0} '{1}'", (ushort)xTypedCharValue, xTypedCharValue);
                        }
                    }
                    else if (mDebugInfo.TYPENAME == typeof(char[]).AssemblyQualifiedName)
                    {
                        xData = mProcess.mDbgConnector.GetStackData(OFFSET, 4);
                        if (xData == null)
                        {
                            propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
                        }
                        else
                        {
                            uint xArrayPointer = BitConverter.ToUInt32(xData, 0);
                            if (xArrayPointer == 0)
                            {
                                propertyInfo.bstrValue = NULL;
                            }
                            else
                            {
                                xData = mProcess.mDbgConnector.GetMemoryData(xArrayPointer + mArrayLengthOffset, 4, 4);
                                if (xData == null)
                                {
                                    propertyInfo.bstrValue = String.Format("Error! Memory data received was null!");
                                }
                                else
                                {
                                    uint xDataLength = BitConverter.ToUInt32(xData, 0);
                                    bool xIsTooLong = xDataLength > 512;
                                    var xSB = new StringBuilder();
                                    xSB.AppendFormat("Char[{0}] at 0x{1} {{ ", xDataLength, xArrayPointer.ToString("X"));
                                    if (xIsTooLong)
                                    {
                                        xDataLength = 512;
                                    }
                                    if (xDataLength > 0)
                                    {
                                        xData = mProcess.mDbgConnector.GetMemoryData(xArrayPointer + mArrayFirstElementOffset, xDataLength * 2);
                                        if (xData == null)
                                        {
                                            xSB.Append(String.Format("Error! Memory data received was null!"));
                                        }
                                        else
                                        {
                                            bool first = true;
                                            for (int i = 0; (i / 2) < xDataLength; i += 2)
                                            {
                                                if (!first)
                                                    xSB.Append(", ");
                                                char c = BitConverter.ToChar(xData, i);
                                                xSB.Append('\'');
                                                if (c == '\0')
                                                {
                                                    xSB.Append("\\0");
                                                }
                                                else
                                                {
                                                    xSB.Append(c);
                                                }
                                                xSB.Append('\'');

                                                first = false;
                                            }
                                        }
                                    }
                                    if (xIsTooLong)
                                    {
                                        xSB.Append(", ..");
                                    }

                                    xSB.Append(" }");
                                    propertyInfo.bstrValue = xSB.ToString();
                                }
                            }
                        }
                    }
                    #endregion

                    #region short
                    // Short
                    else if (mDebugInfo.TYPENAME == typeof(short).AssemblyQualifiedName)
                    {
                        ReadData<short>(ref propertyInfo, new Func<byte[], int, short>(BitConverter.ToInt16));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(short[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<short>(ref propertyInfo, "short");
                    }
                    #endregion

                    #region ushort
                    // UShort
                    else if (mDebugInfo.TYPENAME == typeof(ushort).AssemblyQualifiedName)
                    {
                        ReadData<ushort>(ref propertyInfo, new Func<byte[], int, ushort>(BitConverter.ToUInt16));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(ushort[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<ushort>(ref propertyInfo, "ushort");
                    }
                    #endregion

                    #region int
                    // Int32
                    else if (mDebugInfo.TYPENAME == typeof(int).AssemblyQualifiedName)
                    {
                        ReadData<int>(ref propertyInfo, new Func<byte[], int, int>(BitConverter.ToInt32));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(int[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<int>(ref propertyInfo, "int");
                    }
                    #endregion

                    #region uint
                    // UInt32
                    else if (mDebugInfo.TYPENAME == typeof(uint).AssemblyQualifiedName)
                    {
                        ReadData<uint>(ref propertyInfo, new Func<byte[], int, uint>(BitConverter.ToUInt32));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(uint[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<uint>(ref propertyInfo, "uint");
                    }
                    #endregion

                    #region long
                    // Long
                    else if (mDebugInfo.TYPENAME == typeof(long).AssemblyQualifiedName)
                    {
                        ReadData<long>(ref propertyInfo, new Func<byte[], int, long>(BitConverter.ToInt64));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(long[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<long>(ref propertyInfo, "long");
                    }
                    #endregion

                    #region ulong
                    // ULong
                    else if (mDebugInfo.TYPENAME == typeof(ulong).AssemblyQualifiedName)
                    {
                        ReadData<ulong>(ref propertyInfo, new Func<byte[], int, ulong>(BitConverter.ToUInt64));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(ulong[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<ulong>(ref propertyInfo, "ulong");
                    }
                    #endregion

                    #region float
                    // Float
                    else if (mDebugInfo.TYPENAME == typeof(float).AssemblyQualifiedName)
                    {
                        ReadData<float>(ref propertyInfo, new Func<byte[], int, float>(BitConverter.ToSingle));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(float[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<float>(ref propertyInfo, "float");
                    }
                    #endregion

                    #region double
                    // Double
                    else if (mDebugInfo.TYPENAME == typeof(double).AssemblyQualifiedName)
                    {
                        ReadData<double>(ref propertyInfo, new Func<byte[], int, double>(BitConverter.ToDouble));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(double[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<double>(ref propertyInfo, "double");
                    }
                    #endregion

                    #region bool
                    // Bool
                    else if (mDebugInfo.TYPENAME == typeof(bool).AssemblyQualifiedName)
                    {
                        ReadData<bool>(ref propertyInfo, new Func<byte[], int, bool>(BitConverter.ToBoolean));
                    }
                    else if (mDebugInfo.TYPENAME == typeof(bool[]).AssemblyQualifiedName)
                    {
                        ReadDataArray<bool>(ref propertyInfo, "bool");
                    }
                    #endregion

                    else
                    {
                        if (m_variableInformation.IsReference)
                        {
                            xData = mProcess.mDbgConnector.GetMemoryData(m_variableInformation.Pointer, 4, 4);
                        }
                        else
                        {
                            xData = mProcess.mDbgConnector.GetStackData(OFFSET, 4);
                        }
                        if (xData == null)
                        {
                            propertyInfo.bstrValue = String.Format("Error! Stack data received was null!");
                        }
                        else
                        {
                            var xPointer = BitConverter.ToUInt32(xData, 0);
                            if (xPointer == 0)
                            {
                                propertyInfo.bstrValue = NULL;
                            }
                            else
                            {
                                try
                                {
                                    var mp = mProcess.mDebugInfoDb.GetFieldMap(mDebugInfo.TYPENAME);
                                    foreach (string str in mp.FieldNames)
                                    {
                                        Cosmos.Debug.Common.FIELD_INFO xFieldInfo;
                                        xFieldInfo = mProcess.mDebugInfoDb.GetFieldInfoByName(str);
                                        var inf = new DebugLocalInfo();
                                        inf.IsReference = true;
                                        inf.Type = xFieldInfo.TYPE;
                                        inf.Offset = xFieldInfo.OFFSET;
                                        inf.Pointer = (uint)(xPointer + xFieldInfo.OFFSET + 12);
                                        inf.Name = GetFieldName(xFieldInfo);
                                        this.m_variableInformation.Children.Add(new AD7Property(inf, this.mProcess, this.mStackFrame));
                                    }
                                    propertyInfo.bstrValue = String.Format("{0} (0x{1})", xPointer, xPointer.ToString("X").ToUpper());
                                }
                                catch (Exception ex)
                                {
                                    if (ex.GetType().Name == "SQLiteException")
                                    {
                                        //Ignore but warn user
                                        propertyInfo.bstrValue = "SQLiteException. Could not get type information for " + mDebugInfo.TYPENAME;
                                    }
                                    else
                                    {
                                        throw new Exception("Unexpected error in AD7Property.cs:459", ex);
                                    }
                                }
                            }
                        }
                    }
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                }

                if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
                {
                    // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                    propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                    if (this.m_variableInformation.Children.Count > 0)
                    {
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
            }
            catch
            {
            }

            return propertyInfo;
        }

        private static string GetFieldName(Cosmos.Debug.Common.FIELD_INFO fInf)
        {
            string s = fInf.NAME;
            int i = s.LastIndexOf('.');
            if (i > 0)
            {
                s = s.Substring(i + 1, s.Length - i - 1);
                return s;
            }
            return s;
        }

        #region IDebugProperty2 Members

        // Enumerates the children of a property. This provides support for dereferencing pointers, displaying members of an array, or fields of a class or struct.
        // The sample debugger only supports pointer dereferencing as children. This means there is only ever one child.
        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref System.Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = null;

            if (this.m_variableInformation.Children.Count > 0)
            {
                List<DEBUG_PROPERTY_INFO> infs = new List<DEBUG_PROPERTY_INFO>();
                foreach (AD7Property dp in m_variableInformation.Children)
                {
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
        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // This method exists for the purpose of retrieving information that does not lend itself to being retrieved by calling the IDebugProperty2::GetPropertyInfo
        // method. This includes information about custom viewers, managed type slots and other information.
        // The sample engine does not support this.
        public int GetExtendedInfo(ref System.Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Returns the memory bytes for a property value.
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Returns the memory context for a property value.
        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Returns the parent of a property.
        // The sample engine does not support obtaining the parent of properties.
        public int GetParent(out IDebugProperty2 ppParent)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Fills in a DEBUG_PROPERTY_INFO structure that describes a property.
        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            pPropertyInfo[0] = new DEBUG_PROPERTY_INFO();
            rgpArgs = null;
            pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
            return VSConstants.S_OK;
        }

        //  Return an IDebugReference2 for this property. An IDebugReference2 can be thought of as a type and an address.
        public int GetReference(out IDebugReference2 ppReference)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Returns the size, in bytes, of the property value.
        public int GetSize(out uint pdwSize)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // The debugger will call this when the user tries to edit the property's values
        // the sample has set the read-only flag on its properties, so this should not be called.
        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // The debugger will call this when the user tries to edit the property's values in one of the debugger windows.
        // the sample has set the read-only flag on its properties, so this should not be called.
        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

    }
}
