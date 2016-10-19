using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    public abstract class Instruction : Cosmos.Assembler.Instruction
    {
        /// <summary>
        /// ARMv7 mnemonic conditions
        /// </summary>
        public static Dictionary<ConditionEnum, string> Conditions = new Dictionary<ConditionEnum, string>()
        {
            { ConditionEnum.Equal, "EQ" },
            { ConditionEnum.EqualsZero, "EQ" },
            { ConditionEnum.NotEqual, "NE" },
            { ConditionEnum.CarrySet, "CS" },
            { ConditionEnum.UnsignedHigherOrSame, "HS" },
            { ConditionEnum.CarryClear, "CC" },
            { ConditionEnum.UnsignedLower, "LO" },
            { ConditionEnum.Minus, "MI" },
            { ConditionEnum.Negative, "MI" },
            { ConditionEnum.Plus, "PL" },
            { ConditionEnum.PositiveOrZero, "PL" },
            { ConditionEnum.Overflow, "VS" },
            { ConditionEnum.NoOverflow, "VC" },
            { ConditionEnum.UnsignedHigher, "HI" },
            { ConditionEnum.UnsignedLowerOrSame, "LS" },
            { ConditionEnum.SignedGreaterThanOrEqual, "GE" },
            { ConditionEnum.SignedLessThan, "LT" },
            { ConditionEnum.SignedGreaterThan, "GT" },
            { ConditionEnum.SignedLessThanOrEqual, "LE" },
            { ConditionEnum.Always, "AL" }
        };

        public ConditionEnum? Condition
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
