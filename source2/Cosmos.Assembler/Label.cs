using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Cosmos.Assembler {
  public class Label : Instruction {
    public static string GetFullName(MethodBase aMethod) {
      return LabelName.Get(aMethod);
    }

    public string Comment { get; set; }

    public Label(string aName)
      : this(aName, "") {
    }

    public Label(string aName, string aComment) {
      mName = aName;
      if (aName.StartsWith(".")) {
        QualifiedName = LastFullLabel + aName;
      } else {
        QualifiedName = aName;
        // Some older code passes the whole label in the argument, so we check for any . in it.
        // That assumes that the main prefix can never have a . in it.
        // This code isnt perfect and doenst label X# code properly, but we don't care about
        // auto emitted X# labels for now.
        var xParts = aName.Split('.');
        if (xParts.Length < 3) {
          LastFullLabel = aName;
        }
      }
      Comment = aComment;
    }

    public static string GetLabel(object aObject) {
      Label xLabel = aObject as Label;
      if (xLabel == null)
        return "";
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

    public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput) {
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

    public override void UpdateAddress(Cosmos.Assembler.Assembler aAssembler, ref ulong aAddress) {
      base.UpdateAddress(aAssembler, ref aAddress);
    }

    public override void WriteData(Cosmos.Assembler.Assembler aAssembler, System.IO.Stream aOutput) { }
  }
}
