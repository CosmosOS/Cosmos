using System;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System;

[Plug(Target = typeof(Exception))]
public static class ExceptionImpl
{
    private static string mMessage;

    public static unsafe string GetClassName(
        [ObjectPointerAccess] uint* aThis)
    {
        var xObjectType = (int)*aThis;
        return "**Not Able to determine Exception Type**";
    }

    public static string get_Message(Exception aThis) => mMessage;

    [PlugMethod(Signature =
        "System_String__System_Exception_GetMessageFromNativeResources_System_Exception_ExceptionMessageKind_")]
    public static string GetMessageFromNativeResources(int aKind)
    {
        if (aKind == 0x3)
        {
            return "Out of memory!";
        }

        return "<Exception Message from Native Source>";
    }

    public static void Ctor(Exception aThis, string aMessage) => mMessage = aMessage;

    public static string ToString(Exception aThis) => "Exception: " + aThis.Message;

    public static bool IsImmutableAgileException(Exception aException) => throw new NotImplementedException();

    public static void SaveStackTracesFromDeepCopy(Exception aException, byte[] aByte, object[] aObject) =>
        throw new NotImplementedException();

    public static void PrepareForForeignExceptionRaise() => throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Void__System_Exception_GetStackTracesDeepCopy_System_Exception___System_Byte_____System_Object___")]
    public static void GetStackTracesDeepCopy(Exception aException, ref byte[] aByte, ref object[] aObject) =>
        throw new NotImplementedException();
}
