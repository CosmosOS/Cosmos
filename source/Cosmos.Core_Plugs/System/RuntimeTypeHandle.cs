using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(global::System.RuntimeTypeHandle))]
    public static class RuntimeTypeHandleImpl
    {
        public static unsafe void Ctor(RuntimeTypeHandle aThis, void* aValue)
        {
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_HasInstantiation_System_RuntimeType_")]
        public static bool HasInstantiation(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_ContainsGenericVariables_System_RuntimeType_")]
        public static bool ContainsGenericVariables(object aRuntimeType)
        {
            throw new NotImplementedException();
        }   
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_IsGenericVariable_System_RuntimeType_")]
        public static bool IsGenericVariable(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_IsGenericTypeDefinition_System_RuntimeType_")]
        public static bool IsGenericTypeDefinition(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeType__System_RuntimeTypeHandle_GetElementType_System_RuntimeType_")]
        public static object GetElementType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Int32__System_RuntimeTypeHandle_GetToken_System_RuntimeType_")]
        public static int GetToken(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_RuntimeType__System_RuntimeTypeHandle_GetBaseType_System_RuntimeType_")]
        public static object GetBaseType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_IsInterface_System_RuntimeType_")]
        public static bool IsInterface(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Reflection_TypeAttributes__System_RuntimeTypeHandle_GetAttributes_System_RuntimeType_")]
        public static object GetAttributes(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_IntPtr__System_RuntimeTypeHandle__GetMetadataImport_System_RuntimeType_")]
        public static IntPtr _GetMetadataImport(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Reflection_CorElementType__System_RuntimeTypeHandle_GetCorElementType_System_RuntimeType_")]
        public static object GetCorElementType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Reflection_RuntimeAssembly__System_RuntimeTypeHandle_GetAssembly_System_RuntimeType_")]
        public static object GetAssembly(object aRuntimeType)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_GetConstraints_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetConstraints(object aQCallTypeHandle, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_ConstructName_System_Runtime_CompilerServices_QCallTypeHandle__System_TypeNameFormatFlags__System_Runtime_CompilerServices_StringHandleOnStack_")]
        public static void GetConstraints(object aQCallTypeHandle, object aTypeNameFormatFlags, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_GetInstantiation_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack__Interop_BOOL_")]
        public static void GetInstantiation(object aQCallTypeHandle, object aObjectHandleOnStack, object aInteropBool)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_CanCastTo_System_RuntimeType__System_RuntimeType_")]
        public static bool CanCastTo(object aRuntimeType, object aRuntimeType2)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_GetGenericTypeDefinition_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetGenericTypeDefinition(object aQCallTypeHandle, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }
        public static void Cctor()
        {
            //
        }

        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_IsEquivalentTo_System_RuntimeType__System_RuntimeType_")]
        public static bool IsEquivalentTo(object aRuntimeType, object aRuntimeType2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Reflection_RuntimeModule__System_RuntimeTypeHandle_GetModule_System_RuntimeType_")]
        public static object GetModule(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeType__System_RuntimeTypeHandle_GetDeclaringType_System_RuntimeType_")]
        public static object GetDeclaringType(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Object__System_RuntimeTypeHandle_Allocate_System_RuntimeType_")]
        public static object Allocate(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_GetTypeByNameUsingCARules_System_String__System_Runtime_CompilerServices_QCallModule__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetTypeByNameUsingCARules(string aString, object aQCallModule, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Boolean__System_RuntimeTypeHandle_IsComObject_System_RuntimeType__System_Boolean_")]
        public static bool IsComObject(object aRuntimeType, bool aBool)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_Instantiate_System_Runtime_CompilerServices_QCallTypeHandle__System_IntPtr___System_Int32__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static unsafe void Instantiate(object aQCallTypeHandle, IntPtr* aIntPtrPtr, int aInt, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__System_RuntimeTypeHandle_GetArrayRank_System_RuntimeType_")]
        public static int GetArrayRank(object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle_MakeSZArray_System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void MakeSZArray(object aQCallTypeHandle, object aObjectHandleOnStack)
        {
            throw new NotImplementedException();
        }
    }
}
