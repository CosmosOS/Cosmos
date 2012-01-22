using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Assembler.XSharp {
  public class DataMemberBase : DataMember {
    public DataMemberBase(string aName) : base(aName) {
    }

    public ElementReference Address {
      get {
        return ElementReference.New(Name); 
      }
    }
  }
}
