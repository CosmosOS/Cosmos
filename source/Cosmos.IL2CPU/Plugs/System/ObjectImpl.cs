using System;

using Cosmos.Debug.Kernel;

namespace Cosmos.IL2CPU.Plugs.System
{
    [Plug(Target = typeof(object))]
    public static class ObjectImpl
    {
        private static Debugger mDebugger = new Debugger("IL2CPU", "ObjectImpl");

        public static string ToString(object aThis)
        {
            mDebugger.Send("<Object.ToString not yet implemented!>");
            return "<Object.ToString not yet implemented!>";
        }

        public static void Ctor(object aThis)
        {
        }

        public static Type GetType(object aThis)
        {
            return null;
        }

        public static int GetHashCode(object aThis)
        {
            return (int)aThis;
        }
    }
}
