using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;

namespace Cosmos.Assembler.XSharp {
  public class DataMemberBase : DataMember {
    public DataMemberBase(string aName) : base(aName) {
    }

    public Cosmos.Assembler.ElementReference Address {
      get {
        return Cosmos.Assembler.ElementReference.New(Name); 
      }
    }
  }
}
