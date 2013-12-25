using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System {
  [Plug(Target = typeof(Int32))]
  public class Int32Impl {
    // for instance ones still declare as static but add a aThis argument first of same type as target
      public static Int32 Parse(string s)
      {
          const string digits = "0123456789";
          Int32 result = 0;

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
              Int32 ind = (Int32)digits.IndexOf(s[i]);
              if (ind == -1)
              {
                  throw new FormatException();
              }
              result = (Int32)((result * 10) + ind);
          }

          if (neg)
              result *= -1;

          return result;
      }
  }
}
