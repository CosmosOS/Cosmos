using System;
using Cosmos.Core.Common;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core_Plugs.System
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

        public static unsafe ulong MemberwiseClone([ObjectPointerAccess] uint aThis)
        {
            var xThisPointer = (uint*)aThis;
            var xSize = ObjectUtilities.FieldDataOffset + xThisPointer[2];

            var xResult = GCImplementation.AllocNewObject(xSize);

            var xThisPointerByte = (byte*)xThisPointer;
            var xThatPointerByte = (byte*)xResult;

            for (int i = 0; i < xSize; i++)
            {
                xThatPointerByte[i] = xThisPointerByte[i];
            }

            ulong xReturn = ((ulong)xResult) << (sizeof(ulong) / 2 * 8);

            return xReturn;
        }
    }
}
