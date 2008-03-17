using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lost.JIT.AMD64
{
	class InstructionLabel
	{
		internal InstructionLabel(string name)
		{
			_name = name;

#warning TODO: check label for validity
		}

		static readonly Regex _regex = new Regex(
			@"^(\s*(?<label>\w+)\u003A)*(?<rest>.*$)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		public static InstructionLabel[] ExtractLabels(ref string instruction)
		{
			var match = _regex.Match(instruction);

			var result = new InstructionLabel[match.Groups["label"].Captures.Count];
			for (int i = 0; i < result.Length; i++)
				result[i] = new InstructionLabel(match.Groups["label"].Captures[i].Value);

			instruction = match.Groups["rest"].Value;

			return result;
		}

		public string Name
		{
			get { return _name; }
		} string _name;

		public override string ToString()
		{
			return string.Format("{0}:", Name);
		}
	}
}
