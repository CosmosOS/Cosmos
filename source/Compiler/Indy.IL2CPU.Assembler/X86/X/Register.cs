using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public abstract class Register {
        public Guid GetId() {
            return Registers.GetRegister(GetName());
        }

        public void Push() {
            new Push { DestinationReg = GetId() };
        }

        public void Pop() {
            new Pop { DestinationReg = GetId() };
        }

        protected void Move(uint aAddress) {
            new X86.Move { DestinationReg = GetId(), SourceValue = aAddress, Size=Registers.GetSize(GetId())};
        }

        protected void Move(MemoryAction aAction) {
            new X86.Move { DestinationReg = GetId(), SourceReg = aAction.Register, SourceDisplacement = aAction.Displacement, SourceIsIndirect = aAction.IsIndirect, SourceRef = aAction.Reference, Size = Registers.GetSize(GetId()) };
        }

        protected void Move(Guid aRegister) {
            new X86.Move { DestinationReg = GetId(), SourceReg = aRegister, Size = Registers.GetSize(GetId()) };
        }

        protected void Move(ElementReference aReference) {
            new X86.Move { DestinationReg = GetId(), SourceRef = aReference, Size = Registers.GetSize(GetId())};
        }

        public string GetName() {
            return GetType().Name.Substring("Register".Length);
        }
        public bool isPort(){
            if (GetId().Equals(Registers.AX) || GetId().Equals(Registers.AL) || GetId().Equals(Registers.EAX))
                return true;
            return false;
        }
    }
}
