using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public class RegisterEAX : Register32 {
        public const string Name = "EAX";
        public static readonly RegisterEAX Instance = new RegisterEAX();

        public override string ToString() {
            return Name;
        }

        public static RegisterEAX operator++ (RegisterEAX aRegister) {
            new Inc { DestinationReg = aRegister.GetId() };
            return aRegister;
        }
        public static RegisterEAX operator-- (RegisterEAX aRegister) {
            new Dec { DestinationReg = aRegister.GetId() };
            return aRegister;
        }
        public static RegisterEAX operator<< (RegisterEAX aRegister, int aCount) {
            new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
            return aRegister;
        }
        public static RegisterEAX operator>> (RegisterEAX aRegister, int aCount) {
            new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
            return aRegister;
        }

        public static implicit operator RegisterEAX(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterEAX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEAX(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEAX(RegisterEBX aReg) {
            Instance.Move(aReg.GetId());
            return Instance;
        }

        public static implicit operator RegisterEAX(RegisterECX aReg) {
            Instance.Move(aReg.GetId());
            return Instance;
        }

        public static implicit operator RegisterEAX(RegisterESI aReg)
        {
            Instance.Move(aReg.GetId());
            return Instance;
        }

        public static implicit operator RegisterEAX(RegisterEDX aReg) {
            Instance.Move(aReg.GetId());
            return Instance;
        }

        public static implicit operator PortNumber(RegisterEAX aEAX) {
            return new PortNumber(aEAX.GetId());
        }

        public void RotateRight(int aCount) {
            new RotateRight { DestinationReg = Registers.EBX, SourceValue = (uint)aCount };
        }

    }
}
