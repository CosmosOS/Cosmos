using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.X {
    public abstract class Register {
        public RegistersEnum GetId() {
            return Registers.GetRegister(GetName()).Value;
        }

        public void Push() {
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
