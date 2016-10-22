using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    public abstract class Instruction : Cosmos.Assembler.Instruction
    {
        /// <summary>
        /// ARMv7 mnemonic conditions
        /// </summary>
        public static Dictionary<ConditionsEnum, string> Conditions = new Dictionary<ConditionsEnum, string>()
        {
            { ConditionsEnum.Equal, "EQ" },
            { ConditionsEnum.EqualsZero, "EQ" },
            { ConditionsEnum.NotEqual, "NE" },
            { ConditionsEnum.CarrySet, "CS" },
            { ConditionsEnum.UnsignedHigherOrSame, "HS" },
            { ConditionsEnum.CarryClear, "CC" },
            { ConditionsEnum.UnsignedLower, "LO" },
            { ConditionsEnum.Minus, "MI" },
            { ConditionsEnum.Negative, "MI" },
            { ConditionsEnum.Plus, "PL" },
            { ConditionsEnum.PositiveOrZero, "PL" },
            { ConditionsEnum.Overflow, "VS" },
            { ConditionsEnum.NoOverflow, "VC" },
            { ConditionsEnum.UnsignedHigher, "HI" },
            { ConditionsEnum.UnsignedLowerOrSame, "LS" },
            { ConditionsEnum.SignedGreaterThanOrEqual, "GE" },
            { ConditionsEnum.SignedLessThan, "LT" },
            { ConditionsEnum.SignedGreaterThan, "GT" },
            { ConditionsEnum.SignedLessThanOrEqual, "LE" },
            { ConditionsEnum.Always, "AL" }
        };

        public ConditionsEnum? Condition
        {
            get;
            set;
        }

        protected Instruction(string mnemonic = null)
        {
        }

        protected Instruction(bool aAddToAssembler, string mnemonic = null) : base(aAddToAssembler, mnemonic)
        {
        }
    }
}
