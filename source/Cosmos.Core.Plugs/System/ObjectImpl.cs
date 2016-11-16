using Cosmos.IL2CPU;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System
{
    [Plug(Target = typeof(object))]
    public class ObjectImpl
    {
        public static unsafe ulong MemberwiseClone([ObjectPointerAccess] uint aThis)
        {
            var xThisPointer = (uint*)aThis;
            var xSize = IL2CPU.Plugs.System.ObjectImpl.FieldDataOffset + xThisPointer[2];

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
