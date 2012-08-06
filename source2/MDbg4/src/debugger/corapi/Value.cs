//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Text;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Collections.Generic;

namespace Microsoft.Samples.Debugging.CorDebug
{


    /** A value in the remote process. */
    public class CorValue : WrapperBase
    {
        public CorValue(ICorDebugValue value)
            : base(value)
        {
            m_val = value;
        }

        [CLSCompliant(false)]
        public ICorDebugValue Raw
        {
            get 
            { 
                return m_val;
            }
        }

        /** The simple type of the value. */
        public CorElementType Type
        {
            get
            {
                CorElementType varType;
                m_val.GetType(out varType);
                return varType;
            }
        }

        /** Full runtime type of the object . */
        public CorType ExactType
        {
            get
            {
                ICorDebugValue2 v2 = (ICorDebugValue2)m_val;
                ICorDebugType dt;
                v2.GetExactType(out dt);
                return new CorType(dt);
            }
        }

        /** size of the value (in bytes). */
        public int Size
        {
            get
            {
                uint s = 0;
                m_val.GetSize(out s);
                return (int)s;
            }
        }

        /** Address of the value in the debuggee process. */
        public long Address
        {
            get
            {
                ulong addr = 0;
                m_val.GetAddress(out addr);
                return (long)addr;
            }
        }

        /** Breakpoint triggered when the value is modified. */
        public CorValueBreakpoint CreateBreakpoint()
        {
            ICorDebugValueBreakpoint bp = null;
            m_val.CreateBreakpoint(out bp);
            return new CorValueBreakpoint(bp);
        }

        // casting operations
        public CorReferenceValue CastToReferenceValue()
        {
            if (m_val is ICorDebugReferenceValue)
                return new CorReferenceValue((ICorDebugReferenceValue)m_val);
            else
                return null;
        }

        public CorHandleValue CastToHandleValue()
        {
            if (m_val is ICorDebugHandleValue)
                return new CorHandleValue((ICorDebugHandleValue)m_val);
            else
                return null;
        }

        public CorStringValue CastToStringValue()
        {
            return new CorStringValue((ICorDebugStringValue)m_val);
        }

        public CorObjectValue CastToObjectValue()
        {
            return new CorObjectValue((ICorDebugObjectValue)m_val);
        }

        public CorGenericValue CastToGenericValue()
        {
            if (m_val is ICorDebugGenericValue)
                return new CorGenericValue((ICorDebugGenericValue)m_val);
            else
                return null;
        }

        public CorBoxValue CastToBoxValue()
        {
            if (m_val is ICorDebugBoxValue)
                return new CorBoxValue((ICorDebugBoxValue)m_val);
            else
                return null;
        }

        public CorArrayValue CastToArrayValue()
        {
            if (m_val is ICorDebugArrayValue)
                return new CorArrayValue((ICorDebugArrayValue)m_val);
            else
                return null;
        }

        public CorHeapValue CastToHeapValue()
        {
            if (m_val is ICorDebugHeapValue)
                return new CorHeapValue((ICorDebugHeapValue)m_val);
            else
                return null;
        }

        internal ICorDebugValue m_val = null;

    } /* class Value */


    public class CorReferenceValue : CorValue
    {

        internal CorReferenceValue(ICorDebugReferenceValue referenceValue)
            : base(referenceValue)
        {
            m_refVal = referenceValue;
        }

        public Int64 Value
        {
            get
            {
                UInt64 v;
                m_refVal.GetValue(out v);
                return (Int64)v;
            }
            set
            {
                UInt64 v = (UInt64)value;
                m_refVal.SetValue(v);
            }
        }

        public bool IsNull
        {
            get
            {
                int bNull;
                m_refVal.IsNull(out bNull);
                return (bNull == 0) ? false : true;
            }
        }

        public CorValue Dereference()
        {
            ICorDebugValue v;
            m_refVal.Dereference(out v);
            return (v == null ? null : new CorValue(v));
        }

        private ICorDebugReferenceValue m_refVal = null;
    }


    public sealed class CorHandleValue : CorReferenceValue, System.IDisposable
    {

