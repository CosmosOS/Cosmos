using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharp.Nasm {
  public class Assembler {
    public List<string> Data = new List<string>();
    public List<string> Code = new List<string>();

    public static Assembler operator +(Assembler aThis, string aThat) {
      aThis.Code.Add(aThat);
      return aThis;
    }

    public void Mov(string aDst, string aSrc) {
      Mov("", aDst, aSrc);
    }
    public void Mov(string aSize, string aDst, string aSrc) {
      string xSize = "";
      if (aSize != "") {
        xSize = aSize + " ";
      }
      Code.Add("Mov " + xSize + aDst + ", " + aSrc);
    }
  }
}
