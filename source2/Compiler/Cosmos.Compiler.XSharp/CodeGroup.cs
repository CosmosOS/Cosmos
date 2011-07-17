using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Compiler.XSharp {
  public class CodeGroup {

    public void Assemble() {
      var xThisType = this.GetType();
      var xAsm = Assembler.Assembler.CurrentInstance;

      foreach (var xMember in xThisType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
        if (xMember.FieldType.IsSubclassOf(typeof(DataMember))) {
          var xCtor = xMember.FieldType.GetConstructor(new Type[] { typeof(string) });
          var xInstance = (DataMember)(xCtor.Invoke(new Object[] { xThisType.Name + "_" + xMember.Name }));
          xMember.SetValue(this, xInstance);
          xAsm.DataMembers.Add(xInstance);
        }
      }

      foreach (var xType in xThisType.GetNestedTypes()) {
        if (xType.IsSubclassOf(typeof(CodeBlock))) {
          var xCtor = xType.GetConstructor(new Type[0]);
          var xBlock = (CodeBlock)(xCtor.Invoke(new Object[0]));

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
