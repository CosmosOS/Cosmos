using System.Collections.Generic;
using System.Reflection;

using Cosmos.Assembler.ARMv7;

namespace ASharp.Compiler
{
    public static class ASConditions
    {
        public class Condition
        {
            public readonly string Name;
            public readonly ConditionsEnum ConditionEnum;

            public Condition(ConditionsEnum conditionEnum)
            {
                Name = Instruction.Conditions[conditionEnum];
                ConditionEnum = conditionEnum;
            }

            public static implicit operator ConditionsEnum(Condition condition)
            {
                return condition.ConditionEnum;
            }
        }

        private static readonly Dictionary<string, Condition> mConditions;

        static ASConditions()
        {
            mConditions = new Dictionary<string, Condition>();

            foreach (var xField in typeof(ASRegisters).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                mConditions.Add(xField.Name, (Condition)xField.GetValue(null));
            }
        }

        public static Dictionary<string, Condition> GetConditions()
        {
            return mConditions;
        }

        public static readonly Condition Equal = new Condition(ConditionsEnum.Equal);
        public static readonly Condition EqualsZero = new Condition(ConditionsEnum.EqualsZero);
        public static readonly Condition NotEqual = new Condition(ConditionsEnum.NotEqual);
        public static readonly Condition CarrySet = new Condition(ConditionsEnum.CarrySet);
        public static readonly Condition UnsignedHigherOrSame = new Condition(ConditionsEnum.UnsignedHigherOrSame);
        public static readonly Condition CarryClear = new Condition(ConditionsEnum.CarryClear);
        public static readonly Condition UnsignedLower = new Condition(ConditionsEnum.UnsignedLower);
        public static readonly Condition Minus = new Condition(ConditionsEnum.Minus);
        public static readonly Condition Negative = new Condition(ConditionsEnum.Negative);
        public static readonly Condition Plus = new Condition(ConditionsEnum.Plus);
        public static readonly Condition PositiveOrZero = new Condition(ConditionsEnum.PositiveOrZero);
        public static readonly Condition Overflow = new Condition(ConditionsEnum.Overflow);
        public static readonly Condition NoOverflow = new Condition(ConditionsEnum.NoOverflow);
        public static readonly Condition UnsignedHigher = new Condition(ConditionsEnum.UnsignedHigher);
        public static readonly Condition UnsignedLowerOrSame = new Condition(ConditionsEnum.UnsignedLowerOrSame);
        public static readonly Condition SignedGreaterThanOrEqual = new Condition(ConditionsEnum.SignedGreaterThanOrEqual);
        public static readonly Condition SignedLessThan = new Condition(ConditionsEnum.SignedLessThan);
        public static readonly Condition SignedGreaterThan = new Condition(ConditionsEnum.SignedGreaterThan);
        public static readonly Condition SignedLessThanOrEqual = new Condition(ConditionsEnum.SignedLessThanOrEqual);
        public static readonly Condition Always = new Condition(ConditionsEnum.Always);
    }
}
