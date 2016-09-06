using System;

namespace Cosmos.IL2CPU.Plugs.System
{

    [Plug(Target = typeof(Exception))]
    public static class ExceptionImpl
    {
        private static string mMessage;

        public static unsafe string GetClassName(
            [ObjectPointerAccess] uint* aThis)
        {
            int xObjectType = (int)*aThis;
            return "**Not Able to determine Exception Type**";
        }
        public static string get_Message(Exception aThis)
        {
            return mMessage;
        }

        [PlugMethod(Signature = "System_String__System_Exception_GetMessageFromNativeResources_System_Exception_ExceptionMessageKind_")]
        public static string GetMessageFromNativeResources(int aKind)
        {
            if (aKind == 0x3)
            {
                return "Out of memory!";
            }
            return "<Exception Message from Native Source>";
        }

        public static void Ctor(Exception aThis, string aMessage)
        {
            mMessage = aMessage;
        }

        public static string ToString(Exception aThis)
        {
            return "Exception: " + aThis.Message;
        }
    }
}
