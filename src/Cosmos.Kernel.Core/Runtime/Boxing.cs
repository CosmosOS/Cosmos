using System.Runtime;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

internal static class Boxing
{

    /// <summary>
    /// Boxes the value stored at <paramref name="data"/>, a field address CoreLib
    /// hands over from <c>ValueType.Equals</c>, <c>ValueType.GetHashCode</c> and
    /// <c>Attribute.Equals</c> when it compares a value type field by field: a
    /// value type is copied into a new box, a reference type is already an object,
    /// so the result is the reference the field holds (null when the field is null),
    /// not the address of the field (CosmosOS/Cosmos#3362).
    /// </summary>
    [RuntimeExport("RhBoxAny")]
    public static unsafe void* RhBoxAny(byte* data, MethodTable* pEEType)
    {
        Serial.WriteString("Box Any\n");
        if (pEEType->IsValueType)
        {
            return RhBox(pEEType, data);
        }
        else
        {
            // CoreLib: Unsafe.As<byte, object>(ref data), the reference stored in the slot.
            return *(void**)data;
        }
    }
    [RuntimeExport("RhBox")]
    public static unsafe void* RhBox(MethodTable* pEEType, byte* data)
    {
        Serial.WriteString("Boxing type\n");
        byte* dataAdjustedForNullable = data;
        // If we're boxing a Nullable<T> then either box the underlying T or return null (if the
        // nullable's value is empty).
        if (pEEType->IsNullable)
        {
            // The boolean which indicates whether the value is null comes first in the Nullable struct.
            if (data[0] == 0)
            {
                return null;
            }

            // Switch type we're going to box to the Nullable<T> target type and advance the data pointer
            // to the value embedded within the nullable.
            dataAdjustedForNullable += pEEType->NullableValueOffset;
            pEEType = pEEType->NullableType;
        }

        void* result;

        result = Memory.RhpNewFast(pEEType);
        // result is a pointer to the object, cast to byte* and skip the MethodTable pointer to get to the data
        byte* destPtr = (byte*)result + sizeof(MethodTable*);

        // Copy the unboxed value type data into the new object.
        // Perform any write barriers necessary for embedded reference fields.
        if (pEEType->ContainsGCPointers)
        {
            StartupCodeHelpers.RhBulkMoveWithWriteBarrier(destPtr, dataAdjustedForNullable, pEEType->ValueTypeSize);
        }
        else
        {
            MemoryOp.MemCopy(destPtr, dataAdjustedForNullable, (int)pEEType->ValueTypeSize);
        }

        return result;
    }

    // Additional missing runtime exports
    [RuntimeExport("RhUnbox")]
    public static unsafe void RhUnbox(void* obj, byte* data, MethodTable* pUnboxToEEType)
    {
        Serial.WriteString("Unboxing type\n");
        if (obj == null)
        {
            return;
        }

        byte* dataAdjustedForNullable = data;

        if (pUnboxToEEType != null && pUnboxToEEType->IsNullable)
        {
            Unsafe.As<byte, bool>(ref dataAdjustedForNullable[0]) = true;

            // Adjust the data pointer so that it points at the value field in the Nullable.
            dataAdjustedForNullable += pUnboxToEEType->NullableValueOffset;
        }

        MethodTable* pEEType = *(MethodTable**)obj;
        byte* fields = (byte*)obj + sizeof(MethodTable*);

        if (pEEType->ContainsGCPointers)
        {
            // Copy the boxed fields into the new location in a GC safe manner
            StartupCodeHelpers.RhBulkMoveWithWriteBarrier(dataAdjustedForNullable, fields, pEEType->ValueTypeSize);
        }
        else
        {
            // Copy the boxed fields into the new location.
            MemoryOp.MemCopy(dataAdjustedForNullable, fields, (int)pEEType->ValueTypeSize);
        }
    }

    [RuntimeExport("RhUnbox2")]
    public static unsafe byte* RhUnbox2(MethodTable* pUnboxToEEType, void* obj)
    {
        Serial.WriteString(nameof(RhUnbox2));
        return (byte*)obj + sizeof(MethodTable*);
    }
}
