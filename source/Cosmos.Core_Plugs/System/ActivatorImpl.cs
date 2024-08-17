using System;
using System.Runtime.CompilerServices;
using Cosmos.Core;
using IL2CPU.API.Attribs;
using IL2CPU.API;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cosmos.Core_Plugs.System
{
    [Plug(typeof(global::System.Activator))]
    public static class ActivatorImpl
    {
        public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>()
        {
            return (T)Activator.CreateInstance(typeof(T))!;
        }

        // nonPublic is ignored since, at the moment, we cannot know if a ctor is public or not
        // and we would get a Stack Corruption or Null reference exception if the ctor is not present either way.
        public unsafe static object CreateInstance(CosmosRuntimeType ctr, bool nonPublic, bool wrapExceptions)
        {
            ArgumentNullException.ThrowIfNull(ctr, "Type");

            // get the Type's VTable entry
            var mType = VTablesImpl.mTypes[ctr.mTypeId];

            // Calculate Object Size
            uint dSize = 0;
            if (ctr.IsValueType)
            {
                // For value types this property holds the correct size, so we can avoid the iteration
                dSize = mType.Size;
            }
            else
            {
                // Calculate Object Actual Size.
                var gcType = VTablesImpl.gcTypes[ctr.mTypeId];
                for (int i = 0; i < gcType.GCFieldTypes.Length; i++)
                {
                    dSize += VTablesImpl.GetSize(gcType.GCFieldTypes[i]);
                }
            }

            // Object Allocation
            uint ptr = GCImplementation.AllocNewObject(ObjectUtils.FieldDataOffset + dSize);

            // Set Fields
            var vptr = (uint*)ptr;
            vptr[0] = ctr.mTypeId; // Type
            vptr[1] = ptr;         // Address/Handler?
            vptr[2] = dSize;       // Data Area Size

            object obj = Unsafe.Read<object>(vptr)!;
            // We make the wild assumption that the ctor is the first method address, this may change on the future if the VTable is reworked.
            var ctoraddress = mType.MethodAddresses[0];

            try
            {
                if (ctr.IsValueType)
                {
                    // Struct Ctor Call
                    var cctor = (delegate*<void*, void>)ctoraddress;
                    cctor(vptr + 3); // Struct pointer
                }
                else
                {
                    // Object Ctor Call
                    var cctor = (delegate*<object, void>)ctoraddress;
                    cctor(obj);
                }

                return obj;
            }
            catch (Exception inner) when (wrapExceptions)
            {
                throw new TargetInvocationException(inner);
            }
        }
    }
}
