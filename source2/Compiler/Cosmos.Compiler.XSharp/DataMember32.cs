using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

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
        return new MemoryAction(ElementReference.New(Name)) { IsIndirect = true, Size = 32 };
      }
      set {
        new Move {
          DestinationRef = ElementReference.New(Name),  DestinationIsIndirect = true
          , SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
      }
    }
  }
}
