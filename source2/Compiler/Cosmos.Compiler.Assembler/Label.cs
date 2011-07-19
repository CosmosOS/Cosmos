using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Cosmos.Compiler.Assembler {
  public class Label : Instruction {
    public static string GetFullName(MethodBase aMethod) {
      return MethodInfoLabelGenerator.GenerateLabelName(aMethod);
    }

    public string Comment { get; set; }

    public Label(MethodBase aMethod) : this(MethodInfoLabelGenerator.GenerateLabelName(aMethod), "") { }

    public Label(string aName) : this(aName, "") {
    }

    public Label(string aName, string aComment) {
      // Dont use . in labels. Although they are legal in NASM, GDB cannot handle them in labels while debugging.
      // Some older code still passes in full dotted labels, so we replace.
      mName = aName.Replace('.', '#');
      if (mName.StartsWith("#")) {
        QualifiedName = LastFullLabel + mName;
      } else {
        QualifiedName = mName;
        // Some older code passes the whole label in the argument, so we check for any ./# in it.
        // That assumes that the main prefix can never have a # in it.
        // This code isnt perfect and doenst always label X# code properly.
        var xParts = mName.Split('#');
        if (xParts.Length < 3) {
          LastFullLabel = mName;
        }
      }
      Comment = aComment;
    }

    public static string GetLabel(object aObject) {
      Label xLabel = aObject as Label;
      if (xLabel == null) { 
        return "";
      }
      return xLabel.Name;
    }

    public static string LastFullLabel { get; set; }

    public string QualifiedName {
      get;
      private set;
    }

    public bool IsGlobal {
      get;
      set;
    }

    private string mName;
    public string Name {
      get { return mName; }
    }

    public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput) {
      if (IsGlobal) {
        aOutput.WriteLine("global " + QualifiedName);
      }
      aOutput.Write(QualifiedName + ":");
      if (Comment != "") {
        aOutput.Write(" ;" + Comment);
      }
    }

    public override bool IsComplete(Assembler aAssembler) {
      return true;
    }

    public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress) {
      base.UpdateAddress(aAssembler, ref aAddress);
    }

    public override void WriteData(Assembler aAssembler, System.IO.Stream aOutput) { }
  }
}
