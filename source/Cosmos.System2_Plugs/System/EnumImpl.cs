using System;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Enum))]
    public static class EnumImpl
    {
        //		[PlugMethod(Signature = "System_Void__System_Enum__cctor__")]
        public static void Cctor()
        {
            //
        }

        public static bool Equals(Enum aThis, object aEquals)
        {
            throw new NotSupportedException("Enum.Equals not supported yet!");
        }

        public static string ToString(Enum aThis) => "<Enum.ToString> not implemented";

        public static string ToString(Enum aThis, string format) => aThis.ToString();

        public static int GetHashCode(Enum aThis)
        {
            throw new NotImplementedException("Enum.GetHashCode()");
        }
        [PlugMethod(Signature = "System_RuntimeType__System_Enum_InternalGetUnderlyingType_System_RuntimeType_")]
        public static object InternalGetUnderlyingType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Reflection_CorElementType__System_Enum_InternalGetCorElementType__")]
        public static object InternalGetCorElementType(object aThis)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Object__System_Enum_InternalBoxEnum_System_RuntimeType__System_Int64_")]
        public static object InternalBoxEnum(object aRuntimeType, long aLong)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_Enum_GetEnumValuesAndNames_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack__System_Runtime_CompilerServices_ObjectHandleOnStack__Interop_BOOL_")]
        public static void GetEnumValuesAndNames(object aQCallTypeHandle, object aObjectHandleOnStack, object aObjectHandleOnStack1, bool aBool)
        {
            throw new NotImplementedException();
        }
    }
}
