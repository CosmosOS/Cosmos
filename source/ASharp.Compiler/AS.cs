using System.Collections.Generic;

using Cosmos.Assembler;
using Cosmos.Assembler.ARMv7;
using Label = Cosmos.Assembler.Label;

namespace ASharp.Compiler
{
    using static ASConditions;
    using static ASRegisters;

    public static class AS
    {
        #region Instruction

        private static void Do<T>(Condition condition = null)
          where T : Cosmos.Assembler.ARMv7.Instruction, new()
        {
            new T
            {
                Condition = condition,
            };
        }

        #endregion

        #region InstructionWithDataSizeAndOperandsAndMemoryAddress

        private static void Do<T>(Register firstOperand, Register baseMemoryAddress, Register memoryAddressOffset, OptionalShift memoryAddressOptionalShift, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
          where T : InstructionWithDataSizeAndOperandsAndMemoryAddress, new()
        {
            new T
            {
                Condition = condition,
                DataSize = dataSize,
                OperandReg = firstOperand,
                BaseMemoryAddressReg = baseMemoryAddress,
                MemoryAddressOffsetReg = memoryAddressOffset,
                MemoryAddressOffsetType = memoryAddressOffsetType,
                MemoryAddressOptionalShift = memoryAddressOptionalShift
            };
        }

        private static void Do<T>(Register firstOperand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
          where T : InstructionWithDataSizeAndOperandsAndMemoryAddress, new()
        {
            new T
            {
                Condition = condition,
                DataSize = dataSize,
                OperandReg = firstOperand,
                BaseMemoryAddressReg = baseMemoryAddress,
                MemoryAddressOffsetValue = memoryAddressOffset,
                MemoryAddressOffsetType = memoryAddressOffsetType
            };
        }

        private static void Do<T>(Register firstOperand, Register secondOperand, Register baseMemoryAddress, Register memoryAddressOffset, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
          where T : InstructionWithDataSizeAndOperandsAndMemoryAddress, new()
        {
            new T
            {
                Condition = condition,
                DataSize = dataSize,
                OperandReg = firstOperand,
                SecondOperandReg = secondOperand,
                BaseMemoryAddressReg = baseMemoryAddress,
                MemoryAddressOffsetReg = memoryAddressOffset,
                MemoryAddressOffsetType = memoryAddressOffsetType
            };
        }

        private static void Do<T>(Register firstOperand, Register secondOperand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
          where T : InstructionWithDataSizeAndOperandsAndMemoryAddress, new()
        {
            new T
            {
                Condition = condition,
                DataSize = dataSize,
                OperandReg = firstOperand,
                SecondOperandReg = secondOperand,
                BaseMemoryAddressReg = baseMemoryAddress,
                MemoryAddressOffsetValue = memoryAddressOffset,
                MemoryAddressOffsetType = memoryAddressOffsetType
            };
        }

        #endregion

        #region InstructionWithDestination

        private static void Do<T>(Register destination, Condition condition = null)
          where T : InstructionWithDestination, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination
            };
        }

        #endregion

        #region InstructionWithDestinationAndLabel

        private static void Do<T>(Register destination, string label, uint? labelOffset = null, Condition condition = null)
          where T : InstructionWithDestinationAndLabel, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination,
                Label = label,
                LabelOffset = labelOffset
            };
        }

        #endregion

        #region InstructionWithDestinationAndOperand

