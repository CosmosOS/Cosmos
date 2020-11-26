using System;
using IL2CPU.API.Attribs;
using System.Globalization;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Globalization, System.Private.CoreLib")]

    class GlobalizationImpl
    {
        [PlugMethod(Signature = "System_Int32__Interop_Globalization_LoadICU__")]
        public static int LoadICU()
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__Interop_Globalization_ChangeCase_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
        public static unsafe void ChangeCase(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool)
        {
            throw new NotImplementedException();
        }   
        [PlugMethod(Signature = "System_Void__Interop_Globalization_ChangeCaseTurkish_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
        public static unsafe void ChangeCaseTurkish(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__Interop_Globalization_ChangeCaseInvariant_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
        public static unsafe void ChangeCaseInvariant(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Signature = "System_Void__Interop_Globalization_InitOrdinalCasingPage_System_Int32__System_Char__")]
        public static void InitOrdinalCasingPage()
        {
            throw new NotImplementedException();
        }

        public static void InitICUFunctions(IntPtr aPtr1, IntPtr aPtr2, string aString1, string aString2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__Interop_Globalization_StartsWith_System_IntPtr__System_Char___System_Int32__System_Char___System_Int32__System_Globalization_CompareOptions__System_Int32__")]
        public static unsafe bool StartsWith(IntPtr aIntPtr, char* aCharPtr, int aInt, char* aCharPtr2, int aInt2, CompareOptions aCompareOptions, int* aIntPtr2)
        {
            throw new NotImplementedException();
        }
    }
}
