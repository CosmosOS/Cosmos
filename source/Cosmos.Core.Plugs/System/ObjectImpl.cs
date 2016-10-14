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

            var xSize = xThisPointer[1];

            var xResult = GCImplementation.AllocNewObject(xSize);

            var xThatPointer = (uint*)xResult;
            var xThatPointerByte = (byte*)xThatPointer[0];
            var xThisSimplePointer = (uint*)aThis;
            var xThisPointerByte = (byte*)xThisSimplePointer[0];
            for (int i = 0; i < xSize; i++)
            {
                xThatPointerByte[i] = xThisPointerByte[i];
            }
            return xResult;
        }
    }
}
