using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public abstract class Register {
        protected byte mBitSize;
        public byte BitSize {
            get { return mBitSize; }
        }

        public readonly string Name;

        public Register() {
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
            new Move { DestinationReg = GetId(), SourceValue = aAddress, Size=Registers.GetSize(GetId())};
        }

        protected void Move(MemoryAction aAction) {
            new Move { DestinationReg = GetId(), SourceReg = aAction.Register, SourceDisplacement = aAction.Displacement, SourceIsIndirect = aAction.IsIndirect, SourceRef = aAction.Reference, Size = Registers.GetSize(GetId()) };
        }

        protected void Move(RegistersEnum aRegister)
        {
            new Move { DestinationReg = GetId(), SourceReg = aRegister, Size = Registers.GetSize(GetId()) };
        }

        protected void Move(ElementReference aReference) {
            new Move { DestinationReg = GetId(), SourceRef = aReference, Size = Registers.GetSize(GetId())};
        }

        public bool isPort(){
            if (GetId().Equals(Registers.AX) || GetId().Equals(Registers.AL) || GetId().Equals(Registers.EAX)) {
                return true;
            }
            return false;
        }
    }
}
