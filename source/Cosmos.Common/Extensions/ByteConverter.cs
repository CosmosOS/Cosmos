namespace Cosmos.Common.Extensions
{
  // This class duplicates BitConvertor. BitConvertor uses ulong though and currently does not
  // work in Cosmos. But even when it does work, this syntax is more convenient and we use
  // these conversions frequently.
  // TODO: In the future we should find a way to inline and asm these, or maybe use way to map
  // a record structure on top of a byte array for speed.
  //
  // BitConverter also uses platform specific endianness and cannot be changed.
  // Since we read from disk, network etc we must be able to specify and change endianness.
  //
  // Default methods are LittleEndian
  static public class ByteConverter
  {
    static public ushort ToUInt16(this byte[] n, ulong aPos)
    {
      return (ushort)(n[aPos + 1] << 8 | n[aPos]);
    }

    static public uint ToUInt32(this byte[] n, ulong aPos)
    {
      return (uint)(n[aPos + 3] << 24 | n[aPos + 2] << 16 | n[aPos + 1] << 8 | n[aPos]);
    }

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

    static public string GetAsciiString(this byte[] n, uint aStart, uint aCharCount)
    {
      var xChars = new char[aCharCount];
      for (int i = 0; i < aCharCount; i++)
      {
        xChars[i] = (char)n[(aStart) + i];
        if (xChars[i] == 0)
        {
          return new string(xChars, 0, i);
        }
      }
      return new string(xChars);
    }

    static public string GetUtf8String(this byte[] n, uint aStart, uint aCharCount)
    {
      // TODO: This method handles ASCII only currently, no unicode.
      var xChars = new char[aCharCount];
      for (int i = 0; i < aCharCount; i++)
      {
        xChars[i] = (char)n[(aStart) + i];
        if (xChars[i] == 0)
        {
          return new string(xChars, 0, i);
        }
      }
      return new string(xChars);
    }

    static public byte[] GetUtf8Bytes(this string n, uint aStart, uint aCharCount)
    {
      // TODO: This method handles ASCII only currently, no unicode.
      var xBytes = new byte[aCharCount];
      for (int i = 0; i < aCharCount; i++)
      {
        xBytes[i] = (byte)n[(int)((aStart) + i)];
      }
      return xBytes;
    }

    static public string GetUtf16String(this byte[] n, uint aStart, uint aCharCount)
    {
      //TODO: This routine only handles ASCII. It does not handle unicode yet.
      var xChars = new char[aCharCount];
      for (int i = 0; i < aCharCount; i++)
      {
        uint xPos = (uint)(aStart + i * 2);
        var xChar = (ushort)(n[xPos + 1] << 8 | n[xPos]);
        if (xChar == 0)
        {
          return new string(xChars, 0, i);
        }
        xChars[i] = (char)xChar;
      }
      return new string(xChars);
    }

    static public byte[] GetUtf16Bytes(this string n, uint aStart, uint aCharCount)
    {
      //TODO: This routine only handles ASCII. It does not handle unicode yet.
      var xBytes = new byte[2 * aCharCount];

      for (int i = 0; i < aCharCount; i++)
      {
        xBytes[2 * i] = (byte)n[(int)((aStart) + i)];
        xBytes[2 * i + 1] = (byte)(n[(int)(aStart + i)] >> 8);
      }

      return xBytes;
    }
  }
}