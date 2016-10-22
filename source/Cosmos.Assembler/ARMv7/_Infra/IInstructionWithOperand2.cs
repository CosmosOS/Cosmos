using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOperand2
    {
        RegistersEnum? Operand2Reg
        {
            get;
            set;
        }

        uint? Operand2Value
        {
            get;
            set;
        }

        Operand2Shift Operand2Shift
        {
            get;
            set;
        }
    }

    public class Operand2Shift
    {
        public Operand2ShiftType ShiftType
        {
            get;
            set;
        }

        public RegistersEnum? ShiftRegister
        {
            get;
            set;
        }

        public ushort? ShiftValue
        {
            get;
            set;
        }

        public Operand2Shift(Operand2ShiftType shiftType, RegistersEnum shiftRegister)
        {
            ShiftType = shiftType;
            ShiftRegister = shiftRegister;
        }

        public Operand2Shift(Operand2ShiftType shiftType, ushort shiftValue)
        {
            ShiftType = shiftType;
            ShiftValue = shiftValue;
        }

        private static Dictionary<Operand2ShiftType, string> mShiftNames = new Dictionary<Operand2ShiftType, string>()
        {
            { Operand2ShiftType.ArithmeticShiftRight, "ASR" },
            { Operand2ShiftType.LogicalShiftLeft, "LSL" },
            { Operand2ShiftType.LogicalShiftRight, "LSR" },
            { Operand2ShiftType.RotateRight, "ROR" },
            { Operand2ShiftType.RotateRightWithExtend, "RRX" }
        };

        public static string GetShiftName(Operand2ShiftType shiftType)
        {
            return mShiftNames[shiftType];
        }
    }
}
