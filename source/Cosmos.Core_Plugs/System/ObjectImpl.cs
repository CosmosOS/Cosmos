using Cosmos.Core;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using System;

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

        public static unsafe Type GetType([ObjectPointerAccess] uint* aThis)
        {
            uint xType = *aThis;
            return new CosmosRuntimeType(xType);
        }

        public static int GetHashCode(object aThis)
        {
            return (int)aThis;
        }

        public static unsafe ulong MemberwiseClone([ObjectPointerAccess] uint aThis)
        {
            var xThisPointer = (uint*)aThis;
            var xSize = ObjectUtils.FieldDataOffset + xThisPointer[2];

            var xResult = GCImplementation.AllocNewObject(xSize);

            var xThisPointerByte = (byte*)xThisPointer;
            var xThatPointerByte = (byte*)xResult;

            for (int i = 0; i < xSize; i++)
            {
                xThatPointerByte[i] = xThisPointerByte[i];
            }

            ulong xReturn = (ulong)xResult << (sizeof(ulong) / 2 * 8);

            return xReturn;
        }
    }
}
