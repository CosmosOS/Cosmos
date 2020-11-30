using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection
{
    [Plug("System.Reflection.CustomAttribute, System.Private.CoreLib")]
    class CustomAttributeImpl
    {
        public static void _ParseAttributeUsageAttribute(IntPtr aIntPtr, int aInt, ref int aInt1, ref bool aBool, ref bool aBool2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Object__System_Reflection_CustomAttribute__CreateCaObject_System_Reflection_RuntimeModule__System_RuntimeType__System_IRuntimeMethodInfo__System_Byte____System_Byte___System_Int32__")]
        public static unsafe void _CreateCaObject(object aRuntimeModule, object aRuntimeType, object aIRuntimeMethodInfo, byte** aBytePtr, byte* aBytePtr1, int* aIntPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_CustomAttribute__GetPropertyOrFieldData_System_Reflection_RuntimeModule__System_Byte____System_Byte____System_String___System_Boolean___System_RuntimeType___System_Object_")]
        public static unsafe void _GetPropertyOrFieldData(object aRuntimeModule, byte** aBytePtr, byte* aBytePtr1, ref string aString, ref bool aBool,
            ref object aRuntimeType, ref object aObject)
        {
            throw new NotImplementedException();
        }
    }
}
