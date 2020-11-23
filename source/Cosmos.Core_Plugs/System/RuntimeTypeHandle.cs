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
        public static void Cctor()
        {
            //
        }
    }
}
