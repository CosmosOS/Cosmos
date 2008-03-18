using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lost.JIT.AMD64
{
	class InstructionOperand
	{
		static readonly Regex _regex = new Regex(
			@"^(\s*(?<operand>(\w|\[|\]|\+|\-)+)\s*\,)*\s*$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		internal static InstructionOperand[] GetOperands(string code)
		{
			if (code.Trim() != "") code += ",";
			var match = _regex.Match(code);

			var result = new InstructionOperand[match.Groups["operand"].Captures.Count];
			for (int i = 0; i < result.Length; i++)
				result[i] = InstructionOperand.Parse(match.Groups["operand"].Captures[i].Value);

			return result;
		}

		private static InstructionOperand Parse(string operand)
		{
#warning TODO: IntstructionOperand.Parse is not implemented
			return null;
		}
	}
}
