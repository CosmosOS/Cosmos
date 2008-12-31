using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class MemoryAction {
        public uint? Value;
        public readonly Guid Register;
        public readonly ElementReference Reference;
        public readonly int Displacement;
        
        public override string ToString() {
            if (Value.HasValue) {
                return Value.Value.ToString();
            } else {
                if (Reference != null) {
                    return Reference.ToString();
                }
                return Registers.GetRegisterName(Register);
            }
        }

        public static implicit operator MemoryAction(UInt32 aValue) {
            return new MemoryAction(aValue);
        }

        public static implicit operator MemoryAction(Register aRegister) {
            return new MemoryAction(aRegister.GetId());
        }

        public static MemoryAction operator ++(MemoryAction aTarget) {
            aTarget.ApplyToDest(new X86.Inc());
            return null;
        }

        public static MemoryAction operator --(MemoryAction aTarget) {
            aTarget.ApplyToDest(new X86.Dec());
            return null;
        }

        public bool IsIndirect { get; set; }
        // For registers
        public MemoryAction(Guid aRegister) {
            Register = aRegister;
        }

        public MemoryAction(Guid aRegister, int aDisplacement):this(aRegister) {
            Displacement = aDisplacement;
        }
        // This form used for reading memory - Addresses are passed in
        public MemoryAction(ElementReference aValue, int aDisplacement)
            : this(aValue) {
            Displacement = aDisplacement;
        }
        public MemoryAction(ElementReference aValue) {
            Reference = aValue;
        }

        // For constants/literals
        public MemoryAction(UInt32 aValue, int aDisplacement)
            : this(aValue) {
            Displacement = aDisplacement;
        }

        public MemoryAction(UInt32 aValue) {
            Value = aValue;
        }

        public byte Size { get; set; }

        public static string SizeToString(byte aSize) {
            switch (aSize) {
                case 8:
                    return "byte";
                case 16:
                    return "word";
                case 32:
                    return "dword";
                case 64:
                    return "qword";
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        }

        //TODO: Put memory compare here - will later have to limit it to the size
        // variant and if possible to self usage, ie no assignments. May however result 
        // in too many class variants to be worth while.
        public void Compare(UInt32 aValue) {
            Value = aValue;
            ApplyToDestAndSource(new Compare());
        }

        private void ApplyToDest(InstructionWithDestinationAndSize aDest) {
            aDest.Size = Size;
            aDest.DestinationReg = Register;
            aDest.DestinationRef = Reference;
            aDest.DestinationIsIndirect = IsIndirect;
            aDest.DestinationDisplacement = Displacement;
        }

        private void ApplyToDestAndSource(InstructionWithDestinationAndSourceAndSize aInstruction) {
            aInstruction.Size = Size;
            aInstruction.DestinationReg = Register;
            aInstruction.DestinationRef = Reference;
            aInstruction.DestinationIsIndirect = IsIndirect;
            aInstruction.DestinationDisplacement = Displacement;
            aInstruction.SourceValue = Value.GetValueOrDefault();
        }
    }
}