        internal CorHandleValue(ICorDebugHandleValue handleValue)
            : base(handleValue)
        {
            m_handleVal = handleValue;
        }

        public void Dispose()
        {
            // The underlying ICorDebugHandle has a  Dispose() method which will free
            // its resources (a GC handle). We call that now to free things sooner.
            // If we don't call it now, it will still get freed at some random point after
            // the final release (which the finalizer will call).
            try
            {
                // This is just a best-effort to cleanup resources early.
                // If it fails, just swallow and move on.
                // May throw if handle was already disposed, or if process is not stopped.
                m_handleVal.Dispose();
            }
            catch
            {
                // swallow all
            }
        }

        [CLSCompliant(false)]
        public CorDebugHandleType HandleType
        {
            get
            {
                CorDebugHandleType ht;
                m_handleVal.GetHandleType(out ht);
                return ht;
            }
        }
        private ICorDebugHandleValue m_handleVal = null;
    }

    public sealed class CorStringValue : CorValue
    {

        internal CorStringValue(ICorDebugStringValue stringValue)
            : base(stringValue)
        {
            m_strVal = stringValue;
        }

        public bool IsValid
        {
            get
            {
                int bValid;
                m_strVal.IsValid(out bValid);
                return (bValid == 0) ? false : true;
            }
        }

        public string String
        {
            get
            {
                uint stringSize;
                StringBuilder sb = new StringBuilder(Length + 1); // we need one extra char for null
                m_strVal.GetString((uint)sb.Capacity, out stringSize, sb);
                return sb.ToString();
            }
        }

        public int Length
        {
            get
            {
                uint stringSize;
                m_strVal.GetLength(out stringSize);
                return (int)stringSize;
            }
        }

        private ICorDebugStringValue m_strVal = null;
    }


    public sealed class CorObjectValue : CorValue
    {
        internal CorObjectValue(ICorDebugObjectValue objectValue)
            : base(objectValue)
        {
            m_objVal = objectValue;
        }

        public CorClass Class
        {
            get
            {
                ICorDebugClass iclass;
                m_objVal.GetClass(out iclass);
                return (iclass == null) ? null : new CorClass(iclass);
            }
        }

        public CorValue GetFieldValue(CorClass managedClass, int fieldToken)
        {
            ICorDebugValue val;
            m_objVal.GetFieldValue(managedClass.m_class, (uint)fieldToken, out val);
            return new CorValue(val);
        }

        public CorType GetVirtualMethodAndType(int memberToken, out CorFunction managedFunction)
        {
            ICorDebugType dt = null;
            ICorDebugFunction pfunc = null;
            (m_objVal as ICorDebugObjectValue2).GetVirtualMethodAndType((uint)memberToken, out pfunc, out dt);
            if (pfunc == null)
                managedFunction = null;
            else
                managedFunction = new CorFunction(pfunc);
            return dt == null ? null : new CorType(dt);
        }


        public bool IsValueClass
        {
            get
            {
                int bIsValueClass;
                m_objVal.IsValueClass(out bIsValueClass);
                return bIsValueClass != 0;
            }
        }

        // public Object GetManagedCopy() -- deprecated, therefore we won't make it available at all.
        private ICorDebugObjectValue m_objVal = null;
    }

    public sealed class CorGenericValue : CorValue
    {
        internal CorGenericValue(ICorDebugGenericValue genericValue)
            : base(genericValue)
        {
            m_genVal = genericValue;
        }

