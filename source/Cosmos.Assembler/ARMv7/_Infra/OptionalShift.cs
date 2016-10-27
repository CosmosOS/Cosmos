using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    public class OptionalShift
    {
        public OptionalShiftType ShiftType
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

        public OptionalShift(OptionalShiftType shiftType, RegistersEnum shiftRegister)
        {
            ShiftType = shiftType;
            ShiftRegister = shiftRegister;
        }

        public OptionalShift(OptionalShiftType shiftType, ushort shiftValue)
        {
            ShiftType = shiftType;
            ShiftValue = shiftValue;
        }

        private static Dictionary<OptionalShiftType, string> mShiftNames = new Dictionary<OptionalShiftType, string>()
        {
            { OptionalShiftType.ArithmeticShiftRight, "ASR" },
            { OptionalShiftType.LogicalShiftLeft, "LSL" },
            { OptionalShiftType.LogicalShiftRight, "LSR" },
            { OptionalShiftType.RotateRight, "ROR" },
            { OptionalShiftType.RotateRightWithExtend, "RRX" }
        };

        public static string GetShiftName(OptionalShiftType shiftType)
        {
            return mShiftNames[shiftType];
        }
    }
}
