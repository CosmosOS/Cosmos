using System.Buffers;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Buffers;

[Plug("System.Buffers.ArrayPool`1, System.Private.CoreLib")]
public static class ArrayPoolImpl<T>
{
    public static ArrayPool<T> Shared { get; } = new CosmosArrayPool();

    private class CosmosArrayPool : ArrayPool<T>
    {
        public override T[] Rent(int minimumLength) => new T[minimumLength];

        public override void Return(T[] array, bool clearArray = false)
        {
        }
    }
}
