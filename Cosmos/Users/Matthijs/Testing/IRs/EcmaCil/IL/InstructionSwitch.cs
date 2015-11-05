using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionSwitch: BaseInstruction
    {
        public InstructionSwitch(int aCurrentIndex)
            : base(InstructionKindEnum.Switch, aCurrentIndex)
        {
        }

        public BaseInstruction[] Targets
        {
            get;
            set;
        }
    }
}
