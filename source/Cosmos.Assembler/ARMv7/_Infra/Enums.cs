namespace Cosmos.Assembler.ARMv7
{
    public enum ConditionsEnum
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

    public enum Operand2ShiftType
    {
        ArithmeticShiftRight,
        LogicalShiftLeft,
        LogicalShiftRight,
        RotateRight,
        RotateRightWithExtend
    }

    public enum DataSize
    {
        Byte,
        SignedByte,
        Halfword,
        SignedHalfword,
        Word,
        Doubleword
    }

    public enum MemoryAddressOffsetType
    {
        ImmediateOffset,
        PreIndexedOffset,
        PostIndexedOffset
    }
}
