namespace Cosmos.Common.Extensions
{
  // This class extends BitConvertor. BitConvertor does not give us the ability to set specific parts of a byte array.
  // The extension functions are an easy and quick way to set certain values in a byte array.
  // TODO: In the future we should find a way to inline and asm these, or maybe use way to map
  // a record structure on top of a byte array for speed.
  //
  // BitConverter also uses platform specific endianness and cannot be changed.
  // Since we read from disk, network etc we must be able to specify and change endianness.
  //
  // Default methods are LittleEndian
  static public class ByteConverter
  {
    public static void SetUInt16(this byte[] n, ulong aPos, ushort value)
    {
      n[aPos + 0] = (byte)value;
      n[aPos + 1] = (byte)(value >> 8);
    }

    public static void SetUInt32(this byte[] n, ulong aPos, uint value)
    {
      n[aPos + 0] = (byte)value;
      n[aPos + 1] = (byte)(value >> 8);
      n[aPos + 2] = (byte)(value >> 16);
      n[aPos + 3] = (byte)(value >> 24);
    }

    public static void SetUInt64(this byte[] n, ulong aPos, ulong value)
    {
      n[aPos + 0] = (byte)value;
      n[aPos + 1] = (byte)(value >> 8);
      n[aPos + 2] = (byte)(value >> 16);
      n[aPos + 3] = (byte)(value >> 24);
      n[aPos + 4] = (byte)(value >> 32);
      n[aPos + 5] = (byte)(value >> 40);
      n[aPos + 6] = (byte)(value >> 48);
      n[aPos + 7] = (byte)(value >> 56);
    }
  }
}