        // Convert the supplied value to the type of this CorGenericValue using System.IConvertable.
        // Then store the value into this CorGenericValue.  Any compatible type can be supplied.
        // For example, if you supply a string and the underlying type is ELEMENT_TYPE_BOOLEAN,
        // Convert.ToBoolean will attempt to match the string against "true" and "false".
        public void SetValue(object value)
        {
            try
            {
                switch (this.Type)
                {
                    case CorElementType.ELEMENT_TYPE_BOOLEAN:
                        bool v = Convert.ToBoolean(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_I1:
                        SByte sbv = Convert.ToSByte(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&sbv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_U1:
                        Byte bv = Convert.ToByte(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&bv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_CHAR:
                        Char cv = Convert.ToChar(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&cv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_I2:
                        Int16 i16v = Convert.ToInt16(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&i16v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_U2:
                        UInt16 u16v = Convert.ToUInt16(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&u16v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_I4:
                        Int32 i32v = Convert.ToInt32(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&i32v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_U4:
                        UInt32 u32v = Convert.ToUInt32(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&u32v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_I:
                        Int64 ip64v = Convert.ToInt64(value);
                        IntPtr ipv = new IntPtr(ip64v);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&ipv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_U:
                        UInt64 ipu64v = Convert.ToUInt64(value);
                        UIntPtr uipv = new UIntPtr(ipu64v);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&uipv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_I8:
                        Int64 i64v = Convert.ToInt64(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&i64v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_U8:
                        UInt64 u64v = Convert.ToUInt64(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&u64v));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_R4:
                        Single sv = Convert.ToSingle(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&sv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_R8:
                        Double dv = Convert.ToDouble(value);
                        unsafe
                        {
                            SetValueInternal(new IntPtr(&dv));
                        }
                        break;

                    case CorElementType.ELEMENT_TYPE_VALUETYPE:
                        byte[] bav = (byte[])value;
                        unsafe
                        {
                            fixed (byte* bufferPtr = &bav[0])
                            {
                                Debug.Assert(this.Size == bav.Length);
                                m_genVal.SetValue(new IntPtr(bufferPtr));
                            }
                        }
                        break;

                    default:
                        throw new InvalidOperationException("Type passed is not recognized.");
                }
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException("Wrong type used for SetValue command", e);
            }
        }

        public object GetValue()
        {
            return UnsafeGetValueAsType(this.Type);
        }

        /// <summary>
        /// Get the value as an array of IntPtrs.
        /// </summary>
        public IntPtr[] GetValueAsIntPtrArray()
        {
            int ptrsize = IntPtr.Size;
            int cElem = (this.Size + ptrsize - 1) / ptrsize;
            IntPtr[] buffer = new IntPtr[cElem];

            unsafe
            {
                fixed (IntPtr* bufferPtr = &buffer[0])
                {
                    this.GetValueInternal(new IntPtr(bufferPtr));
                }
            }
            return buffer;
        }

        public byte[] GetValueAsByteArray()
        {
            byte[] buffer = new Byte[this.Size];

            unsafe
            {
                fixed (byte* bufferPtr = &buffer[0])
                {
                    this.GetValueInternal(new IntPtr(bufferPtr));
                }
            }
            return buffer;
        }

        public Object UnsafeGetValueAsType(CorElementType type)
        {
            switch (type)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    byte bValue = 4; // just initialize to avoid compiler warnings
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(byte));
                        this.GetValueInternal(new IntPtr(&bValue));
                    }
                    return (object)(bValue != 0);

                case CorElementType.ELEMENT_TYPE_CHAR:
                    char cValue = 'a'; // initialize to avoid compiler warnings
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(char));
                        this.GetValueInternal(new IntPtr(&cValue));
                    }
                    return (object)cValue;

                case CorElementType.ELEMENT_TYPE_I1:
                    SByte i1Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(SByte));
                        this.GetValueInternal(new IntPtr(&i1Value));
                    }
                    return (object)i1Value;

                case CorElementType.ELEMENT_TYPE_U1:
                    Byte u1Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Byte));
                        this.GetValueInternal(new IntPtr(&u1Value));
                    }
                    return (object)u1Value;

                case CorElementType.ELEMENT_TYPE_I2:
                    Int16 i2Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Int16));
                        this.GetValueInternal(new IntPtr(&i2Value));
                    }
                    return (object)i2Value;

                case CorElementType.ELEMENT_TYPE_U2:
                    UInt16 u2Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(UInt16));
                        this.GetValueInternal(new IntPtr(&u2Value));
                    }
                    return (object)u2Value;

                case CorElementType.ELEMENT_TYPE_I:
                    IntPtr ipValue = IntPtr.Zero;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(IntPtr));
                        this.GetValueInternal(new IntPtr(&ipValue));
                    }
                    return (object)ipValue;

                case CorElementType.ELEMENT_TYPE_U:
                    UIntPtr uipValue = UIntPtr.Zero;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(UIntPtr));
                        this.GetValueInternal(new IntPtr(&uipValue));
                    }
                    return (object)uipValue;

                case CorElementType.ELEMENT_TYPE_I4:
                    Int32 i4Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Int32));
                        this.GetValueInternal(new IntPtr(&i4Value));
                    }
                    return (object)i4Value;

                case CorElementType.ELEMENT_TYPE_U4:
                    UInt32 u4Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(UInt32));
                        this.GetValueInternal(new IntPtr(&u4Value));
                    }
                    return (object)u4Value;

                case CorElementType.ELEMENT_TYPE_I8:
                    Int64 i8Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Int64));
                        this.GetValueInternal(new IntPtr(&i8Value));
                    }
                    return (object)i8Value;

                case CorElementType.ELEMENT_TYPE_U8:
                    UInt64 u8Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(UInt64));
                        this.GetValueInternal(new IntPtr(&u8Value));
                    }
                    return (object)u8Value;

                case CorElementType.ELEMENT_TYPE_R4:
                    Single r4Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Single));
                        this.GetValueInternal(new IntPtr(&r4Value));
                    }
                    return (object)r4Value;

                case CorElementType.ELEMENT_TYPE_R8:
                    Double r8Value = 4;
                    unsafe
                    {
                        Debug.Assert(this.Size == sizeof(Double));
                        this.GetValueInternal(new IntPtr(&r8Value));
                    }
                    return (object)r8Value;


                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    byte[] buffer = new byte[this.Size];
                    unsafe
                    {
                        fixed (byte* bufferPtr = &buffer[0])
                        {
                            Debug.Assert(this.Size == buffer.Length);
                            this.GetValueInternal(new IntPtr(bufferPtr));
                        }
                    }
                    return buffer;

                default:
                    Debug.Assert(false, "Generic value should not be of any other type");
                    throw new NotSupportedException();
            }
        }


        private void SetValueInternal(IntPtr valPtr)
        {
            m_genVal.SetValue(valPtr);
        }

        private void GetValueInternal(IntPtr valPtr)
        {
            m_genVal.GetValue(valPtr);
        }

        private ICorDebugGenericValue m_genVal = null;
    }

    public sealed class CorBoxValue : CorValue
    {
        internal CorBoxValue(ICorDebugBoxValue boxedValue)
            : base(boxedValue)
        {
            m_boxVal = boxedValue;
        }

        public CorObjectValue GetObject()
        {
            ICorDebugObjectValue ov;
            m_boxVal.GetObject(out ov);
            return (ov == null) ? null : new CorObjectValue(ov);
        }

        private ICorDebugBoxValue m_boxVal = null;
    }

    public sealed class CorArrayValue : CorValue
    {
        internal CorArrayValue(ICorDebugArrayValue arrayValue)
            : base(arrayValue)
        {
            m_arrayVal = arrayValue;
        }

        //void CreateRelocBreakpoint(ref CORDBLib.ICorDebugValueBreakpoint ppBreakpoint);
        //void GetBaseIndicies(UInt32 cdim, IntPtr indicies);

        public int Count
        {
            get
            {
                uint pnCount;
                m_arrayVal.GetCount(out pnCount);
                return (int)pnCount;
            }
        }


        public CorElementType ElementType
        {
            get
            {
                CorElementType type;
                m_arrayVal.GetElementType(out type);
                return type;
            }
        }

        public int Rank
        {
            get
            {
                uint pnRank;
                m_arrayVal.GetRank(out pnRank);
                return (int)pnRank;
            }
        }

        public bool HasBaseIndicies
        {
            get
            {
                int pbHasBaseIndicies;
                m_arrayVal.HasBaseIndicies(out pbHasBaseIndicies);
                return pbHasBaseIndicies == 0 ? false : true;
            }
        }

        public bool IsValid
        {
            get
            {
                int pbValid;
                m_arrayVal.IsValid(out pbValid);
                return pbValid == 0 ? false : true;
            }
        }

        public int[] GetDimensions()
        {
            Debug.Assert(Rank != 0);
            uint[] dims = new uint[Rank];
            m_arrayVal.GetDimensions((uint)dims.Length, dims);

            int[] sdims = Array.ConvertAll<uint, int>(dims, delegate(uint u) { return (int)u; });
            return sdims;
        }

        public CorValue GetElement(int[] indices)
        {
            Debug.Assert(indices != null);
            ICorDebugValue ppValue;
            m_arrayVal.GetElement((uint)indices.Length, indices, out ppValue);
            return ppValue == null ? null : new CorValue(ppValue);
        }

        public CorValue GetElementAtPosition(int position)
        {
            ICorDebugValue ppValue;
            m_arrayVal.GetElementAtPosition((uint)position, out ppValue);
            return ppValue == null ? null : new CorValue(ppValue);
        }
        private ICorDebugArrayValue m_arrayVal = null;
    }

    public sealed class CorHeapValue : CorValue
    {
        internal CorHeapValue(ICorDebugHeapValue heapValue)
            : base(heapValue)
        {
            m_heapVal = heapValue;
        }

        //void CreateRelocBreakpoint(ref Microsoft.Samples.Debugging.CorDebug.NativeApi.ICorDebugValueBreakpoint ppBreakpoint);
        public CorValueBreakpoint CreateRelocBreakpoint()
        {
            ICorDebugValueBreakpoint bp = null;
            m_heapVal.CreateRelocBreakpoint(out bp);
            return new CorValueBreakpoint(bp);
        }

        //void IsValid(ref Int32 pbValid);
        public bool IsValid
        {
            get
            {
                int bValid;
                m_heapVal.IsValid(out bValid);
                return bValid != 0;
            }
        }

        [CLSCompliant(false)]
        public CorHandleValue CreateHandle(CorDebugHandleType type)
        {
            ICorDebugHandleValue handle;
            (m_heapVal as ICorDebugHeapValue2).CreateHandle(type, out handle);
            return handle == null ? null : new CorHandleValue(handle);
        }

        /// <summary>
        /// Returns the thread which owns the monitor lock or null if no thread owns it
        /// </summary>
        public CorThread GetThreadOwningMonitorLock()
        {
            if (m_heapVal as ICorDebugHeapValue3 == null)
                throw new NotSupportedException();
            ICorDebugThread owner;
            int acquisitionCount;
            (m_heapVal as ICorDebugHeapValue3).GetThreadOwningMonitorLock(out owner, out acquisitionCount);
            return owner == null ? null : new CorThread(owner);
        }

        /// <summary>
        /// Returns the number of times this lock would need to be released in order for it to
        /// be unowned
        /// </summary>
        public int GetMonitorAcquisitionCount()
        {
            if (m_heapVal as ICorDebugHeapValue3 == null)
                throw new NotSupportedException();
            ICorDebugThread owner;
            int acquisitionCount;
            (m_heapVal as ICorDebugHeapValue3).GetThreadOwningMonitorLock(out owner, out acquisitionCount);
            return acquisitionCount;
        }

        /// <summary>
        /// Returns a list of threads waiting for the monitor event associated with this object
        /// </summary>
        /// <returns>The list of waiting threads. The first thread in the list will be released on the
        /// next call to Monitor.Pulse, and each succesive call will release the next thread in the list</returns>
        public CorThread[] GetMonitorEventWaitList()
        {
            if (m_heapVal as ICorDebugHeapValue3 == null)
                throw new NotSupportedException();
            ICorDebugThreadEnum rawThreadEnum;
            (m_heapVal as ICorDebugHeapValue3).GetMonitorEventWaitList(out rawThreadEnum);
            uint threadCount;
            rawThreadEnum.GetCount(out threadCount);
            ICorDebugThread[] rawThreads = new ICorDebugThread[threadCount];
            uint countReceived;
            rawThreadEnum.Next(threadCount, rawThreads, out countReceived);
            Debug.Assert(countReceived == threadCount);
            CorThread[] threads = new CorThread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new CorThread(rawThreads[i]);
            }
            return threads;
        }

        private ICorDebugHeapValue m_heapVal = null;
    }

} /* namespace  */
