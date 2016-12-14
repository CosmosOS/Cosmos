using System;

using Cosmos.Debug.Kernel;

namespace Cosmos.IL2CPU.Plugs.System
{
    [Plug(Target = typeof(object))]
    public static class ObjectImpl
    {
        public static string ToString(object aThis)
        {
            Debugger.DoSend("<Object.ToString not yet implemented!>");
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
