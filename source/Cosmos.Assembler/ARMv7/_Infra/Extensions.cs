using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    public static class InfraExtensions
    {
        private static Dictionary<DataSize, string> mDataSizeNames = new Dictionary<DataSize, string>()
        {
            { DataSize.Byte, "B" },
            { DataSize.SignedByte, "SB" },
            { DataSize.Halfword, "B" },
            { DataSize.SignedHalfword, "H" },
            { DataSize.Word, "" },
            { DataSize.Doubleword, "SH" }
        };

        public static string GetConditionAsString(this Instruction aThis)
        {
            string xCondition = "";

            if (aThis.Condition.HasValue)
            {
                xCondition = Instruction.Conditions[aThis.Condition.Value];
            }

            return xCondition;
        }

        public static string GetDataSizeAsString(this IInstructionWithDataSize aThis)
        {
            return aThis.DataSize.HasValue ? mDataSizeNames[aThis.DataSize.Value] : "";
        }

        public static string GetDestinationAsString(this IInstructionWithDestination aThis)
        {
            string xDestination = "";

            if (aThis.DestinationReg != null)
            {
                xDestination = Registers.GetRegisterName(aThis.DestinationReg.Value);
            }

            return xDestination;
        }

        public static string GetFirstOperandAsString(this IInstructionWithOperand aThis)
        {
            string xOperand = "";

            if (aThis.FirstOperandReg != null)
            {
                xOperand = Registers.GetRegisterName(aThis.FirstOperandReg.Value);
            }

            return xOperand;
        }

        public static string GetSecondOperandAsString(this IInstructionWithOperand aThis)
        {
            string xOperand = "";

            if (aThis.FirstOperandReg != null)
            {
                xOperand = Registers.GetRegisterName(aThis.FirstOperandReg.Value);
            }

            return xOperand;
        }

        public static string GetLabelAsString(this IInstructionWithLabel aThis)
        {
            string xLabel = "";

            if (!string.IsNullOrEmpty(aThis.Label))
            {
                xLabel = aThis.Label;

                if (aThis.LabelOffset.HasValue && aThis.LabelOffset != 0)
                {
                    xLabel += aThis.LabelOffset.Value > 0 ? "+" : "-" + aThis.LabelOffset.Value.ToString();
                }
            }

            return xLabel;
        }

        public static string GetOperand2AsString(this IInstructionWithOperand2 aThis)
        {
            string xOperand2 = "";

            if (aThis.Operand2Reg != null && (aThis.Operand2Shift.ShiftRegister.HasValue || aThis.Operand2Shift.ShiftValue.HasValue))
            {
                xOperand2 = Registers.GetRegisterName(aThis.Operand2Reg.Value);

                if (aThis.Operand2Shift != null)
                {
                    xOperand2 += ", " + Operand2Shift.GetShiftName(aThis.Operand2Shift.ShiftType);

                    if (aThis.Operand2Shift.ShiftRegister.HasValue)
                    {
                        xOperand2 += " " + Registers.GetRegisterName(aThis.Operand2Shift.ShiftRegister.Value);
                    }
                    else
                    {
                        xOperand2 += " #" + aThis.Operand2Shift.ShiftValue.Value.ToString();
                    }
                }
            }
            else if(aThis.Operand2Value.HasValue)
            {
                xOperand2 = "#" + aThis.Operand2Value.Value.ToString();
            }

            return xOperand2;
        }

        public static string GetMemoryAddressAsString(this IInstructionWithMemoryAddress aThis)
        {
            string memoryAddress = "";

            memoryAddress = "[";

            if (aThis.BaseMemoryAddressReg.HasValue)
            {
                memoryAddress += Registers.GetRegisterName(aThis.BaseMemoryAddressReg.Value);

                if (aThis.MemoryAddressOffsetType == MemoryAddressOffsetType.PostIndexedOffset)
                {
                    memoryAddress += "]";
                }

                if (aThis.MemoryAddressOffset.HasValue)
                {
                    memoryAddress += ", #" + aThis.MemoryAddressOffset.Value.ToString();
                }
            }

            if (aThis.MemoryAddressOffsetType != MemoryAddressOffsetType.PostIndexedOffset)
            {
                memoryAddress += "]";
            }

            if(aThis.MemoryAddressOffsetType == MemoryAddressOffsetType.PreIndexedOffset)
            {
                memoryAddress += "!";
            }

            return memoryAddress;
        }

        public static string GetReglistAsString(this IInstructionWithReglist aThis)
        {
            if(aThis.Reglist != null && aThis.Reglist.Length > 0)
            {
                //TODO: Check if grouping 'r' registers like r3-r5 optimizes code (commented code is not complete)
                //
                //List<string> xRegisters = new List<string>();
                //List<ushort> rRegistersNumbers = new List<ushort>();

                //foreach (RegistersEnum reg in aThis.Reglist)
                //{
                //    string regStr = Registers.GetRegisterName(reg);

                //    if (regStr.StartsWith("r"))
                //    {
                //        ushort rRegisterNumber;

                //        ushort.TryParse(regStr.Substring(1), out rRegisterNumber);

                //        rRegistersNumbers.Add(rRegisterNumber);
                //    }
                //}

                //rRegistersNumbers.Sort();

                //bool c;

                //for (ushort i = 0; i < rRegistersNumbers.Count; i++)
                //{
                //    c = rRegistersNumbers[i] == rRegistersNumbers[i - 1] + 1 ? true : false;
                //}

                return "{" + string.Join(",", aThis.Reglist) + "}";
            }
            else
            {
                return "";
            }
        }
    }
}
