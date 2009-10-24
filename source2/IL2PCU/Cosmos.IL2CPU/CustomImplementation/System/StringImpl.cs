using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System {
  [Plug(Target = typeof(String))]
  public static class StringImpl {

    public static string Format(IFormatProvider aFormatProvider, string aFormat, object[] aArgs) {
      string[] xStrings = new string[1 + 2 + (aArgs.Length * 7) - 1];
      xStrings[0] = aFormat;
      xStrings[1] = "(";
      for (int i = 0; i < aArgs.Length; i++) {
        xStrings[2 + (i * 7)] = "Param";
        xStrings[3 + (i * 7)] = i.ToString();
        xStrings[4 + (i * 7)] = "=";
        xStrings[5 + (i * 7)] = "\"";
        xStrings[6 + (i * 7)] = aArgs[i].ToString();
        xStrings[7 + (i * 7)] = "\"";
        if (i < (aArgs.Length - 1)) {
          xStrings[8 + (i * 7)] = ",";
        }
      }
      xStrings[xStrings.Length - 1] = ")";
      return String.Concat(xStrings);
    }

    public static bool StartsWith(string aThis, string aSubstring, StringComparison aComparison) {
      Console.WriteLine("String.StartsWith not working!");
      throw new NotImplementedException();
    }

    public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded) {
        Console.Write("PadHelper, totalWidth = ");
        WriteNumber((uint)totalWidth, 32);
        Console.WriteLine("");
      char[] cs = new char[totalWidth];

      int pos = aThis.Length;

      if (isRightPadded) {
        for (int i = 0; i < aThis.Length; i++)
          cs[i] = aThis[i];

        for (int i = aThis.Length; i < totalWidth; i++)
          cs[i] = paddingChar;
      } else {
        int offset = totalWidth - aThis.Length;
        for (int i = 0; i < aThis.Length; i++)
          cs[i + offset] = aThis[i];

        for (int i = 0; i < offset; i++)
          cs[i] = paddingChar;
      }

      return new string(cs);
    }

    public static string Substring(string aThis, int startpos) {
      char[] cs = new char[aThis.Length - startpos];
      int j = 0;
      for (int i = startpos; i < aThis.Length; i++)
        cs[j++] = aThis[i];

      return new string(cs);
    }

    public static string Substring(string aThis, int startpos, int length) {
      if (startpos + length > aThis.Length)
        length = aThis.Length - startpos;

      char[] cs = new char[length];

      int j = 0;
      for (int i = startpos; i < startpos + length; i++)
        cs[j++] = aThis[i];

      return new string(cs);
    }

    public static string Replace(string aThis, char oldValue, char newValue) {
      char[] cs = new char[aThis.Length];

      for (int i = 0; i < aThis.Length; i++) {
        if (aThis[i] != oldValue)
          cs[i] = aThis[i];
        else
          cs[i] = newValue;
      }

      return new string(cs);
    }

    // HACK: We need to redo this once char support is complete (only returns 0, -1).
    public static int CompareTo(string aThis, string other) {
      if (aThis.Length != other.Length)
        return -1;
      for (int i = 0; i < aThis.Length; i++)
        if (aThis[i] != other[i])
          return -1;
      return 0;
    }

    public static int IndexOf(string aThis, char value, int startIndex, int count) {
        var xEndIndex = aThis.Length;
        if((startIndex + count) < xEndIndex){
            xEndIndex=(startIndex + count);
        }
        for (int i = startIndex; i < xEndIndex; i++)
        {
            if (aThis[i] == value)
            {
                return i;
            }
        }

        return -1;     
    }

    public static char[] ToCharArray(string aThis)
    {
        return GetStorageArray(aThis);
    }

    [PlugMethod(Enabled = false)]
    public static uint GetStorage(string aString) {
      return 0;
    }

    [PlugMethod(Enabled = false)]
    public static char[] GetStorageArray(string aString) {
      return null;
    }

    public static int IndexOf(string aThis, string aSubstring, int aIdx, int aLength, StringComparison aComparison){
      throw new Exception("Not implemented");
    }

    public static void WriteNumber(uint aValue,
                      byte aBitCount)
    {
        uint xValue = aValue;
        byte xCurrentBits = aBitCount;
        Console.Write("0x");
        while (xCurrentBits >= 4)
        {
            xCurrentBits -= 4;
            byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
            string xDigitString = null;
            switch (xCurrentDigit)
            {
                case 0:
                    xDigitString = "0";
                    goto default;
                case 1:
                    xDigitString = "1";
                    goto default;
                case 2:
                    xDigitString = "2";
                    goto default;
                case 3:
                    xDigitString = "3";
                    goto default;
                case 4:
                    xDigitString = "4";
                    goto default;
                case 5:
                    xDigitString = "5";
                    goto default;
                case 6:
                    xDigitString = "6";
                    goto default;
                case 7:
                    xDigitString = "7";
                    goto default;
                case 8:
                    xDigitString = "8";
                    goto default;
                case 9:
                    xDigitString = "9";
                    goto default;
                case 10:
                    xDigitString = "A";
                    goto default;
                case 11:
                    xDigitString = "B";
                    goto default;
                case 12:
                    xDigitString = "C";
                    goto default;
                case 13:
                    xDigitString = "D";
                    goto default;
                case 14:
                    xDigitString = "E";
                    goto default;
                case 15:
                    xDigitString = "F";
                    goto default;
                default:
                    Console.Write(xDigitString);
                    break;
            }
        }
    }

  } 
}