using Cosmos.Core;
using IL2CPU.API.Attribs;
using System;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(Type))]
    public unsafe class TypeImpl
    {
        public static void CCtor()
        {
        }

        [PlugMethod(Signature = "System_RuntimeTypeHandle__System_Type_get_Typehandle")]
        public static RuntimeTypeHandle get_TypeHandle(CosmosRuntimeType aThis)
        {
            
        }

        [PlugMethod(Signature = "System_Type__System_Type_GetTypeFromHandle_System_RuntimeTypeHandle_")]
        public static Type GetTypeFromHandle(ulong aHandle)
        {
            uint x = (uint)(aHandle >> 32);
            uint* y = (uint*)x;
            uint xTypeId = *y;
            var xType = new CosmosRuntimeType(xTypeId);
            return xType;
        }

        public static bool op_Equality(CosmosRuntimeType aLeft, CosmosRuntimeType aRight)
        {
            if (aLeft is null) //Compare with null without equality check, since that causes stack overflow
            {
                return aRight is null;
            }
            if(aRight is null)
            {
                return false;
            }
            return aLeft.mTypeId == aRight.mTypeId;
        }

        public static bool op_Inequality(CosmosRuntimeType aLeft, CosmosRuntimeType aRight)
        {
            if (aLeft is null)
            {
                return !(aRight is null);
            }
            if (aRight is null)
            {
                return true;
            }
            return aLeft.mTypeId != aRight.mTypeId;
        }

        [PlugMethod(Signature ="System_Type__System_Type_get_BaseType", IsOptional = false)]
        public static Type get_BaseType(CosmosRuntimeType aThis)
        {
            return aThis.BaseType;
        }
    }
}
