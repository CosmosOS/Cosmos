using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Assembler.XSharp {
  public abstract class Register {
    protected byte mBitSize;
    public byte BitSize {
      get { return mBitSize; }
    }

    public readonly string Name;

    protected Register() {
      Name = GetType().Name.Substring(typeof(Register).Name.Length);
    }

    public override string ToString() {
      return Name;
    }

    public RegistersEnum GetId() {
      return Registers.GetRegister(Name).Value;
    }

    public void Push() {
      // TODO: This emits Push dword which generates warnings about dword being ignored
      new Push { DestinationReg = GetId() };
    }

    public void Pop() {
      new Pop { DestinationReg = GetId() };
    }

    protected void Move(uint aAddress) {
      new Mov { DestinationReg = GetId(), SourceValue = aAddress, Size = Registers.GetSize(GetId()) };
    }

    protected void Move(MemoryAction aAction) {
      new Mov { DestinationReg = GetId(), SourceReg = aAction.Register, SourceDisplacement = aAction.Displacement, SourceIsIndirect = aAction.IsIndirect, SourceRef = aAction.Reference, Size = Registers.GetSize(GetId()) };
    }

    protected void Move(RegistersEnum aRegister) {
      new Mov { DestinationReg = GetId(), SourceReg = aRegister, Size = Registers.GetSize(GetId()) };
    }

    protected void Move(Cosmos.Assembler.ElementReference aReference) {
      new Mov { DestinationReg = GetId(), SourceRef = aReference, Size = Registers.GetSize(GetId()) };
    }

    public bool isPort() {
      if (GetId().Equals(Registers.AX) || GetId().Equals(Registers.AL) || GetId().Equals(Registers.EAX)) {
        return true;
      }
      return false;
    }
  }
}
