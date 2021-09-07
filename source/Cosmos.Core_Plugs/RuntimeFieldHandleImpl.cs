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
    }
}
