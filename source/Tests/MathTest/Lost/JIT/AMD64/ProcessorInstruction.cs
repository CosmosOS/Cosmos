using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lost.JIT.AMD64
{
	class ProcessorInstruction
	{
		internal ProcessorInstruction()
		{
			_labels = new HashSet<InstructionLabel>();
			_prefixes = new HashSet<InstructionPrefix>();
		}

		internal ProcessorInstruction(string opCode, InstructionOperand[] operands, InstructionLabel[] labels, InstructionPrefix[] prefixes, string comments)
		{
			_opCode = opCode;
			_operands = new List<InstructionOperand>(operands);
			_labels = new HashSet<InstructionLabel>(labels);
			_prefixes = new HashSet<InstructionPrefix>(prefixes);
			_comments = comments;
		}

		ICollection<InstructionLabel> Labels
		{
			get { return _labels; }
		} HashSet<InstructionLabel> _labels;
		ICollection<InstructionPrefix> Prefixes
		{
			get { return _prefixes; }
		} HashSet<InstructionPrefix> _prefixes;
		IList<InstructionOperand> Operands
		{
			get { return _operands; }
		} List<InstructionOperand> _operands;
		string OpCode
		{
			get { return _opCode; }
		} string _opCode;
		string Comments
		{
			get { return _comments; }
		} string _comments;

		#region Parsing
		public static ProcessorInstruction Parse(string code)
		{
			var labels = InstructionLabel.ExtractLabels(ref code);
			var prefixes = InstructionPrefix.ExtractPrefixes(ref code);
			var instr = ExtractInstruction(ref code);
			var comments = ExtractComments(ref code);
			var operands = InstructionOperand.GetOperands(code);

#warning FIRST STEP
			var result = new ProcessorInstruction(instr, operands, labels, prefixes, comments);

			return result;
		}

		static readonly Regex _regexComments = new Regex(
			@"^(?<rest>.*)\/\/\s*(?<comments>.*)\s*$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static string ExtractComments(ref string code)
		{
			var match = _regexComments.Match(code);
			if (match == null) return null;
			if (!match.Success) return null;

			var value = match.Groups["comments"].Value;
			code = match.Groups["rest"].Value;

			return value;
		}

		static readonly Regex _regexInstruction = new Regex(
			@"^\s*(?<instr>\w+)(?<rest>.*$)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		private static string ExtractInstruction(ref string code)
		{
			var match = _regexInstruction.Match(code);

			var value = match.Groups["instr"].Value;
			code = match.Groups["rest"].Value;

			return value;
		}
		#endregion
	}
}
