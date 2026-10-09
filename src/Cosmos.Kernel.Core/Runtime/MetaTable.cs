using System.Runtime;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Contains runtime exports for various metadata and dispatch operations
/// </summary>
internal unsafe class MetaTable
{
    [RuntimeExport("RhGetModuleFileName")]
    internal static int RhGetModuleFileName(IntPtr moduleHandle, out byte* moduleName)
    {
        moduleName = (byte*)0x00;
        return 0;
    }

    [RuntimeExport("RhHandleGetDependent")]
    internal static GCObject* RhHandleGetDependent(IntPtr handle, out GCObject* pSecondary)
    {
        GCObject* primary = GarbageCollector.HandleGetPrimary(handle);
        if (primary != null)
        {
            pSecondary = GarbageCollector.HandleGetSecondary(handle);
        }
        else
        {
            pSecondary = null;
        }

        return primary;
    }

    [RuntimeExport("RhHandleSetDependentSecondary")]
    internal static void RhHandleSetDependentSecondary(IntPtr handle, GCObject* pSecondary)
    {
        GarbageCollector.HandleSetSecondary(handle, pSecondary);
    }

    [RuntimeExport("RhGetRuntimeHelperForType")]
    internal static unsafe IntPtr RhGetRuntimeHelperForType(MethodTable* pEEType, RuntimeHelperKind kind)
    {
        switch (kind)
        {
            case RuntimeHelperKind.AllocateObject:
                if (pEEType->IsFinalizable)
                {
                    return (IntPtr)(delegate*<MethodTable*, void*>)&StartupCodeHelpers.RhpNewFinalizable;
                }
                else
                {
                    return (IntPtr)(delegate*<MethodTable*, void*>)&Memory.RhpNewFast;
                }

            case RuntimeHelperKind.IsInst:
                if (pEEType->HasGenericVariance || pEEType->IsParameterizedType || pEEType->IsFunctionPointer)
                {
                    return (IntPtr)(delegate*<object, MethodTable**, int, object?>)&Casting.RhTypeCast_IsInstanceOfAny;
                }
                else if (pEEType->IsInterface)
                {
                    return (IntPtr)(delegate*<object, MethodTable*, bool>)&Casting.RhTypeCast_IsInstanceOfInterface;
                }
                else
                {
                    return (IntPtr)(delegate*<object, MethodTable*, object?>)&Casting.RhTypeCast_IsInstanceOfClass;
                }

            case RuntimeHelperKind.CastClass:
                if (pEEType->HasGenericVariance || pEEType->IsParameterizedType || pEEType->IsFunctionPointer)
                {
                    return (IntPtr)(delegate*<object, MethodTable*, object>)&Casting.RhTypeCast_CheckCastAny;
                }
                else if (pEEType->IsInterface)
                {
                    return (IntPtr)(delegate*<object, MethodTable*, object?>)&Casting.RhTypeCast_CheckCastInterface;
                }
                else
                {
                    return (IntPtr)(delegate*<object, MethodTable*, object>)&Casting.RhTypeCast_CheckCastClass;
                }

            case RuntimeHelperKind.AllocateArray:
                return (IntPtr)(delegate*<MethodTable*, int, void*>)&Memory.RhpNewArrayFast;

            default:
                return IntPtr.Zero;
        }
    }
}
