using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
  public class CodeGroup {
    protected Assembler.Assembler mAsm = Assembler.Assembler.CurrentInstance;

    protected void SetDataMembers(object aInst) {
      var xThisType = aInst.GetType();
      var xParts = xThisType.FullName.Split('.');
      string xBaseLabel = xParts[xParts.Length - 1].Replace('+', '_') + "_";

      foreach (var xMember in xThisType.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
        if (xMember.FieldType.IsSubclassOf(typeof(DataMember))) {
          var xCtor = xMember.FieldType.GetConstructor(new Type[] { typeof(string) });
          var xInst = (DataMember)(xCtor.Invoke(new Object[] { xBaseLabel + xMember.Name }));
          xMember.SetValue(aInst, xInst);
          mAsm.DataMembers.Add(xInst);
        }
      }
    }

    public void Assemble() {
      var xThisType = this.GetType();

      // Generate Global DataMembers
      SetDataMembers(this);

      foreach (var xType in xThisType.GetNestedTypes()) {
        if (xType.IsSubclassOf(typeof(CodeBlock)) && !xType.IsAbstract) {
          var xCtor = xType.GetConstructor(new Type[0]);
          var xBlock = (CodeBlock)(xCtor.Invoke(new Object[0]));

          // Generate Local DataMembers
          SetDataMembers(xBlock);

          // Genereate code
          //
          // Issue label for the routine
          xBlock.Label = CodeBlock.MakeLabel(xType);
          // Assemble the routine itself
          xBlock.Assemble();
          // Issue the return for the routine
          xBlock.Return();
        }
      }
    }

  }

}
