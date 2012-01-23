using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Assembler.XSharp {
  // This is the new type of DataMember, eventually we can eliminate much of the
  // code in DataMember (base)
  public class DataMember32 : DataMemberBase {
    public DataMember32(string aName) : base(aName) {
      int[] xValue = { 0 };
      UntypedDefaultValue = xValue.Cast<object>().ToArray();
    }

    public MemoryAction Value {
      get {
        return new MemoryAction(Cosmos.Assembler.ElementReference.New(Name)) { IsIndirect = true, Size = 32 };
      }
      set {
        // For DataMember32 value will be null with ++ and --, this is important and purposeful.
        // ++ and -- try to set again, but for us we dont want that.
        if (value != null) {
          new Mov {
            DestinationRef = Cosmos.Assembler.ElementReference.New(Name),
            DestinationIsIndirect = true,
            SourceValue = value.Value.GetValueOrDefault(),
            SourceRef = value.Reference,
            SourceReg = value.Register,
            SourceIsIndirect = value.IsIndirect
          };
        }
      }
    }
  }
}
