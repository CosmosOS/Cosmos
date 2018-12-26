using System;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    // System.Private.CoreLib, internal
    [Plug(TargetName = "System.Environment, System.Private.CoreLib")]
    public static class InternalEnvironmentImpl
    {
        public static void CCtor()
        {
        }

        // todo: implement correctly
        public static int get_TickCount() => 0;

        public static int get_ProcessorCount() => 1;

        public static string GetEnvironmentVariable(string variable) => null;
    }

    // System.Runtime.Extensions, public
    [Plug(typeof(Environment))]
    public static class EnvironmentImpl
    {
        public static void CCtor()
        {
        }
    }
}
