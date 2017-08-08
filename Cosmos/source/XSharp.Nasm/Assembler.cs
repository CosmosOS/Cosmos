using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharp.Nasm {
  //public class Assembler {
  //  public List<string> Data = new List<string>();
  //  public List<string> Code = new List<string>();

  //  public static Assembler operator +(Assembler aThis, string aThat) {
  //    aThis.Code.Add(aThat);
  //    return aThis;
  //  }

  //  public void Mov(string aDst, string aSrc) {
  //    Mov("", aDst, aSrc);
  //  }
  //  public void Mov(string aSize, string aDst, string aSrc) {
  //    Code.Add("Mov " + (aSize + " ").TrimStart() + aDst + ", " + aSrc);
  //  }

  //  public void Cmp(string aDst, string aSrc) {
  //    Cmp("", aDst, aSrc);
  //  }
  //  public void Cmp(string aSize, string aDst, string aSrc) {
  //    Code.Add("Cmp " + (aSize + " ").TrimStart() + aDst + ", " + aSrc);
  //  }

  //  public string GetCode(bool aEndNewLine = true) {
  //    string xResult = "";

  //    foreach (string x in Code) {
  //      xResult += x + Environment.NewLine;
  //    }
  //    if (!aEndNewLine) {
  //      xResult = xResult.Remove(xResult.Length - Environment.NewLine.Length);
  //    }

  //    return xResult;
  //  }

  //}
}
