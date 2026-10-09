// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime
{
    /// <summary>
    /// Generic variance rules for interface dispatch and casting: an object implementing
    /// <c>IEnumerator&lt;Derived&gt;</c> is an <c>IEnumerator&lt;Base&gt;</c>, an
    /// <c>IComparer&lt;object&gt;</c> is an <c>IComparer&lt;string&gt;</c>, a <c>string[]</c> is an
    /// <c>IList&lt;object&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Ported from NativeAOT's Runtime.Base <c>System.Runtime.TypeCast</c> (TypeParametersAreCompatible,
    /// AreTypesAssignableInternalUncached and their helpers), without its cast cache.
    /// </remarks>
    internal static unsafe class TypeVariance
    {
        internal enum AssignmentVariation
        {
            /// <summary>Conversion from an object (castclass, isinst): value types are compatible with
            /// Object, ValueType, Enum and their interfaces.</summary>
            BoxedSource = 0,

            /// <summary>Compatibility of unboxed types (type arguments): value types only when identical.</summary>
            Unboxed = 1,

            /// <summary>Same-sized integral types and enums are equivalent (array element types).</summary>
            AllowSizeEquivalence = 2,
        }

        /// <summary>The type pairs being compared up the stack, which ends self-dependent cycles.</summary>
        internal struct TypePairList
        {
            private readonly MethodTable* _type1;
            private readonly MethodTable* _type2;
            private readonly TypePairList* _next;

            public TypePairList(MethodTable* type1, MethodTable* type2, TypePairList* next)
            {
                _type1 = type1;
                _type2 = type2;
                _next = next;
            }

            public static bool Exists(TypePairList* list, MethodTable* type1, MethodTable* type2)
            {
                while (list != null)
                {
                    if ((list->_type1 == type1 && list->_type2 == type2) || (list->_type1 == type2 && list->_type2 == type1))
                    {
                        return true;
                    }

                    list = list->_next;
                }

                return false;
            }
        }

        /// <summary>
        /// Whether an object of <paramref name="pObjType"/> is a <paramref name="pTargetType"/>
        /// interface: one in its interface map, or a variant instantiation of one.
        /// </summary>
        internal static bool ImplementsInterface(MethodTable* pObjType, MethodTable* pTargetType, TypePairList* pVisited)
        {
            int numInterfaces = pObjType->NumInterfaces;
            MethodTable** interfaceMap = pObjType->InterfaceMap;

            for (int i = 0; i < numInterfaces; i++)
            {
                if (interfaceMap[i] == pTargetType)
                {
                    return true;
                }
            }

            // Not in the map: the object can still match when the target interface has a co- or
            // contravariant type parameter and the object implements another instantiation of it with
            // compatible type arguments. Interfaces which are only variant for arrays have the
            // HasGenericVariance flag set too.
            if (!pTargetType->HasGenericVariance)
            {
                return false;
            }

            bool fArrayCovariance = pObjType->IsArray;
            MethodTable* pTargetGenericType = pTargetType->GenericDefinition;
            MethodTableList targetInstantiation = pTargetType->GenericArguments;
            int targetArity = (int)pTargetType->GenericArity;
            GenericVariance* pTargetVarianceInfo = pTargetType->GenericVariance;

            for (int i = 0; i < numInterfaces; i++)
            {
                MethodTable* pInterfaceType = interfaceMap[i];

                if (!pInterfaceType->HasGenericVariance || pInterfaceType->GenericDefinition != pTargetGenericType)
                {
                    continue;
                }

                if (TypeParametersAreCompatible(targetArity, pInterfaceType->GenericArguments, targetInstantiation,
                        pTargetVarianceInfo, fArrayCovariance, pVisited))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether two sets of type arguments of the same generic definition are assignment compatible,
        /// per <paramref name="pVarianceInfo"/>. <paramref name="fForceCovariance"/> treats every parameter
        /// as array covariant (the generic interfaces of arrays).
        /// </summary>
        internal static bool TypeParametersAreCompatible(int arity,
                                                         MethodTableList sourceInstantiation,
                                                         MethodTableList targetInstantiation,
                                                         GenericVariance* pVarianceInfo,
                                                         bool fForceCovariance,
                                                         TypePairList* pVisited)
        {
            for (int i = 0; i < arity; i++)
            {
                MethodTable* pTargetArgType = targetInstantiation[i];
                MethodTable* pSourceArgType = sourceInstantiation[i];

                GenericVariance varType = fForceCovariance ? GenericVariance.ArrayCovariant : pVarianceInfo[i];

                switch (varType)
                {
                    case GenericVariance.NonVariant:
                        if (pSourceArgType != pTargetArgType)
                        {
                            return false;
                        }

                        break;

                    case GenericVariance.Covariant:
                        // class Foo : ICovariant<string> is ICovariant<object>
                        if (!AreTypesAssignableInternal(pSourceArgType, pTargetArgType, AssignmentVariation.Unboxed, pVisited))
                        {
                            return false;
                        }

                        break;

                    case GenericVariance.ArrayCovariant:
                        // string[,,] is object[,,]; int[,,] is uint[,,]
                        if (!AreTypesAssignableInternal(pSourceArgType, pTargetArgType, AssignmentVariation.AllowSizeEquivalence, pVisited))
                        {
                            return false;
                        }

                        break;

                    case GenericVariance.Contravariant:
                        // class Foo : IContravariant<object> is IContravariant<string>
                        if (!AreTypesAssignableInternal(pTargetArgType, pSourceArgType, AssignmentVariation.Unboxed, pVisited))
                        {
                            return false;
                        }

                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether a value of <paramref name="pSourceType"/> can be stored in a location of
        /// <paramref name="pTargetType"/>, under <paramref name="variation"/>.
        /// </summary>
        internal static bool AreTypesAssignableInternal(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, TypePairList* pVisited)
        {
            // Also what ends recursion over identical types.
            if (pSourceType == pTargetType)
            {
                return true;
            }

            if (TypePairList.Exists(pVisited, pSourceType, pTargetType))
            {
                return false;
            }

            TypePairList visited = new TypePairList(pSourceType, pTargetType, pVisited);
            return AreTypesAssignableUncached(pSourceType, pTargetType, variation, &visited);
        }

        private static bool AreTypesAssignableUncached(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, TypePairList* pVisited)
        {
            bool fBoxedSource = variation == AssignmentVariation.BoxedSource;
            bool fAllowSizeEquivalence = (variation & AssignmentVariation.AllowSizeEquivalence) == AssignmentVariation.AllowSizeEquivalence;

            if (pTargetType->IsInterface)
            {
                // Value types can only be cast to interfaces when boxed.
                if (!fBoxedSource && pSourceType->IsValueType)
                {
                    return false;
                }

                if (ImplementsInterface(pSourceType, pTargetType, pVisited))
                {
                    return true;
                }

                // An interface source: compatible through generic variance only.
                if (pTargetType->HasGenericVariance && pSourceType->HasGenericVariance)
                {
                    return TypesAreCompatibleViaGenericVariance(pSourceType, pTargetType, pVisited);
                }

                return false;
            }

            if (pSourceType->IsInterface)
            {
                // The only non-interface type an interface can be cast to is Object.
                return IsSystemObject(pTargetType);
            }

            if (pTargetType->IsParameterizedType)
            {
                if (pSourceType->IsParameterizedType && pTargetType->ParameterizedTypeShape == pSourceType->ParameterizedTypeShape)
                {
                    MethodTable* pSourceRelatedParameterType = pSourceType->RelatedParameterType;

                    // Pointers, byrefs and function pointers match exactly only, and the identical case
                    // was handled above.
                    if (pSourceRelatedParameterType->IsPointer || pSourceRelatedParameterType->IsByRef || pSourceRelatedParameterType->IsFunctionPointer)
                    {
                        return false;
                    }

                    // Array covariance, and IFoo[] -> Foo[]; int[] is not object[], hence not BoxedSource.
                    return AreTypesAssignableInternal(pSourceRelatedParameterType, pTargetType->RelatedParameterType,
                        AssignmentVariation.AllowSizeEquivalence, pVisited);
                }

                return false;
            }

            if (pTargetType->IsFunctionPointer)
            {
                return false;
            }

            if (pSourceType->IsArray)
            {
                // Arrays are also Object and System.Array.
                return IsValidArrayBaseType(pTargetType);
            }

            if (pSourceType->IsParameterizedType || pSourceType->IsFunctionPointer)
            {
                return false;
            }

            if (pSourceType->IsValueType)
            {
                // Same-sized integers, and enums and their underlying integers, are equivalent as array
                // element types.
                if (fAllowSizeEquivalence && pTargetType->IsPrimitive)
                {
                    return GetNormalizedIntegralArrayElementType(pSourceType) == GetNormalizedIntegralArrayElementType(pTargetType);
                }

                // An unboxed value type is only its own type: value types are sealed.
                if (!fBoxedSource)
                {
                    return false;
                }
            }

            // Two instantiations of one delegate type with variant parameters (only interfaces and
            // delegates have variance, and the target is no interface).
            if (pTargetType->HasGenericVariance && pSourceType->HasGenericVariance)
            {
                return TypesAreCompatibleViaGenericVariance(pSourceType, pTargetType, pVisited);
            }

            return IsDerived(pSourceType, pTargetType);
        }

        private static bool TypesAreCompatibleViaGenericVariance(MethodTable* pSourceType, MethodTable* pTargetType, TypePairList* pVisited)
        {
            if (pSourceType->GenericDefinition != pTargetType->GenericDefinition)
            {
                return false;
            }

            return TypeParametersAreCompatible((int)pTargetType->GenericArity, pSourceType->GenericArguments,
                pTargetType->GenericArguments, pTargetType->GenericVariance, false, pVisited);
        }

        private static bool IsDerived(MethodTable* pDerivedType, MethodTable* pBaseType)
        {
            do
            {
                if (pDerivedType == pBaseType)
                {
                    return true;
                }

                pDerivedType = pDerivedType->NonArrayBaseType;
            }
            while (pDerivedType != null);

            return false;
        }

        private static bool IsSystemObject(MethodTable* pType)
        {
            return pType->IsCanonical && pType->NonArrayBaseType == null && !pType->IsInterface;
        }

        /// <summary>System.Array or System.Object.</summary>
        private static bool IsValidArrayBaseType(MethodTable* pType)
        {
            EETypeElementType elementType = pType->ElementType;
            return elementType == EETypeElementType.SystemArray
                || (elementType == EETypeElementType.Class && pType->NonArrayBaseType == null);
        }

        private static EETypeElementType GetNormalizedIntegralArrayElementType(MethodTable* type)
        {
            EETypeElementType elementType = type->ElementType;
            switch (elementType)
            {
                case EETypeElementType.Byte:
                case EETypeElementType.UInt16:
                case EETypeElementType.UInt32:
                case EETypeElementType.UInt64:
                case EETypeElementType.UIntPtr:
                    return elementType - 1;
            }

            return elementType;
        }
    }
}
