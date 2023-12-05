using IL2CPU.API.Attribs;
using Microsoft.Win32.SafeHandles;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(typeof(RandomAccess))]
    public static class RandomAccessImpl
    {
        public static int ReadAtOffset(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
        {
            throw new NotImplementedException();
        }

        public static int WriteAtOffset(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset)
        {
            throw new NotImplementedException();
        }
    }
}