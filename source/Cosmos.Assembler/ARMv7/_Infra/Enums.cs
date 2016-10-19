namespace Cosmos.Assembler.ARMv7
{
    public enum ConditionEnum : byte
    {
        Equal,
        EqualsZero,
        NotEqual,
        CarrySet,
        UnsignedHigherOrSame,
        CarryClear,
        UnsignedLower,
        Minus,
        Negative,
        Plus,
        PositiveOrZero,
        Overflow,
        NoOverflow,
        UnsignedHigher,
        UnsignedLowerOrSame,
        SignedGreaterThanOrEqual,
        SignedLessThan,
        SignedGreaterThan,
        SignedLessThanOrEqual,
        Always
    }
}