        private static void Do<T>(Register destination, Register operand, Condition condition = null)
          where T : InstructionWithDestinationAndOperand, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination,
                OperandReg = operand
            };
        }

        #endregion

        #region InstructionWithDestinationAndTwoOperands

        private static void Do<T>(Register destination, Register firstOperand, Register secondOperand, Condition condition = null)
          where T : InstructionWithDestinationAndTwoOperands, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination,
                OperandReg = firstOperand,
                SecondOperandReg = secondOperand
            };
        }

        #endregion

        #region InstructionWithLabel

        private static void Do<T>(string label, uint? labelOffset = null, Condition condition = null)
          where T : Cosmos.Assembler.ARMv7.Instruction, IInstructionWithLabel, new()
        {
            new T
            {
                Condition = condition,
                Label = label,
                LabelOffset = labelOffset
            };
        }

        #endregion

        #region InstructionWithOptionalSuffixAndDestinationAndOperand

        private static void Do<T>(Register destination, Register firstOperand, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand, new()
        {
            new T
            {
                Condition = condition,
                UpdateFlags = updateFlags,
                DestinationReg = destination,
                OperandReg = firstOperand
            };
        }

        #endregion

        #region InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2

        private static void Do<T>(Register destination, Register operand2, OptionalShift operand2Shift = null, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination,
                Operand2Reg = operand2,
                Operand2Shift = operand2Shift,
                UpdateFlags = updateFlags
            };
        }

        private static void Do<T>(Register destination, uint operand2, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                DestinationReg = destination,
                Operand2Value = operand2,
                UpdateFlags = updateFlags
            };
        }

        #endregion

        #region InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2

        private static void Do<T>(Register destination, Register firstOperand, Register operand2, OptionalShift operand2Shift = null, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                UpdateFlags = updateFlags,
                DestinationReg = destination,
                OperandReg = firstOperand,
                Operand2Reg = operand2,
                Operand2Shift = operand2Shift
            };
        }

        private static void Do<T>(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                UpdateFlags = updateFlags,
                DestinationReg = destination,
                OperandReg = firstOperand,
                Operand2Value = secondOperand
            };
        }

        #endregion

        #region InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands

        private static void Do<T>(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
          where T : InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands, new()
        {
            new T
            {
                Condition = condition,
                UpdateFlags = updateFlags,
                DestinationReg = destination,
                OperandReg = firstOperand,
                SecondOperandReg = secondOperand,
            };
        }

        #endregion

        #region InstructionWithReglist

        private static void Do<T>(List<Register> registerList, Condition condition = null)
          where T : InstructionWithReglist, new()
        {
            RegistersEnum[] reglist = new RegistersEnum[registerList.Count];

            for (ushort i = 0; i < registerList.Count; i++)
            {
                reglist[i] = registerList[i].RegEnum;
            }

            new T
            {
                Condition = condition,
                Reglist = reglist
            };
        }

        #endregion

        #region InstructionWithOperandAndOperand2

        private static void Do<T>(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, Condition condition = null)
          where T : InstructionWithOperandAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                OperandReg = firstOperand,
                Operand2Reg = secondOperand,
                Operand2Shift = secondOperandShift
            };
        }

        private static void Do<T>(Register firstOperand, uint secondOperand, Condition condition = null)
          where T : InstructionWithOperandAndOperand2, new()
        {
            new T
            {
                Condition = condition,
                OperandReg = firstOperand,
                Operand2Value = secondOperand
            };
        }

        #endregion

        #region ShiftInstruction

        private static void Do<T>(Register destination, Register firstOperand, byte bitsToShift, bool updateFlags = false, Condition condition = null)
          where T : ShiftInstruction, new()
        {
            new T
            {
                Condition = condition,
                UpdateFlags = updateFlags,
                DestinationReg = destination,
                OperandReg = firstOperand,
                ShiftValue = bitsToShift
            };
        }

        #endregion

        public static void Comment(string comment)
        {
            new Comment(comment);
        }

        public static void Const(string name, string value)
        {
            new LiteralAssemblerCode(".equ " + name + ", " + value);
        }

        public static void DataMember(string name, uint value = 0)
        {
            Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
        }

        public static void DataMember(string name, string value)
        {
            Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
        }

        public static void DataMemberBytes(string name, byte[] value)
        {
            Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
        }

        public static void DataMember(string name, uint elementCount, string size, string value)
        {
            Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, size, value));
        }

        public static void Label(string label, bool isGlobal = false)
        {
            new Label(label, isGlobal);
        }

        public static void LiteralCode(string code)
        {
            new LiteralAssemblerCode(code);
        }

        public static void Add(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Add>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void Add(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Add>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void Add(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<Add>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void Add(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<Add>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void AddWithCarry(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<AddWithCarry>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void AddWithCarry(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<AddWithCarry>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void AddWithCarry(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<AddWithCarry>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void AddWithCarry(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<AddWithCarry>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void And(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, Condition condition = null)
        {
            Do<And>(destination, firstOperand, secondOperand, secondOperandShift, false, condition);
        }

        public static void And(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<And>(destination, firstOperand, secondOperand, false, condition);
        }

        public static void ArithmeticShiftRight(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<ArithmeticShiftRight>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void ArithmeticShiftRight(Register destination, Register operand, byte bitsToShift, bool updateFlags = false, Condition condition = null)
        {
            Do<ArithmeticShiftRight>(destination, operand, bitsToShift, updateFlags, condition);
        }

        public static void BitClear(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<BitClear>(destination, firstOperand, secondOperand, null, updateFlags, condition);
        }

        public static void BitClear(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<BitClear>(destination, firstOperand, secondOperand, false, condition);
        }

        public static void Branch(string label, uint? labelOffset = null, Condition condition = null)
        {
            Do<Branch>(label, labelOffset, condition);
        }

        public static void BranchAndExchange(Register operand, Condition condition = null)
        {
            Do<BranchAndExchange>(operand, condition);
        }

        public static void BranchWithLink(string label, uint? labelOffset = null, Condition condition = null)
        {
            Do<BranchWithLink>(label, labelOffset, condition);
        }

        public static void BranchWithLinkAndExchange(Register operand, Condition condition = null)
        {
            Do<BranchWithLinkAndExchange>(operand, condition);
        }

        public static void BranchWithLinkAndExchange(string label, uint? labelOffset = null, Condition condition = null)
        {
            Do<BranchWithLinkAndExchange>(label, labelOffset, condition);
        }

        public static void Compare(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift, Condition condition = null)
        {
            Do<Compare>(firstOperand, secondOperand, secondOperandShift, condition);
        }

        public static void Compare(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<Compare>(firstOperand, secondOperand, condition);
        }

        public static void CompareNegative(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift, Condition condition = null)
        {
            Do<CompareNegative>(firstOperand, secondOperand, secondOperandShift, condition);
        }

        public static void CompareNegative(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<CompareNegative>(firstOperand, secondOperand, condition);
        }

        public static void CountLeadingZeros(Register destination, Register operand, Condition condition = null)
        {
            Do<CountLeadingZeros>(destination, operand, condition);
        }

        public static void ExceptionReturn(Condition condition = null)
        {
            Do<ExceptionReturn>(condition);
        }

        public static void ExclusiveOr(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<ExclusiveOr>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void ExclusiveOr(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<ExclusiveOr>(destination, firstOperand, secondOperand, false, condition);
        }

        public static void LabelAddress(Register destination, string label, uint? labelOffset = null, Condition condition = null)
        {
            Do<LabelAddress>(destination, label, labelOffset, condition);
        }

        public static void LoadLabelAddress(Register destination, string label, uint? labelOffset = null, Condition condition = null)
        {
            Do<LoadLabelAddress>(destination, label, labelOffset, condition);
        }

        public static void LoadRegister(Register operand, Register baseMemoryAddress, Register memoryAddressOffset, OptionalShift memoryAddressOptionalShift = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<LoadRegister>(operand, baseMemoryAddress, memoryAddressOffset, memoryAddressOptionalShift, memoryAddressOffsetType, dataSize, condition);
        }

        public static void LoadRegister(Register operand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<LoadRegister>(operand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void LoadRegister(Register firstOperand, Register secondOperand, Register baseMemoryAddress, Register memoryAddressOffset, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<LoadRegister>(firstOperand, secondOperand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void LoadRegister(Register firstOperand, Register secondOperand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<LoadRegister>(firstOperand, secondOperand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void LogicalShiftLeft(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<LogicalShiftLeft>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void LogicalShiftLeft(Register destination, Register operand, byte bitsToShift, bool updateFlags = false, Condition condition = null)
        {
            Do<LogicalShiftLeft>(destination, operand, bitsToShift, updateFlags, condition);
        }

        public static void LogicalShiftRight(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<LogicalShiftRight>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void LogicalShiftRight(Register destination, Register operand, byte bitsToShift, bool updateFlags = false, Condition condition = null)
        {
            Do<LogicalShiftRight>(destination, operand, bitsToShift, updateFlags, condition);
        }

        public static void Move(Register destination, Register operand2, OptionalShift operand2Shift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Move>(destination, operand2, operand2Shift, updateFlags, condition);
        }

        public static void Move(Register destination, uint operand2, bool updateFlags = false, Condition condition = null)
        {
            Do<Move>(destination, operand2, updateFlags, condition);
        }

        public static void MoveNot(Register destination, Register operand2, OptionalShift operand2Shift, bool updateFlags = false, Condition condition = null)
        {
            Do<MoveNot>(destination, operand2, operand2Shift, updateFlags, condition);
        }

        public static void MoveNot(Register destination, uint operand2, bool updateFlags = false, Condition condition = null)
        {
            Do<MoveNot>(destination, operand2, updateFlags, condition);
        }

        public static void Multiply(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Multiply>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void Multiply(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Multiply>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void Negate(Register destination, Register operand, Condition condition = null)
        {
            Do<Negate>(destination, operand, condition);
        }

        public static void NoOp(Condition condition = null)
        {
            Do<NoOp>(condition);
        }

        public static void Or(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Or>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void Or(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<Or>(destination, firstOperand, secondOperand, false, condition);
        }

        public static void OrNot(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<OrNot>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void OrNot(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<OrNot>(destination, firstOperand, secondOperand, false, condition);
        }

        public static void Pop(Register register, Condition condition = null)
        {
            Do<Pop>(new List<Register>() { register }, condition);
        }

        public static void Pop(List<Register> reglist, Condition condition = null)
        {
            Do<Pop>(reglist, condition);
        }

        public static void Push(Register register, Condition condition = null)
        {
            Do<Push>(new List<Register>() { register }, condition);
        }

        public static void Push(List<Register> reglist, Condition condition = null)
        {
            Do<Push>(reglist, condition);
        }

        public static void ReverseSubtract(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtract>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void ReverseSubtract(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtract>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void ReverseSubtract(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtract>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void ReverseSubtract(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtract>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void ReverseSubtractWithCarry(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtractWithCarry>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void ReverseSubtractWithCarry(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtractWithCarry>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void ReverseSubtractWithCarry(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtractWithCarry>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void ReverseSubtractWithCarry(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<ReverseSubtractWithCarry>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void RotateRight(Register destination, Register firstOperand, Register secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<RotateRight>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void RotateRight(Register destination, Register operand, byte bitsToShift, bool updateFlags = false, Condition condition = null)
        {
            Do<RotateRight>(destination, operand, bitsToShift, updateFlags, condition);
        }

        public static void RotateRightWithExtend(Register destination, Register firstOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<RotateRightWithExtend>(destination, firstOperand, updateFlags, condition);
        }

        public static void SetByte(uint address, byte value)
        {
            Do<Move>(r11, address, updateFlags: false);
            Do<Move>(r12, value, updateFlags: false);
            Do<StoreRegister>(r12, r11, dataSize: DataSize.Byte);
        }

        public static void SetEvent(Condition condition = null)
        {
            Do<SetEvent>(condition);
        }

        public static void SetEventLocally(Condition condition = null)
        {
            Do<SetEventLocally>(condition);
        }

        public static void SignedDivide(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<SignedDivide>(destination, firstOperand, secondOperand, condition);
        }

        public static void SignedDivide(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<SignedDivide>(null, firstOperand, secondOperand, condition);
        }

        public static void StoreRegister(Register operand, Register baseMemoryAddress, Register memoryAddressOffset, OptionalShift memoryAddressOptionalShift = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<StoreRegister>(operand, baseMemoryAddress, memoryAddressOffset, memoryAddressOptionalShift, memoryAddressOffsetType, dataSize, condition);
        }

        public static void StoreRegister(Register operand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<StoreRegister>(operand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void StoreRegister(Register firstOperand, Register secondOperand, Register baseMemoryAddress, Register memoryAddressOffset, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<StoreRegister>(firstOperand, secondOperand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void StoreRegister(Register firstOperand, Register secondOperand, Register baseMemoryAddress, short? memoryAddressOffset = null, MemoryAddressOffsetType memoryAddressOffsetType = MemoryAddressOffsetType.ImmediateOffset, DataSize dataSize = DataSize.Word, Condition condition = null)
        {
            Do<StoreRegister>(firstOperand, secondOperand, baseMemoryAddress, memoryAddressOffset, memoryAddressOffsetType, dataSize, condition);
        }

        public static void Subtract(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Subtract>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void Subtract(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<Subtract>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void Subtract(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<Subtract>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void Subtract(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<Subtract>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void SubtractWithCarry(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<SubtractWithCarry>(destination, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void SubtractWithCarry(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<SubtractWithCarry>(null, firstOperand, secondOperand, secondOperandShift, updateFlags, condition);
        }

        public static void SubtractWithCarry(Register destination, Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<SubtractWithCarry>(destination, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void SubtractWithCarry(Register firstOperand, uint secondOperand, bool updateFlags = false, Condition condition = null)
        {
            Do<SubtractWithCarry>(null, firstOperand, secondOperand, updateFlags, condition);
        }

        public static void TestBits(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift, Condition condition = null)
        {
            Do<TestBits>(firstOperand, secondOperand, secondOperandShift, condition);
        }

        public static void TestBits(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<TestBits>(firstOperand, secondOperand, condition);
        }

        public static void TestEquivalence(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift, Condition condition = null)
        {
            Do<TestEquivalence>(firstOperand, secondOperand, secondOperandShift, condition);
        }

        public static void TestEquivalence(Register destination, Register firstOperand, uint secondOperand, Condition condition = null)
        {
            Do<TestEquivalence>(firstOperand, secondOperand, condition);
        }

        public static void UnsignedDivide(Register destination, Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<UnsignedDivide>(destination, firstOperand, secondOperand, condition);
        }

        public static void UnsignedDivide(Register firstOperand, Register secondOperand, OptionalShift secondOperandShift = null, bool updateFlags = false, Condition condition = null)
        {
            Do<UnsignedDivide>(null, firstOperand, secondOperand, condition);
        }

        public static void WaitForEvent(Condition condition = null)
        {
            Do<WaitForEvent>(condition);
        }

        public static void WaitForInterrupt(Condition condition = null)
        {
            Do<WaitForInterrupt>(condition);
        }

        public static void Yield(Condition condition = null)
        {
            Do<Yield>(condition);
        }
    }
}
