using System;
using System.Globalization;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Interop;

[Plug("Interop+Globalization, System.Private.CoreLib")]
internal class GlobalizationImpl
{
    [PlugMethod(Signature = "System_Int32__Interop_Globalization_LoadICU__")]
    public static int LoadICU() => 0; //this is required for at least DateTimeFormat

    [PlugMethod(Signature =
        "System_Void__Interop_Globalization_ChangeCase_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
    public static unsafe void ChangeCase(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool) =>
        throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Void__Interop_Globalization_ChangeCaseTurkish_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
    public static unsafe void ChangeCaseTurkish(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool) =>
        throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Void__Interop_Globalization_ChangeCaseInvariant_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
    public static unsafe void ChangeCaseInvariant(char* aChar, int aInt, char* aChar2, int aInt2, bool Bool) =>
        throw new NotImplementedException();

    [PlugMethod(Signature = "System_Void__Interop_Globalization_InitOrdinalCasingPage_System_Int32__System_Char__")]
    public static void InitOrdinalCasingPage() => throw new NotImplementedException();

    public static void InitICUFunctions(IntPtr aPtr1, IntPtr aPtr2, string aString1, string aString2) =>
        throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Boolean__Interop_Globalization_StartsWith_System_IntPtr__System_Char___System_Int32__System_Char___System_Int32__System_Globalization_CompareOptions__System_Int32__")]
    public static unsafe bool StartsWith(IntPtr aIntPtr, char* aCharPtr, int aInt, char* aCharPtr2, int aInt2,
        CompareOptions aCompareOptions, int* aIntPtr2) => throw new NotImplementedException();

    public static bool GetLocaleInfoInt(string aString, uint aUint, ref int aInt) =>
        throw new NotImplementedException();

    public static bool GetLocaleInfoGroupingSizes(string aString, uint aUint, ref int aInt, ref int aInt2) =>
        throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Int32__Interop_Globalization_GetCalendars_System_String__System_Globalization_CalendarId____System_Int32_")]
    public static int GetCalendars(string aString, ushort[] aCalendarIds, int aInt) =>
        throw new NotImplementedException();

    public static int GetLatestJapaneseEra() => throw new NotImplementedException();

    public static int GetJapaneseEraStartDate(int aInt, ref int aInt2, ref int aInt3, ref int aInt4) =>
        throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Int32__Interop_Globalization_CompareString_System_IntPtr__System_Char___System_Int32__System_Char___System_Int32__System_Globalization_CompareOptions_")]
    public static unsafe int CompareString(IntPtr aIntPtr, char* aCharPtr, int aInt, char* aCharPtr2, int aInt2,
        object aCompareOptions) => throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Boolean__Interop_Globalization_EnumCalendarInfo_System_IntPtr__System_String__System_Globalization_CalendarId__System_Globalization_CalendarDataType__System_IntPtr_")]
    public static bool EnumCalendarInfo(IntPtr aIntPtr, string aString, object aCalendarId, object aCalendarDataType,
        IntPtr aIntPtr1) => throw new NotImplementedException();

    [PlugMethod(Signature =
        "Interop_Globalization_ResultCode__Interop_Globalization_GetCalendarInfo_System_String__System_Globalization_CalendarId__System_Globalization_CalendarDataType__System_Char___System_Int32_")]
    public static unsafe object GetCalendarInfo(string aString, object aCalendarId, object aCalendarDataType,
        char* aCharPtr, int aInt) => throw new NotImplementedException();
}
