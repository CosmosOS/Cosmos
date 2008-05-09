using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class LabelOperand: InstructionOperand
	{
		public string Label { get; set; }

		public static implicit operator LabelOperand(string label)
		{
			return new LabelOperand() { Label = label };
		}
	}
}
