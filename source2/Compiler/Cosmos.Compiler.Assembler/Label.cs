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

    public Label(MethodBase aMethod) : this(MethodInfoLabelGenerator.GenerateLabelName(aMethod)) { }
    
    public Label(string aName) {
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

    public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput) {
      if (IsGlobal) {
        aOutput.Write("global ");
        aOutput.WriteLine(QualifiedName);
      }
      aOutput.Write(QualifiedName);
      aOutput.Write(":");
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
