using IL2CPU.API.Attribs;
using System;

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

        public static int get_CurrentManagedThreadId() => 0;

        public static void FailFast(string aString)
        {
            throw new NotImplementedException();
        }

        public static void FailFast(string aString, Exception aException)
        {
            throw new NotImplementedException();
        }

        public static long get_TickCount64()
        {
            throw new NotImplementedException();
        }
    }

    [Plug(TargetName = "System.Environment+WindowsVersion, System.Private.CoreLib", IsOptional = true)]
    public static class WindowsVersionImpl
    {
        public static bool GetIsWindows8OrAbove()
        {
            return false;
        }
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