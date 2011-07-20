using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Compiler.XSharp {
  // This is the new type of DataMember, eventually we can eliminate much of the
  // code in DataMember (base)
  public class DataMember32 : DataMemberBase {
    public DataMember32(string aName) : base(aName) {
      int[] xValue = { 0 };
      UntypedDefaultValue = xValue.Cast<object>().ToArray();
    }

    public MemoryAction Value {
      get {
        return new MemoryAction(ElementReference.New(Name)) { IsIndirect = true };
      }
      set {

      }
    }
  }
}
