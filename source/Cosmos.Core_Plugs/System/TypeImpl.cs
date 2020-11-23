using Cosmos.Core;
using Cosmos.Debug.Kernel;
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
            if (aLeft == null)
            {
                if (aRight == null)
                {
                    return true;
                }
                return false;
            }
            if (aRight == null)
            {
                return false;
            }
            return aLeft.mTypeId == aRight.mTypeId;
        }

        public static bool op_Inequality(CosmosRuntimeType aLeft, CosmosRuntimeType aRight)
        {
            if(aLeft == null)
            {
                if(aRight == null)
                {
                    return true;
                }
                return false;
            }
            if(aRight == null)
            {
                return false;
            }
            return aLeft.mTypeId != aRight.mTypeId;
        }

        [PlugMethod(Signature ="System_Type__System_Type_get_BaseType", IsOptional = false)]
        public static Type get_BaseType(Type aThis)
        {
            return typeof(object);
        }
    }
}
