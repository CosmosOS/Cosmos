using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Lost.JIT.AMD64OLD
{
	class InstructionPrefix
	{
		public InstructionPrefix(string name)
		{
			if (!_prefixes.Contains(name))
				throw new ArgumentException("name");

			_name = name;
		}

		public string Name
		{
			get { return _name; }
		} string _name;

		public override string ToString()
		{
			return Name;
		}

		#region Parsing
		static readonly HashSet<string> _prefixes = new HashSet<string>() {
			"lock",
		};
		static InstructionPrefix()
		{
			var sb = new StringBuilder();
			foreach (var prefix in _prefixes) sb.AppendFormat("{0}|", prefix);

			Debug.Assert(sb.Length > 0);
			sb.Remove(sb.Length - 1, 1);

			_regex =  new Regex(
				string.Format(@"^(\s*(?<prefix>({0})))*(?<rest>.*$)", sb),
				RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		static readonly Regex _regex;

		public static InstructionPrefix[] ExtractPrefixes(ref string instruction)
		{
			var match = _regex.Match(instruction);

			var result = new InstructionPrefix[match.Groups["prefix"].Captures.Count];
			for (int i = 0; i < result.Length; i++)
				result[i] = new InstructionPrefix(match.Groups["prefix"].Captures[i].Value);

			instruction = match.Groups["rest"].Value;

			return result;
		}
		#endregion
	}
}
