using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Assembler.XSharp {
    public class MemoryAction {
        public uint? Value;
        public readonly RegistersEnum? Register;
        public readonly ElementReference Reference;
        public readonly int Displacement;
        
        public override string ToString() {
            if (Value.HasValue) {
                return Value.Value.ToString();
            } else {
                if (Reference != null) {
                    return Reference.ToString();
                }
                return Registers.GetRegisterName(Register.Value);
            }
        }

        public static implicit operator MemoryAction(UInt32 aValue) {
            return new MemoryAction(aValue);
        }

        public static implicit operator MemoryAction(Register aRegister) {
            return new MemoryAction(aRegister.GetId());
        }

        public static MemoryAction operator ++(MemoryAction aTarget) {
            aTarget.ApplyToDest(new INC());
            // Must return null, see DataMember.this[] comment
            return null;
        }

        public static MemoryAction operator --(MemoryAction aTarget) {
            aTarget.ApplyToDest(new Dec());
            // Must return null, see DataMember.this[] comment
            return null;
        }

        public bool IsIndirect { get; set; }
        // For registers
        public MemoryAction(RegistersEnum aRegister)
        {
            Register = aRegister;
            // Apparently MemoryAction is sometimes used for registers, which it should not :(
            // so for memory, use the regsiter, int variant.. Its hacked for now till this can
            // be rewritten.
            //IsIndirect = true;
        }

        public MemoryAction(RegistersEnum aRegister, int aDisplacement)
            : this(aRegister)
        {
            Displacement = aDisplacement;
            IsIndirect = true;
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
