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
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__System_Reflection_MetadataImport__IsValidToken_System_IntPtr__System_Int32_")]
        public static bool _IsValidToken(IntPtr scope, int token)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// i could not find this in the dotnet repo
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="token"></param>
        /// <param name="t"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [PlugMethod(Signature = "System_Boolean__System_Reflection_MetadataImport__GetFieldOffset_System_IntPtr__System_Int32__System_Int32___System_Int32_")]
        public static unsafe bool _GetFieldOffset(
            IntPtr scope,
            int typeTokenDef, int fieldTokenDef, out int offset)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetFieldMarshal_System_IntPtr__System_Int32___System_Reflection_ConstArray_")]
        public static void _GetFieldMarshal(IntPtr scope,
            int fieldToken,
            ref object fieldMarshal)// type is ConstArray
        {
            throw new NotImplementedException();
        }

        public static void _GetParentToken(IntPtr aIntPtr, int aInt, ref int aInt1)
        {
            throw new NotImplementedException();
        }

        public static void _GetMarshalAs(IntPtr pNativeType, int cNativeType, out int unmanagedType,
            out int safeArraySubType, out string safeArrayUserDefinedSubType,
            out int arraySubType, out int sizeParamIndex, out int sizeConst, out string marshalType,
            out string marshalCookie,
            out int iidParamIndex)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetPropertyProps_System_IntPtr__System_Int32__System_Void##___System_Int32___System_Reflection_ConstArray_")]
        public static unsafe void _GetPropertyProps(IntPtr scope,
            int mdToken, void** name, out int propertyAttributes, out object signature) //ConstArray
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetName_System_IntPtr__System_Int32__System_Void##_")]
        public static unsafe void _getName(IntPtr scope, int mdToken, out object name)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetFieldDefProps_System_IntPtr__System_Int32___System_Int32_")]
        public static unsafe void _GetFieldDefProps(IntPtr scope, int mdToken, out int fieldAttributes)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetEventProps_System_IntPtr__System_Int32__System_Void##___System_Int32_")]
        public static unsafe void _GetEventProps(IntPtr scope, int mdToken, out void* name, out int eventAttributes)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_MetadataImport__GetSigOfFieldDef_System_IntPtr__System_Int32___System_Reflection_ConstArray_")]
        public static unsafe int _GetSigOfFieldDef(IntPtr scope, int fieldToken, object fieldMarshal)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_String__System_Reflection_MetadataImport__GetDefaultValue_System_IntPtr__System_Int32___System_Int64___System_Int32___System_Int32_")]
        public static unsafe string _GetDefaultValue(IntPtr scope,
            int mdToken,
            out long value,
            out int length,
            out int corElementType)
        {
            throw new NotImplementedException();
        }
    }
}
