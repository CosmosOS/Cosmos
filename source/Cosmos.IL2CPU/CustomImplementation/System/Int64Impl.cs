using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
  // Not supported yet, so commentd out.
  [Plug(Target = typeof(Int64))]
  public class Int64Impl {
    public static string ToString(ref long aThis) {
      return Int64Impl2.GetNumberString(aThis);
    }

    public static Int64 Parse(string s)
    {
        const string digits = "0123456789";
        Int64 result = 0;

        int z = 0;
        bool neg = false;

        if (s.Length >= 1)
        {
            if (s[0] == '+')
                z = 1;
            if (s[0] == '-')
            {
                z = 1;
                neg = true;
            }
        }

        for (int i = z; i < s.Length; i++)
        {
            Int64 ind = (Int64)digits.IndexOf(s[i]);
            if (ind == -1)
            {
                throw new FormatException();
            }
            result = (Int64)((result * 10) + ind);
        }

        if (neg)
            result *= -1;

        return result;
    }
  }

  // See note in UInt32Impl2
  public class Int64Impl2 {
    public static string GetNumberString(long aValue) {
        bool xIsNegative = false;
        if (aValue < 0)
        {
            xIsNegative = true;
            aValue *= -1;
        }
        return UInt64Impl2.GetNumberString((ulong)aValue, xIsNegative);
    }
  }
}
