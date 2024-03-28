using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection
{
    [Plug("System.Reflection.MetadataImport, System.Private.CoreLib")]
    class MetadataImportImpl
    {
        [PlugMethod(Signature= "System_Void__System_Reflection_MetadataImport__GetGenericParamProps_System_IntPtr__System_Int32___System_Int32_")]
        public static unsafe void __GetGenericParamProps(IntPtr aPtr1, int aInt, int* aPtr2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__Enum_System_IntPtr__System_Int32__System_Int32___System_Reflection_MetadataEnumResult_")]
        public static void _Enum(IntPtr scope, int type, int parent, out object result)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetMemberRefProps_System_IntPtr__System_Int32___System_Reflection_ConstArray_")]
        public static void _GetMemberRefProps(IntPtr scope,
            int memberTokenRef,
            out object signature)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature =
            "System_Void__System_Reflection_MetadataImport__GetCustomAttributeProps_System_IntPtr__System_Int32___System_Int32___System_Reflection_ConstArray_")]
        public static void _GetCustomAttributeProps(IntPtr scope,
            int customAttributeToken,
            out int constructorToken,
            out object signature)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetSigOfMethodDef_System_IntPtr__System_Int32___System_Reflection_ConstArray_")]
        public static void _GetSigOfMethodDef(IntPtr scope,
            int methodToken,
            ref object signature)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetNamespace_System_IntPtr__System_Int32__System_Void##_")]
        public static unsafe void _GetNamespace(IntPtr scope, int mdToken, void** namesp)
        {

        }

        [PlugMethod(Signature = "System_Boolean__System_Reflection_MetadataImport__IsValidToken_System_IntPtr__System_Int32_")]
        public static bool _IsValidToken(IntPtr scope, int token)
        {
            throw new NotImplementedException();
        }

        public static void _GetParentToken(IntPtr aIntPtr, int aInt, ref int aInt1)
        {
            throw new NotImplementedException();
        }
    }
}
