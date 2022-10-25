using IL2CPU.API.Attribs;
using System.IO;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(UnmanagedMemoryStream))]
    public unsafe static class UnmanagedMemoryStreamImpl
    {
        public static void WriteCore(this UnmanagedMemoryStream aStream, ReadOnlySpan<byte> aSpan)
        {
            for (int I = 0; I < aSpan.Length; I++)
            {
                aStream.WriteByte(aSpan[I]);
            }
        }
        public static int ReadCore(this UnmanagedMemoryStream aStream, Span<byte> aSpan)
        {
            long n = Math.Min(aStream.Length - aStream.Position, aSpan.Length);
            if (n <= 0)
            {
                return 0;
            }

            int nInt = (int)n; // Safe because n <= count, which is an Int32
            if (nInt < 0)
            {
                return 0;  // _position could be beyond EOF
            }

            unsafe
            {
                if (aSpan != null)
                {
                    byte* pointer = null;

                    try
                    {
                        for (int I = 0; I < nInt; I++)
                        {
                            aSpan[I] = *(pointer + aStream.Position + aStream.Capacity);
                        }
                    }
                    catch { }
                }
                else
                {
                    for (int I = 0; I < nInt; I++)
                    {
                        aSpan[I] = *(aStream.PositionPointer + aStream.Position);
                    }
                }
            }

            aStream.Position += n;
            return nInt;
        }
        public static void WriteByte(this UnmanagedMemoryStream aStream, byte aValue)
        {
            aStream.PositionPointer[aStream.Position++] = aValue;
        }
        public static int ReadByte(this UnmanagedMemoryStream aStream)
        {
            return aStream.PositionPointer[aStream.Position++];
        }
    }
}
