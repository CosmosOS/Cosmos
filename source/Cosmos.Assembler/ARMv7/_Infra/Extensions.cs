namespace Cosmos.Assembler.ARMv7
{
    public static class InfraExtensions
    {
        public static string GetConditionAsString(this Instruction aThis)
        {
            string xCondition = "";

            if (aThis.Condition.HasValue)
            {
                xCondition = Instruction.Conditions[aThis.Condition.Value];
            }

            return xCondition;
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

        public static string GetSecondOperandAsString(this IInstructionWithOperand2 aThis)
        {
            string xOperand2 = "";

            if (aThis.SecondOperandReg != null)
            {
                xOperand2 = Registers.GetRegisterName(aThis.SecondOperandReg.Value);
            }
            else if(aThis.SecondOperandValue != 0)
            {
                xOperand2 = "#" + aThis.SecondOperandValue.ToString();
            }

            return xOperand2;
        }

        public static string GetLabelAsString(this IInstructionWithLabel aThis)
        {
            string xLabel = "";

            if (!string.IsNullOrEmpty(aThis.Label))
            {
                xLabel = aThis.Label;

                if(aThis.LabelOffset.HasValue && aThis.LabelOffset != 0)
                {
                    xLabel += aThis.LabelOffset.Value > 0 ? "+" : "-" + aThis.LabelOffset.Value.ToString();
                }
            }

            return xLabel;
        }
    }
}
