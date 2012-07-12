using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp.Nasm {
  public class Assembler {
    public List<string> Data = new List<string>();
    public List<string> Code = new List<string>();

    public static Assembler operator +(Assembler aThis, string aThat) {
      aThis.Code.Add(aThat);
      return aThis;
    }
  }
}
