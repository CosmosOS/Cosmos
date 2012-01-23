using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Assembler.XSharp {
  public class Register32 : Register {

    public Register32() {
      mBitSize = 32;
    }

    // Not all overloads can go here.
    // -C# overloads specifically by exact class and does not inherit in many cases
    // -x86 does not support all operations on all registers
    // -Operator overloading and implicits must exist ON the final class
    // -Generics cannot be used because of above

    public MemoryAction this[int aOffset] {
      get {
        return new MemoryAction(GetId(), aOffset);
      }
      set {
        new Mov { DestinationReg = GetId(), DestinationDisplacement = aOffset, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
      }
    }

    public void Add(UInt32 aValue) {
      new Add { DestinationReg = GetId(), SourceValue = aValue };
    }
    public void Add(Register32 aReg) {
      new Add { DestinationReg = GetId(), SourceReg = aReg.GetId() };
    }

    public void Sub(UInt32 aValue) {
      new Sub { DestinationReg = GetId(), SourceValue = aValue };
    }
    public void Sub(Register32 aReg) {
      new Sub { DestinationReg = GetId(), SourceReg = aReg.GetId() };
    }

    public void Compare(UInt32 aValue) {
      new Compare { DestinationReg = GetId(), SourceValue = aValue };
    }
    public void Compare(MemoryAction aAction) {
      new Compare {
        DestinationRef = aAction.Reference,
        DestinationIsIndirect = true,
        SourceReg = GetId()
      };
    }

    public void Test(UInt32 aValue) {
      new Test { DestinationReg = GetId(), SourceValue = aValue, Size = 32 };
    }
  }
}