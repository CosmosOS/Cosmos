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
      Code.Add("Mov " + (aSize + " ").TrimStart() + aDst + ", " + aSrc);
    }

    public void Cmp(string aDst, string aSrc) {
      Cmp("", aDst, aSrc);
    }
    public void Cmp(string aSize, string aDst, string aSrc) {
      Code.Add("Cmp " + (aSize + " ").TrimStart() + aDst + ", " + aSrc);
    }

    public string GetCode(bool endNewLine = true) { 
        string ret = "";

        foreach (string c in Code)
            ret += c + Environment.NewLine;
        if (!endNewLine)
            ret = ret.Remove(ret.Length - Environment.NewLine.Length);
        
        return ret;
    }
    public string GetData(bool endNewLine = true)
    {
        string ret = "";

        foreach (string d in Data)
            ret += d + Environment.NewLine;
        if (!endNewLine)
            ret = ret.Remove(ret.Length - Environment.NewLine.Length);

        return ret;
    }
  }
}
