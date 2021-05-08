using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.RuntimeMethodHandle, System.Private.CoreLib")]
    class RuntimeMethodHandleImpl
    {
        [PlugMethod(Signature = "System_RuntimeType__System_RuntimeMethodHandle_GetDeclaringType_System_RuntimeMethodHandleInternal_")]
        public static object GetDeclaringType(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Reflection_MethodAttributes__System_RuntimeMethodHandle_GetAttributes_System_RuntimeMethodHandleInternal_")]
        public static MethodAttributes GetAttributes(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_RuntimeMethodHandle_IsDynamicMethod_System_RuntimeMethodHandleInternal_")]
        public static bool IsDynamicMethod(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Resolver__System_RuntimeMethodHandle_GetResolver_System_RuntimeMethodHandleInternal_")]
        public static object GetResolver(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_String__System_RuntimeMethodHandle_GetName_System_RuntimeMethodHandleInternal_")]
        public static string GetName(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_RuntimeMethodHandle_IsGenericMethodDefinition_System_RuntimeMethodHandleInternal_")]
        public static bool IsGenericMethodDefinition(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeMethodHandleInternal__System_RuntimeMethodHandle_GetMethodFromCanonical_System_RuntimeMethodHandleInternal__System_RuntimeType_")]
        public static object GetMethodFromCanonical(object aRuntimeMethodHandleInternal, object aRuntimeType)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeMethodHandleInternal__System_RuntimeMethodHandle_GetStubIfNeeded_System_RuntimeMethodHandleInternal__System_RuntimeType__System_RuntimeType___")]
        public static object GetStubIfNeeded(object aRuntimeMethodHandleInternal, object aRuntimeType, object[] aRuntimeTypes)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_RuntimeMethodHandle_IsConstructor_System_RuntimeMethodHandleInternal_")]
        public static bool IsConstructor(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_RuntimeMethodHandle_HasMethodInstantiation_System_RuntimeMethodHandleInternal_")]
        public static bool HasMethodInstantiation(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_RuntimeMethodHandle_GetMethodInstantiation_System_RuntimeMethodHandleInternal__System_Runtime_CompilerServices_ObjectHandleOnStack__Interop_BOOL_")]
        public static void GetMethodInstantiation(object aRuntimeMethodHandleInternal, object aObjectHandleOnStack, bool aBool)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Reflection_LoaderAllocator__System_RuntimeMethodHandle_GetLoaderAllocator_System_RuntimeMethodHandleInternal_")]
        public static object GetLoaderAllocator(object aRuntimeMethodHandleInternal)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "Interop_BOOL__System_RuntimeMethodHandle_IsCAVisibleFromDecoratedType_System_Runtime_CompilerServices_QCallTypeHandle__System_RuntimeMethodHandleInternal__System_Runtime_CompilerServices_QCallTypeHandle__System_Runtime_CompilerServices_QCallModule_")]
        public static bool IsCAVisibleFromDecoratedType(object aQCallTypeHandle, object aRuntimeMethodHandleInternal, object aQCallTypeHandle1, object aQCallModule)
        {
            throw new NotImplementedException();
        }
    }
}
