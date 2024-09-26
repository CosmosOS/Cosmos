using Cosmos.Core_Plugs.System;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Cosmos.Core_Plugs
{
    [Plug(Target = typeof(global::System.RuntimeFieldHandle))]
    public static class RuntimeFieldHandleImpl
    {
        [PlugMethod(Signature = "System_Object__System_RuntimeFieldHandle_GetValue_System_Reflection_RtFieldInfo__System_Object__System_RuntimeType__System_RuntimeType___System_Boolean_")]
        public static object GetValue(FieldInfo field, object instance, object fieldType, object declaringType, ref bool domainInitialized)
        {
            throw new NotImplementedException("RuntimeFieldHandle.GetValue()");
        }

        [PlugMethod(Signature = "System_Reflection_FieldAttributes__System_RuntimeFieldHandle_GetAttributes_System_RuntimeFieldHandleInternal_")]
        public static unsafe object GetAttributes(object type)  
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeFieldHandleInternal__System_RuntimeFieldHandle_GetStaticFieldForGenericType_System_RuntimeFieldHandleInternal__System_RuntimeType_")]
        public static unsafe object GetStaticFieldForGenericType(object field, object declaringType)  
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void#__System_RuntimeFieldHandle__GetUtf8Name_System_RuntimeFieldHandleInternal_")]
        public static unsafe void* _GetUtf8Name(object type)  
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_String__System_RuntimeFieldHandle_GetName_System_Reflection_RtFieldInfo_")]
        public static unsafe string GetName(object field)  
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__System_RuntimeFieldHandle_GetToken_System_Reflection_RtFieldInfo_")]
        public static unsafe int GetToken(object field)  
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_RuntimeType__System_RuntimeFieldHandle_GetApproxDeclaringType_System_RuntimeFieldHandleInternal_")]
        public static unsafe object GetApproxDeclaringType(object field)  
        {
            throw new NotImplementedException();
        }
    }
}
