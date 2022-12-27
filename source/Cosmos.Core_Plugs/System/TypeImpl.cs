using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;
using System;
using System.Runtime.CompilerServices;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(Type))]
    public unsafe class TypeImpl
    {
        public static void CCtor()
        {
        }

        [PlugMethod(Signature = "System_RuntimeTypeHandle__System_Type_get_Typehandle")]
        public static unsafe RuntimeTypeHandle get_TypeHandle(CosmosRuntimeType aThis)
        {
            fixed(uint* ptr = &aThis.mTypeId) // this has to actually stay fixed
            {
                return CreateRuntimeTypeHandle((int)(uint)ptr);   // internally they do weird stuff and store the pointer as an int,
                                                                                        //so we need to pass the pointer straight away
            }
        }

        static RuntimeTypeHandle CreateRuntimeTypeHandle(int value)
        {
            throw new NotImplementedException(); // Implemented directly in ILReader.cs
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

        [PlugMethod(Signature = "System_Boolean__System_Type_op_Equality_System_Type__System_Type_")]
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

        [PlugMethod(Signature = "System_Boolean__System_Type_op_Inequality_System_Type__System_Type_")]
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

        [PlugMethod(Signature = "System_Type__System_Type_get_Type_System_String", IsOptional = false)]
        public static CosmosRuntimeType GetType(string aName)
        {
            int typeId = VTablesImpl.GetType(aName);
            if(typeId == -1)
            {
                return null;
            }
            return new CosmosRuntimeType((uint)typeId);
        }
    }
}
