using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System {
  [Plug(Target = typeof(Int32))]
  public class Int32Impl {
    // for instance ones still declare as static but add a aThis argument first of same type as target
    public static int Parse(string s) {
      int xResult = 0;
      for (int i = 0; i < s.Length; i++) {
        xResult = xResult * 10;
        int j = s[i] - '0';
        if (j < 0 || j > 9) {
          throw new Exception("Non numeric digit found in int.parse");
        }
        xResult = xResult + j;
      }
      return xResult;
    }
  }
}
