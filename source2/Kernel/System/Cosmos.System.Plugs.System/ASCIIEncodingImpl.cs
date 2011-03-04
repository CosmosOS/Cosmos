using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System {
  [Plug(Target = typeof(ASCIIEncoding))]
  public class ASCIIEncodingImpl {

    // TODO: Plug string.GetStringFromEncoding instead
    public static string GetString(byte[] aBytes) {
      if (aBytes.Length == 0) {
        return string.Empty;
      } else { 
        var xChars = new char[aBytes.Length];
        for (int i = 0; i < aBytes.Length; i++) {
          xChars[i] = (char)aBytes[i];
        }
        return new string(xChars);
      }
    }

  }
}
