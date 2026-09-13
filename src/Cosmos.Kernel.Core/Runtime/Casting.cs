// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime;
using System.Runtime.CompilerServices;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

internal static unsafe class Casting
{
    [RuntimeExport("RhTypeCast_AreTypesAssignable")]
    public static bool RhTypeCast_AreTypesAssignable(MethodTable* typeHandleSrc, MethodTable* typeHandleDest)
    {
        return typeHandleDest->IsInterface
            ? IsInstanceOfInterface(typeHandleSrc, typeHandleDest)
            : IsInstanceOfClass(typeHandleSrc, typeHandleDest);
    }

    [RuntimeExport("RhTypeCast_IsInstanceOfAny")]
    public static object? RhTypeCast_IsInstanceOfAny(object obj, MethodTable** pTypeHandles, int count)
    {
        if (obj is null)
        {
            return null;
        }

        MethodTable* type = obj.GetMethodTable();
        for (int i = 0; i < count; i++)
        {
            if (RhTypeCast_AreTypesAssignable(type, pTypeHandles[i]))
            {
                return obj;
            }
        }

        return null;
    }

    [RuntimeExport("RhTypeCast_IsInstanceOfInterface")]
    public static bool RhTypeCast_IsInstanceOfInterface(object obj, MethodTable* interfaceTypeHandle)
    {
        if (obj is null)
        {
            return false;
        }

        MethodTable* type = obj.GetMethodTable();
        return IsInstanceOfInterface(type, interfaceTypeHandle);
    }


    // Essential runtime functions needed by the linker
    [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
    public static object? RhTypeCast_IsInstanceOfClass(object obj, MethodTable* classTypeHandle)
    {
        MethodTable* type = obj.GetMethodTable();
        return IsInstanceOfClass(type, classTypeHandle) ? obj : null;
    }

    [RuntimeExport("RhTypeCast_CheckCastInterface")]
    public static object? RhTypeCast_CheckCastInterface(object obj, MethodTable* interfaceTypeHandle)
    {
        if (obj is null)
        {
            return null;
        }

        return !RhTypeCast_IsInstanceOfInterface(obj, interfaceTypeHandle)
            ? throw new InvalidCastException()
            : obj;
    }

    [RuntimeExport("RhTypeCast_CheckCastClass")]
    public static object RhTypeCast_CheckCastClass(object obj, MethodTable* typeHandle)
    {
        return RhTypeCast_IsInstanceOfClass(obj, typeHandle) ?? throw new InvalidCastException();
    }

    [RuntimeExport("RhTypeCast_CheckCastClassSpecial")]
    internal static object? RhTypeCast_CheckCastClassSpecial(object obj, MethodTable* typeHandle, bool fThrow)
    {
        if (obj is null)
        {
            return null;
        }

        if (IsInstanceOfClass(obj.GetMethodTable(), typeHandle))
        {
            return obj;
        }

        return fThrow ? throw new InvalidCastException() : null;
    }

    [RuntimeExport("RhTypeCast_CheckCastAny")]
    internal static unsafe object RhTypeCast_CheckCastAny(object obj, MethodTable* typeHandle)
    {
        return obj;
    }

    [RuntimeExport("RhTypeCast_CheckArrayStore")]
    internal static void CheckArrayStore(object array, object obj)
    {
        __CheckArrayStore(null!, array, obj);
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "CheckArrayStore")]
    private static extern void __CheckArrayStore([UnsafeAccessorType("System.Runtime.TypeCast")] object @this, object array, object obj);

    private static bool IsInstanceOfInterface(MethodTable* type, MethodTable* interfaceType)
    {
        while (type != null)
        {
            for (int i = 0; i < type->NumInterfaces; i++)
            {
                MethodTable* interfaceImpl = type->InterfaceMap[i];
                if (interfaceImpl != interfaceType)
                {
                    continue;
                }

                // Only check generics if the interface is actually generic
                if (!interfaceType->IsGeneric)
                {
                    return true; // Not generic, exact match is sufficient
                }

                return AreGenericsAssignable(interfaceImpl, interfaceType);
            }

            type = type->BaseType;
        }

        return false;
    }

    private static bool IsInstanceOfClass(MethodTable* type, MethodTable* classType)
    {
        while (type != null)
        {
            if (type != classType)
            {
                type = type->BaseType;
                continue;
            }

            // Types match - check if we need to verify generics
            // Only check generics if classType is actually a generic type
            if (!classType->IsGeneric)
            {
                return true; // Not generic, exact match is sufficient
            }

            return AreGenericsAssignable(type, classType); // Check generics
        }

        return false;
    }

    private static bool AreGenericsAssignable(MethodTable* sourceType, MethodTable* targetType)
    {
        // Get arity from GenericArity (for instantiated generics) not GenericParameterCount (for definitions)
        int arity = (int)targetType->GenericArity;
        for (int i = 0; i < arity; i++)
        {
            MethodTable* sourceGeneric = sourceType->GenericArguments[i]; // Generic of the cast target
            MethodTable* targetGeneric = targetType->GenericArguments[i]; // Generic of the cast type;

            if (sourceGeneric == null || targetGeneric == null)
            {
                return false;
            }

            if (!targetGeneric->HasGenericVariance)
            {
                return sourceGeneric == targetGeneric; // Nonvariant generic, check if they are the same
            }

            GenericVariance targetGenericVariance = targetType->GenericVariance[i];
            bool assignable = targetGenericVariance == GenericVariance.Covariant &&
                              RhTypeCast_AreTypesAssignable(sourceGeneric, targetGeneric) ||
                              targetGenericVariance == GenericVariance.Contravariant &&
                              RhTypeCast_AreTypesAssignable(targetGeneric, sourceGeneric) ||
                              (targetGeneric->IsArray && sourceGeneric->IsArray) &&
                              RhTypeCast_AreTypesAssignable(sourceGeneric->RelatedParameterType,
                                  targetGeneric->RelatedParameterType); // Array covariance

            if (!assignable)
            {
                return false;
            }
        }

        return true;
    }
}
