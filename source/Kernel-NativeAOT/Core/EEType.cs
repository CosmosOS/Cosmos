using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime
{
    internal enum EETypeElementType
    {
        // Primitive
        Unknown = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        SByte = 0x04,
        Byte = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0A,
        UInt64 = 0x0B,
        IntPtr = 0x0C,
        UIntPtr = 0x0D,
        Single = 0x0E,
        Double = 0x0F,

        ValueType = 0x10,
        // Enum = 0x11, // EETypes store enums as their underlying type
        Nullable = 0x12,
        // Unused 0x13,

        Class = 0x14,
        Interface = 0x15,

        SystemArray = 0x16, // System.Array type

        Array = 0x17,
        SzArray = 0x18,
        ByRef = 0x19,
        Pointer = 0x1A,
    }

    [Flags]
    internal enum EETypeFlags : ushort
    {
        /// <summary>
        /// There are four kinds of EETypes, defined in <c>Kinds</c>.
        /// </summary>
        EETypeKindMask = 0x0003,

        /// <summary>
        /// This flag is set when m_RelatedType is in a different module.  In that case, _pRelatedType
        /// actually points to an IAT slot in this module, which then points to the desired EEType in the
        /// other module.  In other words, there is an extra indirection through m_RelatedType to get to 
        /// the related type in the other module.  When this flag is set, it is expected that you use the 
        /// "_ppXxxxViaIAT" member of the RelatedTypeUnion for the particular related type you're 
        /// accessing.
        /// </summary>
        RelatedTypeViaIATFlag = 0x0004,

        /// <summary>
        /// This type was dynamically allocated at runtime.
        /// </summary>
        IsDynamicTypeFlag = 0x0008,

        /// <summary>
        /// This EEType represents a type which requires finalization.
        /// </summary>
        HasFinalizerFlag = 0x0010,

        /// <summary>
        /// This type contain GC pointers.
        /// </summary>
        HasPointersFlag = 0x0020,

        /// <summary>
        /// Type implements ICastable to allow dynamic resolution of interface casts.
        /// </summary>
        ICastableFlag = 0x0040,

        /// <summary>
        /// This type is generic and one or more of its type parameters is co- or contra-variant. This
        /// only applies to interface and delegate types.
        /// </summary>
        GenericVarianceFlag = 0x0080,

        /// <summary>
        /// This type has optional fields present.
        /// </summary>
        OptionalFieldsFlag = 0x0100,

        // Unused = 0x0200,

        /// <summary>
        /// This type is generic.
        /// </summary>
        IsGenericFlag = 0x0400,

        /// <summary>
        /// We are storing a EETypeElementType in the upper bits for unboxing enums.
        /// </summary>
        ElementTypeMask = 0xf800,
        ElementTypeShift = 11,

        /// <summary>
        /// Single mark to check TypeKind and two flags. When non-zero, casting is more complicated.
        /// </summary>
        ComplexCastingMask = EETypeKindMask | RelatedTypeViaIATFlag | GenericVarianceFlag
    };

    internal enum EETypeKind : ushort
    {
        /// <summary>
        /// Represents a standard ECMA type
        /// </summary>
        CanonicalEEType = 0x0000,

        /// <summary>
        /// Represents a type cloned from another EEType
        /// </summary>
        ClonedEEType = 0x0001,

        /// <summary>
        /// Represents a parameterized type. For example a single dimensional array or pointer type
        /// </summary>
        ParameterizedEEType = 0x0002,

        /// <summary>
        /// Represents an uninstantiated generic type definition
        /// </summary>
        GenericTypeDefEEType = 0x0003,
    }


    //    [StructLayout(LayoutKind.Sequential)]
    //    internal struct ObjHeader {
    //        // Contents of the object header
    //        private IntPtr _objHeaderContents;
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    internal unsafe struct EEInterfaceInfo {
    //        [StructLayout(LayoutKind.Explicit)]
    //        private unsafe struct InterfaceTypeUnion {
    //            [FieldOffset(0)]
    //            public EEType* _pInterfaceEEType;
    //            [FieldOffset(0)]
    //            public EEType** _ppInterfaceEETypeViaIAT;
    //        }

    //        private InterfaceTypeUnion _interfaceType;

    //        internal EEType* InterfaceType {
    //            get {
    //                if ((unchecked((uint)_interfaceType._pInterfaceEEType) & IndirectionConstants.IndirectionCellPointer) != 0) {
    //#if TARGET_64BIT
    //                    EEType** ppInterfaceEETypeViaIAT = (EEType**)(((ulong)_interfaceType._ppInterfaceEETypeViaIAT) - IndirectionConstants.IndirectionCellPointer);
    //#else
    //                    EEType** ppInterfaceEETypeViaIAT = (EEType**)(((uint)_interfaceType._ppInterfaceEETypeViaIAT) - IndirectionConstants.IndirectionCellPointer);
    //#endif
    //                    return *ppInterfaceEETypeViaIAT;
    //                }

    //                return _interfaceType._pInterfaceEEType;
    //            }
    //#if TYPE_LOADER_IMPLEMENTATION
    //            set
    //            {
    //                _interfaceType._pInterfaceEEType = value;
    //            }
    //#endif
    //        }
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    internal unsafe struct DispatchMap {
    //        [StructLayout(LayoutKind.Sequential)]
    //        internal unsafe struct DispatchMapEntry {
    //            internal ushort _usInterfaceIndex;
    //            internal ushort _usInterfaceMethodSlot;
    //            internal ushort _usImplMethodSlot;
    //        }

    //        private uint _entryCount;
    //        private DispatchMapEntry _dispatchMap; // at least one entry if any interfaces defined

    //        public bool IsEmpty {
    //            get {
    //                return _entryCount == 0;
    //            }
    //        }

    //        public uint NumEntries {
    //            get {
    //                return _entryCount;
    //            }
    //#if TYPE_LOADER_IMPLEMENTATION
    //            set
    //            {
    //                _entryCount = value;
    //            }
    //#endif
    //        }

    //        public int Size {
    //            get {
    //                return sizeof(uint) + sizeof(DispatchMapEntry) * (int)_entryCount;
    //            }
    //        }

    //        public DispatchMapEntry* this[int index] {
    //            get {
    //                fixed (DispatchMap* pThis = &this)
    //                    return (DispatchMapEntry*)((byte*)pThis + sizeof(uint) + (sizeof(DispatchMapEntry) * index));
    //            }
    //        }
    //    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EEType
    {
        private const int POINTER_SIZE = 8;
        private const int PADDING = 1; // _numComponents is padded by one Int32 to make the first element pointer-aligned
        internal const int SZARRAY_BASE_SIZE = POINTER_SIZE + POINTER_SIZE + (1 + PADDING) * 4;

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct RelatedTypeUnion
        {
            // Kinds.CanonicalEEType
            [FieldOffset(0)]
            public EEType* _pBaseType;
            [FieldOffset(0)]
            public EEType** _ppBaseTypeViaIAT;

            // Kinds.ClonedEEType
            [FieldOffset(0)]
            public EEType* _pCanonicalType;
            [FieldOffset(0)]
            public EEType** _ppCanonicalTypeViaIAT;

            // Kinds.ArrayEEType
            [FieldOffset(0)]
            public EEType* _pRelatedParameterType;
            [FieldOffset(0)]
            public EEType** _ppRelatedParameterTypeViaIAT;
        }

        //private static unsafe class OptionalFieldsReader {
        //    internal static uint GetInlineField(byte* pFields, EETypeOptionalFieldTag eTag, uint uiDefaultValue) {
        //        if (pFields == null)
        //            return uiDefaultValue;

        //        bool isLastField = false;
        //        while (!isLastField) {
        //            byte fieldHeader = NativePrimitiveDecoder.ReadUInt8(ref pFields);
        //            isLastField = (fieldHeader & 0x80) != 0;
        //            EETypeOptionalFieldTag eCurrentTag = (EETypeOptionalFieldTag)(fieldHeader & 0x7f);
        //            uint uiCurrentValue = NativePrimitiveDecoder.DecodeUnsigned(ref pFields);

        //            // If we found a tag match return the current value.
        //            if (eCurrentTag == eTag)
        //                return uiCurrentValue;
        //        }

        //        // Reached end of stream without getting a match. Field is not present so return default value.
        //        return uiDefaultValue;
        //    }
        //}

        ///// <summary>
        ///// Gets a value indicating whether the statically generated data structures use relative pointers.
        ///// </summary>
        //internal static bool SupportsRelativePointers {
        //    [Intrinsic]
        //    get {
        //        throw new NotImplementedException();
        //    }
        //}

        private ushort _usComponentSize;
        private ushort _usFlags;
        private uint _uBaseSize;
        private RelatedTypeUnion _relatedType;
        private ushort _usNumVtableSlots;
        private ushort _usNumInterfaces;
        private uint _uHashCode;

        // vtable follows

        // These masks and paddings have been chosen so that the ValueTypePadding field can always fit in a byte of data.
        // if the alignment is 8 bytes or less. If the alignment is higher then there may be a need for more bits to hold
        // the rest of the padding data.
        // If paddings of greater than 7 bytes are necessary, then the high bits of the field represent that padding
        private const uint ValueTypePaddingLowMask = 0x7;
        private const uint ValueTypePaddingHighMask = 0xFFFFFF00;
        private const uint ValueTypePaddingMax = 0x07FFFFFF;
        private const int ValueTypePaddingHighShift = 8;
        private const uint ValueTypePaddingAlignmentMask = 0xF8;
        private const int ValueTypePaddingAlignmentShift = 3;

        internal ushort ComponentSize
        {
            get
            {
                return _usComponentSize;
            }
        }

        //internal ushort GenericArgumentCount {
        //	get {
        //		Debug.Assert(IsGenericTypeDefinition);
        //		return _usComponentSize;
        //	}
        //}

        //internal ushort Flags {
        //	get {
        //		return _usFlags;
        //	}
        //}

        internal uint BaseSize
        {
            get
            {
                return _uBaseSize;
            }
        }

        //internal ushort NumVtableSlots {
        //	get {
        //		return _usNumVtableSlots;
        //	}
        //}

        //internal ushort NumInterfaces {
        //	get {
        //		return _usNumInterfaces;
        //	}
        //}

        //internal uint HashCode {
        //	get {
        //		return _uHashCode;
        //	}
        //}

        private EETypeKind Kind
        {
            get
            {
                return (EETypeKind)(_usFlags & (ushort)EETypeFlags.EETypeKindMask);
            }
        }

        //internal bool HasOptionalFields {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.OptionalFieldsFlag) != 0);
        //	}
        //}

        //// Mark or determine that a type is generic and one or more of it's type parameters is co- or
        //// contra-variant. This only applies to interface and delegate types.
        //internal bool HasGenericVariance {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.GenericVarianceFlag) != 0);
        //	}
        //}

        //internal bool IsFinalizable {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.HasFinalizerFlag) != 0);
        //	}
        //}

        //internal bool IsNullable {
        //	get {
        //		return ElementType == EETypeElementType.Nullable;
        //	}
        //}

        //internal bool IsCloned {
        //	get {
        //		return Kind == EETypeKind.ClonedEEType;
        //	}
        //}

        //internal bool IsCanonical {
        //	get {
        //		return Kind == EETypeKind.CanonicalEEType;
        //	}
        //}

        internal bool IsString
        {
            get
            {
                // String is currently the only non-array type with a non-zero component size.
                return ComponentSize == sizeof(char) && !IsArray && !IsGenericTypeDefinition;
            }
        }

        internal bool IsArray
        {
            get
            {
                EETypeElementType elementType = ElementType;
                return elementType == EETypeElementType.Array || elementType == EETypeElementType.SzArray;
            }
        }


        //internal int ArrayRank {
        //	get {
        //		Debug.Assert(this.IsArray);

        //		int boundsSize = (int)this.ParameterizedTypeShape - SZARRAY_BASE_SIZE;
        //		if (boundsSize > 0) {
        //			// Multidim array case: Base size includes space for two Int32s
        //			// (upper and lower bound) per each dimension of the array.
        //			return boundsSize / (2 * sizeof(int));
        //		}
        //		return 1;
        //	}
        //}

        //internal bool IsSzArray {
        //	get {
        //		return ElementType == EETypeElementType.SzArray;
        //	}
        //}

        //internal bool IsGeneric {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.IsGenericFlag) != 0);
        //	}
        //}

        internal bool IsGenericTypeDefinition
        {
            get
            {
                return Kind == EETypeKind.GenericTypeDefEEType;
            }
        }

        //internal EEType* GenericDefinition {
        //	get {
        //		Debug.Assert(IsGeneric);
        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<IatAwarePointer<EEType>>(EETypeField.ETF_GenericDefinition).Value;

        //		return GetField<IatAwareRelativePointer<EEType>>(EETypeField.ETF_GenericDefinition).Value;
        //	}
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private readonly struct GenericComposition {
        //	public readonly ushort Arity;

        //	private readonly EETypeRef _genericArgument1;
        //	public EETypeRef* GenericArguments {
        //		get {
        //			return (EETypeRef*)Unsafe.AsPointer(ref Unsafe.AsRef(in _genericArgument1));
        //		}
        //	}

        //	public GenericVariance* GenericVariance {
        //		get {
        //			// Generic variance directly follows the last generic argument
        //			return (GenericVariance*)(GenericArguments + Arity);
        //		}
        //	}
        //}

        //internal uint GenericArity {
        //	get {
        //		Debug.Assert(IsGeneric);
        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->Arity;

        //		return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->Arity;
        //	}
        //}

        //internal EETypeRef* GenericArguments {
        //	get {
        //		Debug.Assert(IsGeneric);
        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericArguments;

        //		return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericArguments;
        //	}
        //}

        //internal GenericVariance* GenericVariance {
        //	get {
        //		Debug.Assert(IsGeneric);

        //		if (!HasGenericVariance)
        //			return null;

        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<Pointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericVariance;

        //		return GetField<RelativePointer<GenericComposition>>(EETypeField.ETF_GenericComposition).Value->GenericVariance;
        //	}
        //}

        //internal bool IsPointerType {
        //	get {
        //		return ElementType == EETypeElementType.Pointer;
        //	}
        //}

        //internal bool IsByRefType {
        //	get {
        //		return ElementType == EETypeElementType.ByRef;
        //	}
        //}

        //internal bool IsInterface {
        //	get {
        //		return ElementType == EETypeElementType.Interface;
        //	}
        //}

        //internal bool IsAbstract {
        //	get {
        //		return IsInterface || (RareFlags & EETypeRareFlags.IsAbstractClassFlag) != 0;
        //	}
        //}

        //internal bool IsByRefLike {
        //	get {
        //		return (RareFlags & EETypeRareFlags.IsByRefLikeFlag) != 0;
        //	}
        //}

        //internal bool IsDynamicType {
        //	get {
        //		return (_usFlags & (ushort)EETypeFlags.IsDynamicTypeFlag) != 0;
        //	}
        //}

        //internal bool HasDynamicallyAllocatedDispatchMap {
        //	get {
        //		return (RareFlags & EETypeRareFlags.HasDynamicallyAllocatedDispatchMapFlag) != 0;
        //	}
        //}

        //internal bool IsParameterizedType {
        //	get {
        //		return Kind == EETypeKind.ParameterizedEEType;
        //	}
        //}

        //// The parameterized type shape defines the particular form of parameterized type that
        //// is being represented.
        //// Currently, the meaning is a shape of 0 indicates that this is a Pointer,
        //// shape of 1 indicates a ByRef, and >=SZARRAY_BASE_SIZE indicates that this is an array.
        //// Two types are not equivalent if their shapes do not exactly match.
        //internal uint ParameterizedTypeShape {
        //	get {
        //		return _uBaseSize;
        //	}
        //}

        //internal bool IsRelatedTypeViaIAT {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.RelatedTypeViaIATFlag) != 0);
        //	}
        //}

        //internal bool RequiresAlign8 {
        //	get {
        //		return (RareFlags & EETypeRareFlags.RequiresAlign8Flag) != 0;
        //	}
        //}

        //internal bool IsICastable {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.ICastableFlag) != 0);
        //	}
        //}

        ///// <summary>
        ///// Gets the pointer to the method that implements ICastable.IsInstanceOfInterface.
        ///// </summary>
        //internal IntPtr ICastableIsInstanceOfInterfaceMethod {
        //	get {
        //		Debug.Assert(IsICastable);

        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields != null) {
        //			const ushort NoSlot = 0xFFFF;
        //			ushort uiSlot = (ushort)OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ICastableIsInstSlot, NoSlot);
        //			if (uiSlot != NoSlot) {
        //				if (uiSlot < NumVtableSlots)
        //					return GetVTableStartAddress()[uiSlot];
        //				else
        //					return GetSealedVirtualSlot((ushort)(uiSlot - NumVtableSlots));
        //			}
        //		}

        //		EEType* baseType = BaseType;
        //		if (baseType != null)
        //			return baseType->ICastableIsInstanceOfInterfaceMethod;

        //		Debug.Assert(false);
        //		return IntPtr.Zero;
        //	}
        //}

        ///// <summary>
        ///// Gets the pointer to the method that implements ICastable.GetImplType.
        ///// </summary>
        //internal IntPtr ICastableGetImplTypeMethod {
        //	get {
        //		Debug.Assert(IsICastable);

        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields != null) {
        //			const ushort NoSlot = 0xFFFF;
        //			ushort uiSlot = (ushort)OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ICastableGetImplTypeSlot, NoSlot);
        //			if (uiSlot != NoSlot) {
        //				if (uiSlot < NumVtableSlots)
        //					return GetVTableStartAddress()[uiSlot];
        //				else
        //					return GetSealedVirtualSlot((ushort)(uiSlot - NumVtableSlots));
        //			}
        //		}

        //		EEType* baseType = BaseType;
        //		if (baseType != null)
        //			return baseType->ICastableGetImplTypeMethod;

        //		Debug.Assert(false);
        //		return IntPtr.Zero;
        //	}
        //}

        //internal bool IsValueType {
        //	get {
        //		return ElementType < EETypeElementType.Class;
        //	}
        //}

        //internal bool HasGCPointers {
        //	get {
        //		return ((_usFlags & (ushort)EETypeFlags.HasPointersFlag) != 0);
        //	}
        //}

        //internal bool IsHFA {
        //	get {
        //		return (RareFlags & EETypeRareFlags.IsHFAFlag) != 0;
        //	}
        //}

        //internal uint ValueTypeFieldPadding {
        //	get {
        //		byte* optionalFields = OptionalFieldsPtr;

        //		// If there are no optional fields then the padding must have been the default, 0.
        //		if (optionalFields == null)
        //			return 0;

        //		// Get the value from the optional fields. The default is zero if that particular field was not included.
        //		// The low bits of this field is the ValueType field padding, the rest of the byte is the alignment if present
        //		uint ValueTypeFieldPaddingData = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ValueTypeFieldPadding, 0);
        //		uint padding = ValueTypeFieldPaddingData & ValueTypePaddingLowMask;
        //		// If there is additional padding, the other bits have that data
        //		padding |= (ValueTypeFieldPaddingData & ValueTypePaddingHighMask) >> (ValueTypePaddingHighShift - ValueTypePaddingAlignmentShift);
        //		return padding;
        //	}
        //}

        //internal uint ValueTypeSize {
        //	get {
        //		Debug.Assert(IsValueType);
        //		// get_BaseSize returns the GC size including space for the sync block index field, the EEType* and
        //		// padding for GC heap alignment. Must subtract all of these to get the size used for locals, array
        //		// elements or fields of another type.
        //		return BaseSize - ((uint)sizeof(ObjHeader) + (uint)sizeof(EEType*) + ValueTypeFieldPadding);
        //	}
        //}

        //internal uint FieldByteCountNonGCAligned {
        //	get {
        //		// This api is designed to return correct results for EETypes which can be derived from
        //		// And results indistinguishable from correct for DefTypes which cannot be derived from (sealed classes)
        //		// (For sealed classes, this should always return BaseSize-((uint)sizeof(ObjHeader));
        //		Debug.Assert(!IsInterface && !IsParameterizedType);

        //		// get_BaseSize returns the GC size including space for the sync block index field, the EEType* and
        //		// padding for GC heap alignment. Must subtract all of these to get the size used for the fields of
        //		// the type (where the fields of the type includes the EEType*)
        //		return BaseSize - ((uint)sizeof(ObjHeader) + ValueTypeFieldPadding);
        //	}
        //}

        //internal EEInterfaceInfo* InterfaceMap {
        //	get {
        //		fixed (EEType* start = &this) {
        //			// interface info table starts after the vtable and has _usNumInterfaces entries
        //			return (EEInterfaceInfo*)((byte*)start + sizeof(EEType) + sizeof(void*) * _usNumVtableSlots);
        //		}
        //	}
        //}

        //internal bool HasDispatchMap {
        //	get {
        //		if (NumInterfaces == 0)
        //			return false;
        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields == null)
        //			return false;
        //		uint idxDispatchMap = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.DispatchMap, 0xffffffff);
        //		if (idxDispatchMap == 0xffffffff) {
        //			if (HasDynamicallyAllocatedDispatchMap)
        //				return true;
        //			else if (IsDynamicType)
        //				return DynamicTemplateType->HasDispatchMap;
        //			return false;
        //		}
        //		return true;
        //	}
        //}

        //internal DispatchMap* DispatchMap {
        //	get {
        //		if (NumInterfaces == 0)
        //			return null;
        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields == null)
        //			return null;
        //		uint idxDispatchMap = OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.DispatchMap, 0xffffffff);
        //		if (idxDispatchMap == 0xffffffff && IsDynamicType) {
        //			if (HasDynamicallyAllocatedDispatchMap) {
        //				fixed (EEType* pThis = &this)
        //					return *(DispatchMap**)((byte*)pThis + GetFieldOffset(EETypeField.ETF_DynamicDispatchMap));
        //			}
        //			else
        //				return DynamicTemplateType->DispatchMap;
        //		}

        //		return ((DispatchMap**)TypeManager.DispatchMap)[idxDispatchMap];
        //	}
        //}

        //// Get the address of the finalizer method for finalizable types.
        //internal IntPtr FinalizerCode {
        //	get {
        //		Debug.Assert(IsFinalizable);

        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<Pointer>(EETypeField.ETF_Finalizer).Value;

        //		return GetField<RelativePointer>(EETypeField.ETF_Finalizer).Value;
        //	}
        //}

        //internal EEType* BaseType {
        //	get {
        //		if (IsCloned) {
        //			return CanonicalEEType->BaseType;
        //		}

        //		if (IsParameterizedType) {
        //			if (IsArray)
        //				return GetArrayEEType();
        //			else
        //				return null;
        //		}

        //		Debug.Assert(IsCanonical);

        //		if (IsRelatedTypeViaIAT)
        //			return *_relatedType._ppBaseTypeViaIAT;
        //		else
        //			return _relatedType._pBaseType;
        //	}
        //}

        //internal EEType* NonArrayBaseType {
        //	get {
        //		Debug.Assert(!IsArray, "array type not supported in BaseType");

        //		if (IsCloned) {
        //			// Assuming that since this is not an Array, the CanonicalEEType is also not an array
        //			return CanonicalEEType->NonArrayBaseType;
        //		}

        //		Debug.Assert(IsCanonical, "we expect canonical types here");

        //		if (IsRelatedTypeViaIAT) {
        //			return *_relatedType._ppBaseTypeViaIAT;
        //		}

        //		return _relatedType._pBaseType;
        //	}
        //}

        //internal EEType* NonClonedNonArrayBaseType {
        //	get {
        //		Debug.Assert(!IsArray, "array type not supported in NonArrayBaseType");
        //		Debug.Assert(!IsCloned, "cloned type not supported in NonClonedNonArrayBaseType");
        //		Debug.Assert(IsCanonical || IsGenericTypeDefinition, "we expect canonical types here");

        //		if (IsRelatedTypeViaIAT) {
        //			return *_relatedType._ppBaseTypeViaIAT;
        //		}

        //		return _relatedType._pBaseType;
        //	}
        //}

        internal EEType* RawBaseType
        {
            get
            {
                //Debug.Assert(!IsParameterizedType, "array type not supported in NonArrayBaseType");
                //Debug.Assert(!IsCloned, "cloned type not supported in NonClonedNonArrayBaseType");
                //Debug.Assert(IsCanonical, "we expect canonical types here");
                //Debug.Assert(!IsRelatedTypeViaIAT, "Non IAT");

                return _relatedType._pBaseType;
            }
        }

        //internal EEType* CanonicalEEType {
        //	get {
        //		// cloned EETypes must always refer to types in other modules
        //		Debug.Assert(IsCloned);
        //		if (IsRelatedTypeViaIAT)
        //			return *_relatedType._ppCanonicalTypeViaIAT;
        //		else
        //			return _relatedType._pCanonicalType;
        //	}
        //}

        //internal EEType* NullableType {
        //	get {
        //		Debug.Assert(IsNullable);
        //		Debug.Assert(GenericArity == 1);
        //		return GenericArguments[0].Value;
        //	}
        //}

        ///// <summary>
        ///// Gets the offset of the value embedded in a Nullable&lt;T&gt;.
        ///// </summary>
        //internal byte NullableValueOffset {
        //	get {
        //		Debug.Assert(IsNullable);

        //		// Grab optional fields. If there aren't any then the offset was the default of 1 (immediately after the
        //		// Nullable's boolean flag).
        //		byte* optionalFields = OptionalFieldsPtr;
        //		if (optionalFields == null)
        //			return 1;

        //		// The offset is never zero (Nullable has a boolean there indicating whether the value is valid). So the
        //		// offset is encoded - 1 to save space. The zero below is the default value if the field wasn't encoded at
        //		// all.
        //		return (byte)(OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.NullableValueOffset, 0) + 1);
        //	}
        //}

        //internal EEType* RelatedParameterType {
        //	get {
        //		Debug.Assert(IsParameterizedType);

        //		if (IsRelatedTypeViaIAT)
        //			return *_relatedType._ppRelatedParameterTypeViaIAT;
        //		else
        //			return _relatedType._pRelatedParameterType;
        //	}
        //}

        //internal unsafe IntPtr* GetVTableStartAddress() {
        //	byte* pResult;

        //	// EETypes are always in unmanaged memory, so 'leaking' the 'fixed pointer' is safe.
        //	fixed (EEType* pThis = &this)
        //		pResult = (byte*)pThis;

        //	pResult += sizeof(EEType);
        //	return (IntPtr*)pResult;
        //}

        //private static IntPtr FollowRelativePointer(int* pDist) {
        //	int dist = *pDist;
        //	IntPtr result = (IntPtr)((byte*)pDist + dist);
        //	return result;
        //}

        //internal IntPtr GetSealedVirtualSlot(ushort slotNumber) {
        //	Debug.Assert((RareFlags & EETypeRareFlags.HasSealedVTableEntriesFlag) != 0);

        //	fixed (EEType* pThis = &this) {
        //		if (IsDynamicType || !SupportsRelativePointers) {
        //			uint cbSealedVirtualSlotsTypeOffset = GetFieldOffset(EETypeField.ETF_SealedVirtualSlots);
        //			IntPtr* pSealedVirtualsSlotTable = *(IntPtr**)((byte*)pThis + cbSealedVirtualSlotsTypeOffset);
        //			return pSealedVirtualsSlotTable[slotNumber];
        //		}
        //		else {
        //			uint cbSealedVirtualSlotsTypeOffset = GetFieldOffset(EETypeField.ETF_SealedVirtualSlots);
        //			int* pSealedVirtualsSlotTable = (int*)FollowRelativePointer((int*)((byte*)pThis + cbSealedVirtualSlotsTypeOffset));
        //			IntPtr result = FollowRelativePointer(&pSealedVirtualsSlotTable[slotNumber]);
        //			return result;
        //		}
        //	}
        //}

        //internal byte* OptionalFieldsPtr {
        //	get {
        //		if (!HasOptionalFields)
        //			return null;

        //		if (IsDynamicType || !SupportsRelativePointers)
        //			return GetField<Pointer<byte>>(EETypeField.ETF_OptionalFieldsPtr).Value;

        //		return GetField<RelativePointer<byte>>(EETypeField.ETF_OptionalFieldsPtr).Value;
        //	}
        //}

        //internal EEType* DynamicTemplateType {
        //	get {
        //		Debug.Assert(IsDynamicType);
        //		uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicTemplateType);
        //		fixed (EEType* pThis = &this) {
        //			return *(EEType**)((byte*)pThis + cbOffset);
        //		}
        //	}
        //}

        //internal IntPtr DynamicGcStaticsData {
        //	get {
        //		Debug.Assert((RareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0);
        //		uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicGcStatics);
        //		fixed (EEType* pThis = &this) {
        //			return (IntPtr)((byte*)pThis + cbOffset);
        //		}
        //	}
        //}

        //internal IntPtr DynamicNonGcStaticsData {
        //	get {
        //		Debug.Assert((RareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0);
        //		uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicNonGcStatics);
        //		fixed (EEType* pThis = &this) {
        //			return (IntPtr)((byte*)pThis + cbOffset);
        //		}
        //	}
        //}

        //internal DynamicModule* DynamicModule {
        //	get {
        //		if ((RareFlags & EETypeRareFlags.HasDynamicModuleFlag) != 0) {
        //			uint cbOffset = GetFieldOffset(EETypeField.ETF_DynamicModule);
        //			fixed (EEType* pThis = &this) {
        //				return *(DynamicModule**)((byte*)pThis + cbOffset);
        //			}
        //		}
        //		else {
        //			return null;
        //		}
        //	}
        //}

        //internal TypeManagerHandle TypeManager {
        //	get {
        //		IntPtr typeManagerIndirection;
        //		if (IsDynamicType || !SupportsRelativePointers)
        //			typeManagerIndirection = GetField<Pointer>(EETypeField.ETF_TypeManagerIndirection).Value;
        //		else
        //			typeManagerIndirection = GetField<RelativePointer>(EETypeField.ETF_TypeManagerIndirection).Value;

        //		return *(TypeManagerHandle*)typeManagerIndirection;
        //	}
        //}

        //internal unsafe EETypeRareFlags RareFlags {
        //	get {
        //		// If there are no optional fields then none of the rare flags have been set.
        //		// Get the flags from the optional fields. The default is zero if that particular field was not included.
        //		return HasOptionalFields ? (EETypeRareFlags)OptionalFieldsReader.GetInlineField(OptionalFieldsPtr, EETypeOptionalFieldTag.RareFlags, 0) : 0;
        //	}
        //}

        //internal int FieldAlignmentRequirement {
        //	get {
        //		byte* optionalFields = OptionalFieldsPtr;

        //		// If there are no optional fields then the alignment must have been the default, IntPtr.Size. 
        //		// (This happens for all reference types, and for valuetypes with default alignment and no padding)
        //		if (optionalFields == null)
        //			return IntPtr.Size;

        //		// Get the value from the optional fields. The default is zero if that particular field was not included.
        //		// The low bits of this field is the ValueType field padding, the rest of the value is the alignment if present
        //		uint alignmentValue = (OptionalFieldsReader.GetInlineField(optionalFields, EETypeOptionalFieldTag.ValueTypeFieldPadding, 0) & ValueTypePaddingAlignmentMask) >> ValueTypePaddingAlignmentShift;

        //		// Alignment is stored as 1 + the log base 2 of the alignment, except a 0 indicates standard pointer alignment.
        //		if (alignmentValue == 0)
        //			return IntPtr.Size;
        //		else
        //			return 1 << ((int)alignmentValue - 1);
        //	}
        //}

        internal EETypeElementType ElementType
        {
            get
            {
                return (EETypeElementType)((_usFlags & (ushort)EETypeFlags.ElementTypeMask) >> (ushort)EETypeFlags.ElementTypeShift);
            }
        }

        //public bool HasCctor {
        //	get {
        //		return (RareFlags & EETypeRareFlags.HasCctorFlag) != 0;
        //	}
        //}

        //public uint GetFieldOffset(EETypeField eField) {
        //	// First part of EEType consists of the fixed portion followed by the vtable.
        //	uint cbOffset = (uint)(sizeof(EEType) + (IntPtr.Size * _usNumVtableSlots));

        //	// Then we have the interface map.
        //	if (eField == EETypeField.ETF_InterfaceMap) {
        //		Debug.Assert(NumInterfaces > 0);
        //		return cbOffset;
        //	}
        //	cbOffset += (uint)(sizeof(EEInterfaceInfo) * NumInterfaces);

        //	uint relativeOrFullPointerOffset = (IsDynamicType || !SupportsRelativePointers ? (uint)IntPtr.Size : 4);

        //	// Followed by the type manager indirection cell.
        //	if (eField == EETypeField.ETF_TypeManagerIndirection) {
        //		return cbOffset;
        //	}
        //	cbOffset += relativeOrFullPointerOffset;

        //	// Followed by the pointer to the finalizer method.
        //	if (eField == EETypeField.ETF_Finalizer) {
        //		Debug.Assert(IsFinalizable);
        //		return cbOffset;
        //	}
        //	if (IsFinalizable)
        //		cbOffset += relativeOrFullPointerOffset;

        //	// Followed by the pointer to the optional fields.
        //	if (eField == EETypeField.ETF_OptionalFieldsPtr) {
        //		Debug.Assert(HasOptionalFields);
        //		return cbOffset;
        //	}
        //	if (HasOptionalFields)
        //		cbOffset += relativeOrFullPointerOffset;

        //	// Followed by the pointer to the sealed virtual slots
        //	if (eField == EETypeField.ETF_SealedVirtualSlots)
        //		return cbOffset;

        //	EETypeRareFlags rareFlags = RareFlags;

        //	// in the case of sealed vtable entries on static types, we have a UInt sized relative pointer
        //	if ((rareFlags & EETypeRareFlags.HasSealedVTableEntriesFlag) != 0)
        //		cbOffset += relativeOrFullPointerOffset;

        //	if (eField == EETypeField.ETF_DynamicDispatchMap) {
        //		Debug.Assert(IsDynamicType);
        //		return cbOffset;
        //	}
        //	if ((rareFlags & EETypeRareFlags.HasDynamicallyAllocatedDispatchMapFlag) != 0)
        //		cbOffset += (uint)IntPtr.Size;

        //	if (eField == EETypeField.ETF_GenericDefinition) {
        //		Debug.Assert(IsGeneric);
        //		return cbOffset;
        //	}
        //	if (IsGeneric) {
        //		cbOffset += relativeOrFullPointerOffset;
        //	}

        //	if (eField == EETypeField.ETF_GenericComposition) {
        //		Debug.Assert(IsGeneric);
        //		return cbOffset;
        //	}
        //	if (IsGeneric) {
        //		cbOffset += relativeOrFullPointerOffset;
        //	}

        //	if (eField == EETypeField.ETF_DynamicModule) {
        //		return cbOffset;
        //	}

        //	if ((rareFlags & EETypeRareFlags.HasDynamicModuleFlag) != 0)
        //		cbOffset += (uint)IntPtr.Size;

        //	if (eField == EETypeField.ETF_DynamicTemplateType) {
        //		Debug.Assert(IsDynamicType);
        //		return cbOffset;
        //	}
        //	if (IsDynamicType)
        //		cbOffset += (uint)IntPtr.Size;

        //	if (eField == EETypeField.ETF_DynamicGcStatics) {
        //		Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0);
        //		return cbOffset;
        //	}
        //	if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithGcStatics) != 0)
        //		cbOffset += (uint)IntPtr.Size;

        //	if (eField == EETypeField.ETF_DynamicNonGcStatics) {
        //		Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0);
        //		return cbOffset;
        //	}
        //	if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithNonGcStatics) != 0)
        //		cbOffset += (uint)IntPtr.Size;

        //	if (eField == EETypeField.ETF_DynamicThreadStaticOffset) {
        //		Debug.Assert((rareFlags & EETypeRareFlags.IsDynamicTypeWithThreadStatics) != 0);
        //		return cbOffset;
        //	}
        //	if ((rareFlags & EETypeRareFlags.IsDynamicTypeWithThreadStatics) != 0)
        //		cbOffset += 4;

        //	Debug.Assert(false, "Unknown EEType field type");
        //	return 0;
        //}

        //public ref T GetField<T>(EETypeField eField) {
        //	fixed (EEType* pThis = &this)
        //		return ref Unsafe.AddByteOffset(ref Unsafe.As<EEType, T>(ref *pThis), (IntPtr)GetFieldOffset(eField));
        //}
    }

    //// Wrapper around EEType pointers that may be indirected through the IAT if their low bit is set.
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe struct EETypeRef {
    //	private byte* _value;

    //	public EEType* Value {
    //		get {
    //			if (((int)_value & IndirectionConstants.IndirectionCellPointer) == 0)
    //				return (EEType*)_value;
    //			return *(EEType**)(_value - IndirectionConstants.IndirectionCellPointer);
    //		}
    //	}
    //}

    //// Wrapper around pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal readonly struct Pointer {
    //	private readonly IntPtr _value;

    //	public IntPtr Value {
    //		get {
    //			return _value;
    //		}
    //	}
    //}

    //// Wrapper around pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct Pointer<T> where T : unmanaged {
    //	private readonly T* _value;

    //	public T* Value {
    //		get {
    //			return _value;
    //		}
    //	}
    //}

    //// Wrapper around pointers that might be indirected through IAT
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct IatAwarePointer<T> where T : unmanaged {
    //	private readonly T* _value;

    //	public T* Value {
    //		get {
    //			if (((int)_value & IndirectionConstants.IndirectionCellPointer) == 0)
    //				return _value;
    //			return *(T**)((byte*)_value - IndirectionConstants.IndirectionCellPointer);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal readonly struct RelativePointer {
    //	private readonly int _value;

    //	public unsafe IntPtr Value {
    //		get {
    //			return (IntPtr)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct RelativePointer<T> where T : unmanaged {
    //	private readonly int _value;

    //	public T* Value {
    //		get {
    //			return (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //		}
    //	}
    //}

    //// Wrapper around relative pointers that might be indirected through IAT
    //[StructLayout(LayoutKind.Sequential)]
    //internal unsafe readonly struct IatAwareRelativePointer<T> where T : unmanaged {
    //	private readonly int _value;

    //	public T* Value {
    //		get {
    //			if ((_value & IndirectionConstants.IndirectionCellPointer) == 0) {
    //				return (T*)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + _value);
    //			}
    //			else {
    //				return *(T**)((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in _value)) + (_value & ~IndirectionConstants.IndirectionCellPointer));
    //			}
    //		}
    //	}
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //internal struct DynamicModule {
    //	// Size field used to indicate the number of bytes of this structure that are defined in Runtime Known ways
    //	// This is used to drive versioning of this field
    //	private int _cbSize;

    //	// Pointer to interface dispatch resolver that works off of a type/slot pair
    //	// This is a function pointer with the following signature IntPtr()(IntPtr targetType, IntPtr interfaceType, ushort slot)
    //	private IntPtr _dynamicTypeSlotDispatchResolve;

    //	// Starting address for the the binary module corresponding to this dynamic module.
    //	private IntPtr _getRuntimeException;

    //	public IntPtr DynamicTypeSlotDispatchResolve {
    //		get {
    //			unsafe {
    //				if (_cbSize >= sizeof(IntPtr) * 2) {
    //					return _dynamicTypeSlotDispatchResolve;
    //				}
    //				else {
    //					return IntPtr.Zero;
    //				}
    //			}
    //		}
    //	}

    //	public IntPtr GetRuntimeException {
    //		get {
    //			unsafe {
    //				if (_cbSize >= sizeof(IntPtr) * 3) {
    //					return _getRuntimeException;
    //				}
    //				else {
    //					return IntPtr.Zero;
    //				}
    //			}
    //		}
    //	}
    //}
}
