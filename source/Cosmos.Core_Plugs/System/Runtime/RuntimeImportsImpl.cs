using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime
{
    [Plug("System.Runtime.RuntimeImports, System.Private.CoreLib")]
    public static class RuntimeImportsImpl
    {
        public static unsafe void RhBulkMoveWithWriteBarrier(
            ref byte destination, ref byte source, uint byteCount)
        {
            fixed (byte* srcPtr = &source)
            fixed (byte* dstPtr = &destination)
            {
                for (int i = 0; i < byteCount; i++)
                {
                    dstPtr[i] = srcPtr[i];
                }
            }
            
            // Unsafe.CopyBlock(ref destination, ref source, byteCount);
        }
    }
}
