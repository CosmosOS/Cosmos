using System.Drawing;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;


internal static unsafe class Memory
{
    [RuntimeExport("RhNewArray")]
    internal static unsafe void* RhNewArray(MethodTable* pEEType, int length)
    {
        RhAllocateNewArray(pEEType, (uint)length, 0, out void* result);
        return result;
    }

    [RuntimeExport("RhAllocateNewArray")]
    internal static unsafe void RhAllocateNewArray(MethodTable* pArrayEEType, uint numElements, GC_ALLOC_FLAGS flags,
        out void* pResult)
    {
        uint size = pArrayEEType->BaseSize + numElements * pArrayEEType->ComponentSize;

        GCObject* result = AllocObject(size, flags);

        result->MethodTable = pArrayEEType;
        result->Length = (int)numElements;

        pResult = result;
    }

    [RuntimeExport("RhpNewArray")]
    internal static unsafe void* RhpNewArray(MethodTable* pMT, int length)
    {
        if (length < 0)
        {
            return null;
        }

        uint size = pMT->BaseSize + (uint)length * pMT->ComponentSize;

        GCObject* result = AllocObject(size);

        result->MethodTable = pMT;
        result->Length = length;

        return result;
    }

    [RuntimeExport("RhpNewPtrArrayFast")]
    internal static unsafe void* RhpNewPtrArrayFast(MethodTable* pMT, int length)
    {
        if (length < 0)
        {
            return null;
        }

        uint size = pMT->BaseSize + (uint)length * pMT->ComponentSize;

        GCObject* result = AllocObject(size);

        result->MethodTable = pMT;
        result->Length = length;

        return result;
    }
    [RuntimeExport("RhpNewArrayFast")]
    internal static unsafe void* RhpNewArrayFast(MethodTable* pMT, int length)
    {
        if (length < 0)
        {
            return null;
        }

        uint size = pMT->BaseSize + (uint)length * pMT->ComponentSize;

        GCObject* result = AllocObject(size);
        result->MethodTable = pMT;
        result->Length = length;

        return result;
    }

    [RuntimeExport("RhNewVariableSizeObject")]
    internal static unsafe void* RhNewVariableSizeObject(MethodTable* pEEType, int length)
    {
        return RhpNewArray(pEEType, length);
    }

    [RuntimeExport("RhAllocateNewObject")]
    internal static unsafe void RhAllocateNewObject(MethodTable* pEEType, GC_ALLOC_FLAGS flags, void* pResult)
    {
        GCObject* result = AllocObject(pEEType->RawBaseSize, flags);
        result->MethodTable = pEEType;

        *(void**)pResult = result;
        // as some point we should set flags   
    }

    [RuntimeExport("RhpGcSafeZeroMemory")]
    internal static unsafe byte* RhpGcSafeZeroMemory(byte* dmem, nuint size)
    {
        MemoryOp.MemSet(dmem, 0, (int)size);
        return dmem;
    }

    [RuntimeExport("RhpNewFast")]
    internal static unsafe void* RhpNewFast(MethodTable* pMT)
    {
        // Use RawBaseSize instead of BaseSize because BaseSize only works for canonical/array types
        // For generic type definitions, BaseSize contains parameter count, not the actual size
        GCObject* result = AllocObject(pMT->RawBaseSize);
        result->MethodTable = pMT;
        return (byte*)result;
    }

    [RuntimeExport("RhSpanHelpers_MemCopy")]
    internal static unsafe void RhSpanHelpers_MemCopy(byte* dest, byte* src, UIntPtr len)
    {
        MemoryOp.MemCopy(dest, src, (int)len);
    }

    [RuntimeExport("memmove")]
    internal static unsafe void memmove(byte* dest, byte* src, UIntPtr len)
    {
        MemoryOp.MemMove(dest, src, (int)len);
    }

    [RuntimeExport("memset")]
    internal static unsafe void memset(byte* dest, int value, UIntPtr len)
    {
        MemoryOp.MemSet(dest, (byte)value, (int)len);
    }

    [RuntimeExport("RhNewString")]
    internal static unsafe void* RhNewString(MethodTable* pEEType, int length)
    {
        return RhpNewArray(pEEType, length);
    }

    [RuntimeExport("RhRegisterFrozenSegment")]
    internal static unsafe IntPtr RhRegisterFrozenSegment(void* pSegmentStart, nuint allocSize, nuint commitSize, nuint reservedSize)
    {
        if (GarbageCollector.IsEnabled)
        {
            return GarbageCollector.RegisterFrozenSegment((IntPtr)pSegmentStart, allocSize, commitSize, reservedSize);
        }
        else
        {
            return (IntPtr)pSegmentStart;
        }
    }

    [RuntimeExport("RhUpdateFrozenSegment")]
    internal static unsafe void RhUpdateFrozenSegment(IntPtr seg, void* allocated, void* committed)
    {
        if (GarbageCollector.IsEnabled)
        {
            GarbageCollector.UpdateFrozenSegment((nint)seg, (nint)allocated, (nint)committed);
        }
    }

    [RuntimeExport("RhHandleSet")]
    internal static void RhHandleSet(IntPtr handle, GCObject* obj)
    {
        GarbageCollector.HandleSetPrimary(handle, obj);
    }

    [RuntimeExport("RhHandleFree")]
    internal static void RhHandleFree(IntPtr handle)
    {
        GarbageCollector.FreeHandle(handle);
    }

    [RuntimeExport("RhpHandleAlloc")]
    internal static IntPtr RhpHandleAlloc(GCObject* obj, GCHandleType handleType)
    {
        return GarbageCollector.AllocateHandler(obj, handleType, IntPtr.Zero);
    }

    [RuntimeExport("RhpHandleAllocDependent")]
    internal static IntPtr RhpHandleAllocDependent(GCObject* primary, GCObject* secondary)
    {
        return GarbageCollector.AllocateHandler(primary, (GCHandleType)6, (nint)secondary);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe GCObject* AllocObject(uint size, GC_ALLOC_FLAGS flags = GC_ALLOC_FLAGS.GC_ALLOC_NO_FLAGS)
    {
        if (GarbageCollector.IsEnabled)
        {
            return GarbageCollector.AllocObject((nint)size, flags);
        }
        else
        {
            var result = MemoryOp.Alloc(size);
            MemoryOp.MemSet((byte*)result, 0, (int)size);
            return (GCObject*)result;
        }
    }
}
